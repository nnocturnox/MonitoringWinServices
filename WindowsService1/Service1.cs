using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;


namespace WindowsService1
{

    public partial class Service1 : ServiceBase
    {
       private ServiceMonitor serviceMonitor; //
        private FileSystemWatcher watcher;
        private string logFilePath = @"C:\Users\z004zvav\source\repos\MonitoringService\WindowsService1\service_log.txt";
        

        public Service1()
        {
            InitializeComponent();
            serviceMonitor = new ServiceMonitor();
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                Log("Service started.");
                watcher = new FileSystemWatcher();
                string pathToWatch = @"C:\Deneme";

                if (!Directory.Exists(pathToWatch))
                {
                    throw new ArgumentException($"The directory '{pathToWatch}' does not exist.");
                }

                watcher.Path = pathToWatch;
                watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName;
                watcher.Filter = "*.*";
                watcher.Created += new FileSystemEventHandler(OnChanged);
                watcher.Deleted += new FileSystemEventHandler(OnChanged);

                watcher.EnableRaisingEvents = true;
                serviceMonitor.Initialize(60000, "Service1", logFilePath); // 60 saniyelik aralıklarla kontrol eder //
                serviceMonitor.Start();
            }
            catch (Exception ex)
            {
                Log($"Service failed to start: {ex.Message}");
                throw;
            }
        }


        protected override void OnStop()
        {
            Log("Service stopped.");
            if (watcher != null) {
                watcher.EnableRaisingEvents = false; // ilgili olayların tetiklenmesini durdurur
                watcher.Dispose(); // FileSystemWatcher nesnesini serbest bırakır ve kaynaklarını temizler.
            }
            if (serviceMonitor != null)
            {
                serviceMonitor.Stop();
            }

        }
        private void OnChanged(object source, FileSystemEventArgs e) // FileSystemEventArgs e --> olay hakkında bilgi sağlayan argümanları içerir.  // source--> olayı tetikleyen nesneyi temsil eder.
        {
            Log($"File: {e.FullPath} {e.ChangeType}"); // log mesajında kullanılır, değişikliğin ne olduğu hakkında bilgi verir.
        }





        private void Log(string message) // log dosyasına yazılacak mesajı temsil eder
        {
            Directory.CreateDirectory(Path.GetDirectoryName(logFilePath)); //logFilePath değişkeninde belirtilen log dosyasının bulunduğu dizini oluşturur.
            using (StreamWriter writer = new StreamWriter(logFilePath, true))
            {
                writer.WriteLine($"{DateTime.Now}: {message}");
            }
        }

    } 

}
