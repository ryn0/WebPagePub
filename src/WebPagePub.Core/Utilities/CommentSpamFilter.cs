using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace WebPagePub.Core.Utilities
{
    /// <summary>
    /// Rule-based spam check for public comments (no AI, no outside service). It adds up
    /// signals seen in real comment spam: a website link in the "website" box, a name that is
    /// an advertising phrase, drug / adult / casino wording, stock praise that could sit under
    /// any article, text in a different script from the site, throwaway email names, and
    /// probe or code-injection strings. A comment scoring <see cref="SpamThreshold"/> or more
    /// is filed as Spam (kept, so the admin can still look) and never waits in the moderation list.
    /// </summary>
    public static partial class CommentSpamFilter
    {
        public const int SpamThreshold = 3;

        private static readonly string[] GenericPraise =
        {
            "great post", "nice post", "good post", "very good post", "wonderful blog", "nice blog", "great blog",
            "this weblog", "this web site", "this website", "this web page", "this blog", "your blog", "your web site",
            "your website", "your site", "webmaster", "keep up the good", "keep up the great", "thumbs up",
            "be coming back", "bookmarked", "book marked", "piece of writing", "share your blog", "linking to this",
            "know-how", "thanks for sharing", "thanks in support of sharing", "this post", "this article is",
            "i found it while", "loading pace", "visiting this", "excellent information", "start my own blog",
            "tell us more about this", "read news papers", "audio songs",
        };

        /// <summary>The result of scoring one comment.</summary>
        public sealed record Verdict(int Score, IReadOnlyList<string> Reasons)
        {
            public bool IsSpam => this.Score >= SpamThreshold;
        }

        public static Verdict Evaluate(string? name, string? email, string? website, string? comment, string? honeypot = null)
        {
            var score = 0;
            var reasons = new List<string>();
            void Add(int points, string reason)
            {
                score += points;
                reasons.Add(reason);
            }

            name = (name ?? string.Empty).Trim();
            email = (email ?? string.Empty).Trim();
            website = (website ?? string.Empty).Trim();
            comment = (comment ?? string.Empty).Trim();

            if (!string.IsNullOrWhiteSpace(honeypot))
            {
                Add(SpamThreshold, "hidden field filled in");
            }

            if (website.Length > 0)
            {
                Add(2, "website link supplied");
            }

            if (SpamWords().IsMatch(name))
            {
                Add(3, "name is an advertising phrase");
            }
            else if (name.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length >= 4)
            {
                Add(1, "name is a long phrase");
            }

            if (SpamWords().IsMatch(comment))
            {
                Add(3, "advertising wording in the comment");
            }

            if (NonLatinShare(name + " " + comment) > 0.3)
            {
                Add(3, "written in a different script");
            }

            if (LooksObfuscated(comment))
            {
                Add(2, "look-alike letters used to dodge filters");
            }

            var lower = comment.ToLowerInvariant();
            var praiseHits = GenericPraise.Count(p => lower.Contains(p, StringComparison.Ordinal));
            if (praiseHits >= 2)
            {
                Add(2, "stock praise that fits any article");
            }
            else if (praiseHits == 1)
            {
                Add(1, "stock praise that fits any article");
            }

            // Patterns from the snoringmouthpiecereview queue (2026-11): the comment is just an
            // email address, a few characters, or one long run of letters and digits.
            if (EmailAddress().IsMatch(comment))
            {
                Add(3, "comment contains an email address");
            }

            if (comment.Length < 25)
            {
                Add(3, "too short to be a real comment");
            }

            if (LongRun().IsMatch(comment) || LongRun().IsMatch(name))
            {
                Add(3, "random-looking run of characters");
            }

            if (email.Split('@')[0].Count(ch => ch == '.') >= 3)
            {
                Add(2, "email name split up with dots");
            }

            if (ThrowawayEmail().IsMatch(email))
            {
                Add(1, "generated-looking email name");
            }

            if (ProbeText().IsMatch(name) || ProbeText().IsMatch(comment))
            {
                Add(SpamThreshold, "test or code-injection text");
            }

            return new Verdict(score, reasons);
        }

        private static double NonLatinShare(string text)
        {
            int letters = 0, nonLatin = 0;
            foreach (var ch in text)
            {
                if (!char.IsLetter(ch))
                {
                    continue;
                }

                letters++;
                if (ch > 'ɏ')
                {
                    nonLatin++;
                }
            }

            return letters == 0 ? 0 : (double)nonLatin / letters;
        }

        // Words that mix Latin with look-alike Cyrillic / Greek / Coptic / full-width letters.
        private static bool LooksObfuscated(string text)
        {
            var mixedWords = 0;
            foreach (var word in text.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                bool latin = false, other = false;
                foreach (var ch in word)
                {
                    if (!char.IsLetter(ch))
                    {
                        continue;
                    }

                    if (ch <= 'ɏ')
                    {
                        latin = true;
                    }
                    else
                    {
                        other = true;
                    }
                }

                if (latin && other)
                {
                    mixedWords++;
                }
            }

            return mixedWords >= 3;
        }

        [GeneratedRegex(@"(?<![a-z])(porn\w*|xxx|sex|shemale|blowjob|anal|threesome|orgy|escorts?|casino|betting|slots?|poker|mega888|1xbet|viagra|cialis|tadalafil|xanax|valium|fentanyl|tramadol|cannabis|pharmacy|payday|loans?|seo|backlinks?|gsa|buy\s+\w+\s+online|without\s+prescription|downloader|essay|replica)(?![a-z])|порно|купить|色情|成人|赌", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, 250)]
        private static partial Regex SpamWords();

        [GeneratedRegex(@"[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, 250)]
        private static partial Regex EmailAddress();

        // 18 or more letters/digits in a row with no space ("MEKYUJTYJ854716MAYTRYR").
        [GeneratedRegex(@"[A-Za-z0-9]{18,}", RegexOptions.CultureInvariant, 250)]
        private static partial Regex LongRun();

        // firstnamelastname123@ style addresses produced by spam tools.
        [GeneratedRegex(@"^[a-z]{8,}\d{2,4}@", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, 250)]
        private static partial Regex ThrowawayEmail();

        [GeneratedRegex(@"<\s*script|<\s*/?\s*[a-z]+[^>]*>|\{\{.*?\}\}|\$\{[^}]*\}|(?<![a-z])(select|union|sleep|pg_sleep)\s*\(|'\s*(or|and)\s|--\s*$|(?<![a-z])(probe|ssrf|xss|sqli|qa\d{2,})(?![a-z])", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Singleline, 250)]
        private static partial Regex ProbeText();
    }
}
