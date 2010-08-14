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
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using Microsoft.Windows.Shell;
using Microsoft.Win32;

namespace Bend
{
    class Tab
    {
        #region Member data
            private WrapPanel title;
            private Label titleText;            
            private TextEditor textEditor;
            private String fullFileName;
            private Image closeButton;   

            private static FontFamily fontFamilySegoeUI;
            private static FontFamily fontFamilyConsolas;

            private System.IO.FileSystemWatcher fileChangedWatcher;
            long lastFileChangeTime;
            private static System.Threading.Semaphore showFileModifiedDialog = new System.Threading.Semaphore(1, 1);
        #endregion

        #region Properties
            internal WrapPanel Title {
                get { return title; }
            }

            internal UIElement CloseButton {
                get { return closeButton; }
            }

            internal TextEditor TextEditor {
                get { return textEditor; }
            }
            internal String FullFileName {
                get { return fullFileName; }                
            }
        #endregion

        #region Constructor
            static Tab()
            {
                // Static constructor
                fontFamilySegoeUI = new FontFamily("Segoe UI");
                fontFamilyConsolas = new FontFamily("Consolas");                
            }

            public Tab()
            {
                title = new WrapPanel();                
                titleText = new Label();
                titleText.Content = "New File";
                titleText.Width = 110;
                titleText.Height = 34;
                titleText.VerticalAlignment = VerticalAlignment.Top;
                titleText.VerticalContentAlignment = VerticalAlignment.Center;
                titleText.HorizontalContentAlignment = HorizontalAlignment.Center;
                titleText.FontFamily = Tab.fontFamilySegoeUI;
                titleText.IsTabStop = true;
                Microsoft.Windows.Shell.WindowChrome.SetIsHitTestVisibleInChrome(titleText, /*isHitTestable*/true);
                title.Children.Add(titleText);

                Separator seperator = new Separator();
                seperator.Width = 5;
                seperator.Visibility = Visibility.Hidden;
                title.Children.Add(seperator);

                closeButton = new Image();
                closeButton.Width = 8;
                closeButton.Height = 8;
                BitmapImage closeImage = new BitmapImage();
                closeImage.BeginInit();
                closeImage.UriSource = new Uri("pack://application:,,,/Bend;component/Images/Close-dot.png");
                closeImage.EndInit();                
                closeButton.Source = closeImage;
                closeButton.Margin = new Thickness(0, 6, 0, 0);
                Microsoft.Windows.Shell.WindowChrome.SetIsHitTestVisibleInChrome(closeButton, /*isHitTestable*/true);
                title.Children.Add(closeButton);

                Microsoft.Windows.Shell.WindowChrome.SetIsHitTestVisibleInChrome(title, /*isHitTestable*/true);

                textEditor = new TextEditor();
                textEditor.HorizontalAlignment = HorizontalAlignment.Stretch;
                textEditor.Margin = new Thickness(0);
                textEditor.VerticalAlignment = VerticalAlignment.Stretch;
                textEditor.ShowLineNumbers = true;
                textEditor.FontFamily = Tab.fontFamilyConsolas;
                textEditor.FontSize = 14;
                textEditor.PreviewMouseWheel += Tab.EditorPreviewMouseWheel;
                textEditor.PreviewKeyDown += Tab.EditorPreviewKeyDown;                
                
                this.fileChangedWatcher = null;
                this.lastFileChangeTime = 1;
                this.LoadOptions();
            }
        #endregion

        #region Public API
        private void SetFullFileName(String fullFileName)
        {
            if (this.fullFileName != fullFileName)
            {
                 // File changed
                this.textEditor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinitionByExtension(System.IO.Path.GetExtension(fullFileName));

                if (this.fileChangedWatcher != null)
                {
                    this.fileChangedWatcher.EnableRaisingEvents = false;
                    this.fileChangedWatcher.Dispose();
                    this.fileChangedWatcher = null;
                }

                try
                {
                    this.fileChangedWatcher = new System.IO.FileSystemWatcher(System.IO.Path.GetDirectoryName(fullFileName), System.IO.Path.GetFileName(fullFileName));
                    this.fileChangedWatcher.NotifyFilter = System.IO.NotifyFilters.LastWrite;
                    this.fileChangedWatcher.Changed += new System.IO.FileSystemEventHandler(fileChangedWatcher_Changed);
                    this.fileChangedWatcher.EnableRaisingEvents = true;
                }
                catch
                {
                    // For some reason openeing files from temp folder hits this.
                }
            }

            this.fullFileName = fullFileName;
            this.titleText.Content = System.IO.Path.GetFileName(fullFileName);
            this.title.ToolTip = fullFileName;            
        }

