using Amazon.S3.Model;
using NetFwTypeLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using WpfAppFireWall_UserVersion.MVVM.model;

namespace WpfAppFireWall_UserVersion.MVVM.viewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private Firewall _firewall;
        private TrafficMonitor _trafficMonitor;

        public ObservableCollection<Packet> CapturedPackets { get; private set; }

        private bool _isMonitoring;
        public bool IsMonitoring
        {
            get { return _isMonitoring; }
            set
            {
                if (_isMonitoring != value)
                {
                    _isMonitoring = value;
                    OnPropertyChanged(nameof(IsMonitoring));
                }
            }
        }

        // Словарь для хранения данных о захваченных пакетах для каждого процесса
        private Dictionary<string, ObservableCollection<Packet>> processPacketDict = new Dictionary<string, ObservableCollection<Packet>>();

        // Коллекция для отображения названий процессов в ListBox
        public ObservableCollection<string> ProcessNames { get; private set; }

        public ObservableCollection<string> BlockProcessNames { get; private set; }

        // Коллекция для отображения сетевых пакетов выбранного процесса в DataGrid
        private ObservableCollection<Packet> selectedProcessPackets = new ObservableCollection<Packet>();
        public ObservableCollection<Packet> SelectedProcessPackets
        {
            get { return selectedProcessPackets; }
            set
            {
                selectedProcessPackets = value;
                OnPropertyChanged("SelectedProcessPackets");
            }
        }

        public ICommand StartMonitoringCommand { get; }
        public ICommand StopMonitoringCommand { get; }

        public MainViewModel()
        {
            _firewall = new Firewall();
            CapturedPackets = new ObservableCollection<Packet>();
            ProcessNames = new ObservableCollection<string>();
            // Привязка коллекции ProcessNames к ListBox
            BindingOperations.EnableCollectionSynchronization(ProcessNames, new object());
            BlockProcessNames = new ObservableCollection<string>();
            StartMonitoringCommand = new RelayCommand(StartMonitoring);
            StopMonitoringCommand = new RelayCommand(StopMonitoring);
        }

        private void StartMonitoring()
        {
            _trafficMonitor = new TrafficMonitor();
            _trafficMonitor.PacketArrival += TrafficMonitor_PacketArrival;
            IsMonitoring = true;
        }

        private void StopMonitoring()
        {
            _trafficMonitor.PacketArrival -= TrafficMonitor_PacketArrival;
            _trafficMonitor.Dispose(); 
            IsMonitoring = false;
        }

        private void TrafficMonitor_PacketArrival(object sender, PacketArrivalEventArgs e)
        {
            // Получаем процесс из захваченного пакета
            string processName = e.Packet.processName;

            // Добавляем пакет в словарь
            App.Current.Dispatcher.Invoke(() =>
            {
                if (BlockProcessNames.Contains(processName))
                {
                    Rule rule = new Rule
                    {
                        Name = $"Block_{processName}",
                        Type = RuleAction.Block,
                        ApplicationName = processName,
                        Direction = RuleDirection.Outbound,
                        Protocol = (int)NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_ANY,
                        LocalPort = e.Packet.SourcePort,
                        RemotePort = 0,
                        CreatedAt = DateTime.Now
                    };

                    _firewall.AddRule(rule);
                }
                else if(processPacketDict.ContainsKey(processName))
                {
                    processPacketDict[processName].Add(e.Packet);

                    if (SelectedProcess == processName)
                    {
                        SelectedProcessPackets = processPacketDict[SelectedProcess];
                    }
                }
                else
                {
                    MessageBoxResult result = MessageBox.Show($"The process {processName} is trying to gain network access. Allow it?", "Request for access", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        if (!processPacketDict.ContainsKey(processName))
                        {
                            processPacketDict[processName] = new ObservableCollection<Packet>();
                            ProcessNames.Add(processName); 
                        }
                        processPacketDict[processName].Add(e.Packet);

                        // Обновляем DataGrid с пакетами выбранного процесса, если этот процесс выбран
                        if (SelectedProcess == processName)
                        {
                            SelectedProcessPackets = processPacketDict[SelectedProcess];
                        }
                    }
                    else
                    {
                        BlockProcessNames.Add(processName);
                        Rule rule = new Rule
                        {
                            Name = $"Block_{processName}",
                            Type = NET_FW_ACTION_.NET_FW_ACTION_BLOCK,
                            ApplicationName = processName,
                            Direction = RuleDirection.Outbound,
                            LocalAddresses = "*",
                            RemoteAddresses = "*",
                            Protocol = (int)NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_ANY,
                            LocalPort = e.Packet.SourcePort,
                            RemotePort = 0,
                            CreatedAt = DateTime.Now
                        };

                        _firewall.AddRule(rule);
                    }
                }
            });
        }

        // Свойство для выбранного процесса
        private string selectedProcess;
        public string SelectedProcess
        {
            get { return selectedProcess; }
            set
            {
                selectedProcess = value;
                // Обновляем DataGrid с пакетами выбранного процесса
                if (processPacketDict.ContainsKey(SelectedProcess))
                {
                    SelectedProcessPackets = processPacketDict[SelectedProcess];
                }
                OnPropertyChanged("SelectedProcess");
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
