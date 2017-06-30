namespace CsvQuery.Tools
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Windows.Forms;
    using System.Reflection;
    using PluginInfrastructure;

    /// <summary>
    /// Manages application settings
    /// </summary>
    public class Settings
    {
        [Description("In debugmode extra diagnostics are output"), Category("General"), DefaultValue(false)]
        public bool DebugMode { get; set; }

        [Description("Separators that are detected automatically"), Category("General"), DefaultValue(",;|\\t")]
        public string Separators { get; set; } = ",;|\\t:";

        [Description("Saves the query cache to disk between boots"), Category("General"), DefaultValue(true)]
        public bool SaveQueryCache { get; set; }

        [Description("Default separators when generating CSV"), Category("Generation"), DefaultValue(",")]
        public string DefaultSeparator { get; set; }

        [Description("Default quote character when generating CSV"), Category("Generation"), DefaultValue('\"')]
        public char DefaultQuoteChar { get; set; }

        [Description("SQL provider for data storage"), Category("SQL"), DefaultValue(DataStorageProvider.SQLite)]
        public DataStorageProvider StorageProvider { get; set; }

        [Description("For SQLite a file path or ':memory:' for in-memory database. \r\nFor MSSQL name of database in local default instance (trusted connection)"), Category("SQL"), DefaultValue(":memory:")]
        public string Database { get; set; }

        [Description("If true converts strings starting with zeroes (e.g. 0014) to numbers"), Category("General"), DefaultValue(false)]
        public bool ConvertInitialZerosToNumber { get; set; }

        [Description("Maximum length of a numeric string before it's considered a string instead"), Category("General"), DefaultValue(10)]
        public int MaxIntegerStringLength { get; set; }

        #region Inner workings
        private static readonly string IniFilePath;
        private readonly List<Tuple<Func<Settings, bool>, string[]>> _listeners = new List<Tuple<Func<Settings, bool>, string[]>>();

        static Settings()
        {
            // Figure out default N++ config file path
            var sbIniFilePath = new StringBuilder(Win32.MAX_PATH);
            Win32.SendMessage(PluginBase.nppData._nppHandle, (uint) NppMsg.NPPM_GETPLUGINSCONFIGDIR, Win32.MAX_PATH, sbIniFilePath);
            var configDirectory = sbIniFilePath.ToString();
            IniFilePath = Path.Combine(configDirectory, Main.PluginName + ".ini");
        }

        /// <summary>
        /// By default loads settings from the default N++ config folder
        /// </summary>
        /// <param name="loadFromFile"> If false will not load anything and have default values set </param>
        public Settings(bool loadFromFile = true)
        {
            // Set defaults
            foreach (var propertyInfo in GetType().GetProperties())
            {
                if (propertyInfo.GetCustomAttributes(typeof(DefaultValueAttribute), false).FirstOrDefault() is DefaultValueAttribute def)
                {
                    propertyInfo.SetValue(this, def.Value, null);
                }
            }
            if (loadFromFile) ReadFromIniFile();
        }

        /// <summary>
        /// Reads all (existing) settings from an ini-file
        /// </summary>
        /// <param name="filename">File to write to (default is N++ plugin config)</param>
        public void ReadFromIniFile(string filename = null)
        {
            filename = filename ?? IniFilePath;
            if (!File.Exists(filename)) return;

            // Load all sections from file
            var loaded = GetType().GetProperties()
                .Select(x => ((CategoryAttribute) x.GetCustomAttributes(typeof(CategoryAttribute), false).FirstOrDefault())?.Category ?? "General")
                .Distinct()
                .ToDictionary(section => section, section => GetKeys(filename, section));
            
            //var loaded = GetKeys(filename, "General");
            foreach (var propertyInfo in GetType().GetProperties())
            {
                var category = ((CategoryAttribute)propertyInfo.GetCustomAttributes(typeof(CategoryAttribute), false).FirstOrDefault())?.Category ?? "General";
                var name = propertyInfo.Name;
                if (loaded.ContainsKey(category) && loaded[category].ContainsKey(name) && !string.IsNullOrEmpty(loaded[category][name]))
                {
                    var rawString = loaded[category][name];
                    var converter = TypeDescriptor.GetConverter(propertyInfo.PropertyType);
                    if (converter.IsValid(rawString))
                    {
                        propertyInfo.SetValue(this, converter.ConvertFromString(rawString), null);
                    }
                }
            }
        }

        /// <summary>
        /// Saves all settings to an ini-file, under "General" section
        /// </summary>
        /// <param name="filename">File to write to (default is N++ plugin config)</param>
        public void SaveToIniFile(string filename = null)
        {
            filename = filename ?? IniFilePath;
            var dir = Path.GetDirectoryName(filename);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            // Win32.WritePrivateProfileSection (that NppPlugin uses) doesn't work well with non-ASCII characters. So we roll our own.
            using (var fp = new StreamWriter(filename, false, Encoding.UTF8))
            {
                fp.WriteLine("; {0} settings file", Main.PluginName);

                foreach (var section in GetType()
                    .GetProperties()
                    .GroupBy(x => ((CategoryAttribute) x.GetCustomAttributes(typeof(CategoryAttribute), false)
                                      .FirstOrDefault())?.Category ?? "General"))
                {
                    fp.WriteLine(Environment.NewLine + "[{0}]", section.Key);
                    foreach (var propertyInfo in section.OrderBy(x => x.Name))
                    {
                        if (propertyInfo.GetCustomAttributes(typeof(DescriptionAttribute), false).FirstOrDefault() is DescriptionAttribute description)
                            fp.WriteLine("; " + description.Description.Replace(Environment.NewLine, Environment.NewLine + "; "));
                        var converter = TypeDescriptor.GetConverter(propertyInfo.PropertyType);
                        fp.WriteLine("{0}={1}", propertyInfo.Name, converter.ConvertToInvariantString(propertyInfo.GetValue(this, null)));
                    }
                }
            }
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
            var tmp = Encoding.UTF8.GetString(buffer).Trim('\0').Split('\0');
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
                // Run listeners
                foreach (var listener in _listeners)
                {
                    bool result;
                    if (listener.Item2 == null || listener.Item2.Length == 0)
                    {
                        result = listener.Item1(copy);
                    }
                    else
                    {
                        foreach (var setting in listener.Item2)
                        {
                            var prop = GetType().GetProperty(setting);
                            if (!prop.GetValue(this, null).Equals(prop.GetValue(copy, null)))
                            {
                                result = listener.Item1(copy);
                                break;
                            }
                        }
                    }
                }

                copy.SaveToIniFile();
                // Copy all settings to this
                foreach (var propertyInfo in GetType().GetProperties())
                {
                    var oldValue = propertyInfo.GetValue(this, null);
                    var newValue = propertyInfo.GetValue(copy, null);
                    if (!oldValue.Equals(newValue))
                    {
                        Trace.TraceInformation($"Setting {propertyInfo.Name} has changed");
                    }
                    propertyInfo.SetValue(this, newValue , null);
                }
                dialog.Close();
            };

            dialog.ShowDialog();
        }

        /// <summary> Opens the config file directly in Notepad++ </summary>
        public void OpenFile()
        {
            if(!File.Exists(IniFilePath)) SaveToIniFile();
            Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_DOOPEN, 0, IniFilePath);
        }

        public void RegisterListener(Func<Settings,bool> eventListener, params string[] settingName)
        {
            this._listeners.Add(Tuple.Create(eventListener, settingName));
        }
#endregion
    }
}