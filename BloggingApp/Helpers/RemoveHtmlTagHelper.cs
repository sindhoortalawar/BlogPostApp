using System.Text.RegularExpressions;

namespace BloggingApp.Helpers
{
    public static class RemoveHtmlTagHelper
    {
        public static string RemoveHtmlTags(string input)
        {
            return Regex.Replace(input, "<.*?>|&.*?;", " ");
        }

    }
}
