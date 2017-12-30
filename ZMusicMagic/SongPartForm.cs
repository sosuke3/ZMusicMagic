﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using ZMusicMagicControls;
using ZMusicMagicLibrary;

namespace ZMusicMagic
{
    public partial class SongPartForm : DockContent
    {
        public Part m_part;
        public PianoRoll[] pianoRolls = new PianoRoll[8];

        public SongPartForm(Part part)
        {
            InitializeComponent();
            AutoScaleMode = AutoScaleMode.Dpi;

            m_part = part;
            //this.trackControl1.Part = part;
            int i = 0;
            this.pianoRolls[i++] = this.pianoRoll1;
            this.pianoRolls[i++] = this.pianoRoll2;
            this.pianoRolls[i++] = this.pianoRoll3;
            this.pianoRolls[i++] = this.pianoRoll4;
            this.pianoRolls[i++] = this.pianoRoll5;
            this.pianoRolls[i++] = this.pianoRoll6;
            this.pianoRolls[i++] = this.pianoRoll7;
            this.pianoRolls[i++] = this.pianoRoll8;


            i = 0;
            foreach(var channel in part.Channels)
            {
                this.pianoRolls[i++].Channel = channel;
            }

            this.AutoScrollMinSize = new Size(1, 1);
        }



        private string m_fileName = string.Empty;
        public string FileName
        {
            get { return m_fileName; }
            set
            {
                if (value != string.Empty)
                {
                    //Stream s = new FileStream(value, FileMode.Open);

                    //FileInfo efInfo = new FileInfo(value);

                    //string fext = efInfo.Extension.ToUpper();

                    //if (fext.Equals(".RTF"))
                    //    richTextBox1.LoadFile(s, RichTextBoxStreamType.RichText);
                    //else
                    //    richTextBox1.LoadFile(s, RichTextBoxStreamType.PlainText);
                    //s.Close();
                }

                m_fileName = value;
                this.ToolTipText = value;
            }
        }

        protected override string GetPersistString()
        {
            // Add extra information into the persist string for this document
            // so that it is available when deserialized.
            return GetType().ToString() + "," + FileName + "," + Text;
        }

    }
}
