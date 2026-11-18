namespace WebPagePub.WebApp.Helpers
{
    /// <summary>
    /// Writes one CSV field safely. Reports put visitor-supplied text (the Referer header, the clicked
    /// address) into CSV files that admins open in a spreadsheet, so every field is quoted (a comma or
    /// line break can't add columns or rows), and a value starting with =, +, -, @, tab or carriage
    /// return gets a leading single quote so the spreadsheet shows it as text instead of running it as
    /// a formula.
    /// </summary>
    public static class CsvField
    {
        private static readonly char[] FormulaStarts = { '=', '+', '-', '@', '\t', '\r' };

        public static string Escape(string? value)
        {
            var text = value ?? string.Empty;

            if (text.Length > 0 && Array.IndexOf(FormulaStarts, text[0]) >= 0)
            {
                text = "'" + text;
            }

            return "\"" + text.Replace("\"", "\"\"", StringComparison.Ordinal) + "\"";
        }

        public static string Row(params string?[] values)
        {
            return string.Join(",", values.Select(Escape));
        }
    }
}
