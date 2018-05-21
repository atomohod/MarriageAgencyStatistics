using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MarriageAgencyStatistics.Applications;
using Microsoft.Win32;
using RestSharp;
using DateTime = System.DateTime;
using Path = System.IO.Path;

namespace MarriageAgencyStatistics.DesktopClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();

            this.DataContext = new MainViewModel(new BrideForeverApp(new RestClient(@"http://marriageagencystatistics.azurewebsites.net/")
            {
                Timeout = 100000000,
                ReadWriteTimeout = 100000000
            }));
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var settings = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                FileName = Path.GetFileName(ConfigurationManager.AppSettings["path"]),
                Filter = "Excel (*.xlsx)|*.xlsx",
            InitialDirectory = Path.GetDirectoryName(ConfigurationManager.AppSettings["path"])
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                (DataContext as MainViewModel).Path = saveFileDialog.FileName;
                
                settings.AppSettings.Settings.Add("path", saveFileDialog.FileName);
                settings.Save(ConfigurationSaveMode.Modified);
            }
}
    }
}
