using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CsvQuery.Forms
{
    public partial class SettingsDialog : Form
    {
        private readonly Settings _settings;

        public SettingsDialog(Settings settings)
        {
            _settings = settings;
            InitializeComponent();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            // TODO save
            _settings.Save();
            Main.Settings = _settings;
            this.Close();
        }

        private void SettingsDialog_Load(object sender, EventArgs e)
        {
            this.settingsGrid.SelectedObject = _settings;
        }
    }
}
