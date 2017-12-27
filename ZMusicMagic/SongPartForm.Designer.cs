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
            this.trackControl1 = new ZMusicMagicControls.TrackControl();
            this.SuspendLayout();
            // 
            // trackControl1
            // 
            this.trackControl1.AutoScroll = true;
            this.trackControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.trackControl1.Location = new System.Drawing.Point(0, 0);
            this.trackControl1.Name = "trackControl1";
            this.trackControl1.Part = null;
            this.trackControl1.Size = new System.Drawing.Size(284, 261);
            this.trackControl1.TabIndex = 0;
            this.trackControl1.Text = "trackControl1";
            // 
            // SongPartForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Controls.Add(this.trackControl1);
            this.Name = "SongPartForm";
            this.Text = "SongPartForm";
            this.ResumeLayout(false);

        }

        #endregion

        private ZMusicMagicControls.TrackControl trackControl1;
    }
}