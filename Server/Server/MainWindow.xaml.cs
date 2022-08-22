using System;
using System.Collections;
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

namespace Server
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Socket server;
        public String host = "127.0.0.1";
        public int port = 20001;
        public ArrayList clients = new ArrayList(); // socket arrayList

        public MainWindow()
        {
            InitializeComponent();

        }

        //start a socket server
        private void Start(object sender, RoutedEventArgs e)
        {
            if (server == null)
            {
                // 必須輸入值
                if ( !ipInput.Text.Equals("") && !portInput.Text.Equals("") )
                {

                    receive_msg.Text = "Server Start";
                    IPAddress ip = IPAddress.Parse(ipInput.Text.ToString());
                    //socket()
                    server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    //bind()
                    server.Bind(new IPEndPoint(ip, Int32.Parse( portInput.Text.ToString() )));
                    //listen()
                    server.Listen(10);

                    Thread thread = new Thread(Listen);
                    thread.Start();

                } // if()
            }
        }

        private void Stop(object sender, RoutedEventArgs e)
        {
            for( int i = 0; i < clients.Count; i++)
            {
                Socket socket = (Socket)clients[i];

                socket.Send(Encoding.ASCII.GetBytes("Server disconnected." + "\n"));


                socket.Close();


            } // for()

            clients.Clear();

            server.Close();
            server = null;


        }

        //listen to socket client
        private void Listen()
        {
            while (true)
            {
                try
                {
                    //accept()
                    Socket client = server.Accept();
                    Thread receive = new Thread(ReceiveMsg);
                    receive.Start(client);
                }
                catch (SocketException)
                { break; }
            }
        }

        //receive client message and send to client
        public void ReceiveMsg(object client)
        {
            bool firstMessage = false;
            Socket connection = (Socket)client;

            // 每個 connection 存到 clients
            clients.Add(connection);

            

            IPAddress clientIP = (connection.RemoteEndPoint as IPEndPoint).Address;


            String username = "";



            // 接收訊息
            while (connection.Connected)
            {
                try {

                    byte[] result = new byte[1024];
                    //receive message from client


                    int receive_num = connection.Receive(result);


                    String receive_str = Encoding.ASCII.GetString(result, 0, receive_num);

                    // 第一個訊息是為了知道他們的 username
                    if (!firstMessage)
                    {
                        firstMessage = true;

                        receive_msg.Dispatcher.BeginInvoke(
                                                    new Action(() => { receive_msg.Text += "\n" + receive_str + "(" + clientIP + ") connect\n"; }), null);

                        for (int i = 0; i < clients.Count; i++)
                        {
                            Socket socket = (Socket)clients[i];


                            //send welcome message to client
                            socket.Send(Encoding.ASCII.GetBytes("Welcome " + receive_str + "\n"));


                            username = receive_str;

                        } // for()



                    } // if()

                    // client disconnect
                    else if (receive_num > 0 && receive_str.Equals("DISCONNECT"))
                    {

                        // 刪除該 socket
                        for (int i = 0; i < clients.Count; i++)
                        {

                            if (connection.Equals((Socket)clients[i]))
                            {

                                clients.RemoveAt(i);


                            } // if()

                        } // for()

                        receive_msg.Dispatcher.BeginInvoke(
                                                    new Action(() => { receive_msg.Text += username + " disconnect.\n"; }), null);


                        connection.Shutdown(SocketShutdown.Both);
                        connection.Close();

                    }

                    // 其他訊息
                    else if (receive_num > 0)
                    {
                        String send_str = username + "(" + clientIP + ") : " + receive_str;

                        //resend message to client
                        connection.Send(Encoding.ASCII.GetBytes("You send: " + receive_str));


                        // resend message to other client
                        for (int i = 0; i < clients.Count; i++)
                        {
                            if ((Socket)clients[i] != connection)
                            {
                                Socket socket = (Socket)clients[i];

                                socket.Send(Encoding.ASCII.GetBytes(username + ": " + receive_str + "\n"));

                            } // if()

                        } // for()


                        receive_msg.Dispatcher.BeginInvoke(
                            new Action(() => { receive_msg.Text += send_str; }), null);


                    } // else if()



                }
                catch(SocketException) 
                { break; }
                


                


            } // while()


            

        }





        //close() when close window
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            Environment.Exit(0);
        }



    }
}
