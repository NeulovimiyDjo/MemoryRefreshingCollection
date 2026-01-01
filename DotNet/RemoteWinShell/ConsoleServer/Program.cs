using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text.RegularExpressions;
using TcpConnectorLibrary;
using System.Net;
using System.Net.Sockets;

namespace ConsoleServer
{
    static class Program
    {
        static void Main(string[] args)
        {
            var startInfo = new ProcessStartInfo();
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.FileName = "cmd.exe";
            startInfo.CreateNoWindow = true;

            startInfo.Arguments = "/C cd";
            var pf = new Process
            {
                StartInfo = startInfo
            };
            pf.Start();

            string workingDirectory = pf.StandardOutput.ReadToEnd();
            pf.WaitForExit();
            workingDirectory = workingDirectory.Substring(0, workingDirectory.Length - 2); // remove \r\n



            Socket listenSocket = TcpConnector.StartServer();

            string command = TcpConnector.ReadMessage(listenSocket, out Socket handler);

            while (command != "exit")
            {
                Execute(command, ref workingDirectory, startInfo, listenSocket, handler);

                TcpConnector.CloseConnection(handler);

                command = TcpConnector.ReadMessage(listenSocket, out handler);
            }

            Console.WriteLine("Exited blabla exit again or smth");
            Console.ReadLine();
        }

        private static void Execute(string command, ref string workingDirectory, ProcessStartInfo startInfo,Socket listenSocket, Socket handler)
        {
            if ((command.ToLowerInvariant()).StartsWith("cd"))
            {
                command = command.Remove(0, 2);
                command = command.TrimStart();
                if (command == "..")
                {
                    workingDirectory = workingDirectory.Parent();
                }
                else if (command.Length > 0 && command.IsValid())
                {
                    if (ExecuteCommand(startInfo, workingDirectory, "cd " + command, handler))
                    {
                        workingDirectory = workingDirectory + "\\" + command;
                    }
                }
                else
                {
                    TcpConnector.SendMessage(workingDirectory, handler);
                }
            }
            else if ((command.ToLowerInvariant()).StartsWith("push"))
            {
                command = command.Remove(0, 4);
                command = command.TrimStart();
                string fileName = command;
                TcpConnector.SendMessage("msg receaved", handler);
                TcpConnector.ReadFile(listenSocket, out handler, workingDirectory + "\\" + fileName);
                TcpConnector.SendMessage("File successfuly pushed", handler);
            }
            else
            {
                ExecuteCommand(startInfo, workingDirectory, command, handler);
            }
        }

        private static bool ExecuteCommand(ProcessStartInfo startInfo, string workingDirectory, string command, Socket handler)
        {
            startInfo.WorkingDirectory = workingDirectory;
            startInfo.Arguments = $"/C {command}";
            Process p = new Process
            {
                StartInfo = startInfo
            };
            p.Start();

            string output = p.StandardOutput.ReadToEnd();
            string err = p.StandardError.ReadToEnd();
            p.WaitForExit();

            if (output.Length > 0)
            {
                TcpConnector.SendMessage(output, handler);
            }

            if (err.Length > 0)
            {
                TcpConnector.SendMessage(err, handler);
                return false;
            }
            else
            {
                return true;
            }
        }

        private static string Parent(this string dir)
        {
            var match = Regex.Match(dir, @"\\[^\\/<>:""\|\?\*]+$");
            return Regex.Replace(dir, "\\" + match.Value, "");
        }

        private static bool IsValid(this string command)
        {
            var match = Regex.Match(command, @"^.[\\/]+|^[.]+");
            return !match.Success;
        }
    }
}
