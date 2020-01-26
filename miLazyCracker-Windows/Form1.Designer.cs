namespace miLazyCracker_Windows
{
    partial class frmMiLazyCracker
    {
        /// <summary>
        /// Variable nécessaire au concepteur.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Nettoyage des ressources utilisées.
        /// </summary>
        /// <param name="disposing">true si les ressources managées doivent être supprimées ; sinon, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Code généré par le Concepteur Windows Form

        /// <summary>
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
            this.rtbOutput = new System.Windows.Forms.RichTextBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.btnHelpAndInfo = new System.Windows.Forms.Button();
            this.btnTools = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.btnEditAnalyseDumpFile = new System.Windows.Forms.Button();
            this.btnWriteTag = new System.Windows.Forms.Button();
            this.btnreadTag = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // rtbOutput
            // 
            this.rtbOutput.BackColor = System.Drawing.Color.Black;
            this.rtbOutput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtbOutput.ForeColor = System.Drawing.Color.Lime;
            this.rtbOutput.Location = new System.Drawing.Point(0, 0);
            this.rtbOutput.Name = "rtbOutput";
            this.rtbOutput.Size = new System.Drawing.Size(641, 424);
            this.rtbOutput.TabIndex = 1;
            this.rtbOutput.Text = "";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.btnHelpAndInfo);
            this.splitContainer1.Panel1.Controls.Add(this.btnTools);
            this.splitContainer1.Panel1.Controls.Add(this.button1);
            this.splitContainer1.Panel1.Controls.Add(this.btnEditAnalyseDumpFile);
            this.splitContainer1.Panel1.Controls.Add(this.btnWriteTag);
            this.splitContainer1.Panel1.Controls.Add(this.btnreadTag);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.rtbOutput);
            this.splitContainer1.Size = new System.Drawing.Size(641, 734);
            this.splitContainer1.SplitterDistance = 303;
            this.splitContainer1.SplitterWidth = 7;
            this.splitContainer1.TabIndex = 4;
            // 
            // btnHelpAndInfo
            // 
            this.btnHelpAndInfo.BackgroundImage = global::miLazyCracker_Windows.Properties.Resources.help_and_info;
            this.btnHelpAndInfo.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnHelpAndInfo.Location = new System.Drawing.Point(428, 150);
            this.btnHelpAndInfo.Name = "btnHelpAndInfo";
            this.btnHelpAndInfo.Size = new System.Drawing.Size(209, 141);
            this.btnHelpAndInfo.TabIndex = 8;
            this.btnHelpAndInfo.UseVisualStyleBackColor = true;
            // 
            // btnTools
            // 
            this.btnTools.BackgroundImage = global::miLazyCracker_Windows.Properties.Resources.tools;
            this.btnTools.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnTools.Location = new System.Drawing.Point(428, 3);
            this.btnTools.Name = "btnTools";
            this.btnTools.Size = new System.Drawing.Size(209, 141);
            this.btnTools.TabIndex = 7;
            this.btnTools.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.BackgroundImage = global::miLazyCracker_Windows.Properties.Resources.edit_add_keyfile;
            this.button1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.button1.Location = new System.Drawing.Point(213, 150);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(209, 141);
            this.button1.TabIndex = 6;
            this.button1.UseVisualStyleBackColor = true;
            // 
            // btnEditAnalyseDumpFile
            // 
            this.btnEditAnalyseDumpFile.BackgroundImage = global::miLazyCracker_Windows.Properties.Resources.edit_analyse_dump_file;
            this.btnEditAnalyseDumpFile.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnEditAnalyseDumpFile.Location = new System.Drawing.Point(3, 150);
            this.btnEditAnalyseDumpFile.Name = "btnEditAnalyseDumpFile";
            this.btnEditAnalyseDumpFile.Size = new System.Drawing.Size(204, 141);
            this.btnEditAnalyseDumpFile.TabIndex = 5;
            this.btnEditAnalyseDumpFile.UseVisualStyleBackColor = true;
            // 
            // btnWriteTag
            // 
            this.btnWriteTag.BackgroundImage = global::miLazyCracker_Windows.Properties.Resources.write_tag;
            this.btnWriteTag.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnWriteTag.Location = new System.Drawing.Point(213, 3);
            this.btnWriteTag.Name = "btnWriteTag";
            this.btnWriteTag.Size = new System.Drawing.Size(209, 141);
            this.btnWriteTag.TabIndex = 4;
            this.btnWriteTag.UseVisualStyleBackColor = true;
            this.btnWriteTag.Click += new System.EventHandler(this.btnDump_Click);
            // 
            // btnreadTag
            // 
            this.btnreadTag.BackgroundImage = global::miLazyCracker_Windows.Properties.Resources.read_tag;
            this.btnreadTag.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnreadTag.Location = new System.Drawing.Point(0, 3);
            this.btnreadTag.Name = "btnreadTag";
            this.btnreadTag.Size = new System.Drawing.Size(207, 141);
            this.btnreadTag.TabIndex = 0;
            this.btnreadTag.UseVisualStyleBackColor = true;
            this.btnreadTag.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // frmMiLazyCracker
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(641, 734);
            this.Controls.Add(this.splitContainer1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "frmMiLazyCracker";
            this.Text = "MiLazyCracker Windows";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnreadTag;
        private System.Windows.Forms.RichTextBox rtbOutput;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Button btnWriteTag;
        private System.Windows.Forms.Button btnEditAnalyseDumpFile;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button btnTools;
        private System.Windows.Forms.Button btnHelpAndInfo;
    }
}

