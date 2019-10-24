using System;
using System.Linq;
using System.Text;

namespace Generator.Common
{
    public static class StringExtension
    {
        public static string TrimStart(this string str, string trimString)
        {
            string result = str;
            while (result.StartsWith(trimString))
            {
                result = result.Substring(trimString.Length);
            }

            return result;
        }

        public static string TrimEnd(this string str, string trimString)
        {
            string result = str;
            while (result.EndsWith(trimString))
            {
                result = result.Substring(0, result.Length - trimString.Length);
            }

            return result;
        }

        public static string Indent(this string str, int count = 4)
        {
            return string.Join(string.Empty, Enumerable.Repeat(' ', count)) + str;
        }

        public static string NewLine(this string str)
        {
            return str + Environment.NewLine;
        }

        public static StringBuilder TrimEnd(this StringBuilder sb, string trimStr)
        {
            return sb.Remove(sb.Length - trimStr.Length, trimStr.Length);
        }
    }
}

