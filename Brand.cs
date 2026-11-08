using System;
using System.Text;

namespace Laragon
{
    /// <summary>
    /// The product's own name, in one place.
    ///
    /// The owner asked for the window, the tray icon and every caption to read
    /// "XLaragon" instead of "Laragon". Only what the person reads changes: the folder the
    /// program sits in, <c>usr\laragon.ini</c>, <c>usr\laragon.log</c> and the program file
    /// <c>laragon.exe</c> keep their names, because the ini and the log are read and written
    /// under those names and renaming them would break the program that is already installed.
    /// That is also why the replacement is case-sensitive: a path shows the folder as
    /// "laragon" in lower case and must stay untouched.
    /// </summary>
    internal static class Brand
    {
        /// <summary>What the person sees wherever the original says "Laragon".</summary>
        internal const string Name = "XLaragon";

        /// <summary>The word being replaced, exactly as the original writes it.</summary>
        private const string From = "Laragon";

        /// <summary>
        /// Puts <see cref="Name"/> in place of every stand-alone "Laragon". Calling it twice
        /// gives the same answer: "XLaragon" already holds "Laragon" from its second letter,
        /// and that second one is left alone (checked by the character before it).
        /// </summary>
        internal static string Fix(string text)
        {
            if (string.IsNullOrEmpty(text) || text.IndexOf(From, StringComparison.Ordinal) < 0)
            {
                return text;
            }

            var built = new StringBuilder(text.Length + 8);
            int at = 0;
            while (at < text.Length)
            {
                bool hit = at + From.Length <= text.Length
                    && string.CompareOrdinal(text, at, From, 0, From.Length) == 0;

                if (!hit)
                {
                    built.Append(text[at]);
                    at++;
                    continue;
                }

                // a name already branded keeps its X and does not get a second one
                built.Append(at > 0 && text[at - 1] == 'X' ? From : Name);
                at += From.Length;
            }

            return built.ToString();
        }
    }
}
