using CsvHelper.Configuration;
using CsvHelper;
using CsvHelper.TypeConversion;
using System.Globalization;

namespace WebPagePub.PageManager.Console.Converters
{
    public class NullableInt32Converter : DefaultTypeConverter
    {
        public override object? ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)  // Made 'text' nullable
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return 0; // return a default value
            }

            if (int.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out int result))
            {
                return result;
            }

            object? baseResult = base.ConvertFromString(text, row, memberMapData);
            return baseResult ?? 0;  // If baseResult is null, return 0
        }
    }
}