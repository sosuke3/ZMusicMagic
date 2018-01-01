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

            m_primarySongCollection = currentRom.OverworldSongs; // new SongCollection();
            //m_primarySongCollection.LoadFromNspc(m_currentRom.BaseNSPC);
            //m_primarySongCollection.FixDurations();
            //m_primarySongCollection.DisplayName = "Startup";

            LoadProjectTree();
        }

        void LoadProjectTree()
        {
            this.projectTreeView.Nodes.Clear();

            this.projectTreeView.Nodes.Add(SongCollectionToTreeNodes(this.m_currentRom.BaseSongs));
            this.projectTreeView.Nodes.Add(SongCollectionToTreeNodes(this.m_currentRom.OverworldSongs));
            this.projectTreeView.Nodes.Add(SongCollectionToTreeNodes(this.m_currentRom.IndoorSongs));
            this.projectTreeView.Nodes.Add(SongCollectionToTreeNodes(this.m_currentRom.EndingSongs));
        }

        TreeNode SongCollectionToTreeNodes(SongCollection songCollection)
        {
            var baseNode = new TreeNode();
            baseNode.Text = songCollection.DisplayName;
            baseNode.Tag = songCollection;
            foreach (var s in songCollection.Songs)
            {
                var node = new TreeNode();
                node.Text = s.DisplayName;
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

                baseNode.Nodes.Add(node);
            }
            return baseNode;
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
                OnSongPartSelectionChanged(this, new SongPartChangedEventArgs()
                {
                    Part = selectedPart,
                    SongCollectionName = selected.Parent.Parent.Text,
                    SongTitle = selected.Parent.Text,
                    PartTitle = selected.Text
                });
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
        public string SongCollectionName { get; set; }
        public string SongTitle { get; set; }
        public string PartTitle { get; set; }
    }
}
