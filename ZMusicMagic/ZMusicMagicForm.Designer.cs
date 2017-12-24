namespace ZMusicMagic
{
    partial class ZMusicMagicForm
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
            this.loadRomButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // loadRomButton
            // 
            this.loadRomButton.Location = new System.Drawing.Point(37, 70);
            this.loadRomButton.Name = "loadRomButton";
            this.loadRomButton.Size = new System.Drawing.Size(75, 23);
            this.loadRomButton.TabIndex = 0;
            this.loadRomButton.Text = "Load Rom";
            this.loadRomButton.UseVisualStyleBackColor = true;
            this.loadRomButton.Click += new System.EventHandler(this.loadRomButton_Click);
            // 
            // ZMusicMagicForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(341, 261);
            this.Controls.Add(this.loadRomButton);
            this.Name = "ZMusicMagicForm";
            this.Text = "ZMusicMagic";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button loadRomButton;
    }
}

