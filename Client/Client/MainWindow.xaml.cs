using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
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
using System.Reflection;
using System.Threading;


namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Socket client;
        public String host = "127.0.0.1";
        public int port = 20001;
        String username = "";
        

        public MainWindow()
        {
            InitializeComponent();


        }

        private void Connect(object sender, RoutedEventArgs e)
        {
            username = Username.Text;
            Thread receiveThread = new Thread(ReceiveMessage);

            if (btnConnect.Content.Equals("Disconnect"))
            {
                btnConnect.Content = "Connect";

                client.Send(Encoding.ASCII.GetBytes("DISCONNECT"));

                receive_msg.Dispatcher.BeginInvoke(
                            new Action(() => { receive_msg.Text += "Disconnect Success.\n"; }), null);

                client.Shutdown(SocketShutdown.Both);
                client.Close();

            }

            else if (btnConnect.Content.Equals("Connect"))
            {
                btnConnect.Content = "Disconnect";
                Status.Text = "online";

                //connect to socket server
                IPAddress ip = IPAddress.Parse(ipClient.Text.ToString());
                //socket()
                client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                try
                {
                    //connect()
                    client.Connect(new IPEndPoint(ip, Int32.Parse(portClient.Text.ToString())));

                    client.Send(Encoding.ASCII.GetBytes(username));

                    receiveThread = new Thread(ReceiveMessage);
                    receiveThread.Start();
                }catch(SocketException)
                {
                    receive_msg.Dispatcher.BeginInvoke(
                            new Action(() => { receive_msg.Text += "Connection Failed\n"; }), null);

                    btnConnect.Content = "Connect";
                    Status.Text = "offline";
                }
            }

        }

        //send()
        private void Send(object sender, RoutedEventArgs e)
        {
            String text = msg.Text;
            try
            {
                //send message to server
                client.Send(Encoding.ASCII.GetBytes(text + "\n"));
                msg.Text = "";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                receive_msg.Text += "send Fail\n";
            }
        }

        //receive()
        public void ReceiveMessage()
        {

            while (true)
            {
                try
                {
                    byte[] result = new byte[1024];
                    int receiveNumber = client.Receive(result);
                    String recStr = Encoding.ASCII.GetString(result, 0, receiveNumber);
                    receive_msg.Dispatcher.BeginInvoke(
                            new Action(() => { receive_msg.Text += recStr; }), null);
                }
                catch
                {
                    if (client.Connected)
                    {
                        client.Shutdown(SocketShutdown.Both);
                        client.Close();

                    }


                    this.Dispatcher.Invoke((Action)(() =>
                    {
                        Status.Text = "offline";

                        // server中斷連線
                        if (btnConnect.Content.Equals("Disconnect"))
                        {
                            btnConnect.Content = "Connect";

                        } // 

                    }));

                    

                    break;
                }
                
            } // while()


        }


    }
}