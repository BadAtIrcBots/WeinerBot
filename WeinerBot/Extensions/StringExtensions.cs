using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace WeinerBot.Extensions
{
    public static class StringExtensions
    {
        public static IEnumerable<string> SplitInParts(this string s, int partLength = 430)
        {
            if (s == null)
                throw new ArgumentNullException(nameof(s));
            if (partLength <= 0)
                throw new ArgumentException(@"Part length has to be positive.", nameof(partLength));

            for (var i = 0; i < s.Length; i += partLength)
                yield return s.Substring(i, Math.Min(partLength, s.Length - i));
        }

        public static string RemoveMultipleSpaces(this string s)
        {
            return Regex.Replace(s, @"\s+", " ");
        }

        public static string GetNick(this string s)
        {
            return s.Split('!')[0];
        }

        public static string ReplaceNewlines(this string s, string substitution = "")
        {
            return s.Replace("\n", substitution).Replace("\r", substitution);
        }
    }
}
