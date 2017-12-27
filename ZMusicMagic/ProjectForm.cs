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

        public event EventHandler<SongPartChangedEventArgs> OnSongPartSelectionChanged;

        public ProjectForm()
        {
            InitializeComponent();

            CloseButton = false;
            CloseButtonVisible = false;
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
            this.projectTreeView.Nodes.Clear();

            int songNumber = 0;
            foreach (var s in m_primarySongCollection.Songs)
            {
                var node = new TreeNode();
                node.Text = $"Song {songNumber}";
                node.Tag = s;

                int partNumber = 0;
                foreach (var p in s.Parts)
                {
                    var partNode = new TreeNode();
                    partNode.Text = $"Part {partNumber}";

                    partNode.Tag = p;

                    node.Nodes.Add(partNode);

                    partNumber++;
                }

                this.projectTreeView.Nodes.Add(node);

                songNumber++;
            }
        }

        private void projectTreeView_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            OpenSongPart(e);
        }

        private void OpenSongPart(TreeNodeMouseClickEventArgs e)
        {
            TreeNode selected = e.Node;

            Part selectedPart = selected.Tag as Part;
            if (selectedPart != null && OnSongPartSelectionChanged != null)
            {
                OnSongPartSelectionChanged(this, new SongPartChangedEventArgs() { Part = selectedPart, SongTitle = selected.Parent.Text, PartTitle = selected.Text });
            }
        }

        private void projectTreeView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            OpenSongPart(e);
        }
    }

    public class SongPartChangedEventArgs : EventArgs
    {
        public Part Part { get; set; }
        public string SongTitle { get; set; }
        public string PartTitle { get; set; }
    }
}
