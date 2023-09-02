using WebPagePub.ChatCommander.Models.SettingsModels;
using WebPagePub.Managers.Interfaces;

namespace WebPagePub.ChatCommander.WorkFlows.Generators
{
    public abstract class BaseGenerator
    {
        public string SectionKey { get; set; }
        protected const int maxAttempts = 3;
        protected int MinutesOffsetForArticleMin { get; set; } = 90;
        protected int MinutesOffsetForArticleMax { get; set; } = 10080/*1 week*/;
        protected DateTime startDateTime;
        protected int completed = 0;
        protected ChatGptSettings chatGptSettings;
        protected ISitePageManager sitePageManager;
        protected ChatGPT chatGPT;

        public BaseGenerator(
            ChatGptSettings chatGptSettings,
            ISitePageManager sitePageManager)
        {
            this.chatGptSettings = chatGptSettings;
            this.sitePageManager = sitePageManager;
            chatGPT = new ChatGPT(chatGptSettings);
        }

        protected DateTime OffSetTime(DateTime now)
        {
            DateTime startDate;
            DateTime endDate;

            if (MinutesOffsetForArticleMin >= 0 && MinutesOffsetForArticleMax >= 0)
            {
                startDate = now.AddMinutes(MinutesOffsetForArticleMin);
                endDate = now.AddMinutes(MinutesOffsetForArticleMax);
            }
            else
            {
                startDate = now.AddMinutes(MinutesOffsetForArticleMax);
                endDate = now.AddMinutes(MinutesOffsetForArticleMin);
            }

            var randomTest = new Random();
            TimeSpan timeSpan = endDate - startDate;
            TimeSpan newSpan = new TimeSpan(0, randomTest.Next(0, (int)timeSpan.TotalMinutes), 0);

            return startDate + newSpan;
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
            var firstAuthor = sitePageManager.GetAllAuthors();

            if (firstAuthor == null || firstAuthor.Count == 0)
            {
                return null;
            }

            return firstAuthor.First().AuthorId;
        }
    }
}