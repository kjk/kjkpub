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
    /// Interaction logic for StyledMessageBox.xaml
    /// </summary>
    public partial class StyledMessageBox : Window
    {
        internal bool OkButtonClicked;        

        public string Title
        {
            get { return this.MessageTitle.Text; }
            set { this.MessageTitle.Text = value; }
        }

        public string Message
        {
            get { return this.MessageTextBlock.Text;}
            set { this.MessageTextBlock.Text = value;}
        }

        public StyledMessageBox()
        {
            InitializeComponent();
            OkButtonClicked = false;
        }

        public static bool Show(string title, string message, bool onDarkBackground = false)
        {            
            StyledMessageBox messageBox = new StyledMessageBox();
            messageBox.Title = title;
            messageBox.Message = message;
            messageBox.Owner = Application.Current.MainWindow;
            if (onDarkBackground)
            {
                messageBox.ShadowBorder.Color = Colors.WhiteSmoke;
                messageBox.ShadowBorder.Opacity = 0.58;
            }
            messageBox.ShowDialog();
            
            return messageBox.OkButtonClicked;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            OkButtonClicked = true;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            OkButtonClicked = false;
            this.Close();
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            cancelButton.Focus();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                OkButtonClicked = false;
                this.Close();
            }
        }
    }
}
