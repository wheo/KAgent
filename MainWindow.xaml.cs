using KAgent.Config;
using KAgent.Interface;
using log4net;
using MediaInfo;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using KAgent.Singleton;
using System.Threading.Tasks;

namespace KAgent
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private Settings _settings;
        private int _counter = 0;

        public MainWindow()
        {
            InitializeComponent();
            LoadConfig();
        }

        private bool LoadConfig()
        {
            // config.json 읽기
            logger.Info(Directory.GetCurrentDirectory());
            _settings = Settings.GetInstance();

            try
            {
                if (!File.Exists(_settings.configFileName))
                {
                    logger.Info(_settings.configFileName + " 파일이 없습니다. 환경설정 파일을 읽지 못해 기본값으로 설정합니다.");
                    //default value
                    _settings.ip = "10.10.60.57";
                    _settings.port = 3306;
                    _settings.id = "root";
                    _settings.pw = "tnmtech";
                    _settings.DatabaseName = "kbsmedia_CMS";
                }
                else
                {
                    using (StreamReader file = File.OpenText(_settings.configFileName))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        _settings = (Settings)serializer.Deserialize(file, typeof(Settings));
                    }
                }
                DatabaseManager.GetInstance().SetConnectionString(_settings.ip, _settings.port, _settings.id, _settings.pw, _settings.DatabaseName);
                _settings.Save();
                logger.Info("KAgent is starting ...");
            }
            catch (Exception ex)
            {
                logger.Info(ex.Message);
            }

            return true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            lvStatus.ItemsSource = Status.GetInstance().items;

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += new EventHandler(Service);
            timer.Start();
        }

        [Obsolete]
        private void mediainfoTest(string fname)
        {
            MediaInfoWrapper mi = new MediaInfoWrapper(fname);
            logger.Info(mi.Codec);
        }

        private void Service(object sender, EventArgs e)
        {
            //logger.Info(string.Format($"{DateTime.Now.ToString()} - {++_counter}"));
            Job job = new Job();
            List<Sender> ss = job.Get();
            ss.ForEach(async s =>
            {
                s.initModel();
                var task_service = Task.Run(() => s.FtpService());
                logger.Info(string.Format($"{s.srcpath} 를 task로 보냄"));
                bool result = await task_service;
                logger.Info(string.Format($"task ({result}) 결과"));
            });
        }
    }
}