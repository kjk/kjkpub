using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using Microsoft.Windows.Shell;
using Microsoft.Win32;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using System.Collections;

namespace Bend
{      
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const int shadowThickness = 15;

        #region Member Data
        HwndSource mainWindow;

        System.Threading.Thread findOnPageThread;
        bool lastKeyWasEnter;

        Window findAndReplaceWindow;

        List<Tab> tab;
        int currentTabIndex;
        int currentSearchIndex;
        #endregion

        #region Window management

        public MainWindow()
        {
            InitializeComponent();
            findAndReplaceWindow = null;
            var style = (Style)Resources["PlainStyle"];
            this.Style = style;
            tab = new List<Tab>();
            this.Top = PersistantStorage.StorageObject.mainWindowTop;
            this.Left = PersistantStorage.StorageObject.mainWindowLeft;
            this.Width = PersistantStorage.StorageObject.mainWindowWidth;
            this.Height = PersistantStorage.StorageObject.mainWindowHeight;
        }

        public void Window_SourceInitialized(object sender, EventArgs e)
        {
            this.mainWindow = PresentationSource.FromVisual((Visual)this) as HwndSource;
 
            // Reopen from explorer or last session or create empty tab
            bool tabOpened = false;
            try
            {
                string[] fileNames = AppDomain.CurrentDomain.SetupInformation.ActivationArguments.ActivationData;                
                if (fileNames == null || fileNames.Length <= 0)
                {
                    fileNames = PersistantStorage.StorageObject.mruFile;
                }
                if (fileNames != null)
                {
                    for (int mruCount = 0; mruCount < fileNames.Length; mruCount++)
                    {
                        string fileName = fileNames[mruCount];
                        if (System.IO.File.Exists(fileName))
                        {
                            this.AddNewTab();
                            int lastTab = this.tab.Count - 1;
                            this.tab[lastTab].OpenFile(fileName);
                            this.tab[lastTab].Title.Opacity = 0.5;
                            this.tab[lastTab].TextEditor.Visibility = Visibility.Hidden;
                            tabOpened = true;
                        }
                    }
                }
            }
            catch
            {
            }
            if (!tabOpened)
            {
                // Create default new file tab
                this.AddNewTab();
            }
            
            // this.tab.Count will atleast be 1 at this point
            this.currentTabIndex = this.tab.Count - 1;
            this.tab[this.currentTabIndex].Title.Opacity = 1;
            this.tab[this.currentTabIndex].TextEditor.Visibility = Visibility.Visible;
            tab[this.currentTabIndex].TextEditor.Focus();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Save file name to MRU list
            try
            {
                PersistantStorage.StorageObject.mruFile = new String[this.tab.Count];
                for (int mruCount = 0; mruCount < this.tab.Count; mruCount++)
                {
                    PersistantStorage.StorageObject.mruFile[mruCount] = this.tab[mruCount].FullFileName;                    
                }

                PersistantStorage.StorageObject.mainWindowTop = this.Top;
                PersistantStorage.StorageObject.mainWindowLeft = this.Left;
                PersistantStorage.StorageObject.mainWindowWidth = this.Width;
                PersistantStorage.StorageObject.mainWindowHeight = this.Height;
            }
            catch
            {
            }

        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (e.Data is System.Windows.DataObject &&
                ((System.Windows.DataObject)e.Data).ContainsFileDropList())
            {
                bool fileAdded = false;
                foreach (string filePath in ((System.Windows.DataObject)e.Data).GetFileDropList())
                {
                    Tab newTab = new Tab();
                    tab.Add(newTab);
                    // Hook up tab band event handlers
                    newTab.Title.MouseLeftButtonUp += this.TabClick;
                    newTab.Title.ContextMenu = (ContextMenu)Resources["TabTitleContextMenu"];
                    newTab.CloseButton.MouseLeftButtonUp += this.TabClose;

                    TabBar.Children.Add(newTab.Title);
                    Editor.Children.Add(newTab.TextEditor);

                    newTab.OpenFile(filePath);                    
                    fileAdded = true;
                }

                if (fileAdded)
                {
                    // Switch focus to the new file
                    if (currentTabIndex >= 0)
                    {
                        tab[currentTabIndex].TextEditor.Visibility = Visibility.Hidden;
                        tab[currentTabIndex].Title.Opacity = 0.5;
                    }
                    
                    int newTabFocus = tab.Count - 1;
                    this.currentTabIndex = newTabFocus;
                    tab[newTabFocus].Title.Opacity = 1.0;
                    tab[newTabFocus].TextEditor.Visibility = Visibility.Visible;
                    tab[newTabFocus].TextEditor.Focus();
                }
            }
        }

