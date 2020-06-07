using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace server
{
    class Program
    {
        static DataContext db;
        static ManualResetEvent done = new ManualResetEvent(false);
        static BinaryFormatter formatter = new BinaryFormatter();
        class WorkObject
        {
            public static readonly int SIZE = 1024;
            public byte[] Buffer { get; set; } = new byte[SIZE];
            public Socket Socket { get; set; }
        }
        static void Main(string[] args)
        {
            using (db = new DataContext())

            {
                if (db.Indexes.Count() <= 0)
                {
                    db.Indexes.Add(new Index { PostIndex = "33001" });
                    db.Indexes.Add(new Index { PostIndex = "33002" });
                    db.Indexes.Add(new Index { PostIndex = "33003" });
                    db.SaveChanges();
                }
                db.Indexes.Load();
                if (db.Streets.Count() <= 0)
                {
                    db.Streets.Add(new Street { IndexId = 1, Name = "Street 1" });
                    db.Streets.Add(new Street { IndexId = 1, Name = "Street 2" });
                    db.Streets.Add(new Street { IndexId = 2, Name = "Street 3" });
                    db.Streets.Add(new Street { IndexId = 2, Name = "Street 4" });
                    db.Streets.Add(new Street { IndexId = 3, Name = "Street 5" });
                    db.Streets.Add(new Street { IndexId = 3, Name = "Street 6" });
                    db.Streets.Add(new Street { IndexId = 3, Name = "Street 7" });
                    db.SaveChanges();
                }
                db.Streets.Load();
            }
            IPAddress ip = IPAddress.Parse("127.0.0.1");
            const int PORT = 2020;
            Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                server.Bind(new IPEndPoint(ip, PORT));
                server.Listen(10);

                while (true)
                {
                    done.Reset();
                    Console.WriteLine("waiting for connect");
                    server.BeginAccept(new AsyncCallback(AcceptCallback), server);
                    done.WaitOne();//wait for signal
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine(e.Message);
            }
        }
        private static void AcceptCallback(IAsyncResult ar)
        {
            done.Set();//signal
            Socket server = (Socket)ar.AsyncState;
            Socket client = server.EndAccept(ar);

            WorkObject obj = new WorkObject { Socket = client };

            client.BeginReceive(obj.Buffer, 0, WorkObject.SIZE, 0, new AsyncCallback(ReceiveCallback), obj);
        }
        private static void ReceiveCallback(IAsyncResult ar)
        {
            WorkObject client = (WorkObject)ar.AsyncState;
            int count = client.Socket.EndReceive(ar);
            string request = Encoding.UTF8.GetString(client.Buffer, 0, count);
            Console.WriteLine("Got {0}, from {1}", request, client.Socket.RemoteEndPoint);
            Send(client.Socket, request);
        }
        private static void Send(Socket socket, string request)
        {
            try
            {
                List<string> answer = new List<string>();
                using (db = new DataContext())
                {
                    Street[] streets = db.Streets.Where((x) => x.Index.PostIndex == request).ToArray();
                    foreach (var item in streets)
                    {
                        answer.Add(item.Name);
                    }
                }
                byte[] responce = null;
                using (MemoryStream ms = new MemoryStream())
                {
                    formatter.Serialize(ms, answer);
                    responce = new byte[ms.Length];
                    ms.Position = 0;
                    ms.Read(responce, 0, responce.Length);
                }
                socket.BeginSend(responce, 0, responce.Length, 0, new AsyncCallback(SendCallback), socket);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket socket = (Socket)ar.AsyncState;
                int count = socket.EndSend(ar);
                Console.WriteLine("Send {0} bytes to client", count);
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }
            catch (SocketException e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
