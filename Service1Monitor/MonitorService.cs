using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.ServiceProcess;
using System.Timers;
using System.Xml.Linq;

namespace Service1Monitor
{
    public partial class MonitorService : ServiceBase
    {
        private Timer timer;
        private string serviceToMonitor; //xml den alıyor
        private int checkInterval; //xml den alıyor
        private string logFilePath = @"C:\Users\z004zvav\source\repos\MonitoringService\WindowsService1\service_log.txt"; // Service1 log dosyası
        private static readonly HttpClient httpClient = new HttpClient();
      

         public MonitorService()
         {
             InitializeComponent();
         }

        protected override void OnStart(string[] args)
        {
            LoadConfiguration();
            timer = new Timer(checkInterval); // checkIntervalı xmlden alıyor
            timer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            timer.Start();
            Log("MonitorService started.");
        }

        protected override void OnStop()
        {
            timer.Stop();
            Log("MonitorService stopped.");
        }

        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            CheckService1();
            CheckWebApiPing();
        }

        private void CheckService1()
        {
            ServiceController sc = new ServiceController(serviceToMonitor);

            try
            {
                sc.Refresh();
                if (sc.Status == ServiceControllerStatus.Stopped || sc.Status == ServiceControllerStatus.StopPending)
                {
                    Log($"Service {serviceToMonitor} is stopped. Attempting to start...");
                    sc.Start();
                    sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(15));
                    Log($"Service {serviceToMonitor} started successfully.");
                }
                else
                {
                    Log($"Service {serviceToMonitor} is running.");
                }
            }
            catch (InvalidOperationException ex)
            {
                Log($"Service {serviceToMonitor} does not exist or is not accessible: {ex.Message}");
            }
            catch (System.ServiceProcess.TimeoutException ex)
            {
                Log($"Service {serviceToMonitor} failed to start within the expected time: {ex.Message}");
            }
            catch (Exception ex)
            {
                Log($"An unexpected error occurred while monitoring service {serviceToMonitor}: {ex.Message}");
            }
        }

        private async void CheckWebApiPing()
        {
            try
            {
                HttpResponseMessage response = await httpClient.GetAsync("http://localhost:13455/api/Ping");  
                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    Log($"Ping successful: {responseBody}");
                }
                else
                {
                    Log($"Ping failed with status code: {response.StatusCode}");
                }
            }
            catch (HttpRequestException ex)
            {
                Log($"Ping request failed: {ex.Message}");
                Log($"Detailed error: {ex.InnerException?.Message}");
            }
            catch (Exception ex)
            {
                Log($"An unexpected error occurred while sending Ping request: {ex.Message}");
                Log($"Detailed error: {ex.InnerException?.Message}");
            }
        }

        private void LoadConfiguration()
        {
            try
            {
                XDocument configDoc = XDocument.Load(@"C:\Users\z004zvav\source\repos\MonitoringService\SettingsApp\services.xml");

                var serviceElement = configDoc.Descendants("Service")
                    .FirstOrDefault(s => s.Element("Name")?.Value == "MonitorService");

                if (serviceElement != null)
                {
                    serviceToMonitor = serviceElement.Element("MonitoredService")?.Value ?? "Service1";
                    Log($"Service to monitor set to: {serviceToMonitor}");
                    var intervalElement = serviceElement.Element("CheckInterval")?.Value;
                    if (int.TryParse(intervalElement, out int interval))
                    {
                        checkInterval = interval;
                    }
                    else
                    {
                        Log("Invalid CheckInterval value found. Using default value.");
                    }
                }
                else
                {
                    Log("MonitorService configuration not found in XML.");
                }
            }
            catch (Exception ex)
            {
                Log($"Failed to load configuration: {ex.Message}");
            }
        }
        private void Log(string message)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(logFilePath));
                using (StreamWriter writer = new StreamWriter(logFilePath, true))
                {
                    writer.WriteLine($"{DateTime.Now}: {message}");
                }
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("MonitorService", $"Failed to write log: {ex.Message}", EventLogEntryType.Error);
            }
        }
    }
}
