using System;
using System.Reflection;
using System.Windows;

namespace VerisFlow.VenusDeckParser
{
    public partial class AboutBox : Window
    {
        public AboutBox()
        {
            InitializeComponent();

            // This ensures the AboutBox uses the same icon as the MainWindow
            if (Application.Current.MainWindow != null && Application.Current.MainWindow != this)
            {
                this.Icon = Application.Current.MainWindow.Icon;
            }

            Version version = Assembly.GetExecutingAssembly().GetName().Version;

            if (version != null)
            {
                string versionText = $"Version: {version.Major}.{version.Minor}.{version.Build}";
                VersionTextBlock.Text = versionText;
            }
        }
    }
}