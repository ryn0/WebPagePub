namespace WebPagePub.FileStorage.Helpers
{
    /// <summary>
    /// The one place that decides which file names and folder paths may be written to site storage.
    /// Both storage back ends (Bunny and Azure) use it for uploads, renames, listings and deletes,
    /// so a rule can't be enforced in one place and forgotten in another.
    /// </summary>
    public static class StorageFileRules
    {
        /// <summary>
        /// File types an administrator may put in storage. This is an allow list on purpose: storage
        /// is served from the site's CDN, so anything a browser would run as a page (svg, html, xml,
        /// xhtml, mht) or a server would run (php, aspx) must never get there, and a deny list always
        /// misses something. css, js and the fonts stay because sites reference their own stylesheets,
        /// scripts and fonts from the CDN (set in site snippets).
        /// </summary>
        private static readonly HashSet<string> AllowedExtensions = new (StringComparer.OrdinalIgnoreCase)
        {
            "jpg", "jpeg", "png", "gif", "webp", "avif", "ico",
            "pdf", "txt", "csv",
            "mp4", "webm", "mp3",
            "woff", "woff2",
            "css", "js",
        };

        // Characters that change how a storage address is read: ? and # end the path, % starts an
        // encoded character (so "%2e%2e" could become ".."), : and \ mean drives or other separators.
        private static readonly char[] ForbiddenPathCharacters = { '\\', '%', '?', '#', ':', '*', '"', '<', '>', '|' };

        public static IReadOnlyCollection<string> AllowedFileExtensions => AllowedExtensions;

        /// <summary>
        /// True when a bare file name (no folder part) has an allowed extension and nothing that could
        /// move it to another folder.
        /// </summary>
        public static bool IsAllowedFileName(string? fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName) || fileName != fileName.Trim())
            {
                return false;
            }

            if (fileName.Contains('/') || fileName.Contains("..", StringComparison.Ordinal) || HasForbiddenCharacters(fileName))
            {
                return false;
            }

            // "photo." and "photo" have no real type; ".jpg" has no name.
            var dot = fileName.LastIndexOf('.');
            if (dot <= 0 || dot == fileName.Length - 1)
            {
                return false;
            }

            return AllowedExtensions.Contains(fileName[(dot + 1)..]);
        }

        /// <summary>
        /// True when a folder path stays inside the storage container. Empty means the top folder.
        /// Leading and trailing slashes are fine ("/page_img/12/").
        /// </summary>
        public static bool IsSafeFolderPath(string? folderPath)
        {
            if (string.IsNullOrEmpty(folderPath))
            {
                return true;
            }

            if (folderPath.Contains("..", StringComparison.Ordinal) || HasForbiddenCharacters(folderPath))
            {
                return false;
            }

            // An empty segment in the middle ("a//b") is read differently by different servers.
            var inner = folderPath.Trim('/');
            if (inner.Contains("//", StringComparison.Ordinal))
            {
                return false;
            }

            foreach (var segment in inner.Split('/', StringSplitOptions.RemoveEmptyEntries))
            {
                if (segment == "." || segment != segment.Trim())
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// True when a full storage path ("page_img/12/photo.jpg") is a safe folder plus an allowed
        /// file name. Used for the new name when a file is renamed.
        /// </summary>
        public static bool IsAllowedObjectPath(string? objectPath)
        {
            if (string.IsNullOrWhiteSpace(objectPath))
            {
                return false;
            }

            var slash = objectPath.LastIndexOf('/');
            var folder = slash >= 0 ? objectPath[..(slash + 1)] : string.Empty;
            var name = slash >= 0 ? objectPath[(slash + 1)..] : objectPath;

            return IsSafeFolderPath(folder) && IsAllowedFileName(name);
        }

        /// <summary>
        /// True when an existing storage path can be read or removed: it may have any type (old files
        /// of types no longer accepted can still be deleted), but it must stay inside the container.
        /// </summary>
        public static bool IsSafeExistingPath(string? objectPath)
        {
            // Looser than the folder rule on purpose: files stored years ago may have names with
            // characters new names can't use, and they must stay removable. Climbing out of the
            // container is still refused.
            return !string.IsNullOrWhiteSpace(objectPath)
                && !objectPath.Contains("..", StringComparison.Ordinal)
                && !objectPath.Contains('\\')
                && !objectPath.Any(char.IsControl);
        }

        public static void EnsureAllowedFileName(string? fileName)
        {
            if (!IsAllowedFileName(fileName))
            {
                throw new ArgumentException(
                    $"The file name \"{fileName}\" is not allowed. Allowed types: {string.Join(", ", AllowedExtensions)}.");
            }
        }

        public static void EnsureSafeFolderPath(string? folderPath)
        {
            if (!IsSafeFolderPath(folderPath))
            {
                throw new ArgumentException($"The folder path \"{folderPath}\" is not allowed.");
            }
        }

        public static void EnsureSafeExistingPath(string? objectPath)
        {
            if (!IsSafeExistingPath(objectPath))
            {
                throw new ArgumentException($"The file path \"{objectPath}\" is not allowed.");
            }
        }

        public static void EnsureAllowedObjectPath(string? objectPath)
        {
            if (!IsAllowedObjectPath(objectPath))
            {
                throw new ArgumentException(
                    $"The file path \"{objectPath}\" is not allowed. Allowed types: {string.Join(", ", AllowedExtensions)}.");
            }
        }

        private static bool HasForbiddenCharacters(string value)
        {
            return value.IndexOfAny(ForbiddenPathCharacters) >= 0 || value.Any(char.IsControl);
        }
    }
}
