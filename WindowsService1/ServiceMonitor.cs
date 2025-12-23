using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Timers;

namespace WindowsService1
{
    public partial class ServiceMonitor : Component
    {
        private Timer timer;
        private string serviceToMonitor;
        private string logFilePath;

        public ServiceMonitor()
        {
            InitializeComponent();
        }

        public ServiceMonitor(IContainer container)
        {
            container.Add(this);  //ServiceMonitor nesnesini ekler
            InitializeComponent();
        }

        public void Initialize(double interval, string service, string logPath) // interval--> zamanlayıcının(timer) kaç milisaniye aralıklarla tetikleneceğini belirler.
        {
            serviceToMonitor = service; // hangi servisin izleneceği
            logFilePath = logPath;

            timer = new Timer(interval); // belirli aralıklarla çalışan yeni bir timer nesnesi oluşturur 
            timer.Elapsed += new ElapsedEventHandler(OnTimedEvent); // zamanlayıcı her tetiklendiğinde onTimedEvent metodu çağırılır
        }

        public void Start()
        {
            timer.Start();
            Log("Service monitor started.");
        }

        public void Stop()
        {
            timer.Stop();
            Log("Service monitor stopped.");
        }

        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            ServiceController sc = new ServiceController(serviceToMonitor);      // sc değişkeni belirtilen servisin durumunu kontrol etmek yönetmek için 

            try
            {
                sc.Refresh();
                if (sc.Status == ServiceControllerStatus.Stopped || sc.Status == ServiceControllerStatus.StopPending) // servisin durduğunu veya durma sürecinde olduğunu kontrol eder
                {
                    Log($"Service {serviceToMonitor} is stopped. Attempting to start...");
                    sc.Start();
                    sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(15)); // servisin çalışıyor duruma geçmesini 15 saniye bekler
                    Log($"Service {serviceToMonitor} started successfully.");
                }
                else
                {
                    Log($"Service {serviceToMonitor} is running.");
                }
            }
            catch (Exception ex)
            {
                Log($"Error while monitoring service {serviceToMonitor}: {ex.Message}");
            }
        }

        private void Log(string message)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(logFilePath));
            using (StreamWriter writer = new StreamWriter(logFilePath, true))
            {
                writer.WriteLine($"{DateTime.Now}: {message}");
            }
        }
    }
}
