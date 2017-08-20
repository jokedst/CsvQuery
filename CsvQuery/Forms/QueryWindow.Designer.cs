namespace CsvQuery.Forms
{
    using System.Windows.Forms;

    partial class QueryWindow
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
            this.components = new System.ComponentModel.Container();
            this.txbQuery = new System.Windows.Forms.TextBox();
            this.dataGrid = new System.Windows.Forms.DataGridView();
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyWithHeadersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.createNewCSVToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.btnAnalyze = new System.Windows.Forms.Button();
            this.btnExec = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dataGrid)).BeginInit();
            this.contextMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // txbQuery
            // 
            this.txbQuery.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txbQuery.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.txbQuery.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this.txbQuery.Location = new System.Drawing.Point(4, 0);
            this.txbQuery.Margin = new System.Windows.Forms.Padding(6);
            this.txbQuery.Name = "txbQuery";
            this.txbQuery.Size = new System.Drawing.Size(216, 31);
            this.txbQuery.TabIndex = 0;
            this.txbQuery.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txbQuery_KeyDown);
            // 
            // dataGrid
            // 
            this.dataGrid.AllowUserToAddRows = false;
            this.dataGrid.AllowUserToDeleteRows = false;
            this.dataGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGrid.ContextMenuStrip = this.contextMenuStrip;
            this.dataGrid.Location = new System.Drawing.Point(26, 52);
            this.dataGrid.Margin = new System.Windows.Forms.Padding(6);
            this.dataGrid.Name = "dataGrid";
            this.dataGrid.ReadOnly = true;
            this.dataGrid.RowHeadersVisible = false;
            this.dataGrid.Size = new System.Drawing.Size(518, 429);
            this.dataGrid.TabIndex = 1;
            // 
            // contextMenuStrip
            // 
            this.contextMenuStrip.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copyToolStripMenuItem,
            this.copyWithHeadersToolStripMenuItem,
            this.toolStripMenuItem1,
            this.createNewCSVToolStripMenuItem});
            this.contextMenuStrip.Name = "contextMenuStrip";
            this.contextMenuStrip.Size = new System.Drawing.Size(289, 118);
            // 
            // copyToolStripMenuItem
            // 
            this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            this.copyToolStripMenuItem.Size = new System.Drawing.Size(288, 36);
            this.copyToolStripMenuItem.Text = "&Copy";
            // 
            // copyWithHeadersToolStripMenuItem
            // 
            this.copyWithHeadersToolStripMenuItem.Name = "copyWithHeadersToolStripMenuItem";
            this.copyWithHeadersToolStripMenuItem.Size = new System.Drawing.Size(288, 36);
            this.copyWithHeadersToolStripMenuItem.Text = "Copy with &headers";
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(285, 6);
            // 
            // createNewCSVToolStripMenuItem
            // 
            this.createNewCSVToolStripMenuItem.Name = "createNewCSVToolStripMenuItem";
            this.createNewCSVToolStripMenuItem.Size = new System.Drawing.Size(288, 36);
            this.createNewCSVToolStripMenuItem.Text = "Create &new CSV...";
            this.createNewCSVToolStripMenuItem.Click += new System.EventHandler(this.createNewCSVToolStripMenuItem_Click);
            // 
            // btnAnalyze
            // 
            this.btnAnalyze.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAnalyze.Location = new System.Drawing.Point(394, 0);
            this.btnAnalyze.Margin = new System.Windows.Forms.Padding(6);
            this.btnAnalyze.Name = "btnAnalyze";
            this.btnAnalyze.Size = new System.Drawing.Size(150, 44);
            this.btnAnalyze.TabIndex = 2;
            this.btnAnalyze.Text = "&Read File";
            this.btnAnalyze.UseVisualStyleBackColor = true;
            this.btnAnalyze.Click += new System.EventHandler(this.btnAnalyze_Click);
            // 
            // btnExec
            // 
            this.btnExec.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExec.Location = new System.Drawing.Point(232, 0);
            this.btnExec.Margin = new System.Windows.Forms.Padding(6);
            this.btnExec.Name = "btnExec";
            this.btnExec.Size = new System.Drawing.Size(150, 44);
            this.btnExec.TabIndex = 3;
            this.btnExec.Text = "&Execute";
            this.btnExec.UseVisualStyleBackColor = true;
            this.btnExec.Click += new System.EventHandler(this.btnExec_Click);
            // 
            // QueryWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(568, 504);
            this.Controls.Add(this.btnExec);
            this.Controls.Add(this.btnAnalyze);
            this.Controls.Add(this.dataGrid);
            this.Controls.Add(this.txbQuery);
            this.Margin = new System.Windows.Forms.Padding(6);
            this.Name = "QueryWindow";
            this.Text = "QueryWindow";
            ((System.ComponentModel.ISupportInitialize)(this.dataGrid)).EndInit();
            this.contextMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txbQuery;
        private System.Windows.Forms.DataGridView dataGrid;
        private System.Windows.Forms.Button btnAnalyze;
        private System.Windows.Forms.Button btnExec;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyWithHeadersToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem createNewCSVToolStripMenuItem;
    }
}