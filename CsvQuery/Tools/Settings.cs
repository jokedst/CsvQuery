namespace CsvQuery.Tools
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Windows.Forms;
    using PluginInfrastructure;

    /// <summary>
    /// Manages application settings
    /// </summary>
    public class Settings
    {
        [Description("In debugmode extra diagnostics are output"), Category("General"), DefaultValue(false)]
        public bool DebugMode { get; set; }

        [Description("Separators that are detected automatically"), Category("General"), DefaultValue(",;|\t")]
        public string Separators { get; set; } = ",;|\t:";

#region Inner workings
        private static readonly string IniFilePath;

        static Settings()
        {
            // Figure out default N++ config file path
            var sbIniFilePath = new StringBuilder(Win32.MAX_PATH);
            Win32.SendMessage(PluginBase.nppData._nppHandle, (uint) NppMsg.NPPM_GETPLUGINSCONFIGDIR, Win32.MAX_PATH, sbIniFilePath);
            var configDirectory = sbIniFilePath.ToString();
            if (!Directory.Exists(configDirectory)) Directory.CreateDirectory(configDirectory);
            IniFilePath = Path.Combine(configDirectory, Main.PluginName + ".ini");
        }

        /// <summary>
        /// By default loads settings from the default N++ config folder
        /// </summary>
        /// <param name="loadFromFile"> If false will not load anything and have default values set </param>
        public Settings(bool loadFromFile = true)
        {
            if (loadFromFile) ReadFromIniFile();
        }

        /// <summary>
        /// Saves all settings to an ini-file, under "General" section
        /// </summary>
        /// <param name="filename">File to write to (default is N++ plugin config)</param>
        public void ReadFromIniFile(string filename = null)
        {
            filename = filename ?? IniFilePath;
            if (!File.Exists(filename)) return;

            var loaded = GetKeys(filename, "General");
            foreach (var propertyInfo in GetType().GetProperties())
            {
                var name = propertyInfo.Name;
                if (loaded.ContainsKey(name) && !string.IsNullOrEmpty(loaded[name]))
                {
                    var converter = TypeDescriptor.GetConverter(propertyInfo.PropertyType);
                    if (converter.IsValid(loaded[name]))
                    {
                        propertyInfo.SetValue(this, converter.ConvertFromString(loaded[name]), null);
                    }
                }
            }
        }

        /// <summary>
        /// Saves all settings to an ini-file, under "General" section
        /// </summary>
        /// <param name="filename">File to load (default is N++ plugin config)</param>
        public void SaveToIniFile(string filename = null)
        {
            filename = filename ?? IniFilePath;
            var sb = new StringBuilder();
            foreach (var propertyInfo in GetType().GetProperties())
                sb.AppendFormat("{0}={1}\0", propertyInfo.Name, propertyInfo.GetValue(this, null));
            Win32.WritePrivateProfileSection("General", sb.ToString(), filename);
        }

        /// <summary>
        /// Read a section from an ini-file
        /// </summary>
        /// <param name="iniFile">Path to ini-file</param>
        /// <param name="category">Section to read</param>
        private Dictionary<string, string> GetKeys(string iniFile, string category)
        {
            var buffer = new byte[8 * 1024];

            Win32.GetPrivateProfileSection(category, buffer, buffer.Length, iniFile);
            var tmp = Encoding.ASCII.GetString(buffer).Trim('\0').Split('\0');
            return tmp.Select(x => x.Split(new[] {'='}, 2))
                .Where(x => x.Length == 2)
                .ToDictionary(x => x[0], x => x[1]);
        }

        /// <summary>
        /// Opens a window that edits all settings
        /// </summary>
        public void ShowDialog()
        {
            // We bind a copy of this object and only apply it after they click "Ok"
            var copy = (Settings) MemberwiseClone();
            var dialog = new Form
            {
                Text = "Settings",
                ClientSize = new Size(300, 300),
                MinimumSize = new Size(250, 250),
                ShowIcon = false,
                Controls =
                {
                    new Button
                    {
                        Name = "Cancel",
                        Text = "&Cancel",
                        Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                        Size = new Size(75, 23),
                        Location = new Point(300 - 75 - 13, 300 - 23 - 13),
                        UseVisualStyleBackColor = true
                    },
                    new Button
                    {
                        Name = "Ok",
                        Text = "&Ok",
                        Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                        Size = new Size(75, 23),
                        Location = new Point(300 - 75 - 13 - 81, 300 - 23 - 13),
                        UseVisualStyleBackColor = true
                    },
                    new PropertyGrid
                    {
                        Name = "Grid",
                        Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                        Location = new Point(13, 13),
                        Size = new Size(300 - 13 - 13, 300 - 55),
                        SelectedObject = copy
                    }
                }
            };

            dialog.Controls["Cancel"].Click += (a, b) => dialog.Close();
            dialog.Controls["Ok"].Click += (a, b) =>
            {
                copy.SaveToIniFile();
                // Copy all settings to this
                foreach (var propertyInfo in GetType().GetProperties())
                    propertyInfo.SetValue(this, propertyInfo.GetValue(copy, null), null);
                dialog.Close();
            };

            dialog.ShowDialog();
        }
#endregion
    }
}