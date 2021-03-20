using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        private const int serverPort = 2365;
        private static TcpClient client;
        private static string fullmessage;

        static async Task Main(string[] args)
        {
            Console.WriteLine("Tcp User start");
            Console.Write("Enter fullname: ");
            string fullname = Console.ReadLine();
            Console.Write("Enter email: ");
            string email = Console.ReadLine();
            Console.Write("Enter login: ");
            string login = Console.ReadLine();
            Console.Write("Enter password: ");
            string password = Console.ReadLine();
            Console.Write("Enter birthday: ");
            string birthday = Console.ReadLine();
            fullmessage = fullname + ',' + email + ',' + login + ',' + password + ',' + birthday;
            Console.WriteLine(fullmessage);
            client = new TcpClient();
            await client.ConnectAsync(IPAddress.Loopback, serverPort);
            NetworkStream stream = client.GetStream();
            StreamReader reader = new StreamReader(stream);
            StreamWriter writer = new StreamWriter(stream);
            await writer.WriteLineAsync(fullmessage);
            await writer.FlushAsync();
            string answer = await reader.ReadLineAsync();
            Console.WriteLine($"answer is {answer}");
            client.Close();
        }
    }
}