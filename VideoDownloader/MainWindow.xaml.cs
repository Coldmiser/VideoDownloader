using System;
using System.Collections.Generic;
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
using System.Reflection;
using System.Xml;
using YoutubeExtractor;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;


namespace VideoDownloader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            CheckForUpdate();
            InitializeComponent();
            this.PreviewKeyDown += new KeyEventHandler(HandleEsc);
        }
        private void HandleEsc(object sender, KeyEventArgs e)
        {
            // ESCape key closes app:  http://stackoverflow.com/questions/7691713/how-to-close-a-window-in-wpf-on-a-escape-key
            if (e.Key == Key.Escape)
                Close();
        }
        private static void CheckForUpdate()
        {
            /*
            Taken from Youtube:
            https://www.youtube.com/watch?v=eS688uFAKPA&list=PL3Nd7VJ5ZiWlN-YzFASid_JzvEY6W9aYb&index=30
            https://www.youtube.com/watch?v=XnMgjsizad8&list=PL3Nd7VJ5ZiWlN-YzFASid_JzvEY6W9aYb&index=31
            */

            string downloadUrl = "";
            Version newVersion = null;
            string xmlUrl = "https://dl.dropboxusercontent.com/s/15fkx1o8jpnecof/VideoDownloader.xml";
            Version applicationVersion = System.Reflection.Assembly.GetEntryAssembly().GetName().Version;
            string applicationName = System.Reflection.Assembly.GetEntryAssembly().GetName().Name;
            //MessageBox.Show("\"" + applicationName + "\"", "This app is called:");
            XmlTextReader reader = null;
            try
            {
                reader = new XmlTextReader(xmlUrl);
                reader.MoveToContent();
                string elementName = "";
                if ((reader.NodeType == XmlNodeType.Element) && (reader.Name == applicationName))
                {
                    while (reader.Read())
                    {
                        if (reader.NodeType == XmlNodeType.Element)
                        {
                            elementName = reader.Name;
                        }
                        else
                        {
                            if ((reader.NodeType == XmlNodeType.Text) && (reader.HasValue))
                            {
                                switch (elementName)
                                {
                                    case "version":
                                        newVersion = new Version(reader.Value);
                                        break;
                                    case "url":
                                        downloadUrl = reader.Value;
                                        break;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR:  " + ex.Message);
                Environment.Exit(1);
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }
            if (applicationVersion.CompareTo(newVersion) < 0)
            {
                MessageBoxResult result = MessageBox.Show("Version " + newVersion.Major + "." + newVersion.Minor + "." +
                    newVersion.Build + " of " + applicationName + " is available, would you like to download it?",
                    "New Version", MessageBoxButton.YesNo);

                if (result == MessageBoxResult.Yes)
                {
                    System.Diagnostics.Process.Start(downloadUrl);
                }
            }
            else
            {
                //MessageBox.Show("This application is up to date");
                //Version();
            }
        }
        public static void Version()
        {
            string Version = System.Reflection.Assembly.GetExecutingAssembly()
            .GetName().Version.ToString();

            string AssmName = System.Reflection.Assembly.GetExecutingAssembly()
            .GetName().Name.ToString();

            MessageBox.Show(("This is Version " + Version + "\nYou are up to date."),
            (AssmName), MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void btnDownload_Click(object sender, RoutedEventArgs e)
        {
            switch (btnDownload.Content.ToString())
            {
                case "Download":
                    {
                        progressBar.Minimum = 0;
                        progressBar.Maximum = 100;
                        IEnumerable<VideoInfo> videos = DownloadUrlResolver.GetDownloadUrls(txtUrl.Text);


                        //http://stackoverflow.com/questions/36231664/c-sharp-find-highest-available-youtube-resolution-on-a-video
                        int MaxResolution = 0;
                        using (StreamWriter writetext = new StreamWriter("write.txt"))
                            foreach (VideoInfo v in videos)
                            {
                                writetext.WriteLine(v.DownloadUrl);
                                // MessageBox.Show(v.DownloadUrl);
                                if (v.Resolution > MaxResolution)
                                {
                                    MaxResolution = v.Resolution;
                                }
                                writetext.WriteLine("Maximum Resolution is:  " + MaxResolution);
                                writetext.WriteLine();
                            }

                        //https://github.com/flagbug/YoutubeExtractor/
                        //VideoInfo video = videos.First(p => p.VideoType == VideoType.Mp4 && p.Resolution == Convert.ToInt32(cboResolution.Text));
                        VideoInfo video = videos.First(p => p.VideoType == VideoType.Mp4 && p.Resolution == MaxResolution);
                        if (video.RequiresDecryption)
                            DownloadUrlResolver.DecryptDownloadUrl(video);
                        /*VideoDownloader downloader = new VideoDownloader
                         * (video, Path.Combine(Application.StartupPath + "\\", 
                         * video.Title + video.VideoExtension));
                         */

                        YoutubeExtractor.VideoDownloader downloader = new YoutubeExtractor.VideoDownloader
                            (video, System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyVideos), 
                            RemoveIllegalPathCharacters(video.Title + video.VideoExtension)));

                        //TODO:  Progress Indicator is not working, this needs to be fixed
                        //videoDownloader.DownloadProgressChanged += (sender, args) => Console.WriteLine(args.ProgressPercentage);
                        //downloader.DownloadProgressChanged += Downloader_DownloadProgressChanged;
                        downloader.DownloadProgressChanged += (sender, args) => lblPercentage.Content;
                        Thread thread = new Thread(() => { downloader.Execute(); }) { IsBackground = true };
                        thread.Start();

                        break;
                    }
                case "Finished":
                    {
                        Environment.Exit(0);

                        break;
                    }
            }

        }
        private void Downloader_DownloadProgressChanged(object sender, ProgressEventArgs e)
        {
            //TODO:  Once Progress bar is fixed, this can be re-enabled
            //Invoke(new MethodInvoker(delegate ()
            //{
            //    progressBar.Value = (int)e.ProgressPercentage;
            //    lblPercentage.Content = $"{string.Format("{0:0.##}", e.ProgressPercentage)}%";
            //    progressBar.Update();
            //    if (progressBar.Value == progressBar.Maximum)
            //    {
            //        Thread.Sleep(1000);
            //        btnDownload.Text = "Finished";
            //    }
            //}));
        }
        private static string RemoveIllegalPathCharacters(string path)
        {
            string regexSearch = new string(System.IO.Path.GetInvalidFileNameChars()) + new string(System.IO.Path.GetInvalidPathChars());
            var r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
            return r.Replace(path, "");
        }

    }
}
