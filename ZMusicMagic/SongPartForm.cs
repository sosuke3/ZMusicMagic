using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using ZMusicMagicLibrary;

namespace ZMusicMagic
{
    public partial class SongPartForm : DockContent
    {
        public Part m_part;

        public SongPartForm(Part part)
        {
            InitializeComponent();
            AutoScaleMode = AutoScaleMode.Dpi;

            m_part = part;
            this.trackControl1.Part = part;
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
