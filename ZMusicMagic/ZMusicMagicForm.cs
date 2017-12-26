using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using ZMusicMagicLibrary;

namespace ZMusicMagic
{
    public partial class ZMusicMagicForm : Form
    {
        private DockPanel dockpanel;
        private VisualStudioToolStripExtender vsToolStripExtender1;
        private readonly ToolStripRenderer _toolStripProfessionalRenderer = new ToolStripProfessionalRenderer();
        private bool m_bSaveLayout = true;
        private DeserializeDockContent m_deserializeDockContent;

        private bool _showSplash;
        private SplashScreen _splashScreen;

        public ZMusicMagicForm()
        {
            InitializeComponent();
            AutoScaleMode = AutoScaleMode.Dpi;

            //SetSplashScreen();

            this.vsToolStripExtender1 = new VisualStudioToolStripExtender(this.components);
            this.vsToolStripExtender1.DefaultRenderer = _toolStripProfessionalRenderer;

            var theme = new VS2015LightTheme();

            this.dockpanel = new WeifenLuo.WinFormsUI.Docking.DockPanel();
            this.dockpanel.Dock = DockStyle.Fill;
            this.dockpanel.DocumentStyle = DocumentStyle.DockingMdi;
            this.dockpanel.Theme = theme;
            this.EnableVSRenderer(VisualStudioToolStripExtender.VsVersion.Vs2015, theme);
            this.Controls.Add(this.dockpanel);

            var part = new SongPartForm();
            part.Text = "something";
            part.Show(this.dockpanel);

            m_deserializeDockContent = new DeserializeDockContent(GetContentFromPersistString);
        }

        private IDockContent GetContentFromPersistString(string persistString)
        {
            //if (persistString == typeof(DummySolutionExplorer).ToString())
            //    return m_solutionExplorer;
            //else if (persistString == typeof(DummyPropertyWindow).ToString())
            //    return m_propertyWindow;
            //else if (persistString == typeof(DummyToolbox).ToString())
            //    return m_toolbox;
            //else if (persistString == typeof(DummyOutputWindow).ToString())
            //    return m_outputWindow;
            //else if (persistString == typeof(DummyTaskList).ToString())
            //    return m_taskList;
            //else
            //{
                // DummyDoc overrides GetPersistString to add extra information into persistString.
                // Any DockContent may override this value to add any needed information for deserialization.

                string[] parsedStrings = persistString.Split(new char[] { ',' });
                if (parsedStrings.Length != 3)
                    return null;

                if (parsedStrings[0] != typeof(SongPartForm).ToString())
                    return null;

                SongPartForm songPart = new SongPartForm();
                if (parsedStrings[1] != string.Empty)
                    songPart.FileName = parsedStrings[1];
                if (parsedStrings[2] != string.Empty)
                    songPart.Text = parsedStrings[2];

                return songPart;
            //}
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

        #region Splash Screen
        private void SetSplashScreen()
        {

            _showSplash = true;
            _splashScreen = new SplashScreen();

            //ResizeSplash();
            _splashScreen.Visible = true;
            _splashScreen.TopMost = true;

            Timer _timer = new Timer();
            _timer.Tick += (sender, e) =>
            {
                _splashScreen.Visible = false;
                _timer.Enabled = false;
                _showSplash = false;
            };
            _timer.Interval = 2000;
            _timer.Enabled = true;
        }

        private void ResizeSplash()
        {
            if (_showSplash)
            {
                var screenBounds = Screen.FromControl(this).Bounds;

                var centerXMain = screenBounds.Width / 2.0; // (this.Location.X + this.Width) / 2.0;
                var LocationXSplash = Math.Max(0, centerXMain - (_splashScreen.Width / 2.0));

                var centerYMain = screenBounds.Height / 2.0; // (this.Location.Y + this.Height) / 2.0;
                var LocationYSplash = Math.Max(0, centerYMain - (_splashScreen.Height / 2.0));

                _splashScreen.Location = new Point((int)Math.Round(LocationXSplash), (int)Math.Round(LocationYSplash));
            }
        }
        #endregion

        private IDockContent FindDocument(string text)
        {
            foreach (IDockContent content in dockpanel.Documents)
            {
                if (content.DockHandler.TabText == text)
                {
                    return content;
                }
            }

            return null;
        }

        private void CloseAllDocuments()
        {
            foreach (IDockContent document in dockpanel.DocumentsToArray())
            {
                // IMPORANT: dispose all panes.
                document.DockHandler.DockPanel = null;
                document.DockHandler.Close();
            }
        }

        private void EnableVSRenderer(VisualStudioToolStripExtender.VsVersion version, ThemeBase theme)
        {
            vsToolStripExtender1.SetStyle(mainMenu, version, theme);
            vsToolStripExtender1.SetStyle(toolBar, version, theme);
            vsToolStripExtender1.SetStyle(statusBar, version, theme);
        }

        #region event handlers
        private void loadRomButton_Click(object sender, EventArgs e)
        {
            LoadRom();
        }

        private void ZMusicMagicForm_Load(object sender, EventArgs e)
        {
            string configFile = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "DockPanel.config");

            if (File.Exists(configFile))
            {
                dockpanel.LoadFromXml(configFile, m_deserializeDockContent);
            }
        }

        private void ZMusicMagicForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            string configFile = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "DockPanel.config");
            if (m_bSaveLayout)
            {
                dockpanel.SaveAsXml(configFile);
            }
            else if (File.Exists(configFile))
            {
                File.Delete(configFile);
            }
        }

        private void ZMusicMagicForm_SizeChanged(object sender, EventArgs e)
        {
            //ResizeSplash();
        }

        private void aboutZMusicMagicToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutDialog about = new AboutDialog();
            about.ShowDialog(this);
        }
        #endregion

        private void fileToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            //menuItemClose.Enabled = (dockpanel.ActiveDocument != null);
            //menuItemCloseAll.Enabled =
            //    menuItemCloseAllButThisOne.Enabled = (dockpanel.DocumentsCount > 0);
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dockpanel.ActiveDocument.DockHandler.Close();
        }
    }
}
