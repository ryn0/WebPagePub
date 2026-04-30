# WebPagePub deploy

Shared deploy logic for any WebPagePub site. Committed to source control.
Contains no secrets and no per-site values.

## Files in this folder

- `deploy.sh` — generic deploy script. Takes a path to a per-site config
  file as its argument, builds the project, generates
  `appsettings.Production.json` from `appsettings.template.json`, rsyncs
  to the VPS, configures systemd / nginx / TLS / (optional) Tor.
- `appsettings.template.json` — template with `${VAR}` placeholders.
  Rendered at deploy time using values from the per-site config.

## Where the per-site configs and orchestrator live

OUTSIDE this repo, on your laptop only, at:

```
~/repos/_deploy/webpagepub/
├── deploy-all.sh          # orchestrator (deploys all sites or a subset)
├── sites/                 # one config file per blog
│   ├── bootbaron.sh
│   ├── ryanwilliamsio.sh
│   └── ...
└── tor-keys/              # optional, per-site Tor keys
```

Per-site configs hold real values — connection strings, AWS keys, IPinfo
tokens — so they never live in source control.

## Variables the template uses

The current `appsettings.template.json` references these variables. Each
per-site config must set them:

| Variable                  | Maps to                                           |
|---------------------------|---------------------------------------------------|
| `SQL_SERVER_CONNECTION`   | `ConnectionStrings.SqlServerConnection`           |
| `AWS_ACCESS_KEY`          | `AmazonEmailCredentials.AccessKey`                |
| `AWS_SECRET_KEY`          | `AmazonEmailCredentials.SecretKey`                |
| `AWS_EMAIL_FROM`          | `AmazonEmailCredentials.EmailFrom`                |
| `IPINFO_ACCESS_TOKEN`     | `IPinfo.AccessToken`                              |

The deploy script also requires these structural values from each
config (used for systemd, nginx, paths, etc., not for the appsettings):

| Variable        | Purpose                                                |
|-----------------|--------------------------------------------------------|
| `VPS`           | SSH alias from `~/.ssh/config`                         |
| `APP_NAME`      | Unique per app (drives systemd / nginx / `/opt/<name>`)|
| `DOMAIN`        | Public domain (e.g. `bootbaron.com`)                   |
| `APP_PORT`      | Loopback port for Kestrel (unique per app)             |
| `WEB_PROJECT`   | Path to the `.csproj` to publish                       |
| `DLL_NAME`      | Output DLL filename (e.g. `WebPagePub.Web.dll`)        |
| `DEPLOY_PATH`   | Destination on the VPS (typically `/opt/$APP_NAME`)    |
| `SERVICE_USER`  | Linux user that runs the app (typically `webapp`)      |
| `ADMIN_EMAIL`   | Used for Let's Encrypt registration                    |
| `TOR_KEYS_DIR`  | Optional: path to Tor key folder. Empty = no onion.    |

## Usage

Direct (deploy one site):

```bash
cd ~/repos/WebPagePub/CI
./deploy.sh ~/repos/_deploy/webpagepub/sites/bootbaron.sh
```

Flags:

```bash
./deploy.sh <config> --skip-build   # skip dotnet publish (faster)
./deploy.sh <config> --no-ssl       # skip TLS (DNS not yet pointed)
```

Normally you'd use the orchestrator in `_deploy/webpagepub/deploy-all.sh`
instead.

## Adding a new variable to the template

1. Add a `${MY_NEW_VAR}` placeholder to `appsettings.template.json`.
2. Add `export MY_NEW_VAR="${MY_NEW_VAR:-}"` to the export block in
   `deploy.sh` (under "Build appsettings.Production.json from template").
3. Add `MY_NEW_VAR=...` to each per-site config in `_deploy/`.

`envsubst` only substitutes EXPORTED variables — the export step is
required.

## Requirements on your laptop

- `dotnet` SDK (matching the project's target version)
- `rsync`, `ssh`, `scp`
- `envsubst` — install on Debian/Ubuntu with:
  ```
  sudo apt-get install gettext-base
  ```
