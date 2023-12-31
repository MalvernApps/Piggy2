﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
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

using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;
using UglyToad.PdfPig;
using System.Globalization;

using System.Net;
using System.Security.Policy;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Piggy2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<ResultLine> myResults = new List<ResultLine>();
        List<string> alllinks = new List<string>();

        int index = 0;

        string AllPDFFiles = "targets2.txt";
    
        public MainWindow()
        {
            InitializeComponent();

            CreateDir();
            GetAListOfMine();
        }

        private void CreateDir()
        {
            if ( !Directory.Exists(@".\PDFs"))
            {
                Directory.CreateDirectory(@".\PDFs");
            }
        }

        public bool TestMyList( string input )
        {
            bool res;
            foreach( string str in myFunds )
            {
                res = input.Contains( str );
                Trace.WriteLine( str );
                Trace.WriteLine(input);
                Trace.WriteLine(res);

                if (res == true) 
                    return true;
                 
            }

            return false;

        }

        private void doit()
        {
            var targets = Directory.GetFiles("PDFs").OrderBy(f => f);

            foreach (string filename in targets)
            {
                ResultLine rl = new ResultLine();

                string[] decoded = filename.Split('_');
                string[] spl = decoded[1].Split('.');

                int index = int.Parse(spl[0]);
                string u = alllinks[index - 1];

                Trace.WriteLine("###########" + filename );
                rl.download_Link = u;  
                
                rl.IsMine = TestMyList( u );

                using (var pdf = PdfDocument.Open(filename))
                {
                    foreach (var page in pdf.GetPages())
                    {

                        try
                        {
                            getData(rl, page);
                        }
                        catch( Exception ex) 
                        { 
                            Trace.WriteLine ( filename + ": " + ex.Message );   
                        }
                    }



                }
                myResults.Add(rl);
            }

            List<ResultLine> SortedList = myResults .OrderByDescending(o => o.mon3).ToList();
            myData.ItemsSource = SortedList; 



            }

        private static void getData(ResultLine rl, UglyToad.PdfPig.Content.Page page)
        {
            var text = ContentOrderTextExtractor.GetText(page);

            // Or based on grouping letters into words.
            var otherText = string.Join(" ", page.GetWords());

            // Or the raw text of the page's content stream.
            var rawText = page.Text;

            string[] result = text.Split(new[] { '\r', '\n' });

            foreach (string str in result)
            {
                if (str.Contains("SW Mercer"))
                {
                    Trace.WriteLine(str);
                    rl.FundName = str;
                }

                if (str.StartsWith(@"Fund "))
                {
                    string[] test = str.Split(' ');
                    if (test.Count() == 6)
                    {
                        Trace.WriteLine(str);
                        if (rl.Performance == null)
                        {
                            try
                            {
                                string bob = str.Replace("Fund ", "");
                                bob = bob.Replace("%", "");
                                rl.Performance = bob;
                                string[] spl = bob.Split(' ');

                                rl.mon3 = double.Parse(spl[0], CultureInfo.InvariantCulture);
                                rl.mon6 = double.Parse(spl[1], CultureInfo.InvariantCulture);
                                rl.mon12 = double.Parse(spl[2], CultureInfo.InvariantCulture);

                                if (spl[3] != "-")
                                    rl.Month18 = double.Parse(spl[3], CultureInfo.InvariantCulture);

                                // note if the data does not exist we get a '-' so check for it
                                if (spl[4] != "-")
                                    rl.mon60 = double.Parse(spl[4], CultureInfo.InvariantCulture);
                            }
                            catch( Exception ex)
                            {
                                Trace.WriteLine("formatting error caught: " + ex.Message);
                            }
                        }
                    }

                }
            }
        }

        public void ConfigureList(string filename)
        {
            var lines = File.ReadAllLines(filename);
            for (var i = 0; i < lines.Length; i += 1)
            {
                var line = lines[i];
                alllinks.Add(line);              
            }

        }

        public void pdfsV2( string filename)
        {
            var lines = File.ReadAllLines(filename);
            for (var i = 0; i < lines.Length; i += 1)
            {
                var line = lines[i];
                alllinks.Add(line);
                Trace.WriteLine( line);
                status.Content = line;  
                GetPDF( line  );
                // Process line
            }

        }

        public void GetPDF(string address)
        {
            index++;
            WebClient webClient = new WebClient();
            string filename = @"pdfs/myfile_" + index + ".pdf" ;
            webClient.DownloadFile(address, filename);

            Trace.WriteLine( "??????????" + filename + address );
        }

        /// <summary>
        /// download the files
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menuDownload(object sender, RoutedEventArgs e)
        {
            pdfsV2(AllPDFFiles);

            MessageBox.Show("Downloading Finished", "Information", MessageBoxButton.OK, MessageBoxImage.Information );
           
        }

        private void menuViewPdfs(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", @".\PDFs");
        }

    

        private void menuProcess(object sender, RoutedEventArgs e)
        {
            ConfigureList(AllPDFFiles);
            doit();
        }

        private void openURL(object sender, MouseButtonEventArgs e)
        {
            ResultLine rl = (ResultLine)myData.SelectedItem;
            GoToSite( rl.download_Link);
        }

        public static void GoToSite(string url)
        {
            Process.Start("chrome.exe", url);
           
        }

        /// <summary>
        /// refer https://stackoverflow.com/questions/13579034/how-do-you-rename-datagrid-columns-when-autogeneratecolumns-true
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            var displayName = GetPropertyDisplayName(e.PropertyDescriptor);

            if (!string.IsNullOrEmpty(displayName))
            {
                e.Column.Header = displayName;
            }
        }       

        public static string GetPropertyDisplayName(object descriptor)
        {
            var pd = descriptor as PropertyDescriptor;

            if (pd != null)
            {
                // Check for DisplayName attribute and set the column header accordingly
                var displayName = pd.Attributes[typeof(DisplayNameAttribute)] as DisplayNameAttribute;

                if (displayName != null && displayName != DisplayNameAttribute.Default)
                {
                    return displayName.DisplayName;
                }

            }
            else
            {
                var pi = descriptor as PropertyInfo;

                if (pi != null)
                {
                    // Check for DisplayName attribute and set the column header accordingly
                    Object[] attributes = pi.GetCustomAttributes(typeof(DisplayNameAttribute), true);
                    for (int i = 0; i < attributes.Length; ++i)
                    {
                        var displayName = attributes[i] as DisplayNameAttribute;
                        if (displayName != null && displayName != DisplayNameAttribute.Default)
                        {
                            return displayName.DisplayName;
                        }
                    }
                }
            }

            return null;
        }

        private void menuHome(object sender, RoutedEventArgs e)
        {
            GoToSite(@"https://money4life.scottishwidows.co.uk/");
        }


        /// <summary>
        /// https://stackoverflow.com/questions/1288718/how-to-delete-all-files-and-folders-in-a-directory
        /// </summary>
        private void DeleteAllpdfs()
        {
            System.IO.DirectoryInfo di = new DirectoryInfo("PDFs");

            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }

        }
        /// <summary>
        /// delete all pdfs downloads
        ///  https://stackoverflow.com/questions/1288718/how-to-delete-all-files-and-folders-in-a-directory
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menuDeletePdfs(object sender, RoutedEventArgs e)
        {
            DeleteAllpdfs();
            MessageBox.Show("Deletion Finished", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void menuDownloadMine(object sender, RoutedEventArgs e)
        {
            pdfsV2("MyFunds.txt");
        }

        private void menuProcessMine(object sender, RoutedEventArgs e)
        {
            ConfigureList("MyFunds.txt");
            doit();
        }

        // a list containing all my funds
        List<string>  myFunds = new List<string>();

        public void GetAListOfMine()
        {
            var lines = File.ReadAllLines("MyFunds.txt");
            for (var i = 0; i < lines.Length; i += 1)
            {
                var line = lines[i];
                myFunds.Add(line);
            }

        }
    }
}
