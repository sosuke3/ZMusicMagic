using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZMusicMagicLibrary;

namespace ZMusicMagic
{
    public partial class ZMusicMagicForm : Form
    {
        public ZMusicMagicForm()
        {
            InitializeComponent();
        }

        private void loadRomButton_Click(object sender, EventArgs e)
        {
            LoadRom();
        }

        private void LoadRom()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "*.sfc|*.sfc|*.*|*.*";

            if(ofd.ShowDialog() == DialogResult.OK)
            {
                Rom rom = new Rom();
                rom.LoadRom(ofd.FileName);
            }
        }
    }
}