        private void MinimizeButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.WindowState = System.Windows.WindowState.Minimized;
        }

        private void MaximizeButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (this.WindowState == System.Windows.WindowState.Maximized)
            {
                this.WindowState = System.Windows.WindowState.Normal;
            }
            else
            {
                this.WindowState = System.Windows.WindowState.Maximized;
            }
        }

        private void QuitButtonUp(object sender, MouseButtonEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                if (this.currentTabIndex > 0 && this.currentTabIndex < tab.Count)
                {
                    tab[this.currentTabIndex].TextEditor.Select(0, 0);
                    this.currentSearchIndex = 0;
                }
            }
        }

        private void ThisWindow_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == System.Windows.WindowState.Maximized)
            {
                this.MainWindowGrid.Margin = new Thickness(0, 0, 6, 6);
                this.ResizeCrimp.Visibility = System.Windows.Visibility.Hidden;
            }
            if (this.WindowState == System.Windows.WindowState.Normal)
            {
                this.MainWindowGrid.Margin = new Thickness(0);
                this.ResizeCrimp.Visibility = System.Windows.Visibility.Visible;
            }
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
        private void ResizeCrimp_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {            
            const uint WM_SYSCOMMAND = 274;
            const uint DIRECTION_BOTTOMRIGHT = 61448;
            SendMessage(this.mainWindow.Handle, WM_SYSCOMMAND, (IntPtr)DIRECTION_BOTTOMRIGHT, IntPtr.Zero);
        }

        private void CommandSave(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.currentTabIndex >= 0)
            {
                try
                {
                    bool fileSaved = false;
                    if (this.tab[this.currentTabIndex].FullFileName != null)
                    {
                        this.tab[this.currentTabIndex].SaveFile(this.tab[this.currentTabIndex].FullFileName);
                        fileSaved = true;
                    }
                    else
                    {
                        SaveFileDialog dlg = new SaveFileDialog();
                        FileExtensions flex = new FileExtensions();
                        dlg.Filter = flex.GetFilterString();  
                        if (this.currentTabIndex >= 0 && this.tab[this.currentTabIndex].FullFileName != null)
                        {
                            string initialDirectory = System.IO.Path.GetDirectoryName(this.tab[this.currentTabIndex].FullFileName);
                            if (initialDirectory != null && initialDirectory.Length != 0)
                            {
                                dlg.InitialDirectory = initialDirectory;
                            }
                        }

                        if (dlg.ShowDialog(this) ?? false)
                        {
                            this.tab[this.currentTabIndex].SaveFile(dlg.FileName);
                            fileSaved = true;
                        }
                    }
                    if (fileSaved)
                    {
                        this.SetStatusText("FILE SAVED");
                        System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
                        dispatcherTimer.Tick += new EventHandler(SaveDispatcherTimer_Tick);
                        dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
                        dispatcherTimer.Start();
                    }
                }
                catch (Exception exception)
                {
                    StyledMessageBox.Show("ERROR", "Error Saving File" + exception.ToString());
                }
            }
        }

        private void SaveDispatcherTimer_Tick(object sender, EventArgs e)
        {
            ((DispatcherTimer)sender).Stop();
            this.SetStatusText("");
        }

        private void CommandOpen(object sender, ExecutedRoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.CheckFileExists = true;
            FileExtensions flex = new FileExtensions();
            dlg.Filter = flex.GetFilterString();            
            if (this.currentTabIndex >= 0 && this.tab[this.currentTabIndex].FullFileName != null)
            {
                string initialDirectory = System.IO.Path.GetDirectoryName(this.tab[this.currentTabIndex].FullFileName);
                if (initialDirectory != null && initialDirectory.Length != 0)
                {
                    dlg.InitialDirectory = initialDirectory;
                }
            }

            if (dlg.ShowDialog(this) ?? false)
            {
                // No tabs / Non Empty new file / tab has some file open
                if (this.currentTabIndex < 0 || this.tab[this.currentTabIndex].TextEditor.Document.TextLength != 0 || this.tab[this.currentTabIndex].FullFileName != null)
                {
                    if (this.currentTabIndex >= 0)
                    {
                        tab[this.currentTabIndex].Title.Opacity = 0.5;
                        tab[this.currentTabIndex].TextEditor.Visibility = Visibility.Hidden;
                    }

                    this.AddNewTab();

                    this.currentTabIndex = tab.Count - 1;
                    tab[this.currentTabIndex].TextEditor.Focus();
                }

                this.tab[this.currentTabIndex].OpenFile(dlg.FileName);
            }
        }

        private void CommandNew(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.currentTabIndex >= 0)
            {
                tab[this.currentTabIndex].Title.Opacity = 0.5;
                tab[this.currentTabIndex].TextEditor.Visibility = Visibility.Hidden;
            }

            this.AddNewTab();
            
            this.currentTabIndex = tab.Count - 1;
            tab[this.currentTabIndex].TextEditor.Focus();
        }

        private void CommandRefresh(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.currentTabIndex >= 0 && tab[this.currentTabIndex].FullFileName != null && System.IO.File.Exists(tab[this.currentTabIndex].FullFileName))
            {
                tab[this.currentTabIndex].OpenFile(tab[this.currentTabIndex].FullFileName);
            }            
        }

        private void CommandReplace(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.findAndReplaceWindow == null)
            {
                this.findAndReplaceWindow = new FindAndReplace(this);
                this.findAndReplaceWindow.Owner = this;
            }
            if (this.findAndReplaceWindow.IsVisible)
            {
                this.findAndReplaceWindow.Hide();
                this.Editor.Focus();
            }
            else
            {                
                this.findAndReplaceWindow.Show();
                this.findAndReplaceWindow.Focus();
            }
        }

        private void CommandGoto(object sender, ExecutedRoutedEventArgs e)
        {
            GotoLine.ShowGotoLineWindow(this);   
        }

        public void CommandGoto(int lineNumber)
        {
            if (this.currentTabIndex >= 0 && tab[this.currentTabIndex].FullFileName != null && System.IO.File.Exists(tab[this.currentTabIndex].FullFileName))
            {
                try
                {
                    tab[this.currentTabIndex].TextEditor.ScrollToLine(lineNumber);                    
                }
                catch
                {
                }
            }            
        }

        private void CommandHelp(object sender, ExecutedRoutedEventArgs e)
        {
            if (Settings.Visibility == System.Windows.Visibility.Hidden)
            {
                Logo_MouseDown(null, null);
            }
            else
            {
                BackImage_MouseDown(null, null);
            }
        }
        #endregion

        #region Menu band management
        private void NewButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.CommandNew(sender, null);
            }
            e.Handled = true;
        }

        private void OpenButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.CommandOpen(sender, null);
            }
            e.Handled = true;
        }

        private void SaveButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.CommandSave(sender, null);
            }
            e.Handled = true;
        }

        private void SavePlusButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                if (this.currentTabIndex >= 0)
                {
                    try
                    {
                        SaveFileDialog dlg = new SaveFileDialog();
                        if (this.currentTabIndex >= 0 && this.tab[this.currentTabIndex].FullFileName != null)
                        {
                            string initialDirectory = System.IO.Path.GetDirectoryName(this.tab[this.currentTabIndex].FullFileName);
                            if (initialDirectory != null && initialDirectory.Length != 0)
                            {
                                dlg.InitialDirectory = initialDirectory;
                            }
                        }

                        FileExtensions flex = new FileExtensions();
                        dlg.Filter = flex.GetFilterString();   
                        if (dlg.ShowDialog(this) ?? false)
                        {
                            this.tab[this.currentTabIndex].SaveFile(dlg.FileName);                            
                        }
                    }
                    catch (Exception exception)
                    {
                        StyledMessageBox.Show("ERROR", "Error Saving File" + exception.ToString());
                    }
                }
            }
            e.Handled = true;
        }

        private void FindButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.CommandFind(sender, null);
            }
            e.Handled = true;
        }

        private void Logo_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                System.Windows.Media.Animation.Storyboard settingsAnimation = (System.Windows.Media.Animation.Storyboard)FindResource("slideSettingsIn");
                MainWindowGridRotateTransform.CenterX = this.Width / 3;
                MainWindowGridRotateTransform.CenterY = this.Height;
                SettingsGridRotateTransform.CenterX = this.Width / 1.5;
                SettingsGridRotateTransform.CenterY = this.Height;
                settingsAnimation.Begin(this);
            }
            catch
            {
            }
        }
        
        private void BackImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {                
                System.Windows.Media.Animation.Storyboard settingsAnimation = (System.Windows.Media.Animation.Storyboard)FindResource("slideSettingsOut");
                MainWindowGridRotateTransform.CenterX = this.Width / 3;
                MainWindowGridRotateTransform.CenterY = this.Height;
                SettingsGridRotateTransform.CenterX = this.Width / 1.5;
                SettingsGridRotateTransform.CenterY = this.Height;
                settingsAnimation.Begin(this);
            }
            catch
            {
            }
        }

        public void CancelSettingsUI()
        {
            this.BackImage_MouseDown(null, null);
        }

        private void ReplaceButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.CommandReplace(sender, null);
        }
        #endregion

        #region Tab band management
        private void AddNewTab()
        {
            Tab newTab = new Tab();
            tab.Add(newTab);
            // Hook up tab band event handlers
            newTab.Title.MouseLeftButtonUp += this.TabClick;
            newTab.Title.ContextMenu = (ContextMenu)Resources["TabTitleContextMenu"];
            newTab.CloseButton.MouseLeftButtonUp += this.TabClose;

            TabBar.Children.Add(newTab.Title);
            Editor.Children.Add(newTab.TextEditor);
        }

        private void TabClick(object sender, MouseButtonEventArgs e)
        {
            // Find the tab title in tab collection
            for (int i = 0; i < tab.Count; i++)
            {
                if (tab[i].Title == sender)
                {
                    if (currentTabIndex >= 0)
                    {
                        tab[currentTabIndex].TextEditor.Visibility = Visibility.Hidden;
                        tab[currentTabIndex].Title.Opacity = 0.5;
                    }
                    // Clear find on page state
                    this.currentSearchIndex = 0;
                    tab[this.currentTabIndex].TextEditor.Select(0,0);

                    this.currentTabIndex = i;                    
                    tab[i].Title.Opacity = 1.0;
                    tab[i].TextEditor.Visibility = Visibility.Visible;
                    tab[i].TextEditor.Focus();
                    break;
                }
            }
            // Tab was not found - fail silently
        }

        private void TabClose(object sender, MouseButtonEventArgs e)
        {
            WrapPanel wrapPanel = (WrapPanel)((Image)sender).Parent;
            // Find the tab title in tab collection
            for (int i = 0; i < tab.Count; i++)
            {
                if (tab[i].Title == wrapPanel)
                {
                    this.TabClose(i);
                }
            }
            // Tab was not found - fail silently
        }       

        private void ContextCloseAllButThis(object sender, RoutedEventArgs e)
        {
            UIElement tabTitle = (Control)((MenuItem)e.OriginalSource).Parent;
            tabTitle = ((System.Windows.Controls.Primitives.Popup)((Control)tabTitle).Parent).PlacementTarget;

            // Close all the other tabs
            for (int i = tab.Count - 1; i >= 0; i--)
            {
                if (tab[i].Title != tabTitle)
                {
                    // Delete cthe tab                
                    TabBar.Children.Remove(tab[i].Title);
                    Editor.Children.Remove(tab[i].TextEditor);
                    tab.RemoveAt(i);                 
                }
            }
            
            // Now set focus on the first tab.
            if (tab.Count > 0)
            {
                this.currentTabIndex = 0;
                tab[this.currentTabIndex].Title.Opacity = 1.0;
                tab[this.currentTabIndex].TextEditor.Visibility = Visibility.Visible;
                tab[this.currentTabIndex].TextEditor.Focus();
            }
        }

        private void ContextCopyFullPath(object sender, RoutedEventArgs e)
        {
            UIElement tabTitle = (Control)((MenuItem)e.OriginalSource).Parent;
            tabTitle = ((System.Windows.Controls.Primitives.Popup)((Control)tabTitle).Parent).PlacementTarget;

            // Find the tab title in tab collection
            for (int i = 0; i < tab.Count; i++)
            {
                if (tab[i].Title == tabTitle)
                {
                    Clipboard.SetText(tab[i].FullFileName);
                    break;
                }
            }
        }

        private void ContextOpenContainingFolder(object sender, RoutedEventArgs e)
        {
            UIElement tabTitle = (Control)((MenuItem)e.OriginalSource).Parent;
            tabTitle = ((System.Windows.Controls.Primitives.Popup)((Control)tabTitle).Parent).PlacementTarget;
            
            // Find the tab title in tab collection
            for (int i = 0; i < tab.Count; i++)
            {
                if (tab[i].Title == tabTitle)
                {
                    if (tab[i].FullFileName != null && tab[i].FullFileName.Length > 0)
                    {
                        System.Diagnostics.Process.Start("explorer.exe", System.IO.Path.GetDirectoryName(tab[i].FullFileName));
                    }
                    break;
                }
            }
        }       

        private void ContextClose(object sender, RoutedEventArgs e)
        {
            UIElement tabTitle = (Control)((MenuItem)e.OriginalSource).Parent;
            tabTitle = ((System.Windows.Controls.Primitives.Popup)((Control)tabTitle).Parent).PlacementTarget;
            
            // Find the tab title in tab collection
            for (int i = 0; i < tab.Count; i++)
            {
                if (tab[i].Title == tabTitle)
                {
                    this.TabClose(i);
                    break;
                }
            }
            // Tab was not found - fail silently
        }

        private void TabClose(int tabIndex)
        {
            if (tabIndex == this.currentTabIndex)
            {
                // Switch to an existing tab
                // We know i < tab.Count - check if we are the last tab before switching to a tab after us.
                // if we are the last tab switch to a tab before us.
                if (tabIndex == (tab.Count - 1))
                {
                    this.currentTabIndex = tabIndex - 1;
                    if (this.currentTabIndex >= 0)
                    {
                        tab[this.currentTabIndex].Title.Opacity = 1.0;
                        tab[this.currentTabIndex].TextEditor.Visibility = Visibility.Visible;
                        tab[this.currentTabIndex].TextEditor.Focus();
                    }
                }
                else
                {
                    // After deletion all indexes after i shift.
                    this.currentTabIndex = tabIndex;

                    tab[this.currentTabIndex + 1].Title.Opacity = 1.0;
                    tab[this.currentTabIndex + 1].TextEditor.Visibility = Visibility.Visible;
                    tab[this.currentTabIndex + 1].TextEditor.Focus();
                }
            }
            else
            {
                // The indexes shifted, since a tab was deleted.
                if (this.currentTabIndex > tabIndex)
                {
                    this.currentTabIndex--;
                }
            }

            // Clear find on page
            this.currentSearchIndex = 0;            

            // Delete current tab                    
            TabBar.Children.Remove(tab[tabIndex].Title);
            Editor.Children.Remove(tab[tabIndex].TextEditor);
            tab[tabIndex].Close();
            tab.RemoveAt(tabIndex);
        }

        private void ContextRefresh(object sender, RoutedEventArgs e)
        {
            UIElement tabTitle = (Control)((MenuItem)e.OriginalSource).Parent;
            tabTitle = ((System.Windows.Controls.Primitives.Popup)((Control)tabTitle).Parent).PlacementTarget;

            // Find the tab title in tab collection
            for (int i = 0; i < tab.Count; i++)
            {
                if (tab[i].Title == tabTitle)
                {
                    if (tab[i].FullFileName != null && System.IO.File.Exists(tab[i].FullFileName))
                    {
                        tab[i].TextEditor.Load(tab[i].FullFileName);
                    }
                    break;
                }
            }
        }

        private delegate void SetStatusText_Delegate(string status);
        internal void SetStatusText(string statusText)        
        {
            if (statusText.Length == 0)
            {
                this.StatusText.Visibility = System.Windows.Visibility.Hidden;
                this.StatusText.Content = "";
            }
            else
            {
                this.StatusText.Visibility = System.Windows.Visibility.Visible;
                this.StatusText.Content = statusText;
            }            
        }
        #endregion

        #region Editor Context Menu
        private void ContextCopy(object sender, RoutedEventArgs e)
        {
            this.CommandCopy(sender, null);
        }

        private void ContextCut(object sender, RoutedEventArgs e)
        {
            if (this.currentTabIndex >= 0)
            {
                this.tab[this.currentTabIndex].TextEditor.Cut();
            }
        }

        private void ContextUndo(object sender, RoutedEventArgs e)
        {
            if (this.currentTabIndex >= 0)
            {
                this.tab[this.currentTabIndex].TextEditor.Undo();
            }
        }

        private void ContextRedo(object sender, RoutedEventArgs e)
        {
            if (this.currentTabIndex >= 0)
            {
                this.tab[this.currentTabIndex].TextEditor.Redo();
            }
        }

        private void ContextPaste(object sender, RoutedEventArgs e)
        {
            this.CommandPaste(sender, null);
        }
        #endregion

        #region Clip board management
        private void CommandCopy(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.currentTabIndex >= 0)
            {
                Clipboard.SetText(this.tab[this.currentTabIndex].TextEditor.SelectedText);
            }
        }
        private void CommandPaste(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.currentTabIndex >= 0)
            {
                this.tab[this.currentTabIndex].TextEditor.Paste();
            }
        }
        #endregion

        #region Find On page
        private void CommandFind(object sender, ExecutedRoutedEventArgs e)
        {
            FindText.Focus();
        }       

        private void FindText_KeyDown(object sender, KeyEventArgs e)
        {
            if (this.currentTabIndex >= 0)
            {
                if (e.Key == Key.Enter)
                {   
                    this.FindNextStringOnPage(FindText.Text, (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)), false, false, false);
                }
                else
                {
                    this.lastKeyWasEnter = false;
                    if (e.Key == Key.Escape)
                    {
                        this.tab[this.currentTabIndex].TextEditor.Select(0, 0);
                        this.tab[this.currentTabIndex].TextEditor.Focus();
                        this.currentSearchIndex = 0;
                        this.SetStatusText("");
                    }
                }
            }
        }

        internal void FindNextStringOnPage(string findText, bool backward, bool resetSearch, bool matchCase, bool useRegex)
        {
            if (this.currentTabIndex >= 0 && this.currentTabIndex < this.tab.Count)
            {
                bool thisSearchWasFromBegin;
                int matchLength = 0;
            repeatSearch:
                if (backward)
                {
                    // Search backwards
                    currentSearchIndex--;
                    if (resetSearch || currentSearchIndex < 0 || currentSearchIndex >= tab[this.currentTabIndex].TextEditor.Text.Length) currentSearchIndex = 0;
                    thisSearchWasFromBegin = (currentSearchIndex == 0);
                    currentSearchIndex = tab[this.currentTabIndex].TextEditor.Text.LastIndexOf(findText, this.currentSearchIndex, matchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase);
                    matchLength = findText.Length;
                }
                else
                {
                    // Search forwards
                    currentSearchIndex++;
                    if (resetSearch || currentSearchIndex < 0 || currentSearchIndex >= tab[this.currentTabIndex].TextEditor.Text.Length) currentSearchIndex = 0;
                    thisSearchWasFromBegin = (currentSearchIndex == 0);
                    if (useRegex)
                    {
                        try
                        {
                            System.Text.RegularExpressions.Regex regEx = new System.Text.RegularExpressions.Regex(findText, matchCase ? System.Text.RegularExpressions.RegexOptions.None : System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                            System.Text.RegularExpressions.Match regExMatch = regEx.Match(tab[this.currentTabIndex].TextEditor.Text, currentSearchIndex);
                            if (regExMatch.Success)
                            {
                                currentSearchIndex = regExMatch.Index;
                                matchLength = regExMatch.Length;
                            }
                            else
                            {
                                currentSearchIndex = -1;
                            }
                        }
                        catch
                        {
                            currentSearchIndex = -1;
                        }
                    }
                    else
                    {
                        currentSearchIndex = tab[this.currentTabIndex].TextEditor.Text.IndexOf(findText, this.currentSearchIndex, matchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase);
                        matchLength = findText.Length;
                    }
                }
                if (!this.lastKeyWasEnter)
                {
                    this.lastKeyWasEnter = true;
                    goto repeatSearch;
                }

                if (currentSearchIndex >= 0)
                {
                    tab[this.currentTabIndex].TextEditor.Select(currentSearchIndex, matchLength, /*isFindOnPageSelection*/true);
                    
                    // We are looping and changed to status text to "NO MORE MATCHES", change it back to the match count.
                    if (thisSearchWasFromBegin)
                    {
                        UpdateFind(findText, false, matchCase, useRegex);
                    }
                }
                else
                {
                    if (thisSearchWasFromBegin)
                    {
                        this.SetStatusText("NO MATCHES FOUND");
                    }
                    else
                    {
                        this.SetStatusText("NO MORE MATCHES");
                    }
                    tab[this.currentTabIndex].TextEditor.Select(0, 0);
                }
            }            
        }

        private void FindText_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateFind(FindText.Text, true, false, false);            
        }

        private void Editor_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            this.currentSearchIndex = 0;
        }

        // TODO: this is hacky this must not be public. Code needs to be merged
        public void UpdateFind(string findText, bool highLightFirstMatch, bool matchCase, bool useRegex)
        {
            if (this.currentTabIndex >= 0)
            {
                string text = tab[this.currentTabIndex].TextEditor.Text;
                TextEditor textEditor = tab[this.currentTabIndex].TextEditor;
                if (this.findOnPageThread != null)
                {
                    this.findOnPageThread.Abort();
                    this.findOnPageThread = null;
                }
                if (findText.Length > 0 && text.Length > 0)
                {
                    this.findOnPageThread = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(UpdateFind_WorkerThread));
                    this.findOnPageThread.IsBackground = true;
                    Object[] parameters = new Object[7];
                    parameters[0] = text;
                    parameters[1] = findText;
                    parameters[2] = highLightFirstMatch;
                    parameters[3] = matchCase;
                    parameters[4] = useRegex;
                    parameters[5] = null;
                    parameters[6] = textEditor;
                    this.findOnPageThread.Start(parameters);
                }
                else
                {
                    textEditor.Select(0, 0, true);
                    this.SetStatusText("NO MATCHES FOUND");
                }
            }            
        }        

        private delegate void EditorFindOnPageSelection_Delegate(int slectionStart, int selectionLength, bool isFindOnPageSelection);
        private void UpdateFind_WorkerThread(object parameters)
        {
            string text = (string) ((object[])parameters)[0];
            string findText = (string) ((object[])parameters)[1];
            bool highLightFirstMatch = (bool)((object[])parameters)[2];
            bool matchCase = (bool)((object[])parameters)[3];
            bool useRegEx = (bool)((object[])parameters)[4];
            string replacementText = (string)((object[])parameters)[5];
            TextEditor textEditor = (TextEditor)((object[])parameters)[6];            
            int findIndex = 0;
            int matchLength = 0;
            int count = 0;

            System.Text.RegularExpressions.Regex regEx = null;
            if (useRegEx)
            {
                try
                {
                    regEx = new System.Text.RegularExpressions.Regex(findText, matchCase ? System.Text.RegularExpressions.RegexOptions.None : System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                }
                catch
                {
                    return;
                }
            }

            while (true)
            {
                if (useRegEx)
                {
                    try
                    {                        
                        System.Text.RegularExpressions.Match regExMatch = regEx.Match(text, findIndex);
                        if (regExMatch.Success)
                        {
                            findIndex = regExMatch.Index;
                            matchLength = regExMatch.Length;
                        }
                        else
                        {
                            findIndex = -1;
                            matchLength = 0;
                        }
                    }
                    catch
                    {
                        findIndex = -1;
                        matchLength = 0;
                    }
                }
                else
                {
                    findIndex = text.IndexOf(findText, findIndex, matchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase);
                    matchLength = findText.Length;
                }

                if (findIndex >= 0)
                {
                    count++;
                    if (count == 1 && highLightFirstMatch == true)
                    {
                        Object[] callingParameters = new Object[3];
                        callingParameters[0] = findIndex;
                        callingParameters[1] = matchLength;
                        callingParameters[2] = true;
                        this.Dispatcher.BeginInvoke(new EditorFindOnPageSelection_Delegate(textEditor.Select), callingParameters);
                    }
                    if (count % 10 == 0)
                    {
                        this.Dispatcher.BeginInvoke(new SetStatusText_Delegate(this.SetStatusText), count.ToString() + " MATCHES");
                    }
                    if (replacementText != null)
                    {
                        textEditor.ReplaceText(findIndex, matchLength, replacementText);
                    }

                    findIndex++;
                    if (findIndex >= text.Length)
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }                
            }
            if (count == 0)
            {                
                this.Dispatcher.BeginInvoke(new SetStatusText_Delegate(this.SetStatusText), "NO MATCHES FOUND");
                Object[] callingParameters = new Object[3];
                callingParameters[0] = 0;
                callingParameters[1] = 0;
                callingParameters[2] = true;
                this.Dispatcher.BeginInvoke(new EditorFindOnPageSelection_Delegate(textEditor.Select), callingParameters);
            }
            else
            {
                this.Dispatcher.BeginInvoke(new SetStatusText_Delegate(this.SetStatusText), count.ToString() + " MATCHES");
            }            
        }
        
        private void FindText_LostFocus(object sender, RoutedEventArgs e)
        {
            this.SetStatusText("");
        }

        public void ReplaceSelectedText(string replacementText)
        {
            if (this.currentTabIndex >= 0 && this.currentTabIndex < this.tab.Count)
            {
                if (this.tab[this.currentTabIndex].TextEditor.SelectedText.Length > 0)
                {
                    this.tab[this.currentTabIndex].TextEditor.SelectedText = replacementText;
                }
            }
        }

        public void ReplaceStringOnPage(string searchText, string replacementText, bool matchCase, bool useRegex)
        {
            if (replacementText.Length > 0 && this.currentTabIndex >= 0 && this.currentTabIndex < this.tab.Count && searchText != replacementText)
            {
                Object[] parameters = new Object[7];
                string text = tab[this.currentTabIndex].TextEditor.Text;
                TextEditor textEditor = tab[this.currentTabIndex].TextEditor;
                textEditor.BeginChange();
                parameters[0] = text;
                parameters[1] = searchText;
                parameters[2] = false;
                parameters[3] = matchCase; //matchCase;
                parameters[4] = useRegex; //useRegex;
                parameters[5] = replacementText;
                parameters[6] = textEditor;
                this.UpdateFind_WorkerThread(parameters);
                textEditor.EndChange();
            }
        }
        #endregion

        #region Public API
        internal Tab GetActiveTab()
        {
            if (this.currentTabIndex >= 0 && this.currentTabIndex < this.tab.Count)
            {
                return this.tab[this.currentTabIndex];
            }
            else
            {
                return null;
            }
        }        
        #endregion
    }
}
