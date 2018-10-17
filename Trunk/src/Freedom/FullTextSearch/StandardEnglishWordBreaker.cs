using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Freedom.FullTextSearch.Configuration;

namespace Freedom.FullTextSearch
{
    public class StandardEnglishWordBreaker : IWordBreaker
    {
        private static readonly string[] EmptyArray = new string[0];
        private static readonly char[] SplitChar = {' '};

        private readonly List<TextSubstitution> _substitutions = new List<TextSubstitution>();
        private readonly List<string> _noiseWords = new List<string>();

        public StandardEnglishWordBreaker()
            : this(FullTextSearchConfigurationSection.GetSection())
        {
        }

        public StandardEnglishWordBreaker(FullTextSearchConfigurationSection configurationSection)
        {
            if (configurationSection == null) return;

            if (configurationSection.Substitutions != null)
            {
                foreach (SubstitutionElement substitution in configurationSection.Substitutions)
                {
                    if (substitution.IsValid)
                        AddSubstitution(substitution.Find, substitution.ReplaceWith);
                }
            }

            if (configurationSection.NoiseWords != null)
            {
                foreach (string noiseWord in configurationSection.NoiseWords)
                {
                    if (string.IsNullOrWhiteSpace(noiseWord)) continue;

                    string normalized = Normalize(noiseWord.Trim()).ToString();

                    if (!string.IsNullOrWhiteSpace(normalized))
                        _noiseWords.Add(normalized);
                }
            }
        }

        public string[] BreakText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return EmptyArray;

            StringBuilder normalized = Normalize(text);

            WellKnownSubstitutions(normalized);

            normalized.Replace(" and ", " & ");
            
            NormalizeLetterAndLetter(normalized);

            NormalizeInitialsApostropheS(normalized);

            NormalizeWordApostropheS(normalized);
            
            NormalizeDroppedGs(normalized);

            NormalizeConsecutiveInitials(normalized);

            normalized.Replace('\'', ' ');  // Remove any remaining apostrophies

            HashSet<string> words = new HashSet<string>(normalized.ToString().Split(SplitChar, StringSplitOptions.RemoveEmptyEntries));

            RemoveSingleLetters(words);

            RemoveNoiseWords(words);

            return words.ToArray();
        }

        public void AddSubstitution(string find, string replaceWith)
        {
            if (find == null)
                throw new ArgumentNullException(nameof(find));

            if (replaceWith == null)
                throw new ArgumentNullException(nameof(replaceWith));

            if (string.IsNullOrWhiteSpace(find))
                throw new ArgumentException("find can't be empty or whitespace", nameof(find));

            if (string.IsNullOrWhiteSpace(replaceWith))
                throw new ArgumentException("replaceWith can't be empty or whitespace", nameof(replaceWith));

            find = NormalizeAndPad(find);

            replaceWith = NormalizeAndPad(replaceWith);

            _substitutions.Add(new TextSubstitution(find, replaceWith));
        }

        private static string NormalizeAndPad(string input)
        {
            StringBuilder result = Normalize(input.Trim());

            result.Insert(0, SplitChar);

            result.Append(SplitChar);

            return result.ToString();
        }

        private void WellKnownSubstitutions(StringBuilder sb)
        {
            foreach (TextSubstitution substitution in _substitutions)
                sb.Replace(substitution.Find, substitution.ReplaceWith);
        }

        /// <summary>
        /// Looks specifically for the pattern of: space letter space ampersand space letter space
        /// and removes the spaces around the ampersand.
        /// </summary>
        //  e.g. a & w becomes a&w, T & T becomes T&T
        private static void NormalizeLetterAndLetter(StringBuilder sb)
        {
            for (int i = 3; i < sb.Length - 3; i++)
            {
                if (sb[i] == '&' && 
                    sb[i - 3] == ' ' && char.IsLetterOrDigit(sb[i - 2]) && sb[i - 1] == ' ' &&
                    sb[i + 1] == ' ' && char.IsLetterOrDigit(sb[i + 2]) && sb[i + 3] == ' ')
                {
                    sb.Remove(i + 1, 1);
                    sb.Remove(i - 1, 1);
                }
            }
        }

