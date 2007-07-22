using System;

using System.IO;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Text;
using System.Diagnostics;

/*
 * TODO:
 *  - ability to provide a file name on command line
 *  - saving of preferences. Save: font name, font size, last window position (what else?)
 *  - implement open recent i.e. store the list of all opened files and add selection via File/Open recent menu
 *  - tabbed windows - ability to watch more than one file at once
 *  - automatic scrolling of display
 *  - check how things work if file being watched is deleted
 *  - change the exe name from wTail.exe to wtail.exe
 *  - improve Help/About box
 * */
namespace wTail
{
    public class wTailForm : System.Windows.Forms.Form
    {
        private Scintilla.ScintillaControl scintillaControl1;
        private System.Windows.Forms.Button btOpen;
        private System.Windows.Forms.Button btClear;
        private System.Windows.Forms.MainMenu mainMenu1;
        private System.Windows.Forms.MenuItem menuItem1;
        private System.Windows.Forms.MenuItem menuItem6;
        private System.Windows.Forms.MenuItem menuItem7;
        private System.Windows.Forms.MenuItem menuFileOpen;
        private System.Windows.Forms.MenuItem menuFileClose;
        private System.Windows.Forms.MenuItem menuFileExit;
        private System.Windows.Forms.MenuItem menuToolsWrap;
        private System.Windows.Forms.MenuItem menuToolsFont;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;
        private System.Windows.Forms.MenuItem menuItem3;
        private System.Windows.Forms.MenuItem menuToolsClear;

        private string fileName = null;
        private DateTime lastWriteTime;
        private System.Windows.Forms.Button btClose;
        private long lastFileSize;
        private bool fWrap;
        private FileSystemWatcher watcher;

        private string defaultFontName = "Tahoma";
        private int    defaultFontSize = 10;

        private string curFontName;
        private System.Windows.Forms.MenuItem menuItem2;
        private System.Windows.Forms.MenuItem menuHelpAbout;
        private System.Windows.Forms.MenuItem menuHelpWebsite;
        private int    curFontSize;

        private long GetFileSize(string filePath)
        {
            long size;
            FileInfo fp = new FileInfo(filePath);
            size = fp.Length;
            return size;
        }

        private void SetFontSizeNameForAllStyles(string fontName, int fontSize)
        {
            this.curFontName = fontName;
            this.curFontSize = fontSize;
            int stylesBits = this.scintillaControl1.StyleBits;
            int stylesCount = 2^stylesBits;
            for (int i=0; i<stylesCount; i++)
            {
                this.scintillaControl1.StyleSetFont(i,fontName);
                this.scintillaControl1.StyleSetSize(i,fontSize);
            }
        }

        public wTailForm()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            //
            // TODO: Add any constructor code after InitializeComponent call
            //
            SetFontSizeNameForAllStyles(defaultFontName, defaultFontSize);
            this.scintillaControl1.Text = "Please open a file watch...";
            this.scintillaControl1.ReadOnly = true;
            setWrap(false);
        }

