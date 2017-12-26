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
    public partial class ProjectForm : ToolWindowBaseForm
    {
        Rom m_currentRom;

        SongCollection m_primarySongCollection;

        public ProjectForm()
        {
            InitializeComponent();
        }

        public void SetRom(Rom currentRom)
        {
            m_currentRom = currentRom;

            m_primarySongCollection = new SongCollection();
            m_primarySongCollection.LoadFromNspc(m_currentRom.BaseNSPC);

            LoadProjectTree();
        }

        void LoadProjectTree()
        {
            this.treeView1.Nodes.Clear();

            int songNumber = 0;
            foreach(var s in m_primarySongCollection.Songs)
            {
                var node = new TreeNode();
                node.Text = $"Song {songNumber}";

                int partNumber = 0;
                foreach(var p in s.Parts)
                {
                    var partNode = new TreeNode();
                    partNode.Text = $"Part {partNumber}";

                    node.Nodes.Add(partNode);

                    partNumber++;
                }

                this.treeView1.Nodes.Add(node);

                songNumber++;
            }
        }
    }
}
