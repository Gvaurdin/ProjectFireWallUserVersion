using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Threading;

namespace WpfAppFireWall_UserVersion.MVVM.viewModel
{
    public class FindPortsAndProcess
    {
        public static List<Port> Ports { get; set; }
        public static Process netstat { get; set; }
        public FindPortsAndProcess()
        {
            Ports = new List<Port>();
            netstat = new Process();
            ProcessStartInfo ps = new ProcessStartInfo();
            ps.Arguments = "-a -n -o";
            ps.FileName = "netstat.exe";
            ps.UseShellExecute = false;
            ps.WindowStyle = ProcessWindowStyle.Hidden;
            ps.RedirectStandardInput = true;
            ps.RedirectStandardOutput = true;
            ps.RedirectStandardError = true;

            netstat.StartInfo = ps;
            netstat.Start();
        }

        public async void GetNetStatPorts()
        {
            await Task.Run(() =>
            {
                try
                {
                    netstat.Refresh();
                    StreamReader stdOutput = netstat.StandardOutput;
                    StreamReader stdError = netstat.StandardError;

                    //string content = await stdOutput.ReadToEndAsync() + await stdError.ReadToEndAsync();
                    string content = stdOutput.ReadToEnd() + stdError.ReadToEnd();
                    string exitStatus = netstat.ExitCode.ToString();

                    if (exitStatus != "0")
                    {
                        // Command Errored. Handle Here If Need Be
                    }

                    //Get The Rows
                    string[] rows = Regex.Split(content, "\r\n");
                    foreach (string row in rows)
                    {
                        //Split it baby
                        string[] tokens = Regex.Split(row, "\\s+");
                        if (tokens.Length > 4 && (tokens[1].Equals("UDP") || tokens[1].Equals("TCP")))
                        {
                            string localAddress = Regex.Replace(tokens[2], @"\[(.*?)\]", "1.1.1.1");
                            string processName = tokens[1] == "UDP" ? LookupProcess(Convert.ToInt16(tokens[4])) : LookupProcess(Convert.ToInt16(tokens[5]));

                            // Проверка на системные процессы
                            if (!IsSystemProcess(processName))
                            {
                                Ports.Add(new Port
                                {
                                    protocol = localAddress.Contains("1.1.1.1") ? String.Format("{0}v6", tokens[1]) : String.Format("{0}v4", tokens[1]),
                                    port_number = localAddress.Split(':')[1],
                                    process_name = processName
                                });
                            }
                        }
                    }

                }

                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            });
        }


        public string GetProcessNameFromPort(int sourcePort, int destinatiionPort)
        {
            string findNameProcess = "";
            bool isFind = false;
            GetNetStatPorts();
            try
            {
                foreach (Port port in Ports)
                {
                    if (!IsSystemProcess(port.process_name))
                    {
                        if (int.Parse(port.port_number) == destinatiionPort || int.Parse(port.port_number) == sourcePort)
                        {
                            findNameProcess = port.process_name;
                            isFind = true;
                            break;
                        }
                    }
                    if (isFind) break;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return findNameProcess;
        }

        private static bool IsSystemProcess(string processName)
        {
            // Добавьте сюда логику для проверки системного процесса по имени
            // Например, вы можете проверить, начинается ли имя процесса с "System" или "svchost", и т.д.
            return processName.ToLower().StartsWith("system") || processName.ToLower().StartsWith("svchost");
        }

        public static string LookupProcess(int pid)
        {
            string procName;
            try { procName = Process.GetProcessById(pid).ProcessName; }
            catch (Exception) { procName = "-"; }
            return procName;
        }
    }
    // ===============================================
    // The Port Class We're Going To Create A List Of
    // ===============================================
    public class Port
    {
        public string name
        {
            get
            {
                return string.Format("{0} ({1} port {2})", this.process_name, this.protocol, this.port_number);
            }
            set { }
        }
        public string port_number { get; set; }
        public string process_name { get; set; }
        public string protocol { get; set; }
    }
}