        protected override void Dispose( bool disposing )
        {
            if( disposing )
            {
                if (components != null) 
                {
                    components.Dispose();
                }
            }
            base.Dispose( disposing );
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.scintillaControl1 = new Scintilla.ScintillaControl();
            this.btOpen = new System.Windows.Forms.Button();
            this.btClear = new System.Windows.Forms.Button();
            this.mainMenu1 = new System.Windows.Forms.MainMenu();
            this.menuItem1 = new System.Windows.Forms.MenuItem();
            this.menuFileOpen = new System.Windows.Forms.MenuItem();
            this.menuFileClose = new System.Windows.Forms.MenuItem();
            this.menuItem6 = new System.Windows.Forms.MenuItem();
            this.menuFileExit = new System.Windows.Forms.MenuItem();
            this.menuItem7 = new System.Windows.Forms.MenuItem();
            this.menuToolsClear = new System.Windows.Forms.MenuItem();
            this.menuItem3 = new System.Windows.Forms.MenuItem();
            this.menuToolsWrap = new System.Windows.Forms.MenuItem();
            this.menuToolsFont = new System.Windows.Forms.MenuItem();
            this.menuItem2 = new System.Windows.Forms.MenuItem();
            this.menuHelpAbout = new System.Windows.Forms.MenuItem();
            this.menuHelpWebsite = new System.Windows.Forms.MenuItem();
            this.btClose = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // scintillaControl1
            // 
            this.scintillaControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
                | System.Windows.Forms.AnchorStyles.Left) 
                | System.Windows.Forms.AnchorStyles.Right)));
            this.scintillaControl1.AnchorPosition = 0;
            this.scintillaControl1.BackSpaceUnIndents = false;
            this.scintillaControl1.BufferedDraw = true;
            this.scintillaControl1.CaretForeground = 0;
            this.scintillaControl1.CaretLineBackground = 65535;
            this.scintillaControl1.CaretLineVisible = false;
            this.scintillaControl1.CaretPeriod = 500;
            this.scintillaControl1.CaretWidth = 1;
            this.scintillaControl1.CodePage = 0;
            this.scintillaControl1.Configuration = null;
            this.scintillaControl1.ConfigurationLanguage = "";
            this.scintillaControl1.ControlCharSymbol = 0;
            this.scintillaControl1.CurrentPos = 0;
            this.scintillaControl1.CursorType = -1;
            this.scintillaControl1.EdgeColour = 12632256;
            this.scintillaControl1.EdgeColumn = 0;
            this.scintillaControl1.EdgeMode = 0;
            this.scintillaControl1.EndAtLastLine = true;
            this.scintillaControl1.EOLCharactersVisible = false;
            this.scintillaControl1.EOLMode = 0;
            this.scintillaControl1.focus = false;
            this.scintillaControl1.HighlightGuide = 0;
            this.scintillaControl1.HorizontalScrollBarVisible = true;
            this.scintillaControl1.Indent = 0;
            this.scintillaControl1.IndentationGuidesVisible = false;
            this.scintillaControl1.LayoutCache = 1;
            this.scintillaControl1.Lexer = 0;
            this.scintillaControl1.LexerLanguage = null;
            this.scintillaControl1.Location = new System.Drawing.Point(0, 24);
            this.scintillaControl1.MarginLeft = 1;
            this.scintillaControl1.MarginRight = 1;
            this.scintillaControl1.ModEventMask = 3959;
            this.scintillaControl1.MouseDownCaptures = true;
            this.scintillaControl1.MouseDwellTime = 10000000;
            this.scintillaControl1.Name = "scintillaControl1";
            this.scintillaControl1.Overtype = false;
            this.scintillaControl1.PrintColourMode = 0;
            this.scintillaControl1.PrintMagnification = 0;
            this.scintillaControl1.PrintWrapMode = 1;
            this.scintillaControl1.ReadOnly = false;
            this.scintillaControl1.ScrollWidth = 2000;
            this.scintillaControl1.SearchFlags = 0;
            this.scintillaControl1.SelectionEnd = 0;
            this.scintillaControl1.SelectionStart = 0;
            this.scintillaControl1.Size = new System.Drawing.Size(368, 309);
            this.scintillaControl1.Status = 0;
            this.scintillaControl1.StyleBits = 5;
            this.scintillaControl1.TabIndents = true;
            this.scintillaControl1.TabIndex = 0;
            this.scintillaControl1.TabWidth = 8;
            this.scintillaControl1.TargetEnd = 0;
            this.scintillaControl1.TargetStart = 0;
            this.scintillaControl1.UsePalette = false;
            this.scintillaControl1.UseTabs = true;
            this.scintillaControl1.VerticalScrollBarVisible = true;
            this.scintillaControl1.WhitespaceVisibleState = 0;
            this.scintillaControl1.WrapMode = 0;
            this.scintillaControl1.XOffset = 0;
            this.scintillaControl1.ZoomLevel = 0;
            // 
            // btOpen
            // 
            this.btOpen.Location = new System.Drawing.Point(0, 0);
            this.btOpen.Name = "btOpen";
            this.btOpen.Size = new System.Drawing.Size(48, 23);
            this.btOpen.TabIndex = 1;
            this.btOpen.Text = "Open";
            this.btOpen.Click += new System.EventHandler(this.btOpen_Click);
            // 
            // btClear
            // 
            this.btClear.Location = new System.Drawing.Point(48, 0);
            this.btClear.Name = "btClear";
            this.btClear.Size = new System.Drawing.Size(48, 23);
            this.btClear.TabIndex = 2;
            this.btClear.Text = "Clear";
            this.btClear.Click += new System.EventHandler(this.btClear_Click);
            // 
            // mainMenu1
            // 
            this.mainMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                                      this.menuItem1,
                                                                                      this.menuItem7,
                                                                                      this.menuItem2});
            // 
            // menuItem1
            // 
            this.menuItem1.Index = 0;
            this.menuItem1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                                      this.menuFileOpen,
                                                                                      this.menuFileClose,
                                                                                      this.menuItem6,
                                                                                      this.menuFileExit});
            this.menuItem1.Text = "File";
            // 
            // menuFileOpen
            // 
            this.menuFileOpen.Index = 0;
            this.menuFileOpen.Text = "Open";
            this.menuFileOpen.Click += new System.EventHandler(this.menuFileOpen_Click);
            // 
            // menuFileClose
            // 
            this.menuFileClose.Index = 1;
            this.menuFileClose.Text = "Close";
            this.menuFileClose.Click += new System.EventHandler(this.menuFileClose_Click);
            // 
            // menuItem6
            // 
            this.menuItem6.Index = 2;
            this.menuItem6.Text = "-";
            // 
            // menuFileExit
            // 
            this.menuFileExit.Index = 3;
            this.menuFileExit.Text = "Exit";
            this.menuFileExit.Click += new System.EventHandler(this.menuFileExit_Click);
            // 
            // menuItem7
            // 
            this.menuItem7.Index = 1;
            this.menuItem7.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                                      this.menuToolsClear,
                                                                                      this.menuItem3,
                                                                                      this.menuToolsWrap,
                                                                                      this.menuToolsFont});
            this.menuItem7.Text = "Tools";
            // 
            // menuToolsClear
            // 
            this.menuToolsClear.Index = 0;
            this.menuToolsClear.Text = "Clear";
            this.menuToolsClear.Click += new System.EventHandler(this.menuItem2_Click);
            // 
            // menuItem3
            // 
            this.menuItem3.Index = 1;
            this.menuItem3.Text = "-";
            // 
            // menuToolsWrap
            // 
            this.menuToolsWrap.Index = 2;
            this.menuToolsWrap.Text = "Wrap";
            this.menuToolsWrap.Click += new System.EventHandler(this.menuToolsWrap_Click);
            // 
            // menuToolsFont
            // 
            this.menuToolsFont.Index = 3;
            this.menuToolsFont.Text = "Font...";
            this.menuToolsFont.Click += new System.EventHandler(this.menuToolsFont_Click);
            // 
            // menuItem2
            // 
            this.menuItem2.Index = 2;
            this.menuItem2.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                                      this.menuHelpAbout,
                                                                                      this.menuHelpWebsite});
            this.menuItem2.Text = "Help";
            // 
            // menuHelpAbout
            // 
            this.menuHelpAbout.Index = 0;
            this.menuHelpAbout.Text = "About";
            this.menuHelpAbout.Click += new System.EventHandler(this.menuHelpAbout_Click);
            // 
            // menuHelpWebsite
            // 
            this.menuHelpWebsite.Index = 1;
            this.menuHelpWebsite.Text = "Visit website";
            this.menuHelpWebsite.Click += new System.EventHandler(this.menuHelpWebsite_Click);
            // 
            // btClose
            // 
            this.btClose.Location = new System.Drawing.Point(96, 0);
            this.btClose.Name = "btClose";
            this.btClose.Size = new System.Drawing.Size(48, 23);
            this.btClose.TabIndex = 4;
            this.btClose.Text = "Close";
            this.btClose.Click += new System.EventHandler(this.btClose_Click);
            // 
            // wTailForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(368, 333);
            this.Controls.Add(this.btClose);
            this.Controls.Add(this.btClear);
            this.Controls.Add(this.btOpen);
            this.Controls.Add(this.scintillaControl1);
            this.Menu = this.mainMenu1;
            this.Name = "wTailForm";
            this.Text = "wTail";
            this.ResumeLayout(false);

        }
        #endregion

        [STAThread]
        static void Main() 
        {
            Application.Run(new wTailForm());
        }

        private void menuFileExit_Click(object sender, System.EventArgs e)
        {
            this.Close();
            Application.Exit();		
        }

        private void FileOpen()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = Application.StartupPath;
            openFileDialog.RestoreDirectory = true;
            openFileDialog.Multiselect = false;
       
            if(openFileDialog.ShowDialog() != DialogResult.OK)
                return;

            this.fileName = openFileDialog.FileName;
            if (!File.Exists(this.fileName))
                return;

            ClearTxt();
            AddTxt("Watching file " + this.fileName + "\n");

            this.lastWriteTime = File.GetLastWriteTime(fileName);
            this.lastFileSize = GetFileSize(fileName);

            // Create a new FileSystemWatcher and set its properties.
            this.watcher = new FileSystemWatcher();
            string path = System.IO.Path.GetDirectoryName(this.fileName);
            string name = System.IO.Path.GetFileName(this.fileName);
            watcher.Path = path;
            /* Watch for changes in LastAccess and LastWrite times, and 
               the renaming of files or directories. */
            watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite;
            // Only watch text files.
            watcher.Filter = name;

            // Add event handlers.
            watcher.Changed += new FileSystemEventHandler(OnChanged);
 
            // Begin watching.
            watcher.EnableRaisingEvents = true;

        }

        // Define the event handlers.
        private void OnChanged(object source, FileSystemEventArgs e)
        {
            // Specify what is done when a file is changed, created, or deleted.
            string fileName = e.FullPath;
            Debug.Assert(fileName == this.fileName);
            Debug.Assert(WatcherChangeTypes.Changed == e.ChangeType);

            DateTime curWriteTime = File.GetLastWriteTime(fileName);
            long curFileSize = GetFileSize(fileName);
            if (curWriteTime == this.lastWriteTime)
                return;
            if (curFileSize == this.lastFileSize)
                return;
            string txt = ReadFromFile(this.fileName, this.lastFileSize, curFileSize);
            AddTxt(txt);
            this.lastWriteTime = curWriteTime;
            this.lastFileSize = curFileSize;
        }

        private void AddTxt(string txt)
        {
            int pos = this.scintillaControl1.Length;
            this.scintillaControl1.ReadOnly = false;
            this.scintillaControl1.InsertText(pos, txt);
            this.scintillaControl1.ReadOnly = true;
        }

        private void FileClose()
        {
            if (null==fileName)
            {
                AddTxt("\nNo file to close");
                return;
            }
            AddTxt( String.Format("\nStopped watching file {0}",fileName));
            fileName = null;
        }

        private void menuFileOpen_Click(object sender, System.EventArgs e)
        {
            FileOpen();
        }

        private void menuItem2_Click(object sender, System.EventArgs e)
        {
            ClearTxt();
        }

        private void ClearTxt()
        {
            this.scintillaControl1.ReadOnly = false;
            this.scintillaControl1.Text = " ";
            this.scintillaControl1.ReadOnly = true;
        }

        private void btClear_Click(object sender, System.EventArgs e)
        {
            ClearTxt();
        }

        private string ReadFromFile(string filePath, long startPos, long endPos)
        {
            string s = "\nnot read";
            try 
            {
                Debug.Assert( endPos > startPos );
                Stream stm = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite );
                TextReader tr = new StreamReader(stm);
                stm.Seek(startPos, SeekOrigin.Begin);
                s = tr.ReadToEnd();
            } 
            catch (Exception e)
            {
                s = e.ToString();
            }
            return s;
        }

        private void btOpen_Click(object sender, System.EventArgs e)
        {
            FileOpen();            
        }

        private void btClose_Click(object sender, System.EventArgs e)
        {
            FileClose();
        }

        private void menuFileClose_Click(object sender, System.EventArgs e)
        {
            FileClose();
        }

        private void setWrap(bool fOn)
        {
            this.fWrap = fOn;
            if (fOn)
            {
                this.menuToolsWrap.Checked = true;
                this.scintillaControl1.WrapMode = 1;
            }
            else
            {
                this.menuToolsWrap.Checked = false;
                this.scintillaControl1.WrapMode = 0;
            }
        }

        private void toggleWrap()
        {
            if (this.fWrap)
                setWrap(false);
            else
                setWrap(true);
        }

        private void menuToolsWrap_Click(object sender, System.EventArgs e)
        {
            toggleWrap();        
        }

        private void menuToolsFont_Click(object sender, System.EventArgs e)
        {
            FontDialog fontDlg = new FontDialog();
            System.Drawing.Font font = new System.Drawing.Font(this.curFontName,(float)this.curFontSize);
            fontDlg.Font = font;
            if (fontDlg.ShowDialog() == DialogResult.OK)
            {
                font = fontDlg.Font;
                if ( (this.curFontName != font.Name) || (this.curFontSize != (int)font.Size))
                {
                    SetFontSizeNameForAllStyles(font.Name, (int)font.Size);
                }
            }
        }

        private void menuHelpAbout_Click(object sender, System.EventArgs e)
        {
            AboutDialogBox dlg = new AboutDialogBox();
            dlg.ShowDialog();
        }

        private void menuHelpWebsite_Click(object sender, System.EventArgs e)
        {
            string websiteURL = "http://blog.kowalczyk.info/software/wtail/";
            System.Diagnostics.Process.Start(websiteURL);
        }
    }

    class AboutDialogBox : Form
    {
        public AboutDialogBox()
        {
            Text = "About wTail";
            StartPosition   = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            ControlBox      = false;
            MaximizeBox     = false;
            MinimizeBox     = false;
            ShowInTaskbar   = false;

            Label lbl1    = new Label();
            lbl1.Parent   = this;
            lbl1.Text     = "wTail Version 0.1";
            lbl1.Font     = new System.Drawing.Font("Tahoma", (float)10);

            lbl1.AutoSize     = true;
            lbl1.TextAlign    = ContentAlignment.MiddleCenter;

            int labelMargin   = 10;
            lbl1.Location     = new System.Drawing.Point(labelMargin,lbl1.Font.Height/2);

            int clientDX   = lbl1.Right + labelMargin;

            Button btn      = new Button();
            btn.Parent      = this;
            btn.Text        = "OK";
            btn.Size        = new Size(4*btn.Font.Height, 2* btn.Font.Height);
            btn.Location    = new System.Drawing.Point( (clientDX-btn.Size.Width)/2, lbl1.Bottom+10);
            btn.DialogResult = DialogResult.OK; 

            CancelButton = btn;
            AcceptButton = btn;
            ClientSize = new Size(clientDX, btn.Bottom + 2 * btn.Font.Height);
        }  
    }  
}
