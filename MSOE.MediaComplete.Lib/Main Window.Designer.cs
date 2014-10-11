namespace MSOE.MediaComplete.Lib
{
    partial class Form1
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
            this.menuStrip2 = new System.Windows.Forms.MenuStrip();
            this.menuToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.playlistsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.SongProgress = new System.Windows.Forms.ProgressBar();
            this.Play = new System.Windows.Forms.Button();
            this.Skip = new System.Windows.Forms.Button();
            this.Previous = new System.Windows.Forms.Button();
            this.Shuffle = new System.Windows.Forms.CheckBox();
            this.Repeat = new System.Windows.Forms.CheckBox();
            this.menuStrip2.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip2
            // 
            this.menuStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuToolStripMenuItem,
            this.fileToolStripMenuItem,
            this.playlistsToolStripMenuItem});
            this.menuStrip2.Location = new System.Drawing.Point(0, 0);
            this.menuStrip2.Name = "menuStrip2";
            this.menuStrip2.Size = new System.Drawing.Size(1094, 24);
            this.menuStrip2.TabIndex = 1;
            this.menuStrip2.Text = "menuStrip2";
            // 
            // menuToolStripMenuItem
            // 
            this.menuToolStripMenuItem.Name = "menuToolStripMenuItem";
            this.menuToolStripMenuItem.Size = new System.Drawing.Size(50, 20);
            this.menuToolStripMenuItem.Text = "Menu";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // playlistsToolStripMenuItem
            // 
            this.playlistsToolStripMenuItem.Name = "playlistsToolStripMenuItem";
            this.playlistsToolStripMenuItem.Size = new System.Drawing.Size(61, 20);
            this.playlistsToolStripMenuItem.Text = "Playlists";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(827, 0);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(266, 20);
            this.textBox1.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(780, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Search";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // treeView1
            // 
            this.treeView1.Location = new System.Drawing.Point(0, 27);
            this.treeView1.Name = "treeView1";
            this.treeView1.Size = new System.Drawing.Size(1093, 586);
            this.treeView1.TabIndex = 4;
            // 
            // SongProgress
            // 
            this.SongProgress.Location = new System.Drawing.Point(0, 619);
            this.SongProgress.Name = "SongProgress";
            this.SongProgress.Size = new System.Drawing.Size(1094, 10);
            this.SongProgress.TabIndex = 5;
            // 
            // Play
            // 
            this.Play.Location = new System.Drawing.Point(509, 635);
            this.Play.Name = "Play";
            this.Play.Size = new System.Drawing.Size(65, 52);
            this.Play.TabIndex = 6;
            this.Play.Text = "Play";
            this.Play.UseVisualStyleBackColor = true;
            this.Play.Click += new System.EventHandler(this.button1_Click);
            // 
            // Skip
            // 
            this.Skip.Location = new System.Drawing.Point(580, 635);
            this.Skip.Name = "Skip";
            this.Skip.Size = new System.Drawing.Size(73, 52);
            this.Skip.TabIndex = 6;
            this.Skip.Text = "Skip";
            this.Skip.UseVisualStyleBackColor = true;
            // 
            // Previous
            // 
            this.Previous.Location = new System.Drawing.Point(437, 635);
            this.Previous.Name = "Previous";
            this.Previous.Size = new System.Drawing.Size(66, 52);
            this.Previous.TabIndex = 6;
            this.Previous.Text = "Previous";
            this.Previous.UseVisualStyleBackColor = true;
            // 
            // Shuffle
            // 
            this.Shuffle.AutoSize = true;
            this.Shuffle.Location = new System.Drawing.Point(276, 635);
            this.Shuffle.Name = "Shuffle";
            this.Shuffle.Size = new System.Drawing.Size(115, 17);
            this.Shuffle.TabIndex = 7;
            this.Shuffle.Text = "ShufflePlaceholder";
            this.Shuffle.UseVisualStyleBackColor = true;
            this.Shuffle.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // Repeat
            // 
            this.Repeat.AutoSize = true;
            this.Repeat.Location = new System.Drawing.Point(276, 654);
            this.Repeat.Name = "Repeat";
            this.Repeat.Size = new System.Drawing.Size(123, 17);
            this.Repeat.TabIndex = 7;
            this.Repeat.Text = "RepeatePlaceholder";
            this.Repeat.UseVisualStyleBackColor = true;
            this.Repeat.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1094, 699);
            this.Controls.Add(this.Repeat);
            this.Controls.Add(this.Shuffle);
            this.Controls.Add(this.Previous);
            this.Controls.Add(this.Skip);
            this.Controls.Add(this.Play);
            this.Controls.Add(this.SongProgress);
            this.Controls.Add(this.treeView1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.menuStrip2);
            this.Name = "Form1";
            this.Text = "Media Complete";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.menuStrip2.ResumeLayout(false);
            this.menuStrip2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip2;
        private System.Windows.Forms.ToolStripMenuItem menuToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem playlistsToolStripMenuItem;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TreeView treeView1;
        private System.Windows.Forms.ProgressBar SongProgress;
        private System.Windows.Forms.Button Play;
        private System.Windows.Forms.Button Skip;
        private System.Windows.Forms.Button Previous;
        private System.Windows.Forms.CheckBox Shuffle;
        private System.Windows.Forms.CheckBox Repeat;

    }
}