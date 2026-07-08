#!/usr/bin/env bash
# deploy.sh — generic WebPagePub deployer.
# Lives in WebPagePub/CI/ and is committed to source control.
# No secrets. No per-site values.
#
# Takes a path to a per-site config file as its first argument.
#
# Usage:
#   ./deploy.sh <path-to-site-config.sh> [--skip-build] [--no-ssl]

set -euo pipefail

CONFIG_PATH=""
SKIP_BUILD=0
SKIP_SSL=0
for arg in "$@"; do
    case "$arg" in
        --skip-build) SKIP_BUILD=1 ;;
        --no-ssl)     SKIP_SSL=1 ;;
        -h|--help)    sed -n '2,12p' "$0"; exit 0 ;;
        --*)          echo "Unknown flag: $arg" >&2; exit 1 ;;
        *)
            if [[ -z "$CONFIG_PATH" ]]; then
                CONFIG_PATH="$arg"
            else
                echo "Multiple config paths given: '$CONFIG_PATH' and '$arg'" >&2
                exit 1
            fi
            ;;
    esac
done

if [[ -z "$CONFIG_PATH" ]]; then
    echo "Usage: ./deploy.sh <path-to-site-config.sh> [--skip-build] [--no-ssl]" >&2
    exit 1
fi

[[ -f "$CONFIG_PATH" ]] || { echo "ERROR: config not found: $CONFIG_PATH" >&2; exit 1; }
CONFIG_PATH="$(realpath "$CONFIG_PATH")"

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
TEMPLATE_FILE="$SCRIPT_DIR/appsettings.template.json"
[[ -f "$TEMPLATE_FILE" ]] || { echo "ERROR: template not found: $TEMPLATE_FILE" >&2; exit 1; }

# Source the per-site config.
# shellcheck disable=SC1090
. "$CONFIG_PATH"

# Required values from the site config.
: "${VPS:?$CONFIG_PATH must set VPS}"
: "${APP_NAME:?$CONFIG_PATH must set APP_NAME}"
: "${DOMAIN:?$CONFIG_PATH must set DOMAIN}"
: "${APP_PORT:?$CONFIG_PATH must set APP_PORT}"
: "${WEB_PROJECT:?$CONFIG_PATH must set WEB_PROJECT}"
: "${DEPLOY_PATH:?$CONFIG_PATH must set DEPLOY_PATH}"
: "${SERVICE_USER:?$CONFIG_PATH must set SERVICE_USER}"
: "${DLL_NAME:?$CONFIG_PATH must set DLL_NAME (e.g. WebPagePub.Web.dll)}"
: "${ADMIN_EMAIL:?$CONFIG_PATH must set ADMIN_EMAIL (used for Lets Encrypt)}"
TOR_KEYS_DIR="${TOR_KEYS_DIR:-}"

