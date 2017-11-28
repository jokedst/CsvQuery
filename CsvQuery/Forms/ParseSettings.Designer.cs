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
            this.label2 = new System.Windows.Forms.Label();
            this.txbQuoteChar = new System.Windows.Forms.TextBox();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnReparse
            // 
            this.btnReparse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnReparse.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnReparse.Location = new System.Drawing.Point(13, 145);
            this.btnReparse.Name = "btnReparse";
            this.btnReparse.Size = new System.Drawing.Size(75, 23);
            this.btnReparse.TabIndex = 1;
            this.btnReparse.Text = "&Try again";
            this.btnReparse.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(197, 145);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "&Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // MainLabel
            // 
            this.MainLabel.AutoSize = true;
            this.flowLayoutPanel1.SetFlowBreak(this.MainLabel, true);
            this.MainLabel.Location = new System.Drawing.Point(3, 0);
            this.MainLabel.Name = "MainLabel";
            this.MainLabel.Size = new System.Drawing.Size(251, 39);
            this.MainLabel.TabIndex = 2;
            this.MainLabel.Text = "CSV settings could not be determined automatically.\r\n\r\nPlease enter settings and " +
    "try again";
            // 
            // txbSep
            // 
            this.txbSep.Location = new System.Drawing.Point(3, 42);
            this.txbSep.MaxLength = 2;
            this.txbSep.Name = "txbSep";
            this.txbSep.Size = new System.Drawing.Size(39, 20);
            this.txbSep.TabIndex = 0;
            // 
            // lblSeparator
            // 
            this.lblSeparator.AutoSize = true;
            this.flowLayoutPanel1.SetFlowBreak(this.lblSeparator, true);
            this.lblSeparator.Location = new System.Drawing.Point(48, 39);
            this.lblSeparator.Name = "lblSeparator";
            this.lblSeparator.Padding = new System.Windows.Forms.Padding(0, 6, 0, 0);
            this.lblSeparator.Size = new System.Drawing.Size(53, 19);
            this.lblSeparator.TabIndex = 4;
            this.lblSeparator.Text = "Separator";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.flowLayoutPanel1.SetFlowBreak(this.label2, true);
            this.label2.Location = new System.Drawing.Point(48, 65);
            this.label2.Name = "label2";
            this.label2.Padding = new System.Windows.Forms.Padding(0, 6, 0, 0);
            this.label2.Size = new System.Drawing.Size(82, 19);
            this.label2.TabIndex = 6;
            this.label2.Text = "Text quote char";
            // 
            // txbQuoteChar
            // 
            this.txbQuoteChar.Location = new System.Drawing.Point(3, 68);
            this.txbQuoteChar.MaxLength = 1;
            this.txbQuoteChar.Name = "txbQuoteChar";
            this.txbQuoteChar.Size = new System.Drawing.Size(39, 20);
            this.txbQuoteChar.TabIndex = 2;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.MainLabel);
            this.flowLayoutPanel1.Controls.Add(this.txbSep);
            this.flowLayoutPanel1.Controls.Add(this.lblSeparator);
            this.flowLayoutPanel1.Controls.Add(this.txbQuoteChar);
            this.flowLayoutPanel1.Controls.Add(this.label2);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(12, 12);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(260, 127);
            this.flowLayoutPanel1.TabIndex = 7;
            // 
            // ParseSettings
            // 
            this.AcceptButton = this.btnReparse;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(284, 180);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnReparse);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MinimumSize = new System.Drawing.Size(200, 100);
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
        private System.Windows.Forms.Label label2;
        internal System.Windows.Forms.TextBox txbQuoteChar;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
    }
}