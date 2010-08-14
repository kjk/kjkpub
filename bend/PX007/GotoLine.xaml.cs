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
using System.Windows.Shapes;

namespace Bend
{
    /// <summary>
    /// Interaction logic for GotoLine.xaml
    /// </summary>
    public partial class GotoLine : Window
    {
        #region Member Data
        MainWindow mainWindow;
        static GotoLine singletonGotoLineWindow;
        #endregion  
        
        static GotoLine()
        {
            singletonGotoLineWindow = null;
        }

        private GotoLine(MainWindow mainWindow)
        {
            InitializeComponent();
            this.mainWindow = mainWindow;
        }

        private void Close_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.Hide();
        }

        private void TitleBar_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
        
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Hide();
                e.Handled = true;
            }
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (e.Key == Key.G)
                {
                    this.Hide();
                    e.Handled = true;
                }
            }
        }

        public static void ShowGotoLineWindow(MainWindow mainWindow)
        {
            if (singletonGotoLineWindow == null)
            {
                singletonGotoLineWindow = new GotoLine(mainWindow);
                singletonGotoLineWindow.Owner = mainWindow;
            }
            singletonGotoLineWindow.lineNumber.Text = "";
            singletonGotoLineWindow.Show();
            singletonGotoLineWindow.Focus();
        }

        private void lineNumber_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    int number = int.Parse(lineNumber.Text);
                    mainWindow.CommandGoto(number);
                    singletonGotoLineWindow.Hide();
                }                
                catch
                {

                }
            }
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int number = int.Parse(lineNumber.Text);
                mainWindow.CommandGoto(number);
                singletonGotoLineWindow.Hide();
            }
            catch
            {

            }
        }

        private void lineNumber_PreviewKeyDown(object sender, KeyEventArgs e)
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
    }
}
