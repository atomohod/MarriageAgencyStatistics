using System;
using System.Collections.Generic;
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
using Microsoft.Win32;
using RestSharp;
using DateTime = System.DateTime;

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

            this.DataContext = new MainViewModel(new RestClient(@"http://marriageagencystatistics.azurewebsites.net/")
            {
                Timeout = 300000,
                ReadWriteTimeout = 300000
            });
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var settings = ConfigurationManager.AppSettings;

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                AddExtension = false,
                FileName = settings["path"]
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                (DataContext as MainViewModel).Path = saveFileDialog.FileName;
                
                settings.Set("path", saveFileDialog.FileName);
                

            }
        }
    }
}
