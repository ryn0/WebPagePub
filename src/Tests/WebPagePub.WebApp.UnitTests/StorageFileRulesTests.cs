using WebPagePub.FileStorage.Helpers;
using Xunit;

namespace WebPagePub.WebApp.UnitTests
{
    public class StorageFileRulesTests
    {
        [Theory]
        [InlineData("photo.jpg")]
        [InlineData("photo.JPEG")]
        [InlineData("my-photo_2.png")]
        [InlineData("a.gif")]
        [InlineData("a.webp")]
        [InlineData("a.avif")]
        [InlineData("favicon.ico")]
        [InlineData("guide.pdf")]
        [InlineData("_.txt")]
        [InlineData("data.csv")]
        [InlineData("clip.mp4")]
        [InlineData("clip.webm")]
        [InlineData("sound.mp3")]
        [InlineData("font.woff")]
        [InlineData("font.woff2")]
        [InlineData("site.css")]
        [InlineData("site.min.js")]
        [InlineData("report.html.jpg")]
        public void AllowedFileNames_AreAccepted(string name)
        {
            Assert.True(StorageFileRules.IsAllowedFileName(name));
        }

        [Theory]
        [InlineData("logo.svg")]
        [InlineData("page.html")]
        [InlineData("page.htm")]
        [InlineData("page.xht")]
        [InlineData("page.xhtml")]
        [InlineData("page.mht")]
        [InlineData("feed.xml")]
        [InlineData("shell.php")]
        [InlineData("shell.aspx")]
        [InlineData("tool.exe")]
        [InlineData("config.json")]
        [InlineData("photo.jpg.")]
        [InlineData("photo")]
        [InlineData(".jpg")]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        [InlineData("../photo.jpg")]
        [InlineData("..\\photo.jpg")]
        [InlineData("folder/photo.jpg")]
        [InlineData("photo%2ejpg")]
        [InlineData("photo.jpg?x=1")]
        [InlineData("photo.jpg ")]
        [InlineData("pho\nto.jpg")]
        public void RefusedFileNames_AreRejected(string? name)
        {
            Assert.False(StorageFileRules.IsAllowedFileName(name));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("/")]
        [InlineData("page_img/12/")]
        [InlineData("/page_img/12/")]
        [InlineData("icons")]
        public void SafeFolderPaths_AreAccepted(string? path)
        {
            Assert.True(StorageFileRules.IsSafeFolderPath(path));
        }

        [Theory]
        [InlineData("../")]
        [InlineData("page_img/../../")]
        [InlineData("..")]
        [InlineData("a\\b")]
        [InlineData("a//b")]
        [InlineData("a/./b")]
        [InlineData("%2e%2e/")]
        [InlineData("c:/windows")]
        public void UnsafeFolderPaths_AreRejected(string path)
        {
            Assert.False(StorageFileRules.IsSafeFolderPath(path));
        }

        [Theory]
        [InlineData("page_img/12/photo.jpg", true)]
        [InlineData("photo_thumb.png", true)]
        [InlineData("page_img/12/photo.svg", false)]
        [InlineData("page_img/12/photo.html", false)]
        [InlineData("page_img/../photo.jpg", false)]
        [InlineData("page_img/12/", false)]
        public void ObjectPaths_UseFolderAndNameRules(string path, bool expected)
        {
            Assert.Equal(expected, StorageFileRules.IsAllowedObjectPath(path));
        }

        [Theory]
        [InlineData("page_img/12/old file%20name.svg", true)]
        [InlineData("../other-zone/file.jpg", false)]
        [InlineData("a\\b.jpg", false)]
        [InlineData("", false)]
        public void ExistingPaths_OnlyNeedToStayInside(string path, bool expected)
        {
            Assert.Equal(expected, StorageFileRules.IsSafeExistingPath(path));
        }

        [Fact]
        public void Ensure_ThrowsArgumentException_ForRefusedName()
        {
            Assert.Throws<ArgumentException>(() => StorageFileRules.EnsureAllowedFileName("evil.svg"));
            Assert.Throws<ArgumentException>(() => StorageFileRules.EnsureSafeFolderPath("../"));
            Assert.Throws<ArgumentException>(() => StorageFileRules.EnsureAllowedObjectPath("a/b.html"));
        }
    }
}
