using WebPagePub.ChatCommander.Models.SettingsModels;
using WebPagePub.Managers.Interfaces;

namespace WebPagePub.ChatCommander.WorkFlows.Generators
{
    public abstract class BaseGenerator
    {
        public string SectionKey { get; set; } = string.Empty;
        protected const int maxAttempts = 3;
        protected int MinutesOffsetForArticleMin { get; set; } = 90;
        protected int MinutesOffsetForArticleMax { get; set; } = 10080/*1 week*/;
        protected DateTime startDateTime;
        protected int completed = 0;
        protected OpenAiApiSettings OpenAiApiSettings;
        protected ISitePageManager sitePageManager;
        protected OpenAiApiClient openAiApiClient;

        public BaseGenerator(
            OpenAiApiSettings openAiApiSettings,
            ISitePageManager sitePageManager)
        {
            this.OpenAiApiSettings = openAiApiSettings;
            this.sitePageManager = sitePageManager;
            openAiApiClient = new OpenAiApiClient(openAiApiSettings);
            startDateTime = DateTime.UtcNow;
        }

        public BaseGenerator(ISitePageManager sitePageManager)
        {
            this.sitePageManager = sitePageManager;
            startDateTime = DateTime.UtcNow;
        }

        protected void WriteStartMessage(string typeName)
        {
            Console.WriteLine($"Starting {typeName}: {startDateTime}");
        }

        protected void WriteCompletionMessage()
        {
            Console.WriteLine("--------------------");
            var now = DateTime.UtcNow;
            var timespan = now - startDateTime;
            var totalTime = $"{(int)timespan.TotalMinutes} minutes {timespan.Seconds:00} seconds";
            Console.WriteLine($"Completed: {completed} - Total Time: {totalTime}");
            Console.ReadLine();
        }

        protected int? GetAuthor()
        {
            var allAuthors = sitePageManager.GetAllAuthors();

            if (allAuthors == null || allAuthors.Count == 0)
            {
                return null;
            }

            return allAuthors.First().AuthorId;
        }
    }
}