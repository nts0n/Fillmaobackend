﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using Newtonsoft.Json;
using FireSharp.Config;
using FireSharp.Interfaces;

namespace dieforproject
{
    internal class room
    {
        int id = 0;
        int port = 0;
        List<Socket> clients = new List<Socket>();
        IPEndPoint RoomServerIP;
        Socket server_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        Thread server_run;
        NetworkStream ns;
        Random r = new Random();
        public room(int id, int port) {
            this.id = id;
            this.port = port;
            RoomServerIP = new IPEndPoint(IPAddress.Any, port);
            xx = new FireSharp.FirebaseClient(config);
            server_run = new Thread(Start);
            server_run.Start();
        }

        IFirebaseConfig config = new FirebaseConfig()
        {
            AuthSecret = "vTd2kobcxCaAw3EbOOqfRQmWxdyiD6JkB80yr5t2",
            BasePath = "https://fillmao-4e9e1-default-rtdb.asia-southeast1.firebasedatabase.app/"
        };
        IFirebaseClient xx;

        void send(Socket client, string msg)
        {
            string s = "fuckiiiii";
            byte[] send_data = System.Text.Encoding.UTF8.GetBytes(s);
            ns = new NetworkStream(client);
      
            ns.Write(send_data, 0, send_data.Length);
        }

        void room_create(roomdetail room_Recv, Socket client)
        {
            string idphong = r.Next(100000, 999999).ToString();
            room_Recv.RoomId = idphong;
            var ssetter = xx.Set("room/" + idphong, room_Recv);
            send(client, idphong);
        }
        void receive_and_broacast(object obj)
        {
            Socket client = (Socket)obj;
            try
            {
                while (true){


                    string msg = "";
                    do
                    {
                        byte[] recvd = new byte[1024];
                        client.Receive(recvd);
                        msg += Encoding.UTF8.GetString(recvd);
                    }while (msg[msg.Length-1] != '\0' && msg[msg.Length-1] != '\n');
                    

                    
                    if (msg != null && msg[0] != '\0')
                    {
                        Console.WriteLine(msg);
                        string[] response_Recv = msg.Split(", ");
                        string option = response_Recv[0];
                        Console.WriteLine(response_Recv[1]);
                        if (option == "login")
                        {
                            Player player_Info;
                            player_Info = JsonConvert.DeserializeObject<Player>(response_Recv[1]);
                            string idngchoi = r.Next(100000, 999999).ToString();
                            Player player = new Player(
                                idngchoi,
                                player_Info.PlayerName

                            );
                            var ssetter = xx.Set("player/" + idngchoi, player);
                            send(client, idngchoi);
                        }
                        else if(option == "create_room")
                        {
                            roomdetail room_Recv;
                            room_Recv = JsonConvert.DeserializeObject<roomdetail>(response_Recv[1]);
                            room_create(room_Recv, client);
                        }
                        
                    }
                    else if ((client.Poll(-1, SelectMode.SelectRead)) || (client.Available != 0))
                    {
                        Console.WriteLine("LOLOLOLOLOLO");
                        clients.Remove(client);
                        
                        client.Close();
                        if (clients.Count == 0)
                        {
                            Console.WriteLine("bo m dong server");
                            server_socket.Close();
                        }
                        break;

                    }
                    else
                    {
                        clients.Remove(client);
                        Console.WriteLine("1 thz out");
                        client.Close();
                        if (clients.Count == 0)
                        {
                            Console.WriteLine("bo m dong server");
                            server_socket.Close();
                        }
                        break;
                    }
                    
                }
            }
            catch
            {
                Console.WriteLine("1 thz ou   t");
                clients.Remove(client);
                client.Close();
                if (clients.Count == 0)
                {
                    Console.WriteLine("bo m dong server");
                    server_socket.Close();
                }
            }
        }
        
        public void Start()
        {
            server_socket.Bind(RoomServerIP);
            Thread listen = new Thread(() =>
            {
                try
                {
                    while (true)
                    {
                        Console.WriteLine("hello i'm in");
                        server_socket.Listen(-1);
                        Socket client = server_socket.Accept();
                        clients.Add(client);

                        Thread receive = new Thread(receive_and_broacast);
                        receive.IsBackground = true;
                        receive.Start(client);
                    }
                }
                catch
                {
                    RoomServerIP = new IPEndPoint(IPAddress.Any , port);
                    Socket server_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                }
            });
            //listen.IsBackground = true;
            listen.Start();
        }

    }


}
