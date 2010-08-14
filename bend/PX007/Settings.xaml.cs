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
using Microsoft.Win32;
using System.Deployment.Application;
using EfTidyNet;

namespace Bend
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : UserControl
    {
        public Settings()
        {
            InitializeComponent();
        }

        private void CheckForUpdates_Click(object sender, RoutedEventArgs e)
        {
            UpdateCheckInfo info = null;

            if (ApplicationDeployment.IsNetworkDeployed)
            {
                ApplicationDeployment ad = ApplicationDeployment.CurrentDeployment;

                try
                {
                    info = ad.CheckForDetailedUpdate();

                }
                catch (DeploymentDownloadException dde)
                {
                    StyledMessageBox.Show("UPDATE", "The new version of the application cannot be downloaded at this time.\nPlease check your network connection, or try again later. Error: " + dde.Message, true);                    
                    return;
                }
                catch (InvalidDeploymentException ide)
                {
                    StyledMessageBox.Show("UPDATE", "Cannot check for a new version of the application. The ClickOnce deployment is corrupt. Please redeploy the application and try again. Error: " + ide.Message, true);
                    return;
                }
                catch (InvalidOperationException ioe)
                {
                    StyledMessageBox.Show("UPDATE", "This application cannot be updated. It is likely not a ClickOnce application. Error: " + ioe.Message, true);
                    return;
                }

                if (info.UpdateAvailable)
                {
                    Boolean doUpdate = true;

                    if (!info.IsUpdateRequired)
                    {
                        if (!StyledMessageBox.Show("UPDATE", "An update is available. Choose OK to start update.", true))
                        {
                            doUpdate = false;
                        }
                    }
                    else
                    {
                        // Display a message that the app MUST reboot. Display the minimum required version.
                        StyledMessageBox.Show("UPDATE", "This application has detected a mandatory update from your current " +
                            "version to version " + info.MinimumRequiredVersion.ToString() + ". The application will now install the update.", true);
                    }

                    if (doUpdate)
                    {
                        try
                        {
                            ad.Update();
                            StyledMessageBox.Show("UPDATE", "The application has been upgraded, please save your work and restart application.", true);
                        }
                        catch (DeploymentDownloadException dde)
                        {
                            StyledMessageBox.Show("UPDATE", "Cannot install the latest version of the application.\nPlease check your network connection, or try again later. Error: " + dde, true);
                            return;
                        }
                    }
                }
                else
                {
                    StyledMessageBox.Show("UPDATE", "You already have the latest version of this application.", true);
                }
            }
        }

        private void EnableContextMenu_Click(object sender, RoutedEventArgs e)
        {   
            try
            {
                RegistryKey HKCU = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64);
                RegistryKey HKCU_STAR_SHELL = HKCU.CreateSubKey("Software");
                HKCU_STAR_SHELL = HKCU_STAR_SHELL.CreateSubKey("Classes");

                // Attempt to write the class id that describes bend
                {
                    RegistryKey HKCR_CLSID_UNIQUE = HKCU_STAR_SHELL.CreateSubKey("CLSID");
                    HKCR_CLSID_UNIQUE = HKCR_CLSID_UNIQUE.CreateSubKey("{29C436A6-392B-4069-8DF7-760271B08F67}");
                    HKCR_CLSID_UNIQUE.SetValue("", "Bend - A modern text editor (Explorer right click menu integration)");
                    string applicationId = "Bend.application, Culture = neutral, PublicKeyToken = 0000000000000000, processorArchitecture = x86";
                    HKCR_CLSID_UNIQUE.SetValue("AppId", applicationId);
                    HKCR_CLSID_UNIQUE.SetValue("DeploymentProviderUrl", "http://bend.codeplex.com/releases/clickonce/Bend.application");
                }

                // Write the registry entries that add bend to the right click menu
                HKCU_STAR_SHELL = HKCU_STAR_SHELL.CreateSubKey("*");
                HKCU_STAR_SHELL = HKCU_STAR_SHELL.CreateSubKey("Shell");
                RegistryKey BendShortcutKey = HKCU_STAR_SHELL.CreateSubKey("Bend");
                BendShortcutKey.SetValue("", "Bend file");
                string BendExePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                BendShortcutKey.SetValue("Icon", BendExePath + ",0");                
                BendShortcutKey.CreateSubKey("Command").SetValue("", "rundll32.exe dfshim.dll, ShOpenVerbExtension {29C436A6-392B-4069-8DF7-760271B08F67} %1");
            }
            catch
            {                
            }
            this.UpdateButtons();
        }

        private void DisableContextMenu(object sender, RoutedEventArgs e)
        {
            try
            {
                RegistryKey HKCU = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64);
                RegistryKey HKCU_STAR_SHELL = HKCU.CreateSubKey("Software");
                HKCU_STAR_SHELL = HKCU_STAR_SHELL.CreateSubKey("Classes");
                HKCU_STAR_SHELL.CreateSubKey("CLSID").DeleteSubKeyTree("{29C436A6-392B-4069-8DF7-760271B08F67}");
                HKCU_STAR_SHELL = HKCU_STAR_SHELL.CreateSubKey("*");
                HKCU_STAR_SHELL = HKCU_STAR_SHELL.CreateSubKey("Shell");
                HKCU_STAR_SHELL.DeleteSubKeyTree("Bend");
            }
            catch
            {
            }
            this.UpdateButtons();
        }

        private void UpdateButtons()
        {
            try
            {
                RegistryKey HKCU = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64);
                RegistryKey HKCU_SOFTWARE_CLASSES = HKCU.OpenSubKey("Software").OpenSubKey("Classes");
                RegistryKey BendShortcutKey = HKCU_SOFTWARE_CLASSES.OpenSubKey("*").OpenSubKey("Shell").OpenSubKey("Bend");
                if (BendShortcutKey.GetValue("").ToString() == "Bend file")
                {
                    string BendExePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                    if (BendShortcutKey.GetValue("Icon").ToString() == BendExePath + ",0")
                    {
                        if (BendShortcutKey.OpenSubKey("Command").GetValue("").ToString() == "rundll32.exe dfshim.dll, ShOpenVerbExtension {29C436A6-392B-4069-8DF7-760271B08F67} %1")
                        {
                            RegistryKey HKCU_CLSID_UNIQUE = HKCU_SOFTWARE_CLASSES.OpenSubKey("CLSID").OpenSubKey("{29C436A6-392B-4069-8DF7-760271B08F67}");
                            if (HKCU_CLSID_UNIQUE.GetValue("").ToString() == "Bend - A modern text editor (Explorer right click menu integration)" &&
                                HKCU_CLSID_UNIQUE.GetValue("AppId").ToString() == "Bend.application, Culture = neutral, PublicKeyToken = 0000000000000000, processorArchitecture = x86" &&
                                HKCU_CLSID_UNIQUE.GetValue("DeploymentProviderUrl").ToString() == "http://bend.codeplex.com/releases/clickonce/Bend.application")
                            {
                                DisableContextMenuButton.IsEnabled = true;
                                EnableContextMenuButton.IsEnabled = false;
                            }
                            else throw new Exception();                            
                        }
                        else throw new Exception();
                    }
                    else throw new Exception();
                }
                else throw new Exception();
            }
            catch
            {
                DisableContextMenuButton.IsEnabled = false;
                EnableContextMenuButton.IsEnabled = true;
            }            
        }

        private void ControlInitialized(object sender, EventArgs e)
        {
            this.UpdateButtons();
            CheckForUpdatesButton.IsEnabled = ApplicationDeployment.IsNetworkDeployed;

            // Load defaults from persistant storage            
            JSBeautifyPreserveLine.IsChecked = PersistantStorage.StorageObject.JSBeautifyPreserveLine;
            JSBeautifyIndent.Text = PersistantStorage.StorageObject.JSBeautifyIndent.ToString();
            JSBeautifyUseSpaces.IsChecked = PersistantStorage.StorageObject.JSBeautifyUseSpaces;
            JSBeautifyUseTabs.IsChecked = PersistantStorage.StorageObject.JSBeautifyUseTabs;

            this.UpdateOptions();
        }

        private void AppendToPath_Click(object sender, RoutedEventArgs e)
        {            
            try
            {
                RegistryKey HKCU_ENVIRONMENT = Registry.CurrentUser.CreateSubKey("Environment");
                string BendExePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string bendDirectory = System.IO.Path.GetDirectoryName(BendExePath);
                string currentPath;
                if (HKCU_ENVIRONMENT.GetValue("Path") != null)
                {
                    currentPath = HKCU_ENVIRONMENT.GetValue("Path").ToString();
                    if (currentPath.IndexOf(bendDirectory) >= 0)
                    {
                        // We are already in the path nothing to do here.
                        throw new Exception();
                    }
                }
                else
                {
                    currentPath = "";
                }
                currentPath = bendDirectory + ";" + currentPath;
                HKCU_ENVIRONMENT.SetValue("Path", currentPath);                
            }
            catch
            {                
            }
        }
 
        private void Tab_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                TabItem tabItem = (TabItem)sender;
                TabControl tabControl = (TabControl)tabItem.Parent;                
                for (int i = 0; i < tabControl.Items.Count; i++)
                {
                    ((Label)((TabItem)tabControl.Items[i]).Header).Foreground = Brushes.Gray;
                }

                Label header = (Label)tabItem.Header;
                header.Foreground = Brushes.WhiteSmoke;
            }
            catch
            {
            }
        }

        private Tab CurrentTab()
        {
            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
            if (mainWindow != null)
            {
                return mainWindow.GetActiveTab();
            }
            else
            {
                return null;
            }
        }

        private void CancelSettingsUI()
        {
            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
            if (mainWindow != null)
            {
                mainWindow.CancelSettingsUI();
            }
        }

        private Plugins.JSBeautifyOptions GetAndPersistJsBeautifyOptions()
        {
            Plugins.JSBeautifyOptions jsBeautifyOptions = new Plugins.JSBeautifyOptions();
            if (this.JSBeautifyUseSpaces.IsChecked ?? true)
            {
                jsBeautifyOptions.indent_char = ' ';
                PersistantStorage.StorageObject.JSBeautifyUseSpaces = true;
                PersistantStorage.StorageObject.JSBeautifyUseTabs = false;
            }
            if (this.JSBeautifyUseTabs.IsChecked ?? true)
            {
                jsBeautifyOptions.indent_char = '\t';
                PersistantStorage.StorageObject.JSBeautifyUseTabs = true;
                PersistantStorage.StorageObject.JSBeautifyUseSpaces = false;
            }
            
            int indentSize; 
            if (int.TryParse(JSBeautifyIndent.Text, out indentSize))
            {
                jsBeautifyOptions.indent_size = indentSize;
                PersistantStorage.StorageObject.JSBeautifyIndent = indentSize;
            }

            if (JSBeautifyPreserveLine.IsChecked ?? true) 
            {
                jsBeautifyOptions.preserve_newlines = true;
                PersistantStorage.StorageObject.JSBeautifyPreserveLine = true;
            }
            else
            {
                jsBeautifyOptions.preserve_newlines = false;
                PersistantStorage.StorageObject.JSBeautifyPreserveLine = false;
            }
            return jsBeautifyOptions;
        }

        private void JSBeautifyFile(object sender, RoutedEventArgs e)
        {
            try
            {   
                CurrentTab().TextEditor.BeginChange();
                Plugins.JSBeautify jsBeautify = new Plugins.JSBeautify(CurrentTab().TextEditor.Text, GetAndPersistJsBeautifyOptions());
                string newFile = jsBeautify.GetResult();
                CurrentTab().TextEditor.ReplaceText(0, CurrentTab().TextEditor.Text.Length, newFile);
                CurrentTab().TextEditor.EndChange();
                this.CancelSettingsUI();
            }
            catch
            {
            }
        }

        private void JSBeautifySelection(object sender, RoutedEventArgs e)
        {
            try
            {
                Plugins.JSBeautify jsBeautify = new Plugins.JSBeautify(CurrentTab().TextEditor.SelectedText, GetAndPersistJsBeautifyOptions());
                string formattedScript = jsBeautify.GetResult();
                CurrentTab().TextEditor.SelectedText = formattedScript;
                this.CancelSettingsUI();
            }
            catch
            {
            }
        }

        private void AllowOnlyDigits_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!(e.Key == Key.D0 ||
                e.Key == Key.D1 ||
                e.Key == Key.D2 ||
                e.Key == Key.D3 ||
                e.Key == Key.D4 ||
                e.Key == Key.D5 ||
                e.Key == Key.D6 ||
                e.Key == Key.D7 ||
                e.Key == Key.D8 ||
                e.Key == Key.D9 ||
                e.Key == Key.Enter ||
                e.Key == Key.Back ||
                e.Key == Key.Escape ||
                e.Key == Key.Delete))
            {
                e.Handled = true;
            }            
        }

        private void HTMLTidyProcessFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                String tidyHTML = "";
                TidyNet objTidyNet = new TidyNet();

                // Set up options
                objTidyNet.Option.Clean(true);
                objTidyNet.Option.NewInlineTags("tidy");
                objTidyNet.Option.OutputType(EfTidyNet.EfTidyOpt.EOutputType.XhtmlOut);
                objTidyNet.Option.DoctypeMode(EfTidyNet.EfTidyOpt.EDoctypeModes.DoctypeAuto);
                objTidyNet.Option.Indent(EfTidyNet.EfTidyOpt.EIndentScheme.AUTOINDENT);
                objTidyNet.Option.TabSize(4);
                objTidyNet.Option.IndentSpace(4);

                objTidyNet.TidyMemToMem(CurrentTab().TextEditor.Text, ref tidyHTML);

                int totalWarnings = 0;
                int totalErrors = 0;
                objTidyNet.TotalWarnings(ref totalWarnings);
                objTidyNet.TotalErrors(ref totalErrors);
                string error = objTidyNet.ErrorWarning();

                if (StyledMessageBox.Show("HTML TIDY FINISHED WITH " + totalErrors.ToString() + " ERRORS AND " + totalWarnings.ToString() + " WARNINGS",
                    error,
                    true))
                {
                    CurrentTab().TextEditor.ReplaceText(0, CurrentTab().TextEditor.Text.Length, tidyHTML);
                }
                this.CancelSettingsUI();
            }
            catch
            {
            }
        }

        private void UpdateOptions()
        {
            PersistantStorage persistantStorage = PersistantStorage.StorageObject;
            TextUseSpaces.IsChecked = persistantStorage.TextUseSpaces ? true : false;
            TextUseTabs.IsChecked = persistantStorage.TextUseTabs ? true : false;
            TextIndent.Text = persistantStorage.TextIndent.ToString();
            TextStyleControlCharacters.IsChecked = persistantStorage.TextFormatControlCharacters ? true : false;
            TextFormatHyperLinks.IsChecked = persistantStorage.TextFormatHyperLinks ? true : false;
            TextFormatEmailLinks.IsChecked = persistantStorage.TextFormatEmailLinks ? true : false;
            TextFormatShowFormatting.IsChecked = persistantStorage.TextShowFormatting ? true : false;
            TextWordWrap.IsChecked = persistantStorage.TextWordWrap ? true : false;
        }

        private void OptionsCancel_Click(object sender, RoutedEventArgs e)
        {
            this.UpdateOptions();
            this.CancelSettingsUI();
        }

        private void OptionsSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                PersistantStorage persistantStorage = PersistantStorage.StorageObject;
                persistantStorage.TextUseSpaces = TextUseSpaces.IsChecked ?? true;
                persistantStorage.TextUseTabs = TextUseTabs.IsChecked ?? true;

                int indentSize;
                if (int.TryParse(TextIndent.Text, out indentSize))
                {
                    persistantStorage.TextIndent = indentSize;
                }

                persistantStorage.TextFormatControlCharacters = TextStyleControlCharacters.IsChecked ?? true;
                persistantStorage.TextFormatHyperLinks = TextFormatHyperLinks.IsChecked ?? true;
                persistantStorage.TextFormatEmailLinks = TextFormatEmailLinks.IsChecked ?? true;
                persistantStorage.TextShowFormatting = TextFormatShowFormatting.IsChecked ?? true;
                persistantStorage.TextWordWrap = TextWordWrap.IsChecked ?? true;
                CurrentTab().LoadOptions();
                this.CancelSettingsUI();
            }
            catch
            {
                
            }
        }
    }
}
