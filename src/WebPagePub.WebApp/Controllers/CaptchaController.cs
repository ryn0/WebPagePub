// Controllers/CaptchaController.cs
using System.Drawing;
using System.Drawing.Imaging;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;

namespace WebPagePub.Web.Controllers
{
    [Route("captcha")]
    public class CaptchaController : Controller
    {
        [HttpGet("image")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Image([FromQuery] string? ctx = "default")
        {
            // 1) generate a 5- or 6-char code (A-Z 2-9, skip confusing chars)
            const string alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            string code = string.Concat(Enumerable.Range(0, 6)
                                .Select(_ => alphabet[RandomNumberGenerator.GetInt32(alphabet.Length)]));

            // 2) stash in session (both ctx-specific and legacy fallback key)
            string keyCtx = $"CaptchaCode:{(string.IsNullOrWhiteSpace(ctx) ? "default" : ctx)}";
            this.HttpContext.Session.SetString(keyCtx, code);
            this.HttpContext.Session.SetString("CaptchaCode", code);

            // 3) render a basic PNG
            using var bmp = new Bitmap(150, 50);
            using var gfx = Graphics.FromImage(bmp);
            gfx.Clear(Color.White);

            using var font = new Font(FontFamily.GenericSansSerif, 24, FontStyle.Bold);
            using var brush = new SolidBrush(Color.Black);

            // simple noise
            var pen = Pens.LightGray;
            for (int i = 0; i < 8; i++)
            {
                var x1 = RandomNumberGenerator.GetInt32(0, 150);
                var y1 = RandomNumberGenerator.GetInt32(0, 50);
                var x2 = RandomNumberGenerator.GetInt32(0, 150);
                var y2 = RandomNumberGenerator.GetInt32(0, 50);
                gfx.DrawLine(pen, x1, y1, x2, y2);
            }

            // draw centered-ish
            var size = gfx.MeasureString(code, font);
            float x = (150 - size.Width) / 2f;
            float y = (50 - size.Height) / 2f;
            gfx.DrawString(code, font, brush, x, y);

            // 4) return PNG bytes
            using var ms = new MemoryStream();
            bmp.Save(ms, ImageFormat.Png);
            ms.Position = 0;
            return this.File(ms.ToArray(), "image/png");
        }
    }
}
