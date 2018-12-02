using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace Qlay.Modules.LuaSocket
{
    [MoonSharpUserData]
    public class qConnection
    {
        [MoonSharpHidden]
        internal Qlay plugin;
        [MoonSharpHidden]
        internal Table luaTable;

        public bool IsActive { private set; get; }
        public int id { get; set; }

        [MoonSharpHidden]
        ConcurrentQueue<qLuaPacket> sendQueue = new ConcurrentQueue<qLuaPacket>();
        [MoonSharpHidden]
        ConcurrentQueue<qLuaPacket> receiveQueue = new ConcurrentQueue<qLuaPacket>();

        [MoonSharpHidden]
        Thread send, receive;

        [MoonSharpHidden]
        TcpClient client;
        [MoonSharpHidden]
        Socket socket;
        [MoonSharpHidden]
        qStream stream;

        [MoonSharpHidden]
        public event Action<qConnection> onClose;

        public bool Connect(string host, int port)
        {
            IsActive = true;
            client = new TcpClient();

            try
            {
                client.Connect(host, port);
                if (client.Connected)
                {
                    socket = client.Client;
                    stream = new qStream(client.GetStream());

                    send = new Thread(SendThread);
                    receive = new Thread(ReceiveThread);

                    send.Start();
                    receive.Start();

                    SocketConnections.ConnectionAdd(this);

                    return true;
                }
            }
            catch (Exception ex)
            {
                DynValue OnError = luaTable.Get("OnError");
                if (OnError.Function != null)
                    OnError.Function.Call(luaTable, ex.Message);
                else
                    plugin.log.Info("qlay->socket", "Connection error: " + ex.Message);
            }

            return false;
        }

        [MoonSharpHidden]
        public void ServerConnect(TcpClient client)
        {
            IsActive = true;
            this.client = client;
            if (client.Connected)
            {
                socket = client.Client;
                stream = new qStream(client.GetStream());

                send = new Thread(SendThread);
                receive = new Thread(ReceiveThread);

                send.Start();
                receive.Start();
            }
        }

        public void Close()
        {
            IsActive = false;

            if (socket != null && socket.Connected)
                socket.Close();
            if (client != null && client.Connected)
                client.Close();
            send = null;
            receive = null;

            onClose?.Invoke(this);

            sendQueue = new ConcurrentQueue<qLuaPacket>();
            receiveQueue = new ConcurrentQueue<qLuaPacket>();

            DynValue OnClose = luaTable.Get("OnClose");
            if (OnClose.Function != null)
                OnClose.Function.Call(luaTable);
            else
                plugin.log.Info("qlay->socket", "Connection closed");

            SocketConnections.ConnectionRemove(this);
        }

        public void Send(qLuaPacket packet)
        {
            if (IsActive)
            {
                MemoryStream memoryStream = packet.stream.baseStream as MemoryStream;
                packet.data = memoryStream.ToArray();
                sendQueue.Enqueue(packet);
            }
        }

        [MoonSharpHidden]
        public void SendThread()
        {
            while (IsActive && socket.Connected)
            {
                if (sendQueue.Count > 0)
                {
                    for (int i = 0; i < 1000 && sendQueue.Count > 0; i++)
                    {
                        qLuaPacket packet;
                        if (sendQueue.TryDequeue(out packet))
                        {
                            try
                            {
                                stream.Write(packet.id);
                                stream.Write(packet.data.Length);
                                stream.WriteRaw(packet.data);
                            }
                            catch (Exception ex)
                            {
                                plugin.log.Info("qlay->socket", "Error: " + ex.Message);
                                Close();
                            }
                        }
                    }
                }
                Thread.Sleep(1);
            }
        }

        [MoonSharpHidden]
        byte[] ReadWait(Stream s, int len)
        {
            int total = 0;
            byte[] data = new byte[len];
            int timeout_limit = 100;
            while (total < len)
            {
                var recvc = stream.baseStream.Read(data, total, (int)len - total);
                total += recvc;
                if (recvc == 0)
                {
                    timeout_limit--;
                    if (timeout_limit <= 0)
                    {
                        plugin.log.Info("qlay->socket", "Read timeout error");
                        Close();
                    }
                }
            }
            return data;
        }

        [MoonSharpHidden]
        public void ReceiveThread()
        {
            while (IsActive && socket.Connected)
            {
                try
                {
                    byte[] buffer = new byte[4];
                    for (int i = 0; i < 10; i++)
                    {
                        uint pid = BitConverter.ToUInt32(ReadWait(stream.baseStream, 4), 0);
                        uint len = BitConverter.ToUInt32(ReadWait(stream.baseStream, 4), 0);

                        byte[] data = ReadWait(stream.baseStream, (int)len);

                        qLuaPacket packet = new qLuaPacket(pid);
                        packet.data = data;
                        packet.sender = this;
                        receiveQueue.Enqueue(packet);
                    }
                }
                catch (Exception ex)
                {
                    plugin.log.Info("qlay->socket", "Net msg " + ex.Message);
                }
                Thread.Sleep(1);
            }
        }

        public void ProcessIncomingPacket()
        {
            if (IsActive && socket != null && socket.Connected)
            {
                if (receiveQueue.Count > 0)
                {
                    for (int i = 0; i < 200 && receiveQueue.Count > 0; i++)
                    {
                        qLuaPacket packet;
                        if (receiveQueue.TryDequeue(out packet))
                        {
                            try
                            {
                                MemoryStream memoryStream = new MemoryStream(packet.data);
                                qStream stream = new qStream(memoryStream);
                                packet.stream = stream;

                                DynValue OnReceive = luaTable.Get("OnReceive");
                                if (OnReceive.Function != null)
                                    OnReceive.Function.Call(luaTable, packet);
                                else
                                    plugin.log.Info("qlay->socket", "Packet receive id: " + packet.id);
                            }
                            catch (Exception ex)
                            {
                                plugin.log.Info("qlay->socket", "Read net msg " + ex.Message);
                                Close();
                            }
                        }
                    }
                }
            }
        }
    }

    public static class SocketConnections
    {
        private static List<qConnection> connections = new List<qConnection>();

        public static void ConnectionAdd(qConnection connection)
        {
            connections.Add(connection);
        }

        public static void ConnectionRemove(qConnection connection)
        {
            connections.Remove(connection);
        }

        public static void UpdateConnectionsReceive()
        {
            foreach (qConnection connection in connections)
            {
                connection.ProcessIncomingPacket();
            }
        }
    }
}
