using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using Blog;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Microsoft.EntityFrameworkCore.Storage;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;


namespace Homework6
{
    class Program
    {
        private static TcpListener listener;
        private const int serverPort = 2365;
        private static bool run;
        static async Task Main(string[] args)
        {
            Console.WriteLine("Server start");
            using (Context context = new Context())
                context.Database.EnsureCreated();
            listener = new TcpListener(IPAddress.Any, serverPort);
            run = true;
            await Listen();
        }
        
        private static async Task Listen()
        {
            List<Task> registerTasks = new List<Task>();
            listener.Start();
            while (run)
            {
                TcpClient client = await listener.AcceptTcpClientAsync();
                registerTasks.Add(RegisterClient(client));
                registerTasks.RemoveAll(t => t.IsCompleted);
            }
            listener.Stop();
            foreach (Task task in registerTasks)
                await task;
        }
        
        private static async Task RegisterClient(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            StreamReader reader = new StreamReader(stream);
            StreamWriter writer = new StreamWriter(stream);
            string fullmessage = await reader.ReadLineAsync();
            using(Context context = new Context())
            {
                IDbContextTransaction transaction = await context.Database.BeginTransactionAsync();
                try
                {
                    string[] list_message = fullmessage.Split(",");
                    Console.WriteLine(list_message[0]);
                    Console.WriteLine(list_message[1]);
                    Console.WriteLine(list_message[2]);
                    Console.WriteLine(list_message[3]);
                    Console.WriteLine(list_message[4]);
                    if (IsValidEmail(list_message[1]))
                    {
                        try
                        {
                            Console.WriteLine(DateOfBirthString(list_message[4]));
                            byte[] tmpSource;
                            byte[] tmpHash;
                            tmpSource = ASCIIEncoding.ASCII.GetBytes(list_message[4]);
                            tmpHash = new MD5CryptoServiceProvider().ComputeHash(tmpSource);
                            Console.WriteLine(ByteArrayToString(tmpHash));
                            UserData newUser = new UserData()
                            {
                                FullName = list_message[0],
                                Email = list_message[1],
                                Birthday = DateTime.Parse(list_message[4]),
                                Login = list_message[2],
                                PasswordHash = tmpHash
                            };
                            context.Users.Add(newUser);
                            await transaction.CommitAsync();
                            await context.SaveChangesAsync();
                            await writer.WriteLineAsync("SUCCESS");
                        }catch
                        {
                            await writer.WriteLineAsync("FAILED BIRTHDAY");
                        }
                        
                    }else
                    {
                        await writer.WriteLineAsync("FAILED EMAIL");
                    }
                }catch
                {
                    await writer.WriteLineAsync("FAILED");
                }
                await writer.FlushAsync();
            }
            client.Close();
        }
        static string ByteArrayToString(byte[] arrInput)
        {
            int i;
            StringBuilder sOutput = new StringBuilder(arrInput.Length);
            for (i=0;i < arrInput.Length -1; i++)
            {
                sOutput.Append(arrInput[i].ToString("X2"));
            }
            return sOutput.ToString();
        }
        public static bool IsValidEmail(string source)
        {
            return new EmailAddressAttribute().IsValid(source);
        }
        
        public static bool DateOfBirthString(string dob) //assumes a valid date string
        {
            DateTime dtDOB = DateTime.Parse(dob);
            return DateOfBirthDate(dtDOB);
        }

        public static bool DateOfBirthDate(DateTime dtDOB) //assumes a valid date
        {
            int age = GetAge(dtDOB);
            if (age < 0 || age > 150) { return false; }
            return true;
        }

        public static int GetAge(DateTime birthDate)
        {
            DateTime today = DateTime.Now;
            int age = today.Year - birthDate.Year;
            if (today.Month < birthDate.Month || (today.Month == birthDate.Month && today.Day < birthDate.Day)) { age--; }
            return age;
        }

    }
}