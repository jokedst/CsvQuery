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
            this.label1 = new System.Windows.Forms.Label();
            this.txbSep = new System.Windows.Forms.TextBox();
            this.lblSeparator = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txbQuoteChar = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // btnReparse
            // 
            this.btnReparse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnReparse.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnReparse.Location = new System.Drawing.Point(13, 145);
            this.btnReparse.Name = "btnReparse";
            this.btnReparse.Size = new System.Drawing.Size(75, 23);
            this.btnReparse.TabIndex = 2;
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
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "&Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(251, 39);
            this.label1.TabIndex = 2;
            this.label1.Text = "CSV settings could not be determined automatically.\r\n\r\nPlease enter settings and " +
    "try again";
            // 
            // txbSep
            // 
            this.txbSep.Location = new System.Drawing.Point(13, 71);
            this.txbSep.MaxLength = 2;
            this.txbSep.Name = "txbSep";
            this.txbSep.Size = new System.Drawing.Size(39, 20);
            this.txbSep.TabIndex = 0;
            // 
            // lblSeparator
            // 
            this.lblSeparator.AutoSize = true;
            this.lblSeparator.Location = new System.Drawing.Point(58, 74);
            this.lblSeparator.Name = "lblSeparator";
            this.lblSeparator.Size = new System.Drawing.Size(53, 13);
            this.lblSeparator.TabIndex = 4;
            this.lblSeparator.Text = "Separator";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(58, 100);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(82, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Text quote char";
            // 
            // txbQuoteChar
            // 
            this.txbQuoteChar.Location = new System.Drawing.Point(13, 97);
            this.txbQuoteChar.MaxLength = 1;
            this.txbQuoteChar.Name = "txbQuoteChar";
            this.txbQuoteChar.Size = new System.Drawing.Size(39, 20);
            this.txbQuoteChar.TabIndex = 2;
            // 
            // ParseSettings
            // 
            this.AcceptButton = this.btnReparse;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(284, 180);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txbQuoteChar);
            this.Controls.Add(this.lblSeparator);
            this.Controls.Add(this.txbSep);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnReparse);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "ParseSettings";
            this.ShowIcon = false;
            this.Text = "ParseSettings";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnReparse;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txbSep;
        private System.Windows.Forms.Label lblSeparator;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txbQuoteChar;
    }
}