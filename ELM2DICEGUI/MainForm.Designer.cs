namespace ELM2DICEGUI
{
    partial class MainForm
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
            this.label2 = new System.Windows.Forms.Label();
            this.portCombo = new System.Windows.Forms.ComboBox();
            this.progBar = new System.Windows.Forms.ProgressBar();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.actionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fullDumpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.readEEPROMToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.writeEEPROMToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.restoreEEPROMToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveEEPROMToFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loggingMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.startToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.stopToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 30);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(37, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "PORT";
            // 
            // portCombo
            // 
            this.portCombo.Location = new System.Drawing.Point(55, 27);
            this.portCombo.Name = "portCombo";
            this.portCombo.Size = new System.Drawing.Size(254, 21);
            this.portCombo.TabIndex = 1;
            // 
            // progBar
            // 
            this.progBar.Location = new System.Drawing.Point(12, 90);
            this.progBar.Name = "progBar";
            this.progBar.Size = new System.Drawing.Size(548, 22);
            this.progBar.TabIndex = 3;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.actionsToolStripMenuItem,
            this.loggingMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(572, 24);
            this.menuStrip1.TabIndex = 4;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // actionsToolStripMenuItem
            // 
            this.actionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fullDumpToolStripMenuItem,
            this.readEEPROMToolStripMenuItem,
            this.writeEEPROMToolStripMenuItem,
            this.restoreEEPROMToolStripMenuItem,
            this.saveEEPROMToFileToolStripMenuItem});
            this.actionsToolStripMenuItem.Name = "actionsToolStripMenuItem";
            this.actionsToolStripMenuItem.Size = new System.Drawing.Size(54, 20);
            this.actionsToolStripMenuItem.Text = "Actions";
            // 
            // fullDumpToolStripMenuItem
            // 
            this.fullDumpToolStripMenuItem.Name = "fullDumpToolStripMenuItem";
            this.fullDumpToolStripMenuItem.Size = new System.Drawing.Size(195, 22);
            this.fullDumpToolStripMenuItem.Text = "Full dump";
            this.fullDumpToolStripMenuItem.Click += new System.EventHandler(this.fullDumpToolStripMenuItem_Click);
            // 
            // readEEPROMToolStripMenuItem
            // 
            this.readEEPROMToolStripMenuItem.Name = "readEEPROMToolStripMenuItem";
            this.readEEPROMToolStripMenuItem.Size = new System.Drawing.Size(195, 22);
            this.readEEPROMToolStripMenuItem.Text = "Read EEPROM from DICE";
            this.readEEPROMToolStripMenuItem.Click += new System.EventHandler(this.downloadEEPROMToolStripMenuItem_Click);
            // 
            // writeEEPROMToolStripMenuItem
            // 
            this.writeEEPROMToolStripMenuItem.Name = "writeEEPROMToolStripMenuItem";
            this.writeEEPROMToolStripMenuItem.Size = new System.Drawing.Size(195, 22);
            this.writeEEPROMToolStripMenuItem.Text = "Write EEPROM to DICE";
            this.writeEEPROMToolStripMenuItem.Click += new System.EventHandler(this.uploadEEPROMToolStripMenuItem_Click);
            // 
            // restoreEEPROMToolStripMenuItem
            // 
            this.restoreEEPROMToolStripMenuItem.Name = "restoreEEPROMToolStripMenuItem";
            this.restoreEEPROMToolStripMenuItem.Size = new System.Drawing.Size(195, 22);
            this.restoreEEPROMToolStripMenuItem.Text = "Load EEPROM from file";
            this.restoreEEPROMToolStripMenuItem.Click += new System.EventHandler(this.readEEPROMFromFileToolStripMenuItem_Click);
            // 
            // saveEEPROMToFileToolStripMenuItem
            // 
            this.saveEEPROMToFileToolStripMenuItem.Name = "saveEEPROMToFileToolStripMenuItem";
            this.saveEEPROMToFileToolStripMenuItem.Size = new System.Drawing.Size(195, 22);
            this.saveEEPROMToFileToolStripMenuItem.Text = "Save EEPROM to file";
            this.saveEEPROMToFileToolStripMenuItem.Click += new System.EventHandler(this.saveEEPROMToFileToolStripMenuItem_Click);
            // 
            // loggingMenuItem
            // 
            this.loggingMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.startToolStripMenuItem,
            this.stopToolStripMenuItem});
            this.loggingMenuItem.Name = "loggingMenuItem";
            this.loggingMenuItem.Size = new System.Drawing.Size(56, 20);
            this.loggingMenuItem.Text = "Logging";
            // 
            // startToolStripMenuItem
            // 
            this.startToolStripMenuItem.Name = "startToolStripMenuItem";
            this.startToolStripMenuItem.Size = new System.Drawing.Size(98, 22);
            this.startToolStripMenuItem.Text = "Start";
            this.startToolStripMenuItem.Click += new System.EventHandler(this.startToolStripMenuItem_Click);
            // 
            // stopToolStripMenuItem
            // 
            this.stopToolStripMenuItem.Name = "stopToolStripMenuItem";
            this.stopToolStripMenuItem.Size = new System.Drawing.Size(98, 22);
            this.stopToolStripMenuItem.Text = "Stop";
            this.stopToolStripMenuItem.Click += new System.EventHandler(this.stopToolStripMenuItem_Click);
            // 
            // MainForm
            // 
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.ClientSize = new System.Drawing.Size(572, 124);
            this.Controls.Add(this.progBar);
            this.Controls.Add(this.portCombo);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.menuStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MainMenuStrip = this.menuStrip1;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.Text = "ELM2DICE";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        //private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox portCombo;
        private System.Windows.Forms.ProgressBar progBar;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem actionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fullDumpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem readEEPROMToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem writeEEPROMToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem restoreEEPROMToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveEEPROMToFileToolStripMenuItem;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.Windows.Forms.ToolStripMenuItem loggingMenuItem;
        private System.Windows.Forms.ToolStripMenuItem startToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem stopToolStripMenuItem;
    }
}

