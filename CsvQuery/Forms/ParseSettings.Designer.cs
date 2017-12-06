namespace CsvQuery.Forms
{
    partial class ParseSettings
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnReparse = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.MainLabel = new System.Windows.Forms.Label();
            this.txbSep = new System.Windows.Forms.TextBox();
            this.lblSeparator = new System.Windows.Forms.Label();
            this.QuoteCharLabel = new System.Windows.Forms.Label();
            this.txbQuoteChar = new System.Windows.Forms.TextBox();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.txbCommentChar = new System.Windows.Forms.TextBox();
            this.CommentCharLabel = new System.Windows.Forms.Label();
            this.hasHeaderCheckbox = new System.Windows.Forms.CheckBox();
            this.FixedWidthCheckbox = new System.Windows.Forms.CheckBox();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnReparse
            // 
            this.btnReparse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnReparse.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnReparse.Location = new System.Drawing.Point(26, 374);
            this.btnReparse.Margin = new System.Windows.Forms.Padding(6);
            this.btnReparse.Name = "btnReparse";
            this.btnReparse.Size = new System.Drawing.Size(150, 44);
            this.btnReparse.TabIndex = 1;
            this.btnReparse.Text = "&Try again";
            this.btnReparse.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(394, 374);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(6);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(150, 44);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "&Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // MainLabel
            // 
            this.MainLabel.AutoSize = true;
            this.flowLayoutPanel1.SetFlowBreak(this.MainLabel, true);
            this.MainLabel.Location = new System.Drawing.Point(6, 0);
            this.MainLabel.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.MainLabel.Name = "MainLabel";
            this.MainLabel.Size = new System.Drawing.Size(379, 100);
            this.MainLabel.TabIndex = 2;
            this.MainLabel.Text = "CSV settings could not be determined automatically.\r\n\r\nPlease enter settings and " +
    "try again";
            // 
            // txbSep
            // 
            this.txbSep.Location = new System.Drawing.Point(6, 106);
            this.txbSep.Margin = new System.Windows.Forms.Padding(6);
            this.txbSep.MaxLength = 2;
            this.txbSep.Name = "txbSep";
            this.txbSep.Size = new System.Drawing.Size(74, 31);
            this.txbSep.TabIndex = 0;
            // 
            // lblSeparator
            // 
            this.lblSeparator.AutoSize = true;
            this.flowLayoutPanel1.SetFlowBreak(this.lblSeparator, true);
            this.lblSeparator.Location = new System.Drawing.Point(92, 100);
            this.lblSeparator.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lblSeparator.Name = "lblSeparator";
            this.lblSeparator.Padding = new System.Windows.Forms.Padding(0, 12, 0, 0);
            this.lblSeparator.Size = new System.Drawing.Size(106, 37);
            this.lblSeparator.TabIndex = 4;
            this.lblSeparator.Text = "Separator";
            // 
            // QuoteCharLabel
            // 
            this.QuoteCharLabel.AutoSize = true;
            this.flowLayoutPanel1.SetFlowBreak(this.QuoteCharLabel, true);
            this.QuoteCharLabel.Location = new System.Drawing.Point(92, 143);
            this.QuoteCharLabel.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.QuoteCharLabel.Name = "QuoteCharLabel";
            this.QuoteCharLabel.Padding = new System.Windows.Forms.Padding(0, 12, 0, 0);
            this.QuoteCharLabel.Size = new System.Drawing.Size(233, 37);
            this.QuoteCharLabel.TabIndex = 6;
            this.QuoteCharLabel.Text = "Quote inside text prefix";
            // 
            // txbQuoteChar
            // 
            this.txbQuoteChar.Location = new System.Drawing.Point(6, 149);
            this.txbQuoteChar.Margin = new System.Windows.Forms.Padding(6);
            this.txbQuoteChar.MaxLength = 1;
            this.txbQuoteChar.Name = "txbQuoteChar";
            this.txbQuoteChar.Size = new System.Drawing.Size(74, 31);
            this.txbQuoteChar.TabIndex = 2;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.flowLayoutPanel1.Controls.Add(this.MainLabel);
            this.flowLayoutPanel1.Controls.Add(this.txbSep);
            this.flowLayoutPanel1.Controls.Add(this.lblSeparator);
            this.flowLayoutPanel1.Controls.Add(this.txbQuoteChar);
            this.flowLayoutPanel1.Controls.Add(this.QuoteCharLabel);
            this.flowLayoutPanel1.Controls.Add(this.txbCommentChar);
            this.flowLayoutPanel1.Controls.Add(this.CommentCharLabel);
            this.flowLayoutPanel1.Controls.Add(this.hasHeaderCheckbox);
            this.flowLayoutPanel1.Controls.Add(this.FixedWidthCheckbox);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(24, 23);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(6);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(520, 339);
            this.flowLayoutPanel1.TabIndex = 7;
            // 
            // txbCommentChar
            // 
            this.txbCommentChar.Location = new System.Drawing.Point(6, 192);
            this.txbCommentChar.Margin = new System.Windows.Forms.Padding(6);
            this.txbCommentChar.MaxLength = 1;
            this.txbCommentChar.Name = "txbCommentChar";
            this.txbCommentChar.Size = new System.Drawing.Size(74, 31);
            this.txbCommentChar.TabIndex = 8;
            // 
            // CommentCharLabel
            // 
            this.CommentCharLabel.AutoSize = true;
            this.flowLayoutPanel1.SetFlowBreak(this.CommentCharLabel, true);
            this.CommentCharLabel.Location = new System.Drawing.Point(92, 186);
            this.CommentCharLabel.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.CommentCharLabel.Name = "CommentCharLabel";
            this.CommentCharLabel.Padding = new System.Windows.Forms.Padding(0, 12, 0, 0);
            this.CommentCharLabel.Size = new System.Drawing.Size(246, 37);
            this.CommentCharLabel.TabIndex = 9;
            this.CommentCharLabel.Text = "Comment lines start with";
            // 
            // hasHeaderCheckbox
            // 
            this.hasHeaderCheckbox.AutoSize = true;
            this.hasHeaderCheckbox.Checked = true;
            this.hasHeaderCheckbox.CheckState = System.Windows.Forms.CheckState.Indeterminate;
            this.hasHeaderCheckbox.Location = new System.Drawing.Point(3, 232);
            this.hasHeaderCheckbox.Name = "hasHeaderCheckbox";
            this.hasHeaderCheckbox.Size = new System.Drawing.Size(408, 29);
            this.hasHeaderCheckbox.TabIndex = 7;
            this.hasHeaderCheckbox.Text = "First row is headers (yes/no/unknown)";
            this.hasHeaderCheckbox.ThreeState = true;
            this.hasHeaderCheckbox.UseVisualStyleBackColor = true;
            // 
            // FixedWidthCheckbox
            // 
            this.FixedWidthCheckbox.AutoSize = true;
            this.FixedWidthCheckbox.Location = new System.Drawing.Point(3, 267);
            this.FixedWidthCheckbox.Name = "FixedWidthCheckbox";
            this.FixedWidthCheckbox.Size = new System.Drawing.Size(203, 29);
            this.FixedWidthCheckbox.TabIndex = 10;
            this.FixedWidthCheckbox.Text = "Is fixed-width file";
            this.FixedWidthCheckbox.UseVisualStyleBackColor = true;
            // 
            // ParseSettings
            // 
            this.AcceptButton = this.btnReparse;
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(568, 441);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnReparse);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Margin = new System.Windows.Forms.Padding(6);
            this.MinimumSize = new System.Drawing.Size(374, 127);
            this.Name = "ParseSettings";
            this.ShowIcon = false;
            this.Text = "Parse Settings";
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        internal System.Windows.Forms.Button btnReparse;
        private System.Windows.Forms.Button btnCancel;
        internal System.Windows.Forms.Label MainLabel;
        internal System.Windows.Forms.TextBox txbSep;
        private System.Windows.Forms.Label lblSeparator;
        private System.Windows.Forms.Label QuoteCharLabel;
        internal System.Windows.Forms.TextBox txbQuoteChar;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        internal System.Windows.Forms.TextBox txbCommentChar;
        internal System.Windows.Forms.Label CommentCharLabel;
        internal System.Windows.Forms.CheckBox hasHeaderCheckbox;
        internal System.Windows.Forms.CheckBox FixedWidthCheckbox;
    }
}