namespace WebPagePub.WebApp.Helpers
{
    /// <summary>
    /// Request size limits. Ordinary requests (comments, sign-in, page edits) are small; only the
    /// three admin upload actions accept large bodies. Before 2026-11-18 every address accepted
    /// 64 MB, so a handful of large anonymous posts could push the app into its memory shutdown.
    /// </summary>
    public static class UploadLimits
    {
        /// <summary>Largest admin upload request (several photos at once).</summary>
        public const long MaxBytes = 64L * 1024 * 1024;

        /// <summary>Largest ordinary request. Page content is the big one; 4 MB is far above any article.</summary>
        public const long OrdinaryMaxBytes = 4L * 1024 * 1024;
    }
}
