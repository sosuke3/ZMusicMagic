namespace ZMusicMagic
{
    partial class SongPartForm
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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.trackControl1 = new ZMusicMagicControls.TrackControl();
            this.pianoRoll1 = new ZMusicMagicControls.PianoRoll();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
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
            this.splitContainer1.Panel1.Controls.Add(this.trackControl1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.pianoRoll1);
            this.splitContainer1.Size = new System.Drawing.Size(749, 529);
            this.splitContainer1.SplitterDistance = 247;
            this.splitContainer1.TabIndex = 1;
            // 
            // trackControl1
            // 
            this.trackControl1.AutoScroll = true;
            this.trackControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.trackControl1.Location = new System.Drawing.Point(0, 0);
            this.trackControl1.Name = "trackControl1";
            this.trackControl1.Part = null;
            this.trackControl1.Size = new System.Drawing.Size(749, 247);
            this.trackControl1.TabIndex = 1;
            this.trackControl1.Text = "trackControl1";
            // 
            // pianoRoll1
            // 
            this.pianoRoll1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pianoRoll1.Location = new System.Drawing.Point(0, 0);
            this.pianoRoll1.Name = "pianoRoll1";
            this.pianoRoll1.Part = null;
            this.pianoRoll1.Size = new System.Drawing.Size(749, 278);
            this.pianoRoll1.TabIndex = 0;
            this.pianoRoll1.Text = "pianoRoll1";
            // 
            // SongPartForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(749, 529);
            this.Controls.Add(this.splitContainer1);
            this.Name = "SongPartForm";
            this.Text = "SongPartForm";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private ZMusicMagicControls.TrackControl trackControl1;
        private ZMusicMagicControls.PianoRoll pianoRoll1;
    }
}