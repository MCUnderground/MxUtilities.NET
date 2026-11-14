using MxUtilities.FileHandling.Models;
using System.Text;

namespace MxUtilities.FileHandling.Helpers
{
    public static class MaxFileParserHelper
    {
        public static string GetStreamAsCleanString(MaxStream stream)
        {
            string content = Encoding.UTF8.GetString(stream.GetBuffer());
            content = RemoveNonPrintableChars(content);
            return content;
        }

        public static string CleanString(this string input)
        {
            return RemoveNonPrintableChars(input);
        }

        private static string RemoveNonPrintableChars(string input)
        {
            var builder = new StringBuilder();


            foreach (char ch in input)
            {
                if (!char.IsControl(ch))
                {
                    builder.Append(ch);
                }
            }
            return builder.ToString();
        }
    }
}