# Resolve relative paths against the directory the config file lives in.
CONFIG_DIR="$(dirname "$CONFIG_PATH")"
[[ "$WEB_PROJECT" == /* ]] || WEB_PROJECT="$(realpath -m "$CONFIG_DIR/$WEB_PROJECT")"
if [[ -n "$TOR_KEYS_DIR" && "$TOR_KEYS_DIR" != /* ]]; then
    TOR_KEYS_DIR="$(realpath -m "$CONFIG_DIR/$TOR_KEYS_DIR")"
fi

[[ -f "$WEB_PROJECT" ]] || { echo "ERROR: project not found: $WEB_PROJECT" >&2; exit 1; }

ONION_HOST=""
if [[ -n "$TOR_KEYS_DIR" ]]; then
    [[ -f "$TOR_KEYS_DIR/hostname" ]] || { echo "ERROR: $TOR_KEYS_DIR/hostname not found" >&2; exit 1; }
    [[ -f "$TOR_KEYS_DIR/hs_ed25519_public_key" ]] || { echo "ERROR: $TOR_KEYS_DIR/hs_ed25519_public_key not found" >&2; exit 1; }
    [[ -f "$TOR_KEYS_DIR/hs_ed25519_secret_key" ]] || { echo "ERROR: $TOR_KEYS_DIR/hs_ed25519_secret_key not found" >&2; exit 1; }
    ONION_HOST="$(tr -d '[:space:]' < "$TOR_KEYS_DIR/hostname")"
fi

C_CYAN=$'\033[36m'; C_GREEN=$'\033[32m'; C_RED=$'\033[31m'; C_YELLOW=$'\033[33m'; C_RESET=$'\033[0m'
step() { echo; echo "${C_CYAN}==> [$APP_NAME] $*${C_RESET}"; }
ok()   { echo "${C_GREEN}    OK: $*${C_RESET}"; }
warn() { echo "${C_YELLOW}    WARN: $*${C_RESET}"; }
errx() { echo "${C_RED}ERROR: $*${C_RESET}" >&2; exit 1; }

command -v envsubst >/dev/null 2>&1 || errx "envsubst not installed. Run: sudo apt-get install gettext-base"

TMP="$(mktemp -d)"
trap 'rm -rf "$TMP"' EXIT

# ---- Build -----------------------------------------------------------------
WEB_OUT="$SCRIPT_DIR/../publish/$APP_NAME"
WEB_OUT="$(realpath -m "$WEB_OUT")"

if (( ! SKIP_BUILD )); then
    step "Publishing $APP_NAME (linux-x64)"
    rm -rf "$WEB_OUT"
    PROJECT_DIR="$(dirname "$WEB_PROJECT")"
    find "$PROJECT_DIR/.." -type d \( -name bin -o -name obj \) -prune -exec rm -rf {} + 2>/dev/null || true
    dotnet publish "$WEB_PROJECT" \
        -c Release -r linux-x64 --self-contained false \
        -o "$WEB_OUT" --nologo -v minimal
    ok "Published to $WEB_OUT"
fi

# ---- Build appsettings.Production.json from template ----------------------
step "Building appsettings.Production.json from template"

export POSTGRES_CONNECTION="${POSTGRES_CONNECTION:-}"
export AWS_ACCESS_KEY="${AWS_ACCESS_KEY:-}"
export AWS_SECRET_KEY="${AWS_SECRET_KEY:-}"
export AWS_EMAIL_FROM="${AWS_EMAIL_FROM:-}"
export IPINFO_ACCESS_TOKEN="${IPINFO_ACCESS_TOKEN:-}"

envsubst < "$TEMPLATE_FILE" > "$WEB_OUT/appsettings.Production.json"
ok "Wrote $WEB_OUT/appsettings.Production.json"

# ---- Rsync binaries --------------------------------------------------------
step "Syncing to $VPS:$DEPLOY_PATH"
ssh "$VPS" "sudo mkdir -p $DEPLOY_PATH"
rsync -rlptDz --delete --rsync-path="sudo rsync" \
    "$WEB_OUT/" "$VPS:$DEPLOY_PATH/"
ssh "$VPS" "sudo chown -R $SERVICE_USER:$SERVICE_USER $DEPLOY_PATH"
ok "Binaries synced"

# ---- Systemd unit ----------------------------------------------------------
step "Installing systemd unit"
SVC_FILE="$TMP/$APP_NAME.service"
cat > "$SVC_FILE" <<EOF
[Unit]
Description=$APP_NAME ($DOMAIN)
After=network.target

[Service]
WorkingDirectory=$DEPLOY_PATH
ExecStart=/usr/bin/dotnet $DEPLOY_PATH/$DLL_NAME
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=$APP_NAME
User=$SERVICE_USER
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://127.0.0.1:$APP_PORT
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false
Environment=ASPNETCORE_FORWARDEDHEADERS_ENABLED=true
NoNewPrivileges=true
PrivateTmp=true
ProtectSystem=full
ProtectHome=true
ReadWritePaths=$DEPLOY_PATH

[Install]
WantedBy=multi-user.target
EOF

scp "$SVC_FILE" "$VPS:/tmp/$APP_NAME.service"
ssh "$VPS" "sudo mv /tmp/$APP_NAME.service /etc/systemd/system/"
ssh "$VPS" "sudo systemctl daemon-reload"
ssh "$VPS" "sudo systemctl enable $APP_NAME"
ssh "$VPS" "sudo systemctl restart $APP_NAME"
ok "Service started"

# ---- Tor hidden service (idempotent) --------------------------------------
if [[ -n "$TOR_KEYS_DIR" ]]; then
    step "Checking Tor hidden service"
    TOR_DIR="/var/lib/tor/$APP_NAME"

    EXISTING_ONION=""
    if ssh "$VPS" "sudo test -f $TOR_DIR/hostname"; then
        EXISTING_ONION="$(ssh "$VPS" "sudo cat $TOR_DIR/hostname" | tr -d '[:space:]')"
    fi

    if [[ "$EXISTING_ONION" == "$ONION_HOST" ]] && ssh "$VPS" "systemctl is-active --quiet tor"; then
        ok "Tor already configured for $ONION_HOST — skipping setup"
    else
        echo "    First-time Tor setup or mismatch — installing..."

        if ! ssh "$VPS" 'command -v tor >/dev/null 2>&1'; then
            ssh "$VPS" "sudo apt-get install -y tor"
        fi

        ssh "$VPS" "sudo systemctl stop tor 2>/dev/null || true"
        ssh "$VPS" "sudo mkdir -p $TOR_DIR"
        ssh "$VPS" "sudo chown debian-tor:debian-tor $TOR_DIR"
        ssh "$VPS" "sudo chmod 700 $TOR_DIR"
        ssh "$VPS" "sudo rm -f $TOR_DIR/hostname $TOR_DIR/hs_ed25519_public_key $TOR_DIR/hs_ed25519_secret_key"

        echo "    Uploading keys..."
        scp "$TOR_KEYS_DIR/hostname" "$VPS:/tmp/_tor_hostname"
        scp "$TOR_KEYS_DIR/hs_ed25519_public_key" "$VPS:/tmp/_tor_pubkey"
        scp "$TOR_KEYS_DIR/hs_ed25519_secret_key" "$VPS:/tmp/_tor_seckey"

        ssh "$VPS" "sudo mv /tmp/_tor_hostname $TOR_DIR/hostname"
        ssh "$VPS" "sudo mv /tmp/_tor_pubkey $TOR_DIR/hs_ed25519_public_key"
        ssh "$VPS" "sudo mv /tmp/_tor_seckey $TOR_DIR/hs_ed25519_secret_key"

        ssh "$VPS" "sudo chown debian-tor:debian-tor $TOR_DIR/hostname $TOR_DIR/hs_ed25519_public_key $TOR_DIR/hs_ed25519_secret_key"
        ssh "$VPS" "sudo chmod 600 $TOR_DIR/hs_ed25519_secret_key"
        ssh "$VPS" "sudo chmod 644 $TOR_DIR/hs_ed25519_public_key $TOR_DIR/hostname"

        ssh "$VPS" "grep -q 'HiddenServiceDir $TOR_DIR' /etc/tor/torrc || (echo '' | sudo tee -a /etc/tor/torrc >/dev/null && echo 'HiddenServiceDir $TOR_DIR/' | sudo tee -a /etc/tor/torrc >/dev/null && echo 'HiddenServicePort 80 127.0.0.1:$APP_PORT' | sudo tee -a /etc/tor/torrc >/dev/null)"

        ssh "$VPS" "sudo systemctl enable tor"
        ssh "$VPS" "sudo systemctl restart tor"

        sleep 3
        REMOTE_ONION="$(ssh "$VPS" "sudo cat $TOR_DIR/hostname" | tr -d '[:space:]')"
        if [[ "$REMOTE_ONION" != "$ONION_HOST" ]]; then
            errx "Onion mismatch! Expected: $ONION_HOST  Got: $REMOTE_ONION"
        fi
        ok "Tor configured for $ONION_HOST"
    fi
fi

# ---- Nginx config ----------------------------------------------------------
step "Installing nginx config for $DOMAIN"
NGINX_FILE="$TMP/$APP_NAME.conf"

ONION_HEADER=""
if [[ -n "$ONION_HOST" ]]; then
    ONION_HEADER="        add_header Onion-Location http://$ONION_HOST\$request_uri;"
fi

DOTS=$(echo "$DOMAIN" | tr -cd '.' | wc -c)
if [[ "$DOTS" == "1" ]]; then
    SERVER_NAME_LINE="    server_name $DOMAIN www.$DOMAIN;"
else
    SERVER_NAME_LINE="    server_name $DOMAIN;"
fi

cat > "$NGINX_FILE" <<EOF
server {
    listen 80;
    listen [::]:80;
$SERVER_NAME_LINE

    location /.well-known/acme-challenge/ {
        root /var/www/html;
    }

    client_max_body_size 64M;

    location / {
$ONION_HEADER
        proxy_pass         http://127.0.0.1:$APP_PORT;
        proxy_http_version 1.1;
        proxy_set_header   Upgrade           \$http_upgrade;
        proxy_set_header   Connection        keep-alive;
        proxy_set_header   Host              \$host;
        proxy_cache_bypass \$http_upgrade;
        proxy_set_header   X-Forwarded-For   \$proxy_add_x_forwarded_for;
        proxy_set_header   X-Forwarded-Proto \$scheme;
        proxy_set_header   X-Real-IP         \$remote_addr;
        proxy_read_timeout 120s;
        proxy_send_timeout 120s;
    }

    server_tokens off;
}
EOF

scp "$NGINX_FILE" "$VPS:/tmp/$APP_NAME.conf"
ssh "$VPS" "sudo mv /tmp/$APP_NAME.conf /etc/nginx/sites-available/"
ssh "$VPS" "sudo ln -sf /etc/nginx/sites-available/$APP_NAME.conf /etc/nginx/sites-enabled/$APP_NAME.conf"
ssh "$VPS" "sudo nginx -t"
ssh "$VPS" "sudo systemctl reload nginx"
ok "Nginx configured"

# ---- TLS (idempotent) -----------------------------------------------------
HAS_TLS=0
if (( ! SKIP_SSL )); then
    step "Ensuring TLS cert for $DOMAIN"

    if ssh "$VPS" "sudo test -f /etc/letsencrypt/live/$DOMAIN/fullchain.pem"; then
        ok "TLS cert already present for $DOMAIN — wiring into nginx"
        if [[ "$DOTS" == "1" ]]; then
            ssh "$VPS" "sudo certbot --nginx --non-interactive --reinstall -d $DOMAIN -d www.$DOMAIN --redirect" || warn "certbot reinstall failed"
        else
            ssh "$VPS" "sudo certbot --nginx --non-interactive --reinstall -d $DOMAIN --redirect" || warn "certbot reinstall failed"
        fi
        HAS_TLS=1
    else
        if [[ "$DOTS" == "1" ]]; then
            if ssh "$VPS" "sudo certbot --nginx --non-interactive --agree-tos -m $ADMIN_EMAIL -d $DOMAIN -d www.$DOMAIN --redirect"; then
                ok "TLS issued for $DOMAIN + www.$DOMAIN"
                HAS_TLS=1
            else
                warn "certbot failed — DNS may not be pointed at this VPS yet. Site is HTTP-only."
            fi
        else
            if ssh "$VPS" "sudo certbot --nginx --non-interactive --agree-tos -m $ADMIN_EMAIL -d $DOMAIN --redirect"; then
                ok "TLS issued for $DOMAIN"
                HAS_TLS=1
            else
                warn "certbot failed — DNS may not be pointed at this VPS yet. Site is HTTP-only."
            fi
        fi
    fi
fi

# ---- Force www -> apex (301) ----------------------------------------------
# certbot's --redirect only upgrades http->https and PRESERVES the host, so
# www.$DOMAIN would keep serving the app. Insert a www->apex 301 as the first
# statement of every server block (runs before certbot's redirect and covers the
# 443 block), so www never serves the site — it only redirects to the apex host.
if [[ "$DOTS" == "1" ]] && (( HAS_TLS )); then
    step "Forcing www.$DOMAIN -> $DOMAIN (301)"
    ssh "$VPS" "sudo python3 - '$APP_NAME' '$DOMAIN'" <<'PYEOF'
import sys, re
app, domain = sys.argv[1], sys.argv[2]
path = "/etc/nginx/sites-available/%s.conf" % app
lines = open(path).read().splitlines()
want = 'if ($host = www.%s) { return 301 https://%s$request_uri; }' % (domain, domain)
srv = re.compile(r'^\s*server\s*\{')
sn = re.compile(r'^\s*server_name\s+%s\s+www\.%s\s*;\s*$' % (re.escape(domain), re.escape(domain)))
depth = 0; cur = None; blocks = []
for i, l in enumerate(lines):
    if depth == 0 and srv.match(l): cur = {'open': i, 'ok': False}
    if cur and sn.match(l): cur['ok'] = True
    d0 = depth; depth += l.count('{') - l.count('}')
    if cur and d0 > 0 and depth == 0: cur['close'] = i; blocks.append(cur); cur = None
ins = []
for b in blocks:
    if b['ok'] and want not in "\n".join(lines[b['open']:b['close'] + 1]):
        ins.append((b['open'] + 1, "    " + want))
for idx, t in sorted(ins, reverse=True): lines.insert(idx, t)
if ins: open(path, "w").write("\n".join(lines) + "\n")
print("www-redirect: inserted %d block(s)" % len(ins))
PYEOF
    ssh "$VPS" "sudo nginx -t && sudo systemctl reload nginx" && ok "www -> apex enforced"
fi

# ---- Smoke test ------------------------------------------------------------
PROTO="http://"
(( HAS_TLS )) && PROTO="https://"

step "Smoke testing $PROTO$DOMAIN"
SMOKE_OK=0
for i in 1 2 3 4 5 6 7 8 9 10; do
    code=$(curl -sS -o /dev/null -w '%{http_code}' --max-time 15 "$PROTO$DOMAIN" || echo 000)
    if [[ "$code" == "200" || "$code" == "301" || "$code" == "302" ]]; then
        ok "Site returned HTTP $code"
        SMOKE_OK=1
        break
    fi
    echo "    Attempt $i/10 got HTTP $code, retrying in 5s..."
    sleep 5
done
(( SMOKE_OK )) || warn "Smoke test did not return 200/301/302."

echo
echo "${C_GREEN}===============================${C_RESET}"
echo "${C_GREEN} [$APP_NAME] Deploy complete${C_RESET}"
echo "${C_GREEN}===============================${C_RESET}"
echo " Site:    $PROTO$DOMAIN"
[[ -n "$ONION_HOST" ]] && echo " Onion:   http://$ONION_HOST"
echo " Status:  ssh $VPS sudo systemctl status $APP_NAME"
echo " Logs:    ssh $VPS journalctl -u $APP_NAME -f"
echo