        /// <summary>
        /// Handles the case of two initials followed by apostrophe s
        /// e.g. "a j's" -> "ajs"
        /// </summary>
        private static void NormalizeInitialsApostropheS(StringBuilder sb)
        {
            for (int i = 4; i < sb.Length - 3; i++)
            {
                if (sb[i] == '\'' && sb[i + 1] == 's' && sb[i + 2] == ' ' &&
                    sb[i - 4] == ' ' && char.IsLetter(sb[i - 3]) && 
                    sb[i - 2] == ' ' && char.IsLetter(sb[i - 1]))
                {
                    sb.Remove(i, 1);
                    sb.Remove(i - 2, 1);
                }
            }
        }

        /// <summary>
        /// Handles the case of a followed by apostrophe s  
        /// e.g. "bob's" -> "bobs"
        /// </summary>
        private static void NormalizeWordApostropheS(StringBuilder sb)
        {
            sb.Replace("'s ", "s ");
        }

        /// <summary>
        /// Handles the case of a dropped g in a word ending in -ing
        /// e.g. "keepin'" -> "keeping"
        /// </summary>
        private static void NormalizeDroppedGs(StringBuilder sb)
        {
            for (int i = 2; i < sb.Length - 4; i++)
            {
                if (sb[i] == 'i' && sb[i + 1] == 'n' && sb[i + 2] == '\'' && sb[i + 3] == ' ' &&
                    char.IsLetter(sb[i - 1]))
                {
                    sb[i + 2] = 'g';
                }
            }
        }


        /// <summary>
        /// Removes the spaces around strings of single letters
        /// </summary>
        /// e.g. a b c consulting becomes abc consulting
        private static void NormalizeConsecutiveInitials(StringBuilder sb)
        {
            for (int i = 1; i < sb.Length - 2; i++)
            {
                if (sb[i - 1] == ' ' && char.IsLetter(sb[i]) && sb[i + 1] == ' ')
                {
                    while (i < sb.Length - 4 && char.IsLetterOrDigit(sb[i + 2]) && sb[i + 3] == ' ')
                    {
                        sb.Remove(i + 1, 1);
                        i++;
                    }
                }
            }
        }

        private static void RemoveSingleLetters(HashSet<string> words)
        {
            // Single letters are just noise
            words.RemoveWhere(s => s.Length == 1 && !char.IsDigit(s[0])); 
        }

        private void RemoveNoiseWords(ICollection<string> words)
        {
            foreach (string word in _noiseWords)
                words.Remove(word);
        }

        private static StringBuilder Normalize(string input)
        {
            StringBuilder output = new StringBuilder();

            bool lastCharWasWhitespace = true;
            output.Append(SplitChar[0]);

            foreach (char ch in input.Normalize(NormalizationForm.FormD))
            {
                switch (CharUnicodeInfo.GetUnicodeCategory(ch))
                {
                    case UnicodeCategory.OtherPunctuation:
                        if (ch == '\'' || ch == '&')
                        {
                            output.Append(ch);
                            lastCharWasWhitespace = false;
                            break;
                        }
                        goto case UnicodeCategory.SpaceSeparator;

                    case UnicodeCategory.SpaceSeparator:
                    case UnicodeCategory.Control:
                    case UnicodeCategory.OpenPunctuation:
                    case UnicodeCategory.ClosePunctuation:
                    case UnicodeCategory.DashPunctuation:
                    case UnicodeCategory.CurrencySymbol:
                    case UnicodeCategory.EnclosingMark:
                    case UnicodeCategory.FinalQuotePunctuation:
                    case UnicodeCategory.InitialQuotePunctuation:
                    case UnicodeCategory.Format:
                    case UnicodeCategory.LineSeparator:
                    case UnicodeCategory.MathSymbol:
                    case UnicodeCategory.OtherSymbol:
                    case UnicodeCategory.ParagraphSeparator:
                        if (!lastCharWasWhitespace)
                        {
                            output.Append(SplitChar[0]);
                            lastCharWasWhitespace = true;
                        }
                        break;

                    case UnicodeCategory.LetterNumber:
                    case UnicodeCategory.LowercaseLetter:
                    case UnicodeCategory.DecimalDigitNumber:
                    case UnicodeCategory.OtherLetter:
                    case UnicodeCategory.OtherNumber:
                        output.Append(ch);
                        lastCharWasWhitespace = false;
                        break;

                    case UnicodeCategory.TitlecaseLetter:
                    case UnicodeCategory.UppercaseLetter:
                        output.Append(char.ToLowerInvariant(ch));
                        lastCharWasWhitespace = false;
                        break;
                }
            }

            if (!lastCharWasWhitespace)
                output.Append(SplitChar[0]);

            return output;
        }
    }
}

