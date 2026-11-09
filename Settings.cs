using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace Laragon
{
    /// <summary>
    /// The settings file, kept as an INI in usr\laragon.ini exactly like the original
    /// ("Setting file: laragon.ini").
    /// </summary>
    public sealed class Settings
    {
        private readonly Dictionary<string, string> _values =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// The keys this run set. Only these are written back, so the rest of a file that
        /// belongs to the person (their Laragon's own ini has hundreds of lines) is kept as it
        /// is: a value this rebuild never touched is never lost.
        /// </summary>
        private readonly List<string> _dirty = new List<string>();

        public string FilePath { get; private set; }

        public Settings(string filePath)
        {
            FilePath = filePath;
        }

        private static string Key(string section, string key)
        {
            return section + "." + key;
        }

        public string Get(string section, string key, string fallback)
        {
            string value;
            if (_values.TryGetValue(Key(section, key), out value) && value != null && value.Length > 0)
            {
                return value;
            }

            return fallback;
        }

        public int GetInt(string section, string key, int fallback)
        {
            int parsed;
            string raw = Get(section, key, null);
            if (raw != null && int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out parsed))
            {
                return parsed;
            }

            return fallback;
        }

        public bool GetBool(string section, string key, bool fallback)
        {
            string raw = Get(section, key, null);
            if (raw == null)
            {
                return fallback;
            }

            raw = raw.Trim();

            // The original is a Free Pascal program, where a ticked box is written as -1
            // (the person's own laragon.ini holds RunAtStartup=-1 and AutoVirtualHosts=-1),
            // so -1 has to read as true or every option saved by the real Laragon would
            // come back unticked here.
            return raw == "1" || raw == "-1" || string.Equals(raw, "true", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(raw, "yes", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(raw, "on", StringComparison.OrdinalIgnoreCase);
        }

        public void Set(string section, string key, string value)
        {
            string id = Key(section, key);
            _values[id] = value ?? string.Empty;
            if (!_dirty.Contains(id))
            {
                _dirty.Add(id);
            }
        }

        public void Set(string section, string key, int value)
        {
            Set(section, key, value.ToString(CultureInfo.InvariantCulture));
        }

        public void Set(string section, string key, bool value)
        {
            Set(section, key, value ? "1" : "0");
        }

        public bool Exists
        {
            get { return File.Exists(FilePath); }
        }

        public void Load()
        {
            _values.Clear();
            _dirty.Clear();
            if (!File.Exists(FilePath))
            {
                return;
            }

            string section = "laragon";
            foreach (string rawLine in File.ReadAllLines(FilePath))
            {
                string line = rawLine.Trim();
                if (line.Length == 0 || line.StartsWith(";") || line.StartsWith("#"))
                {
                    continue;
                }

                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    section = line.Substring(1, line.Length - 2).Trim();
                    continue;
                }

                int equals = line.IndexOf('=');
                if (equals <= 0)
                {
                    continue;
                }

                string key = line.Substring(0, equals).Trim();
                string value = line.Substring(equals + 1).Trim();
                if (key.Length > 0)
                {
                    _values[Key(section, key)] = value;
                }
            }
        }

        /// <summary>
        /// Writes the settings back. A file that already exists is changed line by line — the
        /// keys this run set are replaced where they stand and added to their section when the
        /// file has them not at all — so every other line keeps its place. That matters because
        /// the file is the person's own Laragon ini: rewriting it whole would throw away the
        /// settings this rebuild does not know about (ports, mail, language, update flags…).
        /// Where the file has no mention of a key, it is added under its own [section].
        /// </summary>
        public void Save()
        {
            string folder = Path.GetDirectoryName(FilePath);
            if (!string.IsNullOrEmpty(folder))
            {
                Directory.CreateDirectory(folder);
            }

            if (!File.Exists(FilePath))
            {
                File.WriteAllText(FilePath, WholeFileText(), Encoding.UTF8);
                _dirty.Clear();
                return;
            }

            var lines = new List<string>(File.ReadAllLines(FilePath));
            var written = new List<string>();
            string section = "laragon";

            for (int i = 0; i < lines.Count; i++)
            {
                string line = lines[i].Trim();
                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    section = line.Substring(1, line.Length - 2).Trim();
                    continue;
                }

                if (line.Length == 0 || line.StartsWith(";") || line.StartsWith("#"))
                {
                    continue;
                }

                int equals = line.IndexOf('=');
                if (equals <= 0)
                {
                    continue;
                }

                string id = Key(section, line.Substring(0, equals).Trim());
                string value;
                if (_dirty.Contains(id) && _values.TryGetValue(id, out value))
                {
                    string name = line.Substring(0, equals).Trim();
                    lines[i] = name + "=" + value;
                    written.Add(id);
                }
            }

            // a key this file does not have yet: added at the end of its section, so it reads
            // the way a person would have typed it there
            foreach (string id in _dirty)
            {
                if (written.Contains(id))
                {
                    continue;
                }

                string value;
                if (!_values.TryGetValue(id, out value))
                {
                    continue;
                }

                int dot = id.IndexOf('.');
                string where = id.Substring(0, dot);
                string name = id.Substring(dot + 1);

                int header = -1;
                string current = "laragon";
                for (int i = 0; i < lines.Count; i++)
                {
                    string line = lines[i].Trim();
                    if (line.StartsWith("[") && line.EndsWith("]"))
                    {
                        current = line.Substring(1, line.Length - 2).Trim();
                        if (header < 0 && string.Equals(current, where, StringComparison.OrdinalIgnoreCase))
                        {
                            header = i;
                        }
                    }
                }

                if (header < 0)
                {
                    if (lines.Count > 0 && lines[lines.Count - 1].Trim().Length > 0)
                    {
                        lines.Add(string.Empty);
                    }

                    lines.Add("[" + where + "]");
                    lines.Add(name + "=" + value);
                    continue;
                }

                int end = lines.Count;
                for (int i = header + 1; i < lines.Count; i++)
                {
                    string line = lines[i].Trim();
                    if (line.StartsWith("[") && line.EndsWith("]"))
                    {
                        end = i;
                        break;
                    }
                }

                while (end > header + 1 && lines[end - 1].Trim().Length == 0)
                {
                    end--;
                }

                lines.Insert(end, name + "=" + value);
            }

            File.WriteAllText(FilePath, string.Join(Environment.NewLine, lines) + Environment.NewLine, Encoding.UTF8);
            _dirty.Clear();
        }

        /// <summary>The whole file, for a folder that has none yet.</summary>
        private string WholeFileText()
        {
            var sections = new List<string>();
            var lines = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            foreach (var pair in _values)
            {
                int dot = pair.Key.IndexOf('.');
                string section = pair.Key.Substring(0, dot);
                string key = pair.Key.Substring(dot + 1);
                if (!lines.ContainsKey(section))
                {
                    sections.Add(section);
                    lines[section] = new List<string>();
                }

                lines[section].Add(key + "=" + pair.Value);
            }

            sections.Sort(StringComparer.OrdinalIgnoreCase);
            var text = new StringBuilder();
            text.AppendLine("; Laragon settings");
            text.AppendLine("; Written by the Laragon rebuild. Keep it in usr\\laragon.ini.");
            foreach (string section in sections)
            {
                text.AppendLine();
                text.AppendLine("[" + section + "]");
                foreach (string line in lines[section])
                {
                    text.AppendLine(line);
                }
            }

            return text.ToString();
        }
    }
}
