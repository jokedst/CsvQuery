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
            InitializeComponent();
        }

        public ParseSettings(CsvSettings csvSettings)
            : this()
        {
            txbSep.Text = csvSettings.Separator.ToString();
            txbQuoteChar.Text = csvSettings.TextQualifier.ToString();
            txbCommentChar.Text = csvSettings.CommentCharacter.ToString();
            hasHeaderCheckbox.CheckState = csvSettings.HasHeader.ToCheckboxState();
            FixedWidthCheckbox.Checked = csvSettings.FieldWidths != null;
        }

        public CsvSettings Settings =>
            new CsvSettings(
                txbSep.Text.Unescape(),
                txbQuoteChar.Text.Unescape(),
                txbCommentChar.Text.FirstOrDefault(),
                hasHeaderCheckbox.CheckState.ToNullableBool(),
                FixedWidthCheckbox.Checked ? new List<int>() : null);
    }
}