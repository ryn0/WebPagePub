﻿namespace WebPagePub.ChatCommander.Models.SettingsModels
{
    public class ArticleWithCalculatorGeneratorModel
    {
        public int MinutesOffsetForArticleMin { get; set; }
        public int MinutesOffsetForArticleMax { get; set; }
        public string SectionKey { get; set; } = string.Empty;
    }
}
