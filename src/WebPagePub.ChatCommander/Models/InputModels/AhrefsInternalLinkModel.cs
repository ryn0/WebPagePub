using CsvHelper.Configuration.Attributes;
using CsvHelper.TypeConversion;
using WebPagePub.ChatCommander.Converters;

namespace WebPagePub.ChatCommander.Models.InputModels
{
    public class AhrefsInternalLinkModel
    {
        public int PR { get; set; }

        [Name("Source page")]
        public Uri SourcePage { get; set; }

        [Name("Source is canonical")]
        public bool SourceIsCanonical { get; set; }

        [Name("Source is noindex")]
        public bool SourceIsNoIndex { get; set; }

        [Name("Source total traffic")]
        public int SourceTotalTraffic { get; set; }

        public string Keyword { get; set; }

        [Name("Keyword context")]
        public string KeywordContext { get; set; }

        [Name("Keyword search volume")]
        public int KeywordSearchVolume { get; set; }

        [Name("Keyword difficulty")]
        public int KeywordDifficulty { get; set; }

        [Name("Target page")]
        public Uri TargetPage { get; set; }

        [Name("Target position")]
        public int TargetPosition { get; set; }

        [Name("Target traffic")]
        [TypeConverter(typeof(NullableInt32Converter))]
        public int? TargetTraffic { get; set; }
    }
}
