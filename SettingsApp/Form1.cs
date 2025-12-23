using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using System.ServiceProcess;


namespace SettingsApp
{
    public partial class Form1 : Form
    {
        private string xmlFilePath = @"C:\Users\z004zvav\source\repos\MonitoringService\SettingsApp\services.xml";
        private Timer notificationTimer;
        private Label notificationLabel;

        public Form1()
        {
            InitializeComponent();
            InitializeNotificationComponents();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadXmlData();
            ConfigureDataGridView();
            LoadAndUpdateServices();
        }
        private void LoadAndUpdateServices()
        {
            try
            {
                XDocument xmlDoc = XDocument.Load(xmlFilePath);
                var services = xmlDoc.Descendants("Service");

                foreach (var service in services)
                {
                    string serviceName = service.Element("Name")?.Value;
                    string status = service.Element("Status")?.Value;

                    if (!string.IsNullOrEmpty(serviceName) && !string.IsNullOrEmpty(status))
                    {
                        if (status == "active")
                        {
                            StartService(serviceName);
                        }
                        else if (status == "stop")
                        {
                            StopService(serviceName);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading or updating services: {ex.Message}");
            }
        }

        private void StartService(string serviceName)
        {
            if (string.IsNullOrEmpty(serviceName))
            {
                MessageBox.Show("Service name is empty.");
                return;
            }
            try
            {
                using (ServiceController sc = new ServiceController(serviceName))
                {
                    if (sc.Status == ServiceControllerStatus.Stopped || sc.Status == ServiceControllerStatus.StopPending)
                    {
                        sc.Start();
                        sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(30));
                        MessageBox.Show($"Service {serviceName} started successfully.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to start service {serviceName}: {ex.Message}");
            }
        }
        private void StopService(string serviceName)
        {
            try
            {
                using (ServiceController sc = new ServiceController(serviceName))
                {
                    if (sc.Status == ServiceControllerStatus.Running || sc.Status == ServiceControllerStatus.StartPending)
                    {
                        sc.Stop();
                        sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(30));
                        MessageBox.Show($"Service {serviceName} stopped successfully.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to stop service {serviceName}: {ex.Message}");
            }
        }

        private void LoadXmlData()
        {
            try
            {
                DataSet dataSet = new DataSet();
                dataSet.ReadXml(xmlFilePath);

                if (dataSet.Tables["Service"] != null)
                {
                    dataGridView1.DataSource = dataSet.Tables["Service"];
                }
                else
                {
                    MessageBox.Show("Service table not found in XML.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading XML data: {ex.Message}");
            }
        }

        private void ConfigureDataGridView()
        {
            dataGridView1.Columns.Clear();

            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn {Name="Name" ,DataPropertyName = "Name", HeaderText = "Name" });
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Description", HeaderText = "Description" });

            DataGridViewComboBoxColumn statusColumn = new DataGridViewComboBoxColumn
            {
                Name = "Status", 
                DataPropertyName = "Status",
                HeaderText = "Status",
                DataSource = new string[] { "active", "stop" }
            };
            dataGridView1.Columns.Add(statusColumn);

            DataGridViewComboBoxColumn startupTypeColumn = new DataGridViewComboBoxColumn
            {
               
                DataPropertyName = "StartupType",
                HeaderText = "Startup Type",
                DataSource = new string[] { "Manual", "Automatic" }
            };
            dataGridView1.Columns.Add(startupTypeColumn);

            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "CheckInterval", HeaderText = "Check Interval" });

            DataGridViewComboBoxColumn monitoredServiceColumn = new DataGridViewComboBoxColumn
            {
                DataPropertyName = "MonitoredService",
                HeaderText = "Monitored Service",
                DataSource = GetServiceNames(),
                AutoComplete = true,

            };
            dataGridView1.Columns.Add(monitoredServiceColumn);


            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.EnableResizing;
            dataGridView1.ColumnHeadersHeight = 30;

            dataGridView1.CellFormatting += DataGridView1_CellFormatting;
        }

        private void DataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dataGridView1.Columns[e.ColumnIndex].HeaderText == "Monitored Service" && e.RowIndex == 0)
            {
                e.Value = string.Empty;
                e.FormattingApplied = true;
            }
        }
        private List<string> GetServiceNames()
        {

            List<string> serviceNames = new List<string>();

            try
            {
                XDocument xmlDoc = XDocument.Load(xmlFilePath);
                var services = xmlDoc.Descendants("Service");
                foreach (var service in services)
                {
                    string serviceName = service.Element("Name")?.Value;
                    if (!string.IsNullOrEmpty(serviceName) && serviceName != "MonitorService")
                    {
                        serviceNames.Add(serviceName);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading service names: {ex.Message}");
            }

            return serviceNames;
        }
        private void SaveXmlData()
        {
            try
            {
                DataSet dataSet = new DataSet();
                dataSet.ReadXml(xmlFilePath);

                DataTable table = dataSet.Tables["Service"];

                if (table != null)
                {
                    DataTable updatedTable = (DataTable)dataGridView1.DataSource;
                    table.Clear();
                    foreach (DataRow row in updatedTable.Rows)
                    {
                        table.ImportRow(row);
                    }

                    table.AcceptChanges();
                    dataSet.WriteXml(xmlFilePath);

                    ShowSaveNotification();
                }
                else
                {
                    MessageBox.Show("Service table not found in XML.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving XML data: {ex.Message}");
            }
        }
        private void ShowSaveNotification()
        {
            notificationLabel.Text = "Saved!";
            notificationLabel.Visible = true;
            notificationTimer.Start();
        }
        private void InitializeNotificationComponents()
        {
            notificationTimer = new Timer();
            notificationTimer.Interval = 1500; 
            notificationTimer.Tick += NotificationTimer_Tick;

            notificationLabel = new Label();
            notificationLabel.AutoSize = true;
            notificationLabel.BackColor = System.Drawing.Color.GhostWhite;
            notificationLabel.ForeColor = System.Drawing.Color.DarkViolet;
            notificationLabel.Font = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Italic);
            notificationLabel.Visible = false;
            notificationLabel.Location = new System.Drawing.Point(368, 340); 

            this.Controls.Add(notificationLabel);
        }
        private void NotificationTimer_Tick(object sender, EventArgs e)
        {
            notificationLabel.Visible = false;
            notificationTimer.Stop();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SaveXmlData();

        }

    }

}