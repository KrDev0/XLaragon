using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Laragon
{
    /// <summary>
    /// Rewrites the web servers' own configuration files after the document root changes, the
    /// way the original does it.
    ///
    /// The chain in laragon.exe:
    ///   FUN_100083cf0 (0x100083cf0), the switch itself, sets the new root on the Apache record
    ///     and then calls FUN_10007a0d0 (0x10007a0d0) to write it into the files;
    ///   FUN_10007a0d0 reads the value each file holds now — FUN_10007a280 (0x10007a280) for
    ///     httpd.conf's "DocumentRoot" line, FUN_10007a9c0 (0x10007a9c0) for nginx.conf's
    ///     "root " line — and hands the old text and the new folder to
    ///   FUN_100099700 (0x100099700), which does nothing when the two are equal, and otherwise
    ///     loads the file, replaces every occurrence of the old text with the new one and saves
    ///     it again.
    /// So the quotes and the rest of the line stay as they are; only the folder inside them
    /// changes. Nothing here starts or stops anything: it only edits text files, and only when
    /// the person picks a new root from the menu.
    /// </summary>
    internal static class ConfigWriter
    {
        /// <summary>The directive whose argument holds the folder: "DocumentRoot" in httpd.conf.</summary>
        internal const string ApacheDirective = "DocumentRoot";

        /// <summary>The nginx line that holds the folder ("root C:/laragon/www;").</summary>
        internal const string NginxDirective = "root";

        /// <summary>
        /// Rewrites every web server's configuration for the new root. Returns the files that
        /// were actually changed, so the caller can tell the person which ones they were.
        /// </summary>
        internal static IList<string> Rewrite(Stack stack, string newRoot)
        {
            var touched = new List<string>();

            if (stack == null || string.IsNullOrEmpty(newRoot) || !Directory.Exists(newRoot))
            {
                return touched;
            }

            Component apache = stack.Find("apache");
            if (apache != null && apache.Installed)
            {
                Replace(
                    Path.Combine(apache.Folder, "conf", "httpd.conf"),
                    ApacheDirective,
                    newRoot,
                    touched);
            }

            Component nginx = stack.Find("nginx");
            if (nginx != null && nginx.Installed)
            {
                Replace(
                    Path.Combine(nginx.Folder, "conf", "nginx.conf"),
                    NginxDirective,
                    newRoot,
                    touched);
            }

            return touched;
        }

        /// <summary>
        /// FUN_100099700 (0x100099700): read the value the file holds, and when it differs from
        /// the new folder, replace that text everywhere in the file and write it back.
        /// </summary>
        internal static bool Replace(string file, string directive, string newRoot, IList<string> touched)
        {
            if (string.IsNullOrEmpty(file) || !File.Exists(file))
            {
                return false;
            }

            string text;
            try
            {
                text = File.ReadAllText(file);
            }
            catch (IOException)
            {
                return false;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }

            string current = CurrentValue(text, directive);
            if (current.Length == 0)
            {
                return false;
            }

            string replacement = SameSlashesAs(current, newRoot);
            if (string.Equals(current, replacement, StringComparison.Ordinal))
            {
                return false;
            }

            if (text.IndexOf(current, StringComparison.Ordinal) < 0)
            {
                return false;
            }

            // every occurrence, like the original's StringReplace: the DocumentRoot line, the
            // <Directory "..."> line and any other mention of the old folder move together
            string changed = text.Replace(current, replacement);
            if (string.Equals(changed, text, StringComparison.Ordinal))
            {
                return false;
            }

            try
            {
                File.WriteAllText(file, changed, new UTF8Encoding(false));
            }
            catch (IOException)
            {
                return false;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }

            if (touched != null)
            {
                touched.Add(file);
            }

            return true;
        }

        /// <summary>
        /// The folder the file holds now, read the way the original reads it: the first line that
        /// begins with the directive, with the directive's name, the surrounding quotes and (for
        /// nginx) a trailing semicolon taken off. Comments and blank lines are skipped because
        /// they do not begin with the directive.
        /// </summary>
        internal static string CurrentValue(string text, string directive)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(directive))
            {
                return string.Empty;
            }

            string[] lines = text.Replace("\r\n", "\n").Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (line.Length <= directive.Length)
                {
                    continue;
                }

                if (!line.StartsWith(directive, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                // the name has to end at a space or a tab: "DocumentRootX" is a different word
                char after = line[directive.Length];
                if (after != ' ' && after != '\t')
                {
                    continue;
                }

                string value = line.Substring(directive.Length).Trim();
                value = value.Trim('"', '\'').Trim();
                value = value.TrimEnd(';').Trim();
                value = value.Trim('"', '\'').Trim();
                if (value.Length > 0)
                {
                    return value;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// nginx wants forward slashes and Apache on Windows takes either, so the new folder is
        /// written in whichever style the file already uses.
        /// </summary>
        internal static string SameSlashesAs(string like, string folder)
        {
            if (string.IsNullOrEmpty(folder))
            {
                return string.Empty;
            }

            if (!string.IsNullOrEmpty(like) && like.IndexOf('/') >= 0 && like.IndexOf('\\') < 0)
            {
                return folder.Replace('\\', '/');
            }

            return folder;
        }
    }
}
