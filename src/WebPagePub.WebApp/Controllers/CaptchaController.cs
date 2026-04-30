// Controllers/CaptchaController.cs
using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace WebPagePub.Web.Controllers
{
    [Route("captcha")]
    public class CaptchaController : Controller
    {
        private const int Width = 150;
        private const int Height = 50;
        private const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

        // Resolve a font once at startup. Tries common cross-platform sans-serif
        // families and falls back to whatever the system has.
        private static readonly Font CaptchaFont = LoadFont();

        [HttpGet("image")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Image([FromQuery] string? ctx = "default")
        {
            // 1) generate a 6-char code (A-Z 2-9, skip confusing chars)
            string code = string.Concat(Enumerable.Range(0, 6)
                .Select(_ => Alphabet[RandomNumberGenerator.GetInt32(Alphabet.Length)]));

            // 2) stash in session (both ctx-specific and legacy fallback key)
            string keyCtx = $"CaptchaCode:{(string.IsNullOrWhiteSpace(ctx) ? "default" : ctx)}";
            this.HttpContext.Session.SetString(keyCtx, code);
            this.HttpContext.Session.SetString("CaptchaCode", code);

            // 3) render a basic PNG
            using var image = new Image<Rgba32>(Width, Height, Color.White);

            image.Mutate(ctxt =>
            {
                // simple noise
                for (int i = 0; i < 8; i++)
                {
                    var x1 = RandomNumberGenerator.GetInt32(0, Width);
                    var y1 = RandomNumberGenerator.GetInt32(0, Height);
                    var x2 = RandomNumberGenerator.GetInt32(0, Width);
                    var y2 = RandomNumberGenerator.GetInt32(0, Height);
                    ctxt.DrawLine(Color.LightGray, 1f, new PointF(x1, y1), new PointF(x2, y2));
                }

                // measure and center
                var measureOptions = new TextOptions(CaptchaFont);
                var size = TextMeasurer.MeasureSize(code, measureOptions);
                float x = (Width - size.Width) / 2f;
                float y = (Height - size.Height) / 2f;

                ctxt.DrawText(code, CaptchaFont, Color.Black, new PointF(x, y));
            });

            // 4) return PNG bytes
            using var ms = new MemoryStream();
            image.SaveAsPng(ms);
            return this.File(ms.ToArray(), "image/png");
        }

        private static Font LoadFont()
        {
            // Try families that exist on Linux, macOS, and Windows.
            string[] candidates =
            {
                "DejaVu Sans",     // most Linux distros (with fonts-dejavu)
                "Liberation Sans", // RHEL/CentOS, some minimal images
                "Noto Sans",       // newer Linux distros
                "Arial",           // Windows / macOS
                "Helvetica",       // macOS
            };

            foreach (var name in candidates)
            {
                if (SystemFonts.TryGet(name, out var family))
                {
                    return family.CreateFont(24, FontStyle.Bold);
                }
            }

            // Last resort: take whatever the system has.
            var any = SystemFonts.Families.FirstOrDefault();
            if (any != default)
            {
                return any.CreateFont(24, FontStyle.Bold);
            }

            throw new InvalidOperationException(
                "No system fonts found. On Debian/Ubuntu install: " +
                "sudo apt install fontconfig fonts-dejavu");
        }
    }
}