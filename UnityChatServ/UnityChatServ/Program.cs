using System.Net.Sockets;
using System.Net;
using System;
using System.Linq;
using System.IO;
using System.Threading;
using System.Collections.Generic;

namespace UnityChatServ
{
    class Program
    {
        const int PORT = 90;

        static void Main(string[] args)
        {
            string input;

            #region Run server?

            Console.WriteLine("Run server? Y\\N");
            while (true)
            { 
                input = Console.ReadLine().Trim().ToUpper();

                if (!input.Equals("N") && !input.Equals("Y"))
                {
                    Console.WriteLine(string.Format("\n'{0}' is wrong input\n", input));
                }
                else 
                {
                    if (input.Equals("N"))
                    { return; } else { break; }
                }
            }
            #endregion

            #region IP adress

            string host = Dns.GetHostName();

            string totalIPs = "Avalible IP adresses:\n";
            for (int i = 0; i < Dns.GetHostEntry(host).AddressList.Length; i++)
            {
                totalIPs += string.Format("{0}: {1}\n", i + 1, Dns.GetHostEntry(host).AddressList[i]);
            }
            totalIPs += "\nInput number of IP adress.";
            Console.WriteLine(totalIPs);

            int numOfIP = -1;
            while (true)
            { 
                input = Console.ReadLine().Trim();
                if (int.TryParse(input, out numOfIP) && numOfIP > 0 && numOfIP <= Dns.GetHostEntry(host).AddressList.Length)
                {
                    break;
                }
                else { Console.WriteLine(string.Format("\n'{0}' is wrong input\n", input)); }
            }
            IPAddress ip = Dns.GetHostEntry(host).AddressList[numOfIP - 1];
            #endregion

            Console.WriteLine(string.Format("\nServer is launched\nIP adress is: {0}\nPort is: {1}", ip, PORT));

            Server server = new Server(ip, PORT, Console.Out);
            server.Work();
        }
    }
}
