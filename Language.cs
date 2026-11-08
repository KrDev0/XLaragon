using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace Laragon
{
    /// <summary>
    /// The window captions in the language chosen in Preferences.
    ///
    /// CORRECTION (decompiled this round): the original does carry translations — it keeps
    /// them outside the program. FUN_100095400 (0x100095400) reads the ini key "Lang" and
    /// then tests whether <c>&lt;root&gt;\bin\laragon\lang\&lt;Lang&gt;.txt</c> exists, answering
    /// "English" when it does not; FUN_100095530 (0x100095530) reads that file line by line,
    /// ignoring lines that begin with '#', ';', '-' or '/' and keeping only lines whose left
    /// side is a number; and FUN_100095830 (0x100095830) is the getter every caption passes
    /// through: <c>T(id, "English text")</c> returns the file's text for that id, or the
    /// English text it was handed. FUN_10008a690 (0x10008a690) fills the Language drop-down
    /// with the *.txt files it finds in that folder.
    ///
    /// The install on this machine has 33 packs (Arabic.txt … Vietnamese.txt), Spanish.txt
    /// among them, so the rebuild reads the original's own packs and no longer needs to
    /// invent translations. The hand-written English -> Spanish table below stays as the
    /// fallback for when the folder holds no pack (a copy of the project with no Laragon
    /// installed), which is the one deliberate departure; MECHANICS.md §18 records it.
    /// English is never stored: the designer files keep English and the pack is applied over
    /// it, so the designer still reads in one language and the program draws in the chosen one.
    /// </summary>
    internal static class Language
    {
        internal const string English = "English";
        internal const string Spanish = "Spanish";

        /// <summary>The language whose captions are on screen now.</summary>
        internal static string Current { get; set; } = English;

        /// <summary>
        /// The folder the original keeps its packs in: &lt;root&gt;\bin\laragon\lang (the "*" it
        /// appends is 0x1003a1b10 and the ".txt" is 0x1003a5138). Set once at start-up from
        /// Stack; empty means no folder is read and the built-in table is all there is.
        /// </summary>
        internal static string Folder { get; set; } = "";

        /// <summary>id -> text of the chosen pack, per language name; null when it has no file.</summary>
        private static readonly Dictionary<string, Dictionary<ushort, string>> PackCache =
            new Dictionary<string, Dictionary<ushort, string>>();

        /// <summary>English text -> id, read from English.txt of the same folder.</summary>
        private static readonly Dictionary<string, Dictionary<string, ushort>> IdsCache =
            new Dictionary<string, Dictionary<string, ushort>>();

        /// <summary>
        /// Point the rebuild at a language and read its pack. Anything the pack does not hold
        /// falls back to the table below, and anything the table does not hold stays English.
        /// </summary>
        internal static void Load(string wanted)
        {
            Current = Resolve(wanted, Folder);
        }

        /// <summary>
        /// The original's own test (FUN_100095400 at 0x100095400): a language is usable when
        /// &lt;folder&gt;\&lt;name&gt;.txt is there. Without a file it is one of the two built-in
        /// languages, or English.
        /// </summary>
        internal static string Resolve(string wanted, string folder)
        {
            if (wanted == null)
            {
                return English;
            }

            string trimmed = wanted.Trim();
            if (trimmed.Length == 0)
            {
                return English;
            }

            if (PathOf(folder, trimmed) != null)
            {
                return trimmed;
            }

            return Name(trimmed);
        }

        /// <summary>
        /// The languages the drop-down offers: the *.txt packs in the folder, as the original
        /// lists them (FUN_10008a690 at 0x10008a690), or the two built-in ones when it has none.
        /// </summary>
        internal static string[] Available(string folder)
        {
            List<string> names = new List<string>();
            if (!string.IsNullOrEmpty(folder) && Directory.Exists(folder))
            {
                foreach (string file in Directory.GetFiles(folder, "*.txt"))
                {
                    names.Add(Path.GetFileNameWithoutExtension(file));
                }
            }

            if (names.Count == 0)
            {
                names.Add(English);
                names.Add(Spanish);
            }
            else
            {
                names.Sort(System.StringComparer.OrdinalIgnoreCase);
            }

            return names.ToArray();
        }

        /// <summary>Whether the folder holds any pack at all.</summary>
        internal static bool HasPacks(string folder)
        {
            return !string.IsNullOrEmpty(folder) && Directory.Exists(folder)
                && Directory.GetFiles(folder, "*.txt").Length > 0;
        }

        /// <summary>
        /// The original cuts a caption longer than 30 characters down to its first 27 and adds
        /// "..", so a long translated "Start All" still fits its button (FUN_100084460 at
        /// 0x100084460; the ".." is 0x10039e070).
        /// </summary>
        internal static string Elide(string text)
        {
            if (string.IsNullOrEmpty(text) || text.Length < 31)
            {
                return text;
            }

            return text.Substring(0, 27) + "..";
        }

        /// <summary>&lt;folder&gt;\&lt;name&gt;.txt when it exists, else null.</summary>
        private static string PathOf(string folder, string name)
        {
            if (string.IsNullOrEmpty(folder) || string.IsNullOrEmpty(name))
            {
                return null;
            }

            string path = Path.Combine(folder, name + ".txt");
            return File.Exists(path) ? path : null;
        }

        /// <summary>
        /// One pack as id -> text, parsed the way FUN_100095530 (0x100095530) parses it: skip
        /// every line whose first character is '#', ';', '-' or '/', split each remaining line
        /// at its first '=', and keep it only when the left side is a whole number.
        /// </summary>
        private static Dictionary<ushort, string> Read(string path)
        {
            if (path == null)
            {
                return null;
            }

            string[] lines;
            try
            {
                lines = File.ReadAllLines(path);
            }
            catch (IOException)
            {
                return null;
            }
            catch (System.UnauthorizedAccessException)
            {
                return null;
            }

            Dictionary<ushort, string> table = new Dictionary<ushort, string>();
            foreach (string raw in lines)
            {
                string line = raw.Trim();
                if (line.Length == 0)
                {
                    continue;
                }

                char first = line[0];
                if (first == '#' || first == ';' || first == '-' || first == '/')
                {
                    continue;
                }

                int equals = line.IndexOf('=');
                if (equals <= 0)
                {
                    continue;
                }

                ushort id;
                if (!ushort.TryParse(line.Substring(0, equals).Trim(), System.Globalization.NumberStyles.Integer,
                        System.Globalization.CultureInfo.InvariantCulture, out id))
                {
                    continue;
                }

                table[id] = line.Substring(equals + 1).Trim();
            }

            return table;
        }

        /// <summary>The chosen language's pack, cached; null when it has no file.</summary>
        private static Dictionary<ushort, string> Pack(string language)
        {
            string name = Resolve(language, Folder);
            if (!PackCache.ContainsKey(name))
            {
                PackCache[name] = Read(PathOf(Folder, name));
            }

            return PackCache[name];
        }

        /// <summary>English text -> id, from English.txt beside the pack; null without it.</summary>
        private static Dictionary<string, ushort> Ids(string language)
        {
            if (!IdsCache.ContainsKey(language))
            {
                Dictionary<ushort, string> english = Pack(English);
                Dictionary<string, ushort> ids = null;
                if (english != null)
                {
                    ids = new Dictionary<string, ushort>();
                    foreach (KeyValuePair<ushort, string> pair in english)
                    {
                        if (pair.Value.Length != 0 && !ids.ContainsKey(pair.Value))
                        {
                            ids.Add(pair.Value, pair.Key);
                        }
                    }
                }

                IdsCache[language] = ids;
            }

            return IdsCache[language];
        }

        /// <summary>
        /// The pack's text for an English caption, when it has one. This is the getter the
        /// original calls for every caption (FUN_100095830 at 0x100095830).
        /// </summary>
        private static bool PackLookup(string englishText, string language, out string text)
        {
            text = null;
            Dictionary<ushort, string> pack = Pack(language);
            Dictionary<string, ushort> ids = Ids(language);
            ushort id;
            if (pack == null || ids == null || !ids.TryGetValue(englishText, out id))
            {
                return false;
            }

            string found;
            if (!pack.TryGetValue(id, out found) || string.IsNullOrEmpty(found))
            {
                return false;
            }

            text = found;
            return true;
        }

        /// <summary>English caption -> Spanish caption, for every text the windows show.</summary>
        private static readonly Dictionary<string, string> ToSpanish = new Dictionary<string, string>
        {
            // the Preferences window's own frame
            { "Preferences", "Preferencias" },
            { "General", "General" },
            { "Services & Ports ", "Servicios y puertos " },
            { "OK", "Aceptar" },
            { "Cancel", "Cancelar" },
            { "Apply", "Aplicar" },
            { "Setting file: laragon.ini", "Archivo de ajustes: laragon.ini" },
            { "Setting file: ", "Archivo de ajustes: " },
            { "   (saved)", "   (guardado)" },

            // General page
            { "Run Laragon when Windows starts", "Ejecutar Laragon al iniciar Windows" },
            { "Run minimized", "Ejecutar minimizado" },
            { "Start All automatically", "Iniciar todo automáticamente" },
            { "Language", "Idioma" },
            { "Document Root", "Raíz de documentos" },
            { "Data Dir", "Carpeta de datos" },
            { "Auto virtual hosts", "Hosts virtuales automáticos" },
            { "Hostname", "Nombre de host" },
            { "ex: if your project name is Laragon", "ej.: si tu proyecto se llama Laragon" },
            { "Auto Backup", "Copia de seguridad automática" },
            { "Backup Interval (hours)", "Intervalo de copia (horas)" },
            { "keeps the 5 most recent versions", "guarda las 5 versiones más recientes" },
            { "Auto Update", "Actualización automática" },
            { "Advanced", "Avanzado" },
            { "Mail Catcher", "Capturador de correo" },
            { "The Window will be displayed in", "La ventana se mostrará durante" },
            { "second(s)", "segundo(s)" },
            { "Mail Sender", "Enviador de correo" },
            { "Gmail Address", "Dirección de Gmail" },
            { "Gmail Password", "Contraseña de Gmail" },
            { "Test Sending Mail", "Probar el envío" },
            { "Your Gmail Password will be encrypted from curious eyes. When enabled, you can send "
              + "email to Internet easily. Just one line: mail($to, $subject, $message);",
              "Tu contraseña de Gmail se guarda cifrada. Cuando está activo puedes enviar correo a "
              + "Internet con una sola línea: mail($to, $subject, $message);" },

            // Services & Ports page
            { "Service", "Servicio" },
            { "Version", "Versión" },
            { "Port", "Puerto" },
            { "SSL port", "Puerto SSL" },
            { "Enabled", "Activado" },
            { "SSL", "SSL" },
            { "A service is started by \"Start All\" only when it is Enabled here and its program "
              + "exists in the install folder.",
              "Un servicio solo lo arranca «Iniciar todo» cuando está Activado aquí y su programa "
              + "existe en la carpeta de instalación." },

            // the main window's own words (MainForm.Designer.cs and MainForm.cs)
            { "Menu", "Menú" },
            { "Start All", "Iniciar todo" },
            { "Stop", "Detener" },
            { "Database", "Base de datos" },
            { "Root", "Raíz" },
            { "Processing...", "Procesando..." },
            { "Laragon is ready.", "Laragon está listo." },
            { "The dev environment for entrepreneurs", "El entorno de desarrollo para emprendedores" },
            { "All services are stopped.", "Todos los servicios están parados." },
            { "Running: ", "En marcha: " },
            { "Versions", "Versiones" },
            { "Nothing installed under ", "No hay nada instalado en " },

            // what the main window says while it works
            { "Path must not have SPACE!", "¡La ruta no puede tener espacios!" },
            { "No service is enabled: tick 'Enabled' on the ones you want Start All to bring up.",
              "Ningún servicio está activado: marca «Activado» en los que quieras arrancar." },
            { "No servers are installed under ", "No hay servidores instalados en " },
            { " started: ", " arrancados: " },
            { "Stopping...", "Parando..." },
            { "Stopped.", "Parado." },
            { " started.", " arrancado." },
            { " stopped.", " parado." },
            { "You must enable Apache/Nginx and MySQL services in Menu > Preferences > Services and Ports!",
              "Tienes que activar Apache/Nginx y MySQL en Menú > Preferencias > Servicios y puertos." },
            { "Folder not found: ", "No se encuentra la carpeta: " },
            { "File not found: ", "No se encuentra el archivo: " },
            { "Redis is not installed.", "Redis no está instalado." },
            { "Please wait, Laragon is starting your servers...",
              "Espera, Laragon está arrancando tus servidores..." },

            // the popup menu's words that are Spanish words rather than program names
            { "Preferences...", "Preferencias..." },
            { "Exit", "Salir" },
            { "Profile", "Perfil" },
            { "Add profile", "Añadir perfil" },
            { "Tools", "Herramientas" },
            { "Quick app", "App rápida" },
            { "Quick Linux server", "Servidor Linux rápido" },
            { "Deploy to Cloud", "Publicar en la nube" },
            { "Startup file: user-profile.cmd", "Archivo de inicio: user-profile.cmd" },
            { "Right-Click Menu", "Menú del botón derecho" },
            { "Manage Path", "Gestionar la ruta" },
            { "Add Laragon to Path", "Añadir Laragon a la ruta" },
            { "Remove Laragon from Path", "Quitar Laragon de la ruta" },
            { "Run as Windows Service", "Ejecutar como servicio de Windows" },
            { "Add laragon.crt to Trust Store", "Añadir laragon.crt al almacén de confianza" },
            { "Certificate Manager", "Gestor de certificados" },
            { "Generate certs and add to trust store",
              "Generar certificados y añadirlos al almacén de confianza" },
            { "Create database", "Crear base de datos" },
            { "Change root's password", "Cambiar la contraseña de root" },
            { "Get root's password", "Obtener la contraseña de root" },
            { "Backup all databases", "Copia de seguridad de todas las bases" },
            { "Reset and generate a random password for root",
              "Reiniciar y generar una contraseña aleatoria para root" },
            { "Create project", "Crear proyecto" },
            { "Switch project", "Cambiar de proyecto" },
            { "No Project", "Sin proyecto" },
            { "Quick settings", "Ajustes rápidos" },
            { "Extensions", "Extensiones" },
            { "Configuration...", "Configuración..." },
            { "View latest email", "Ver el último correo" },
            { "Open Mail Dir", "Abrir la carpeta de correo" },
            { "Web admin (login: mem/1)", "Administración web (usuario: mem/1)" },
            { "Web UI", "Interfaz web" },
            { "Web admin", "Administración web" },

            // Quick create
            { "Project name", "Nombre del proyecto" },
            { "Create", "Crear" },

            // Messages, dialogs and the About window this rebuild writes itself
            { "About", "Acerca de" },
            { "Active profile:", "Perfil activo:" },
            { "A profile is a set of chosen versions for PHP, MySQL, Apache and so on.",
              "Un perfil es un conjunto de versiones elegidas para PHP, MySQL, Apache, etc." },
            { "A profile is a named copy of usr\\laragon.ini; copy the file and give it the profile's name.",
              "Un perfil es una copia con nombre de usr\\laragon.ini: copia el archivo y ponle el nombre del perfil." },
            { "Start: launches every server that is installed in bin\\ and not running.",
              "Iniciar: arranca todos los servidores instalados en bin\\ que no estén en marcha." },
            { "Web: opens your site in the browser.", "Web: abre tu sitio en el navegador." },
            { "Database: opens phpMyAdmin.", "Base de datos: abre phpMyAdmin." },
            { "Terminal: opens a shell in the Laragon folder.", "Terminal: abre una consola en la carpeta de Laragon." },
            { "Root: opens the Laragon folder in Explorer.", "Root: abre la carpeta de Laragon en el Explorador." },
            { "Settings file:", "Archivo de ajustes:" },
            { "Root in use:", "Carpeta en uso:" },
            { "this program's folder", "la carpeta de este programa" },
            { "an existing Laragon install", "una instalación de Laragon existente" },
            { "Nothing installed under {0}", "No hay nada instalado en {0}" },
            { "This service is not installed in {0}.", "Este servicio no está instalado en {0}." },
            { "Mail is caught by Mailpit in current Laragon versions; see the Mailpit menu.",
              "En las versiones actuales de Laragon el correo lo captura Mailpit; mira el menú Mailpit." },
            { "A newer Laragon is already running.", "Ya hay un Laragon más reciente en marcha." },

            // Create database / root password / backup
            { "Database name", "Nombre de la base de datos" },
            { "New password", "Nueva contraseña" },
            { "Only letters, numbers and _ are allowed in a database name.",
              "En el nombre de la base de datos solo se permiten letras, números y _." },
            { "Database created: ", "Base de datos creada: " },
            { "The MySQL root password is empty (Laragon's default).",
              "La contraseña de root de MySQL está vacía (lo normal en Laragon)." },
            { "The MySQL root password has been copied to the clipboard",
              "La contraseña de root de MySQL se ha copiado al portapapeles" },
            { "The MySQL root password has been changed.", "La contraseña de root de MySQL se ha cambiado." },
            { "The new root password is on the clipboard: ",
              "La nueva contraseña de root está en el portapapeles: " },
            { "All MySQL databases have been backed up successfully to {0}",
              "Todas las bases de datos de MySQL se han respaldado en {0}" },

            // Path, certificates, cloud, PHP settings
            { "Laragon is already in Path.", "Laragon ya está en el Path." },
            { "Laragon is not in Path.", "Laragon no está en el Path." },
            { "Laragon added to Path. Open a new terminal to use it.",
              "Laragon añadido al Path. Abre una terminal nueva para usarlo." },
            { "Laragon removed from Path.", "Laragon quitado del Path." },
            { "sendmail_path copied to the clipboard", "sendmail_path copiado al portapapeles" },
            { "Use \"Generate certs and add to trust store\" first.",
              "Usa primero \"Generar certificados y añadirlos al almacén de confianza\"." },
            { "No file! openssl.exe is not installed, so the certificate cannot be generated here.",
              "¡No hay archivo! openssl.exe no está instalado, así que aquí no se puede generar el certificado." },
            { "Deploying to the Laragon cloud needs a Laragon account and an internet connection; it is not part of this rebuild.",
              "Publicar en la nube de Laragon necesita una cuenta de Laragon y conexión a internet; no forma parte de esta reconstrucción." },
            { "Restart Laragon to use this version: ", "Reinicia Laragon para usar esta versión: " },
            { "The current project is cleared.", "Se ha borrado el proyecto actual." },
            { "This feature is not available on your computer!",
              "¡Esta función no está disponible en tu equipo!" },
            { "Running a server as a Windows service needs administrator rights and is not part of this rebuild: Laragon starts it as a program instead.",
              "Ejecutar un servidor como servicio de Windows necesita permisos de administrador y no forma parte de esta reconstrucción: Laragon lo arranca como programa." },
            { "Drop the Terminal package into this folder, then restart Sublime Text.",
              "Copia el paquete Terminal en esta carpeta y reinicia Sublime Text." },
            { "This feature needs Sublime Text, which is not installed in bin\\.",
              "Esta función necesita Sublime Text, que no está instalado en bin\\." },
            { "{0} is not wired up in this rebuild yet.", "{0} todavía no está conectado en esta reconstrucción." },
            { "The original action is listed in MECHANICS.md.", "La acción original está descrita en MECHANICS.md." },

            // Preferences, quick create and the messages next to them
            { "You must specify a valid Gmail address!", "¡Debes indicar una dirección de Gmail válida!" },
            { "No Laragon folder is open, so the test mail cannot be sent.",
              "No hay ninguna carpeta de Laragon abierta, así que no se puede enviar el correo de prueba." },
            { "PHP was not found, so the test mail cannot be sent.",
              "No se ha encontrado PHP, así que no se puede enviar el correo de prueba." },
            { "The script needs", "El script necesita" },
            { "Could not write ", "No se pudo escribir " },
            { "PHP printed nothing.", "PHP no ha mostrado nada." },
            { "The value: {0} is invalid. Display Time range is from {1} - {2}!",
              "El valor {0} no es válido. El tiempo de visualización va de {1} a {2}." },
            { "Please type a name for the website.", "Escribe un nombre para el sitio web." },
            { "Quick create website", "Crear sitio web rápido" },
            { "Website created:", "Sitio web creado:" },
            { "Open it at", "Ábrelo en" },
            { "Laragon is already running!", "¡Laragon ya se está ejecutando!" },
            { "No servers are installed under {0}.", "No hay servidores instalados en {0}." },
            { "not installed", "no está instalado" },
            { "already running", "ya está en marcha" },
            { "executable not found: ", "no se encuentra el ejecutable: " },
            { "The hosts file could not be changed: ", "No se pudo cambiar el archivo hosts: " },
            { "now", "ahora" },

            // A few menu rows the Spanish pack leaves in English
            { "Terminal  (CTRL+ALT+T)", "Terminal  (CTRL+ALT+T)" },
            { "Add/Update Sublime Text & Terminal", "Añadir/Actualizar Sublime Text y Terminal" },
            { "IDLE (Python GUI)", "IDLE (interfaz de Python)" },
            { "Python Help", "Ayuda de Python" },
        };

        /// <summary>
        /// Families of captions too regular to list one by one: a prefix put in front of a
        /// program's name ("Start Apache", "Reload", "Version [2.4.10]"…). Applied only when
        /// the whole caption is not in the table above, so the table always wins.
        /// </summary>
        private static readonly string[,] Rules =
        {
            { "Start ", "Iniciar " },
            { "Stop ", "Detener " },
            { "Reload ", "Recargar " },
            { "Restart ", "Reiniciar " },
            { "Create ", "Crear " },
            { "Add ", "Añadir " },
            { "Remove ", "Quitar " },
            { "Edit ", "Editar " },
            { "Open ", "Abrir " },
            { "Version", "Versión" },
        };

        private static readonly Dictionary<string, string> ToEnglish = Reverse();

        private static Dictionary<string, string> Reverse()
        {
            Dictionary<string, string> back = new Dictionary<string, string>();
            foreach (KeyValuePair<string, string> pair in ToSpanish)
            {
                if (!back.ContainsKey(pair.Value))
                {
                    back.Add(pair.Value, pair.Key);
                }
            }

            return back;
        }

        /// <summary>Our wording for a caption, or the pack's wording when the two differ.</summary>
        private static string Alias(string text)
        {
            string spelled;
            return text != null && Aliases.TryGetValue(text, out spelled) ? spelled : text;
        }

        /// <summary>The name to store in the ini for a language; anything else means English.</summary>
        internal static string Name(string language)
        {
            if (language == null)
            {
                return English;
            }

            string trimmed = language.Trim();
            if (trimmed.Equals(Spanish, System.StringComparison.OrdinalIgnoreCase)
                || trimmed.Equals("Español", System.StringComparison.OrdinalIgnoreCase)
                || trimmed.Equals("Espanol", System.StringComparison.OrdinalIgnoreCase)
                || trimmed.Equals("es", System.StringComparison.OrdinalIgnoreCase))
            {
                return Spanish;
            }

            return English;
        }

        /// <summary>
        /// Captions this rebuild words its own way, mapped to the wording the original's packs
        /// carry (English.txt, ids in brackets), so the person's Spanish comes from the pack and
        /// not from anything invented here. Taken from tools/langcover.py's list of the captions
        /// no pack entry matched.
        /// </summary>
        private static readonly Dictionary<string, string> Aliases = new Dictionary<string, string>
        {
            { "Data Dir", "Data Directory" },                       // 319
            { "Auto virtual hosts", "Auto-create Virtual Hosts" },  // 322
            { "Gmail Address", "Gmail Account Name" },              // 340
            { "Gmail Password", "Gmail Account Password" },         // 341
            { "Test Sending Mail", "Test Mail Sending" },           // 342
        };

        /// <summary>The caption in the current language.</summary>
        internal static string T(string english)
        {
            // every caption the person reads comes through here, so this is the one place the
            // product's name has to be put right: the packs and the tables below carry the
            // original's own wording, and Brand.Fix renames it without touching a path
            return Brand.Fix(Translate(english, Current));
        }

        /// <summary>A caption in a chosen language, whether it is English or Spanish now.</summary>
        internal static string Translate(string text, string language)
        {
            if (text == null)
            {
                return null;
            }

            // 1. the caption as it stands: Spanish in, English out
            string englishText;
            if (!ToEnglish.TryGetValue(text, out englishText))
            {
                // 2. it came back in English as it stands
                englishText = text;
            }

            // the designer writes "&&" where a menu shows one "&": look the real caption up
            englishText = englishText.Replace("&&", "&");

            // the original's own pack wins over anything built in here — but only when the pack
            // really holds this caption in this language: the Spanish file is shorter than the
            // English one, so an id present in English and missing in Spanish comes back
            // unchanged and must fall through to the table below instead of blocking it
            string packed;
            if (PackLookup(Alias(englishText), language, out packed) && packed != englishText)
            {
                return packed;
            }

            if (Name(language) != Spanish)
            {
                return englishText;
            }

            // 3. a caption written word for word in the table
            string translated;
            if (ToSpanish.TryGetValue(englishText, out translated))
            {
                return translated;
            }

            // 4. a regular family: "Start Apache" and the like
            for (int i = 0; i < Rules.GetLength(0); i++)
            {
                string prefix = Rules[i, 0];
                if (englishText.StartsWith(prefix, System.StringComparison.Ordinal))
                {
                    return Rules[i, 1] + englishText.Substring(prefix.Length);
                }
            }

            return englishText;
        }

        /// <summary>Puts every caption of a window into the current language.</summary>
        internal static void Apply(Form form)
        {
            if (form == null)
            {
                return;
            }

            string originalTitle;
            if (!Originals.TryGetValue(form, out originalTitle))
            {
                originalTitle = form.Text;
                Originals.Add(form, originalTitle);
            }

            Apply((Control)form);
            form.Text = T(originalTitle);

            ToolStrip strip = form.MainMenuStrip;
            if (strip != null)
            {
                ApplyItems(strip.Items);
            }

            // the main window has no menu bar: its whole feature set is one popup menu
            // (mnuLaragon), attached to the form, to the tray icon and to the "Menu" label
            if (form.ContextMenuStrip != null)
            {
                ApplyItems(form.ContextMenuStrip.Items);
            }
        }

        /// <summary>
        /// What each control's caption was before it was translated. A pack's Spanish cannot be
        /// turned back into English (the packs only go one way), so re-reading the current text
        /// would leave the window in Spanish after the person picked English again. Keeping the
        /// first caption seen is what makes a second Apply correct.
        /// </summary>
        private static readonly Dictionary<Control, string> Originals =
            new Dictionary<Control, string>();

        private static readonly Dictionary<ToolStripItem, string> ItemOriginals =
            new Dictionary<ToolStripItem, string>();

        /// <summary>
        /// The captions the original writes itself and then follows with ":" (the literal at
        /// 0x1003a20a0, used by FUN_10008b540 at 0x10008b540 for the preferences window), so
        /// the dialog reads "Language:" while its form resource only says "Language".
        /// </summary>
        private static readonly string[] AfterColon =
        {
            "Language", "Document Root", "Data Dir", "Hostname",
            "Gmail Address", "Gmail Password", "The Window will be displayed in",
        };

        private static void Apply(Control parent)
        {
            foreach (Control child in parent.Controls)
            {
                // only controls that carry a caption are touched: a TextBox or a NumericUpDown
                // holds a value, and writing its designer text back would undo what
                // LoadSettings put there (the PHP port went back to 1025 that way)
                if (CarriesCaption(child))
                {
                    string original;
                    if (!Originals.TryGetValue(child, out original))
                    {
                        original = child.Text;
                        Originals.Add(child, original);
                    }

                    string text = T(original);
                    // a long translated caption is cut to 27 characters plus "..", which is
                    // what the original does to the caption it puts on its Start All button
                    child.Text = child is Button ? Elide(text) : Suffixed(original, text);
                }

                if (child.ContextMenuStrip != null)
                {
                    ApplyItems(child.ContextMenuStrip.Items);
                }

                Apply(child);
            }
        }

        /// <summary>Whether a control shows a caption the packs can translate.</summary>
        private static bool CarriesCaption(Control control)
        {
            return control is Label || control is Button || control is CheckBox
                || control is RadioButton || control is GroupBox || control is TabPage
                || control is LinkLabel;
        }

        /// <summary>
        /// "Backup Interval (hours)" is one caption here and two entries in the original's packs:
        /// it formats T(362, "Backup Interval") + " (" + T(363, "hours") + ")" (FUN_10008b540).
        /// </summary>
        private static string Suffixed(string original, string translated)
        {
            if (original == "Backup Interval (hours)")
            {
                return T("Backup Interval") + " (" + T("hours") + ")";
            }

            for (int i = 0; i < AfterColon.Length; i++)
            {
                if (original == AfterColon[i])
                {
                    return translated + ":";
                }
            }

            return translated;
        }

        private static void ApplyItems(ToolStripItemCollection items)
        {
            foreach (ToolStripItem item in items)
            {
                if (item is ToolStripSeparator)
                {
                    continue;
                }

                string original;
                if (!ItemOriginals.TryGetValue(item, out original))
                {
                    original = item.Text;
                    ItemOriginals.Add(item, original);
                }

                item.Text = T(original);
                ToolStripMenuItem menu = item as ToolStripMenuItem;
                if (menu != null)
                {
                    ApplyItems(menu.DropDownItems);
                }
            }
        }
    }
}
