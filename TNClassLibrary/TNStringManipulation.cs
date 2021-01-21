using System;
using System.Text;
using System.Text.RegularExpressions;

namespace TNClassLibrary
{
    /// <summary>
    /// Class that contains string manipulation functions
    /// </summary>
    public static class TNStringManipulation
    {

        /// <summary>
        /// 2a. a.Add a method called XXExtractDigits that accepts a string and returns a string:
        ///         i.Null is possible, so don’t blow up on it.
        ///         ii.Return a string containing all digits found in the input string.
        /// </summary>
        /// <param name="input">string to find the digits in</param>
        /// <returns>digits possible in the string</returns>
        
        public static  string TNExtractDigits(string input)
        {
            StringBuilder builder = new StringBuilder();
            char c = new char();
            if (!string.IsNullOrEmpty(input))
            {
                for (int i = 0; i < input.Length; i++)
                {
                    c = input[i];
                    if (Char.IsDigit(c))
                    {
                        builder.Append(c);
                    }
                }
            }
            return builder.ToString();
        }

        /// <summary>
        ///   b.Add a method called XXPostalCodeIsValid that accepts two strings and returns a Boolean:
        ///      i.The first string parameter is the given postal code.The second string is the postal code Regex pattern from the country table.
        ///      ii.Return true if the string matches the country’s postal pattern … or is null/empty.
        /// </summary>
        /// <param name="postalCode">The postal code to be checked</param>
        /// <param name="postalCodeRegex">The pattern to check the postal code against</param>
        /// <returns>true if matched or null/empty, false if not a match</returns>
        
        public static bool TNPostalCodeIsValid(string postalCode, string postalCodeRegex)
        {
            bool output = false;
            Regex regex = new Regex(postalCodeRegex);
            if (string.IsNullOrEmpty(postalCode) || regex.IsMatch(postalCode))
            {
                output = true;
            }
            return output;
        }

        /// <summary>
        /// c.Add a method called XXCapitalize that accepts a string and returns a string:
        ///     i.If the input string is null, return it as an empty string.
        ///     ii.Change the input string to lower case and remove leading & trailing spaces.
        ///     iii.Shift the first letter of every word in the string to upper case.
        ///     iv.Return the newly-capitalised string.
        /// </summary>
        /// <param name="input">Input string to have words capitalized</param>
        /// <returns>the capitalized string</returns>
        
        public static string TNCapitalize(string input)
        {
            string output = String.Empty;
            if(!string.IsNullOrEmpty(input))
            {
                output = input.Trim();
                char[] outputAsChars = output.ToCharArray();
                for (int i = 0; i < outputAsChars.Length; i++)
                {
                    if(i == 0)
                    {
                        outputAsChars[i] = char.ToUpper(outputAsChars[i]);
                    }
                    if(outputAsChars[i] == ' ')
                    {
                        outputAsChars[i + 1] = Char.ToUpper(outputAsChars[i + 1]);
                    }
                }
                output = new string(outputAsChars);
            }
            return output;
        }
    }
}