        internal void OpenFile(String fullFileName)
        {
            this.textEditor.Load(fullFileName);            
            this.SetFullFileName(fullFileName);
        }

        internal void SaveFile(String fullFileName)
        {
            System.Threading.Interlocked.Exchange(ref this.lastFileChangeTime, System.DateTime.Now.AddSeconds(2).Ticks);
            this.TextEditor.Save(fullFileName);            
            this.SetFullFileName(fullFileName);
        }

        void fileChangedWatcher_Changed(object sender, System.IO.FileSystemEventArgs e)
        {
            if (this.lastFileChangeTime < System.DateTime.Now.Ticks)
            {
                System.Threading.Interlocked.Exchange(ref this.lastFileChangeTime, System.DateTime.Now.AddSeconds(2).Ticks);
                object[] copyOfEventArgs = { e };
                showFileModifiedDialog.WaitOne();
                System.Threading.Interlocked.Exchange(ref this.lastFileChangeTime, System.DateTime.Now.AddSeconds(2).Ticks);
                titleText.Dispatcher.BeginInvoke(new fileChangedWatcher_ChangedInUIThread_Delegate(fileChangedWatcher_ChangedInUIThread), copyOfEventArgs);
            }
        }
                
        private delegate void fileChangedWatcher_ChangedInUIThread_Delegate(System.IO.FileSystemEventArgs e);
        internal void fileChangedWatcher_ChangedInUIThread(System.IO.FileSystemEventArgs e)
        {
            double originalOpacity = this.Title.Opacity;
            this.Title.Opacity = 0.2;
            if (StyledMessageBox.Show("FILE MODIFIED", e.FullPath + "\n\nwas modified outside this application, do you want to reload ?"))
            {
                this.OpenFile(this.fullFileName);
                System.Threading.Interlocked.Exchange(ref this.lastFileChangeTime, System.DateTime.Now.AddSeconds(2).Ticks);
            }
            this.Title.Opacity = originalOpacity;
            showFileModifiedDialog.Release();
        }

        private static void EditorPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                switch (e.Key)
                {
                    case Key.Add:
                    case Key.OemPlus:
                        // Zoom In
                        {
                            Control control = (Control)sender;
                            double fontSize = control.FontSize + 1;
                            if (fontSize > 0)
                            {
                                control.FontSize = fontSize;
                            }
                            else
                            {
                                control.FontSize = 1;
                            }
                            e.Handled = true;
                        }
                        break;
                    case Key.Subtract:
                    case Key.OemMinus:
                        // Zoom Out
                        {
                            Control control = (Control)sender;
                            double fontSize = control.FontSize - 1;
                            if (fontSize > 0)
                            {
                                control.FontSize = fontSize;
                            }
                            else
                            {
                                control.FontSize = 1;
                            }
                            e.Handled = true;
                        }
                        break;
                    case Key.D0:
                        {
                            // Reset Zoom
                            Control control = (Control)sender;
                            control.FontSize = 14;
                            e.Handled = true;
                        }
                        break;
                }
            }
        }

        private static void EditorPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                Control control = (Control)sender;
                double fontSize = control.FontSize + (e.Delta > 0 ? 1 : -1);
                if (fontSize > 0)
                {
                    control.FontSize = fontSize;
                }
                else
                {
                    control.FontSize = 1;
                }
                e.Handled = true;
            }
        }

        internal void Close()
        {
            if (this.fileChangedWatcher != null)
            {
                this.fileChangedWatcher.EnableRaisingEvents = false;
                this.fileChangedWatcher.Dispose();
                this.fileChangedWatcher = null;
            }
        }

        internal void LoadOptions()
        {
            this.textEditor.Options.ConvertTabsToSpaces = PersistantStorage.StorageObject.TextUseSpaces;
            this.textEditor.Options.IndentationSize = PersistantStorage.StorageObject.TextIndent;
            this.textEditor.Options.ShowBoxForControlCharacters = PersistantStorage.StorageObject.TextFormatControlCharacters;
            this.textEditor.Options.EnableHyperlinks = PersistantStorage.StorageObject.TextFormatHyperLinks;
            this.textEditor.Options.EnableEmailHyperlinks = PersistantStorage.StorageObject.TextFormatEmailLinks;
            if (PersistantStorage.StorageObject.TextShowFormatting)
            {
                this.textEditor.Options.ShowSpaces = true;
                this.textEditor.Options.ShowTabs = true;
                this.textEditor.Options.ShowEndOfLine = true;
            }
            else
            {
                this.textEditor.Options.ShowSpaces = false;
                this.textEditor.Options.ShowTabs = false;
                this.textEditor.Options.ShowEndOfLine = false;
            }
            this.textEditor.WordWrap = PersistantStorage.StorageObject.TextWordWrap;
        }
        #endregion
    }
}
