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
            _values[Key(section, key)] = value ?? string.Empty;
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

        public void Save()
        {
            string folder = Path.GetDirectoryName(FilePath);
            if (!string.IsNullOrEmpty(folder))
            {
                Directory.CreateDirectory(folder);
            }

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

            File.WriteAllText(FilePath, text.ToString(), Encoding.UTF8);
        }
    }
}
