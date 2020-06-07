using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static BinaryFormatter formatter = new BinaryFormatter();
        static ObservableCollection<string> streets;
        static Socket client;
        public MainWindow()
        {
            InitializeComponent();
            streets = new ObservableCollection<string>();
            lbStreets.ItemsSource = streets;
        }
        private void Click_Send(object sender, RoutedEventArgs e)
        {
            try
            {
                client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                client.Connect(IPAddress.Parse("127.0.0.1"), 2020);
                if (client.Connected && !String.IsNullOrWhiteSpace(tbIndex.Text))
                {
                    SendReq(tbIndex.Text);
                    const int SIZE = 1024;
                    byte[] response = new byte[SIZE];
                    int count = client.Receive(response);

                    List<string> tmp = new List<string>();
                    using (MemoryStream ms = new MemoryStream())
                    {
                        ms.Write(response, 0, response.Length);
                        ms.Position = 0;
                        tmp = (List<string>)formatter.Deserialize(ms);
                    }

                    streets.Clear();
                    foreach (var item in tmp)
                    {
                        streets.Add(item);
                    }
                    tbIndex.Clear();
                    client.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        public async void SendReq(string request)
        {
            byte[] req = Encoding.UTF8.GetBytes(request);
            await Task.Run(() => client.Send(req));
        }
    }
}
