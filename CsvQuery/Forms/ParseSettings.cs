namespace CsvQuery.Forms
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Forms;
    using Csv;
    using Tools;

    public partial class ParseSettings : Form
    {
        public ParseSettings()
        {
            this.InitializeComponent();
        }

        public ParseSettings(CsvSettings csvSettings)
            : this()
        {
            this.txbSep.Text = csvSettings.Separator.ToString();
            this.useQuotesCheckBox.Checked = csvSettings.UseQuotes;
            this.txbCommentChar.Text = csvSettings.CommentCharacter.ToString();
            this.hasHeaderCheckbox.CheckState = csvSettings.HasHeader.ToCheckboxState();
            this.FixedWidthCheckbox.Checked = csvSettings.FieldWidths != null;
        }

        public CsvSettings Settings =>
            new CsvSettings(this.txbSep.Text.Unescape(),
                this.useQuotesCheckBox.Checked,
                this.txbCommentChar.Text.FirstOrDefault(),
                this.hasHeaderCheckbox.CheckState.ToNullableBool(),
                this.FixedWidthCheckbox.Checked ? new List<int>() : null);
    }
}