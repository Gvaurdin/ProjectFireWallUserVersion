using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfAppFireWall_UserVersion.MVVM.model;
using SharpPcap;
using SharpPcap.LibPcap;
using SharpPcap.Statistics;
using SharpPcap.Tunneling;
using SharpPcap.WinDivert;
using SharpPcap.WinpkFilter;
using PacketDotNet;
using System.Windows;
using System.Diagnostics;
using PcapDotNet.Core;
using PcapDotNet.Packets;
using System.Management;
using System.Threading;
using System.Net.NetworkInformation;
using System.Net;
using System.Runtime.InteropServices;


namespace WpfAppFireWall_UserVersion.MVVM.viewModel
{

    public class PacketArrivalEventArgs
    {
        public model.Packet Packet { get; }

        public PacketArrivalEventArgs(model.Packet packet)
        {
            Packet = packet;
        }
    }

    public class ProcessEventArgs : EventArgs
    {
        public string ProcessName { get; }

        public ProcessEventArgs(string processName)
        {
            ProcessName = processName;
        }
    }
    public class TrafficMonitor
    {
        private ICaptureDevice _device;
        private PacketDevice _selectedDevice;
        private PacketCommunicator _communicator;
        FindPortsAndProcess findPorts;
        private Thread _captureThread;
        private bool _isCapturing;

        public event EventHandler<PacketArrivalEventArgs> PacketArrival;
        public event EventHandler<ProcessEventArgs> ProcessDetected;
        public TrafficMonitor()
        {
            // Получаем список доступных сетевых интерфейсов
            var devices = CaptureDeviceList.Instance;

            // Проверяем наличие сетевых интерфейсов
            if (devices.Count < 1)
            {
                throw new InvalidOperationException("No network interfaces found.");
            }
            findPorts = new FindPortsAndProcess();
            // Выбираем первый сетевой интерфейс для захвата трафика
            _device = devices[2];

            // Начинаем захват пакетов на выбранном интерфейсе
            _device.OnPacketArrival += new SharpPcap.PacketArrivalEventHandler(device_OnPacketArrival);
            _device.Open(SharpPcap.DeviceModes.Promiscuous);
            StartCapturePacket();
        }

        private async void StartCapturePacket()
        {
            await Task.Run(() =>
            {
                _device.Capture();
            });
        }

        private void device_OnPacketArrival(object sender, SharpPcap.PacketCapture e)
        {
            // Получаем захваченный пакет
            PacketDotNet.Packet packet = PacketDotNet.Packet.ParsePacket(e.GetPacket().LinkLayerType, e.GetPacket().Data);
            // Извлекаем информацию о пакете
            string sourceIP = "", destinationIP = "", protocoltype = "", processName = "";
            int sourcePort = 0, destinationPort = 0;
            DateTime receivedTime = DateTime.Now;
            Type packetType = packet.GetType();

            // Проверяем, является ли пакет TCP-пакетом
            if (packet is PacketDotNet.EthernetPacket ethernetPacket)
            {
                // Получаем содержимое Ethernet-пакета
                var payloadPacket = ethernetPacket.PayloadPacket;

                // Проверяем, является ли содержимое IP-пакетом
                if (payloadPacket is PacketDotNet.IPPacket ipPacket)
                {
                    // Обрабатываем IP-пакет
                    sourceIP = ipPacket.SourceAddress.ToString();
                    destinationIP = ipPacket.DestinationAddress.ToString();

                    // Проверяем, является ли содержимое TCP-пакетом
                    if (ipPacket.PayloadPacket is PacketDotNet.TcpPacket tcpPacket)
                    {
                        sourcePort = tcpPacket.SourcePort;
                        destinationPort = tcpPacket.DestinationPort;
                        protocoltype = "TCP";
                        processName = findPorts.GetProcessNameFromPort(sourcePort,destinationPort);

                        // Далее вы можете продолжить анализировать содержимое пакета TCP
                    }
                    // Проверяем, является ли содержимое UDP-пакетом
                    else if (ipPacket.PayloadPacket is PacketDotNet.UdpPacket udpPacket)
                    {
                        sourcePort = udpPacket.SourcePort;
                        destinationPort = udpPacket.DestinationPort;
                        protocoltype = "UDP";
                        // Далее вы можете продолжить анализировать содержимое пакета UDP
                        processName = findPorts.GetProcessNameFromPort(sourcePort, destinationPort);
                    }
                }
                else
                {
                    return;
                }
            }

            // Извлекаем данные пакета
            string data = packet.PayloadPacket?.ToString();

            // Создаем объект Packet и передаем полученные значения
            model.Packet customPacket = new model.Packet
            {
                ReceivedTime = receivedTime,
                SourceIP = sourceIP,
                DestinationIP = destinationIP,
                SourcePort = sourcePort,
                DestinationPort = destinationPort,
                protocolType = protocoltype,
                processName = processName,
                Data = data
            };
            // Вызываем событие PacketArrival и передаем объект PacketArrivalEventArgs
            PacketArrival?.Invoke(this, new PacketArrivalEventArgs(customPacket));
        }

        internal void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}

