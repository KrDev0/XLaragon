using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Laragon
{
    /// <summary>
    /// What the web server is told about PHP, written out again when the PHP version changes.
    ///
    /// The original does this in FUN_1000572a0 (0x1000572a0), which FUN_10004f3b0 (0x10004f3b0)
    /// runs every time Apache is started, just before it launches httpd. FUN_1000572a0 asks
    /// FUN_1000901b0 (0x1000901b0) which of the two shapes to use, and that answers with two
    /// tests: the folder's name must NOT contain "-nts-" (a non-thread-safe build cannot be an
    /// Apache module at all) and FUN_100090010 (0x100090010) must find a DLL inside it whose name
    /// holds "apache", "php" and ".dll" — php8apache2_4.dll and friends. When both hold, the
    /// server is pointed at etc\apache2\mod_php.conf, written by FUN_100075300 (0x100075300);
    /// otherwise at fcgid.conf, patched by FUN_100074e00 (0x100074e00) and FUN_100075a30
    /// (0x100075a30), which carry the same folder in FcgidInitialEnv PATH, FcgidInitialEnv PHPRC
    /// and FcgidWrapper. FUN_100075680
    /// (0x100075680) is the sibling that writes the mod_php form for the other web servers, and
    /// FUN_100091850 / FUN_1000917e0 are the two paths it writes to. FUN_100099700 (0x100099700)
    /// is the "replace this text with that" helper it uses on the server's own config.
    ///
    /// FUN_100075300 builds the file line by line: the "auto-generated" header, a LoadFile for
    /// each of libcrypto-3-x64.dll and libssl-3-x64.dll (and their 1.1 twins when they are
    /// there), LoadModule with the module name the PHP major version asks for — php_module from
    /// PHP 8 on, php&lt;major&gt;_module before that — then PHPIniDir and the .php handler.
    ///
    /// Why it matters beyond tidiness: every one of those lines names ONE folder, and the file
    /// is what puts that folder in front of Windows when PHP's DLLs load. Leaving the file on the
    /// old folder after a switch is how one process ends up with the new version's
    /// ext\php_curl.dll and the old version's libssh2.dll — and that pairing is refused with
    /// "the procedure entry point libssh2_crypto_engine could not be located".
    ///
    /// Two rules learned the hard way, both of them the reason a switch used to look like it did
    /// nothing:
    ///
    /// 1. What gets corrected is EVERY folder a config names, not just the version the settings
    ///    happened to record before the click. A config left on 8.4.20 while the settings said
    ///    7.4.33 mentions neither "php-version" nor 7.4.33, so a rewrite keyed on the old version
    ///    found nothing to replace and silently changed nothing.
    /// 2. The Perl-style module file name changes with the major version — php7apache2_4.dll is
    ///    not php8apache2_4.dll — so replacing only the folder leaves LoadModule pointing at a
    ///    file that isn't there, and Apache then refuses to start at all.
    /// </summary>
    internal static class PhpSettings
    {
        /// <summary>
        /// Any "…\bin\php\php-&lt;something&gt;" folder, in either slash style, so a config naming
        /// whichever version can be corrected whichever version it names.
        /// </summary>
        private static readonly Regex PhpFolderPath = new Regex(
            @"[A-Za-z]:[\\/][^""\r\n;]*?[\\/]bin[\\/]php[\\/]php-[^\\/\s"";]+",
            RegexOptions.IgnoreCase);

        /// <summary>php8apache2_4.dll, php7apache2_4.dll, php5apache2_4.dll — one per major.</summary>
        private static readonly Regex ApacheModuleFile = new Regex(
            @"php\d+apache2_4\.dll", RegexOptions.IgnoreCase);

        /// <summary>
        /// Writes the web server's PHP settings for <paramref name="phpFolder"/>, so that every
        /// file the web server reads names that folder and no other. Answers the files it
        /// changed, so the caller can say so; an empty list means everything already agreed.
        /// </summary>
        /// <param name="oldFolder">
        /// The folder the settings recorded before the switch, when there was one. It is only a
        /// fallback: a config holding that folder in a shape the pattern cannot see (a relative
        /// path, no drive letter) still gets corrected. Null is allowed and is what the repair
        /// pass uses, because there is no "before" in that case.
        /// </param>
        public static IList<string> Apply(string etcFolder, string phpFolder, string oldFolder = null)
        {
            var touched = new List<string>();
            if (string.IsNullOrEmpty(etcFolder) || string.IsNullOrEmpty(phpFolder) || !Directory.Exists(phpFolder))
            {
                return touched;
            }

            string version = Path.GetFileName(phpFolder.TrimEnd('\\', '/'));
            string apacheEtc = Path.Combine(etcFolder, "apache2");
            string modPhp = Path.Combine(apacheEtc, "mod_php.conf");

            // mod_php.conf has to exist and it has to name the PHP that is in use. It is written
            // from scratch every time — Apache, not us, decides which of its lines are legal, and
            // a line left over from an older major version (a module file name, a libcrypto that
            // is not there) stops the server from starting at all. When the chosen PHP cannot be
            // an Apache module at all (FUN_1000901b0: "-nts-" in the name, or no
            // php*apache2_4.dll inside it) the file is emptied of loadable lines rather than
            // pointed at a module from another folder.
            bool fcgid = WantsFcgid(phpFolder);
            if (Directory.Exists(apacheEtc))
            {
                string text = fcgid ? null : ModPhpText(phpFolder);
                if (text == null)
                {
                    // This build has no Apache module. Writing nothing would leave the previous
                    // folder's module in place — the exact mix this class exists to prevent — so
                    // the file is emptied of loadable lines instead and says why.
                    text = ModPhpStubText(version, phpFolder, fcgid);
                }

                if (!File.Exists(modPhp) || Read(modPhp) != text)
                {
                    if (TryWrite(modPhp, text))
                    {
                        touched.Add(modPhp);
                    }
                }
            }

            // The Apache build's own httpd.conf is where all of the above is picked up from, so it
            // has to name the file that is in use. FUN_100080ff0 writes both the include and the
            // small block of Windows-friendly directives around it, before every Apache start.
            string httpd = EnsureApacheInclude(etcFolder, fcgid);
            if (httpd != null)
            {
                touched.Add(httpd);
            }

            // the FCGI file keeps the folder behind a literal "php-version", and the nginx
            // configs name it too; both are filled in rather than rebuilt, so whatever else the
            // person changed in them stays
            foreach (string file in ConfigFiles(etcFolder))
            {
                if (string.Equals(file, modPhp, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (RewriteFolder(file, version, phpFolder, oldFolder))
                {
                    touched.Add(file);
                }
            }

            // The chosen PHP's own php.ini. When there is none, FUN_1000800d0 (0x1000800d0) makes
            // one before Apache starts: it copies usr\tpl\php.ini.tpl when that exists, and
            // otherwise turns the PHP build's own php.ini-development into a working one (the
            // extension list, error_log, include_path, extension_dir, session.save_path and the
            // raised limits below). When there is one, only absolute paths are corrected:
            // extension_dir, zend_extension, browscap and the like that still name another
            // version's folder load that version's extension DLLs into this one — the other half
            // of the same entry-point error.
            string ini = Path.Combine(phpFolder, "php.ini");
            if (EnsurePhpIni(phpFolder, etcFolder))
            {
                touched.Add(ini);
            }

            if (File.Exists(ini) && RewriteFolder(ini, version, phpFolder, oldFolder))
            {
                touched.Add(ini);
            }

            // The chosen build's own file too: a relative "ext" (which PHP then resolves against
            // whatever folder the program happens to start in) or a folder the pattern above
            // cannot see never gets corrected by path rewriting alone.
            if (ForceExtensionDir(phpFolder) && !touched.Contains(ini, StringComparer.OrdinalIgnoreCase))
            {
                touched.Add(ini);
            }

            // And the same for every other PHP build's php.ini, not only the one being switched
            // to. PHP reads the ini that is named by PHPIniDir, PHPRC or its own folder, and when
            // that turns out to be another version's file its extension_dir sends PHP into that
            // version's ext directory: php_curl.dll then imports libssh2.dll/nghttp2.dll by bare
            // name, Windows finds the other build's copy, and every extension that shares a
            // library name with the newer PHP reports "the procedure entry point ... could not be
            // located". Correcting every folder makes that mix impossible whichever file is read.
            foreach (string other in PhpFolders(etcFolder))
            {
                if (string.Equals(other.TrimEnd('\\', '/'), phpFolder.TrimEnd('\\', '/'), StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (ForceExtensionDir(other))
                {
                    touched.Add(Path.Combine(other, "php.ini"));
                }
            }

            return touched;
        }

        /// <summary>
        /// Makes the files agree with the PHP version the settings name, without a click: the
        /// repair half of <see cref="Apply"/>. Answers the files it changed.
        /// </summary>
        public static IList<string> Sync(Stack stack)
        {
            var touched = new List<string>();
            if (stack == null)
            {
                return touched;
            }

            Component php = stack.Find("php");
            if (php == null || string.IsNullOrEmpty(php.Folder) || !Directory.Exists(php.Folder))
            {
                return touched;
            }

            foreach (string file in Apply(stack.Etc, php.Folder, null))
            {
                touched.Add(file);
            }

            return touched;
        }

        /// <summary>
        /// A PATH that holds exactly one PHP folder — <paramref name="phpFolder"/>, first — with
        /// every other PHP folder taken out.
        ///
        /// This is the half of the "entry point libssh2_crypto_engine could not be located in
        /// …\ext\php_curl.dll" error that no config file can fix. Windows resolves a DLL's own
        /// dependencies from the folder of the program that loaded it and then from PATH, never
        /// from the folder of the DLL itself. So an httpd whose PATH still lists php-8.4.20 while
        /// Apache is told to use php-7.4.33 loads 8.4.20's libssh2.dll next to 7.4.33's
        /// php_curl.dll, and the two do not fit together.
        /// </summary>
        public static string IsolatePath(string phpFolder, string path)
        {
            var parts = new List<string>();
            if (!string.IsNullOrEmpty(phpFolder))
            {
                parts.Add(phpFolder);
            }

            string chosen = (phpFolder ?? string.Empty).Replace('\\', '/').TrimEnd('/');
            foreach (string raw in (path ?? string.Empty).Split(';'))
            {
                string part = raw.Trim();
                if (part.Length == 0)
                {
                    continue;
                }

                string posix = part.Replace('\\', '/').TrimEnd('/');
                string low = posix.ToLowerInvariant();
                if (low.EndsWith("/bin/php", StringComparison.Ordinal) ||
                    low.IndexOf("/bin/php/", StringComparison.Ordinal) >= 0)
                {
                    // any PHP folder at all, ours included: it goes back in at the front
                    continue;
                }

                if (chosen.Length > 0 && string.Equals(posix, chosen, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                parts.Add(part);
            }

            return string.Join(";", parts);
        }

        /// <summary>
        /// FUN_1000901b0 (0x1000901b0): is this PHP run through fcgid rather than loaded into
        /// Apache? Two tests, both of them the original's own: the folder's name must not hold
        /// "-nts-" (a non-thread-safe build cannot be an Apache module), and FUN_100090010
        /// (0x100090010) must find a php*apache2_4.dll inside the folder — it lists the folder's
        /// *.dll files and takes the first whose name holds "apache", "php" and ".dll".
        /// </summary>
        private static bool WantsFcgid(string phpFolder)
        {
            string version = Path.GetFileName(phpFolder.TrimEnd('\\', '/'));
            if (version.IndexOf("-nts-", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            try
            {
                foreach (string dll in Directory.GetFiles(phpFolder, "*.dll"))
                {
                    string name = Path.GetFileName(dll).ToLowerInvariant();
                    if (name.Contains("apache") && name.Contains("php") && name.Contains(".dll"))
                    {
                        return false;
                    }
                }
            }
            catch (Exception)
            {
                return true;
            }

            return true;
        }

        /// <summary>
        /// The Apache build's own httpd.conf. The original addresses it as the apache service's
        /// configuration file (FUN_1000ab020) — that is &lt;apache build&gt;\conf\httpd.conf, and
        /// "the apache build" is whichever folder is installed under bin\apache.
        /// </summary>
        private static string HttpdConf(string etcFolder)
        {
            DirectoryInfo root = Directory.GetParent(etcFolder.TrimEnd('\\', '/'));
            if (root == null)
            {
                return null;
            }

            string builds = Path.Combine(Path.Combine(root.FullName, "bin"), "apache");
            if (!Directory.Exists(builds))
            {
                return null;
            }

            try
            {
                foreach (string build in Directory.GetDirectories(builds))
                {
                    string conf = Path.Combine(Path.Combine(build, "conf"), "httpd.conf");
                    if (File.Exists(conf))
                    {
                        return conf;
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }

            return null;
        }

        /// <summary>
        /// Makes the Apache build's own httpd.conf load the PHP settings at all.
        ///
        /// FUN_100080ff0 (0x100080ff0) does this before every Apache start. When httpd.conf has no
        /// "sites-enabled" line yet, it applies the original's own edits (FollowSymLinks gains
        /// "Includes ExecCGI", DirectoryIndex gains index.php, "ServerName Laragon" replaces the
        /// example, LogLevel warn becomes error, c:/Apache24 becomes the real build folder) and
        /// appends a block ending in Include "&lt;etc&gt;/apache2/mod_php.conf" — or fcgid.conf
        /// when that is the shape in use. Without that last line nothing else here is ever read.
        /// When the line is already there, only the file name may have to follow the PHP build.
        /// </summary>
        private static string EnsureApacheInclude(string etcFolder, bool fcgid)
        {
            string conf = HttpdConf(etcFolder);
            if (conf == null)
            {
                return null;
            }

            string text;
            try
            {
                text = Read(conf);
            }
            catch (Exception)
            {
                return null;
            }

            string posixEtc = etcFolder.Replace('\\', '/').TrimEnd('/');
            string include = "Include \"" + posixEtc + "/apache2/" + (fcgid ? "fcgid.conf" : "mod_php.conf") + "\"";
            string changed;

            if (text.IndexOf("mod_php.conf", StringComparison.OrdinalIgnoreCase) >= 0 ||
                text.IndexOf("fcgid.conf", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                changed = Regex.Replace(
                    text,
                    "Include\\s+\"[^\"]*etc/apache2/(?:mod_php|fcgid)\\.conf\"",
                    include.Replace("$", "$$"),
                    RegexOptions.IgnoreCase);
            }
            else
            {
                changed = text
                    .Replace("Options Indexes FollowSymLinks", "Options Indexes FollowSymLinks Includes ExecCGI")
                    .Replace("DirectoryIndex index.html", "DirectoryIndex index.html index.php")
                    .Replace("#ServerName www.example.com:80", "ServerName Laragon")
                    .Replace("LogLevel warn", "LogLevel error");

                DirectoryInfo confFolder = Directory.GetParent(conf);
                DirectoryInfo build = confFolder == null ? null : confFolder.Parent;
                if (build != null)
                {
                    string posixBuild = build.FullName.Replace('\\', '/');
                    changed = Regex.Replace(changed, "c:/(?:Apache24|Apache22)", posixBuild.Replace("$", "$$"), RegexOptions.IgnoreCase);
                }

                changed = changed.TrimEnd() + "\r\n" +
                    "\r\nAcceptFilter http none\r\nAcceptFilter https none\r\n" +
                    "EnableSendfile Off\r\nEnableMMAP Off\r\n\r\n" +
                    "<IfModule mpm_winnt_module>\r\n    ThreadStackSize 8388608\r\n</IfModule>\r\n" +
                    "IncludeOptional \"" + posixEtc + "/apache2/alias/*.conf\"\r\n" +
                    "IncludeOptional \"" + posixEtc + "/apache2/sites-enabled/*.conf\"\r\n" +
                    "Include \"" + posixEtc + "/apache2/httpd-ssl.conf\"\r\n" +
                    include + "\r\n";
            }

            return changed != text && TryWrite(conf, changed) ? conf : null;
        }

        /// <summary>
        /// The fourteen extensions FUN_1000800d0 (0x1000800d0) enables, in its own order, read out
        /// of the table at 0x1003418e8. Each one is switched on only when the DLL is really in the
        /// folder's ext\ — the original tests the file before it writes the line.
        /// </summary>
        private static readonly string[] PhpExtensions =
        {
            "php_curl.dll", "php_gd2.dll", "php_intl.dll", "php_mbstring.dll", "php_mysqli.dll",
            "php_pdo_mysql.dll", "php_openssl.dll", "php_xsl.dll", "php_fileinfo.dll", "php_gd.dll",
            "php_exif.dll", "php_pdo_sqlite.dll", "php_sodium.dll", "php_sqlite3.dll"
        };

        /// <summary>
        /// Creates the chosen PHP's php.ini when it has none, the way FUN_1000800d0 does it: copy
        /// usr\tpl\php.ini.tpl if the install ships that template, otherwise turn the build's own
        /// php.ini-development into a working file. The settings below are the original's own
        /// values, not guesses: session.gc_maxlifetime and max_execution_time 36000,
        /// memory_limit 512M, upload_max_filesize and post_max_size 2G, an absolute error_log,
        /// include_path with etc/php/pear, extension_dir as the folder's ext, and the extension
        /// lines above. An existing php.ini is left alone, as the original leaves it alone.
        /// </summary>
        private static bool EnsurePhpIni(string phpFolder, string etcFolder)
        {
            string ini = Path.Combine(phpFolder, "php.ini");
            if (File.Exists(ini))
            {
                return false;
            }

            DirectoryInfo root = Directory.GetParent(etcFolder.TrimEnd('\\', '/'));
            string rootPath = root == null ? null : root.FullName;
            if (rootPath != null)
            {
                string tpl = Path.Combine(Path.Combine(Path.Combine(rootPath, "usr"), "tpl"), "php.ini.tpl");
                if (File.Exists(tpl))
                {
                    try
                    {
                        File.Copy(tpl, ini, false);
                        return true;
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }
            }

            string development = Path.Combine(phpFolder, "php.ini-development");
            if (!File.Exists(development))
            {
                return false;
            }

            string text;
            try
            {
                text = Read(development);
            }
            catch (Exception)
            {
                return false;
            }

            string posix = phpFolder.Replace('\\', '/').TrimEnd('/');
            string posixEtc = etcFolder.Replace('\\', '/').TrimEnd('/');

            // FUN_10009d2f0 (0x10009d2f0) builds <install>\tmp\ and appends the file name, and the
            // real install keeps tmp\php_errors.log (422 KB when I looked) plus its sess_* files
            // there, so this line and session.save_path below both point at tmp\.
            text = text.Replace(";error_log = php_errors.log", "error_log = \"" + (rootPath == null ? posix : rootPath.Replace('\\', '/')) + "/tmp/php_errors.log\"");
            text = text.Replace(";include_path = \".;c:\\php\\includes\"", "include_path = \".;" + posixEtc + "/php/pear\"");
            text = text.Replace(";session.save_path = \"/tmp\"", "session.save_path = \"" + (rootPath == null ? "C:/Windows/Temp" : rootPath.Replace('\\', '/')) + "/tmp\"");
            text = text.Replace("session.gc_maxlifetime = 1440", "session.gc_maxlifetime = 36000");
            text = text.Replace("max_execution_time = 30", "max_execution_time = 36000");
            text = text.Replace("memory_limit = 128M", "memory_limit = 512M");
            text = text.Replace("upload_max_filesize = 2M", "upload_max_filesize = 2G");
            text = text.Replace("post_max_size = 8M", "post_max_size = 2G");
            text = text.Replace(";always_populate_raw_post_data = -1", "always_populate_raw_post_data = -1");

            // The template ships extension_dir commented out, and a PHP with no extension_dir
            // resolves "ext" against whatever folder the program was started in. Write the
            // folder's own ext directory, the shape FUN_100074110 uses, so the extension lines
            // below have somewhere real to load from.
            text = Regex.Replace(text, "(?im)^;?[ \t]*extension_dir[ \t]*=.*$", m => "extension_dir =  \"" + posix + "/ext\"");
            if (text.IndexOf("extension_dir", StringComparison.OrdinalIgnoreCase) < 0)
            {
                text = "extension_dir =  \"" + posix + "/ext\"" + "\r\n" + text;
            }

            var lines = new List<string>();
            foreach (string line in text.Replace("\r\n", "\n").Split('\n'))
            {
                lines.Add(line);
                if (line.TrimStart().StartsWith("extension_dir", StringComparison.OrdinalIgnoreCase))
                {
                    lines.AddRange(ExtensionLines(phpFolder));
                }
            }

            if (!text.Contains("extension_dir"))
            {
                lines.AddRange(ExtensionLines(phpFolder));
            }

            if (!File.Exists(ini))
            {
                return TryWrite(ini, string.Join("\r\n", lines));
            }

            return false;
        }

        /// <summary>
        /// Every PHP build folder under &lt;root&gt;\bin\php, so a repair can reach all of them.
        /// </summary>
        private static IEnumerable<string> PhpFolders(string etcFolder)
        {
            DirectoryInfo root = Directory.GetParent(etcFolder.TrimEnd('\\', '/'));
            if (root == null)
            {
                yield break;
            }

            string bin = Path.Combine(Path.Combine(root.FullName, "bin"), "php");
            if (!Directory.Exists(bin))
            {
                yield break;
            }

            string[] folders;
            try
            {
                folders = Directory.GetDirectories(bin);
            }
            catch (Exception)
            {
                yield break;
            }

            foreach (string folder in folders)
            {
                yield return folder;
            }
        }

        /// <summary>
        /// Make one PHP build's php.ini name that build's own ext folder, absolutely, and comment
        /// out an extension= line whose DLL the build does not ship. FUN_100074110 (0x100074110)
        /// writes extension_dir =  "&lt;folder&gt;/ext" — two spaces, the folder's own — while the value
        /// still points into a PHP folder, which is the same rule applied to one build on the
        /// switch; running it over every build means no ini can send PHP into a different
        /// version's ext folder, and no ini can enable a DLL that is not there (which PHP reports
        /// as "unable to load dynamic library"). Answers whether it changed the file.
        /// </summary>
        private static bool ForceExtensionDir(string phpFolder)
        {
            string ini = Path.Combine(phpFolder, "php.ini");
            if (!File.Exists(ini))
            {
                return false;
            }

            string text;
            try
            {
                text = Read(ini);
            }
            catch (Exception)
            {
                return false;
            }

            string posix = phpFolder.Replace('\\', '/').TrimEnd('/');
            string wanted = "extension_dir =  \"" + posix + "/ext\"";
            string extFolder = Path.Combine(phpFolder, "ext");
            var lines = new List<string>();
            bool changed = false;
            foreach (string line in text.Replace("\r\n", "\n").Split('\n'))
            {
                string trimmed = line.TrimStart();
                string written = line;
                if (trimmed.StartsWith("extension_dir", StringComparison.OrdinalIgnoreCase))
                {
                    if (line.TrimEnd() != wanted)
                    {
                        written = wanted;
                        changed = true;
                    }
                }
                else if (trimmed.StartsWith("extension=", StringComparison.OrdinalIgnoreCase)
                    || trimmed.StartsWith("zend_extension=", StringComparison.OrdinalIgnoreCase))
                {
                    string name = ExtensionName(trimmed.Substring(trimmed.IndexOf('=') + 1));
                    if (name.Length > 0 && !ExtensionPresent(extFolder, name))
                    {
                        written = "; " + line.TrimEnd() + "   ; disabled by Laragon: this PHP has no " + name;
                        changed = true;
                    }
                }

                lines.Add(written);
            }

            return changed && TryWrite(ini, string.Join("\r\n", lines));
        }

        /// <summary>"php_curl.dll   ; note" → "php_curl.dll".</summary>
        private static string ExtensionName(string value)
        {
            string name = value.Trim();
            if (name.StartsWith("\""))
            {
                int close = name.IndexOf('"', 1);
                name = close > 0 ? name.Substring(1, close - 1) : name.TrimStart('"');
            }
            else
            {
                int cut = name.IndexOfAny(new[] { ' ', '\t', ';' });
                if (cut >= 0)
                {
                    name = name.Substring(0, cut);
                }
            }

            return name.Trim();
        }

        /// <summary>
        /// Whether this build's ext folder holds the DLL an extension= line names, in either the
        /// old full form (php_curl.dll) or the short one PHP 7.2 and later also accept (curl).
        /// </summary>
        private static bool ExtensionPresent(string extFolder, string name)
        {
            if (!Directory.Exists(extFolder))
            {
                return false;
            }

            if (name.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
            {
                return File.Exists(Path.Combine(extFolder, name));
            }

            return File.Exists(Path.Combine(extFolder, name + ".dll"))
                || File.Exists(Path.Combine(extFolder, "php_" + name + ".dll"));
        }

        /// <summary>One extension= line per entry of the table above, on only when its DLL exists.</summary>
        private static IEnumerable<string> ExtensionLines(string phpFolder)
        {
            var lines = new List<string>();
            foreach (string dll in PhpExtensions)
            {
                bool present = File.Exists(Path.Combine(Path.Combine(phpFolder, "ext"), dll));

                // The table at 0x1003418e8 holds DLL names only so the original can test that the
                // file is really there; the line it writes names the extension the way php.ini
                // does, extension=curl and not extension=php_curl.dll, because FUN_10008f780
                // takes php_ and .dll off the table entry first.
                string shortName = dll.StartsWith("php_", StringComparison.OrdinalIgnoreCase)
                    ? dll.Substring(4, dll.Length - 8)
                    : Path.GetFileNameWithoutExtension(dll);

                lines.Add((present ? "extension=" : ";extension=") + shortName);
            }

            return lines;
        }

        /// <summary>
        /// The DLLs the chosen PHP folder ships beside php.exe that its extensions import by
        /// bare name, most important first. FUN_100075300 preloads the two OpenSSL DLLs with
        /// LoadFile before LoadModule for exactly this reason: Windows resolves the libraries an
        /// extension imports by looking in the program's folder and then PATH, never in the
        /// extension's own folder, so php\...\ext\php_curl.dll can pick up another version's
        /// libssh2.dll and die with "the procedure entry point libssh2_crypto_engine could not
        /// be located in php_curl.dll". Loading them here by absolute path means a later lookup
        /// by name finds the right file already in the process. This only widens the original's
        /// own trick from OpenSSL to the whole folder, so nothing is guessed at run time.
        /// </summary>
        private static List<string> PreloadDlls(string phpFolder)
        {
            var order = new List<string>();
            string[] first = { "libcrypto-3-x64.dll", "libssl-3-x64.dll", "libcrypto-1_1-x64.dll", "libssl-1_1-x64.dll" };
            foreach (string name in first)
            {
                if (File.Exists(Path.Combine(phpFolder, name)))
                {
                    order.Add(name);
                }
            }

            var rest = new List<string>();
            try
            {
                foreach (string file in Directory.GetFiles(phpFolder, "*.dll"))
                {
                    string name = Path.GetFileName(file);
                    if (name.StartsWith("php", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    if (!order.Contains(name, StringComparer.OrdinalIgnoreCase))
                    {
                        rest.Add(name);
                    }
                }
            }
            catch (IOException)
            {
                return order;
            }
            catch (UnauthorizedAccessException)
            {
                return order;
            }

            rest.Sort(StringComparer.OrdinalIgnoreCase);
            order.AddRange(rest);
            return order;
        }

        /// <summary>The mod_php.conf body for one PHP folder, in FUN_100075300's own order.</summary>
        private static string ModPhpText(string phpFolder)
        {
            string version = Path.GetFileName(phpFolder.TrimEnd('\\', '/'));
            int major = MajorVersion(version);
            string module = major >= 8 ? "php_module" : "php" + major + "_module";
            string moduleFile = Path.Combine(phpFolder, "php" + major + "apache2_4.dll");
            if (!File.Exists(moduleFile))
            {
                return null;
            }

            string folder = phpFolder.Replace('\\', '/');
            var text = new StringBuilder();
            text.AppendLine("# This file is auto-generated, so please keep it intact.");
            foreach (string dll in PreloadDlls(phpFolder))
            {
                text.AppendLine("LoadFile \"" + folder + "/" + dll + "\"");
            }

            text.AppendLine("LoadModule " + module + " \"" + folder + "/" + Path.GetFileName(moduleFile) + "\"");
            text.AppendLine("PHPIniDir \"" + folder + "\"");
            text.AppendLine("<IfModule mime_module>");
            text.AppendLine("    AddType application/x-httpd-php .php");
            text.AppendLine("</IfModule>");
            return text.ToString();
        }

        /// <summary>
        /// mod_php.conf for a PHP build that ships no Apache module: the header and why, and no
        /// LoadModule/LoadFile line at all. An empty file is legal to Include and leaves Apache
        /// with no PHP rather than with the wrong one.
        /// </summary>
        private static string ModPhpStubText(string version, string phpFolder, bool fcgid)
        {
            var text = new StringBuilder();
            text.AppendLine("# This file is auto-generated, so please keep it intact.");
            text.AppendLine("# " + version + (fcgid
                ? " is run through fcgid: no PHP is loaded into Apache itself."
                : " has no php" + MajorVersion(version) + "apache2_4.dll in"));
            text.AppendLine("# " + phpFolder.Replace('\\', '/'));
            text.AppendLine("# so apache cannot be given PHP from this folder. Nothing is loaded here on");
            text.AppendLine("# purpose: loading the module of another PHP folder is what makes Apache fail");
            text.AppendLine("# with \"the procedure entry point ... could not be located\".");
            return text.ToString();
        }

        /// <summary>8 for "php-8.4.20-Win32-vs17-x64", 7 for "php-7.4.33-…", 5 for "php-5.6.40-…".</summary>
        private static int MajorVersion(string version)
        {
            int dash = version.IndexOf('-');
            string rest = dash < 0 ? version : version.Substring(dash + 1);
            int end = 0;
            while (end < rest.Length && char.IsDigit(rest[end]))
            {
                end++;
            }

            int major;
            if (end > 0 && int.TryParse(rest.Substring(0, end), out major))
            {
                return major;
            }

            return 8;
        }

        /// <summary>Every .conf under etc\apache2 and etc\nginx.</summary>
        private static IEnumerable<string> ConfigFiles(string etcFolder)
        {
            var found = new List<string>();
            foreach (string folder in new[] { "apache2", "nginx" })
            {
                string path = Path.Combine(etcFolder, folder);
                if (!Directory.Exists(path))
                {
                    continue;
                }

                string[] files;
                try
                {
                    files = Directory.GetFiles(path, "*.conf", SearchOption.AllDirectories);
                }
                catch (Exception)
                {
                    continue;
                }

                found.AddRange(files);
            }

            return found;
        }

        /// <summary>
        /// Corrects one config file to this PHP folder: every PHP folder path it names, in either
        /// slash style, the "php-version" placeholder the install's own templates carry, and the
        /// module file name when the major version changes. Answers true when it wrote something.
        /// </summary>
        private static bool RewriteFolder(string file, string version, string phpFolder, string oldFolder)
        {
            string text;
            try
            {
                text = Read(file);
            }
            catch (Exception)
            {
                return false;
            }

            string posixFolder = phpFolder.Replace('\\', '/');
            string changed = text;

            // 1. every PHP folder the file names, whichever version that is, in the file's own
            //    slash style. This is the rule that makes a drifted config self-correcting.
            changed = PhpFolderPath.Replace(changed, match =>
            {
                string found = match.Value;
                bool posix = found.IndexOf('\\') < 0;
                return posix ? posixFolder : phpFolder;
            });

            // 2. the folder the settings recorded before the click, for the shapes the pattern
            //    above cannot see: a relative path, or a bare folder name in a comment.
            if (!string.IsNullOrEmpty(oldFolder))
            {
                string oldName = Path.GetFileName(oldFolder.TrimEnd('\\', '/'));
                if (oldName.Length > 0 &&
                    !string.Equals(oldName, version, StringComparison.OrdinalIgnoreCase))
                {
                    string oldPosix = oldFolder.Replace('\\', '/');
                    changed = changed.Replace(oldPosix, posixFolder);
                    changed = changed.Replace(oldFolder, phpFolder);
                    changed = changed.Replace(oldName, version);
                }
            }

            // 3. the install's own templates keep the folder behind a literal placeholder. It
            //    goes last on purpose: filling it first turns the tail of an old folder's own
            //    name into the new version, which is how a path used to end up naming two
            //    versions at once (".../php-8.4.20-.../php-7.4.33-...").
            changed = changed.Replace("php-version", version);

            // 4. the Apache module file is named after the major version, so it has to follow.
            changed = ApacheModuleFile.Replace(changed, "php" + MajorVersion(version) + "apache2_4.dll");

            // 5. fcgid hands its own environment to the php-cgi it starts, and its PATH line is the
            //    one place a wrong PHP folder is written into a config: php-cgi.exe resolves the
            //    libraries its extensions import by bare name (libssh2.dll, nghttp2.dll,
            //    libsodium.dll, libzip.dll, libxslt.dll — the same names in every PHP build) out of
            //    the build named there, and every extension sharing one of them then reports "the
            //    procedure entry point ... could not be located". The folder being switched to goes
            //    first and the other PHP folders come out; the rest of the PATH is left alone.
            changed = Regex.Replace(
                changed,
                "(?im)^([ \\t]*FcgidInitialEnv[ \\t]+PATH[ \\t]+)(\"?)([^\"\\r\\n]*)\\2[ \\t]*$",
                m =>
                {
                    var parts = new List<string> { posixFolder.TrimEnd('/') };
                    foreach (string piece in m.Groups[3].Value.Split(';'))
                    {
                        string entry = piece.Trim();
                        if (entry.Length == 0)
                        {
                            continue;
                        }

                        if (entry.Replace('\\', '/').IndexOf("/bin/php", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            continue;
                        }

                        if (!parts.Exists(p => string.Equals(p.TrimEnd('/', '\\'), entry.TrimEnd('/', '\\'), StringComparison.OrdinalIgnoreCase)))
                        {
                            parts.Add(entry);
                        }
                    }

                    return m.Groups[1].Value + "\"" + string.Join(";", parts) + "\"";
                });

            if (changed == text)
            {
                return false;
            }

            return TryWrite(file, changed);
        }

        private static string Read(string file)
        {
            return File.ReadAllText(file, Encoding.UTF8);
        }

        private static bool TryWrite(string file, string text)
        {
            try
            {
                File.WriteAllText(file, text, new UTF8Encoding(false));
                return true;
            }
            catch (Exception)
            {
                // read-only, locked, or no rights: the caller reports nothing changed rather than
                // pretending the server now uses this version
                return false;
            }
        }
    }
}
