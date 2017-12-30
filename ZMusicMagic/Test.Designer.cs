namespace ZMusicMagic
{
    partial class Test
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.pianoRoll1 = new ZMusicMagicControls.PianoRoll();
            this.pianoRoll2 = new ZMusicMagicControls.PianoRoll();
            this.pianoRoll3 = new ZMusicMagicControls.PianoRoll();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.pianoRoll1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.pianoRoll2, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.pianoRoll3, 0, 2);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 8;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 200F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 200F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 200F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 200F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 200F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 200F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 200F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 200F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(717, 1600);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // pianoRoll1
            // 
            this.pianoRoll1.Channel = null;
            this.pianoRoll1.ChannelNumber = 0;
            this.pianoRoll1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pianoRoll1.Location = new System.Drawing.Point(3, 3);
            this.pianoRoll1.Name = "pianoRoll1";
            this.pianoRoll1.Size = new System.Drawing.Size(711, 194);
            this.pianoRoll1.TabIndex = 0;
            // 
            // pianoRoll2
            // 
            this.pianoRoll2.Channel = null;
            this.pianoRoll2.ChannelNumber = 0;
            this.pianoRoll2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pianoRoll2.Location = new System.Drawing.Point(3, 203);
            this.pianoRoll2.Name = "pianoRoll2";
            this.pianoRoll2.Size = new System.Drawing.Size(711, 194);
            this.pianoRoll2.TabIndex = 1;
            // 
            // pianoRoll3
            // 
            this.pianoRoll3.Channel = null;
            this.pianoRoll3.ChannelNumber = 0;
            this.pianoRoll3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pianoRoll3.Location = new System.Drawing.Point(3, 403);
            this.pianoRoll3.Name = "pianoRoll3";
            this.pianoRoll3.Size = new System.Drawing.Size(711, 194);
            this.pianoRoll3.TabIndex = 2;
            // 
            // Test
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(734, 516);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "Test";
            this.Text = "Test";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private ZMusicMagicControls.PianoRoll pianoRoll1;
        private ZMusicMagicControls.PianoRoll pianoRoll2;
        private ZMusicMagicControls.PianoRoll pianoRoll3;
    }
}