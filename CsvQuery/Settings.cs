namespace CsvQuery
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Windows.Forms;
    using PluginInfrastructure;

    public class Settings
    {

        [Description("In debugmode extra diagnostics are output"), Category("General"), DefaultValue(false)]
        public bool DebugMode { get; set; }
        /// <summary> Preferred separators </summary>
        public string Separators { get; set; } = ",;|\t";

        public static readonly string IniFilePath;
        public static Settings Current;

        static Settings()
        {
            var sbIniFilePath = new StringBuilder(Win32.MAX_PATH);
            Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_GETPLUGINSCONFIGDIR, Win32.MAX_PATH, sbIniFilePath);
            var configDirectory = sbIniFilePath.ToString();
            if (!Directory.Exists(configDirectory)) Directory.CreateDirectory(configDirectory);
            IniFilePath = Path.Combine(configDirectory, Main.PluginName + ".ini");
        }
        
        public Settings(bool loadFromFile=true)
        {
            if (loadFromFile && File.Exists(IniFilePath))
            {
                var loaded = GetKeys(IniFilePath, "General");
                foreach (var propertyInfo in this.GetType().GetProperties())
                {
                    var name = propertyInfo.Name;
                    if (loaded.ContainsKey(name) && !string.IsNullOrEmpty(loaded[name]))
                    {
                        if (propertyInfo.PropertyType == typeof(string))
                            propertyInfo.SetValue(this, loaded[name], null);
                        else if (propertyInfo.PropertyType == typeof(bool))
                            if(bool.TryParse(loaded[name], out var dbg))
                                propertyInfo.SetValue(this, dbg, null);
                    }
                }
            }
        }

        public void Save()
        {
            var sb = new StringBuilder();
            //sb.Append($"DebugMode={DebugMode}\0");

            foreach (var propertyInfo in this.GetType().GetProperties())
            {
                sb.AppendFormat("{0}={1}\0", propertyInfo.Name, propertyInfo.GetValue(this, null));
            }
            Win32.WritePrivateProfileSection("General", sb.ToString(), IniFilePath);
        }

        private Dictionary<string, string> GetKeys(string iniFile, string category)
        {
            var buffer = new byte[8*1024];

            Win32.GetPrivateProfileSection(category, buffer, buffer.Length, iniFile);
            var tmp = Encoding.ASCII.GetString(buffer).Trim('\0').Split('\0');
            return tmp.Select(x => x.Split(new[] {'='}, 2)).Where(x => x.Length==2).ToDictionary(x => x[0], x => x[1]);
        }

        public Settings Clone()
        {
            return (Settings) this.MemberwiseClone();
        }

        public void ShowDialog()
        {
            var copy = (Settings)this.MemberwiseClone();
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
                        Location = new Point(300-75-13,300-23-13),
                        UseVisualStyleBackColor = true
                    },
                    new Button
                    {
                        Name = "Ok",
                        Text = "&Ok",
                        Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                        Size = new Size(75, 23),
                        Location = new Point(300-75-13-81,300-23-13),
                        UseVisualStyleBackColor = true
                    },
                    new PropertyGrid
                    {
                        Name = "Grid",
                        Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                        Location = new Point(13,13),
                        Size = new Size(300-13-13,300-55),
                        SelectedObject = copy
                    }
                }
            };

            dialog.Controls["Cancel"].Click += (a, b) => dialog.Close();
            dialog.Controls["Ok"].Click += (a, b) =>
            {
                copy.Save();
                Settings.Current = copy;
                dialog.Close();
            };

            dialog.ShowDialog();
        }
    }

    internal static class Helpers
    {
        internal static void GetBool(this Dictionary<string, string> loaded, string name, ref bool target)
        {
            if (loaded.ContainsKey(name) && bool.TryParse(loaded[name], out var dbg)) target = dbg;
        }
    }
}
