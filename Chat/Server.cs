using System;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.IO;
using System.Net;

namespace Chat
{
    class Server : IDisposable
    {
        private byte[] IPaddr;
        private int listenPort;
        private TcpListener listener;
        private TcpClient client;
        private BinaryReader br;
        private BinaryWriter bw;
        private Task threadRead;
        private Task threadIfOnline;

        public delegate void ChangeStatusTextBlock(string status);
        public ChangeStatusTextBlock changeStatusTextBlock;

        public delegate void PrintMsg(string msg);
        public PrintMsg printMsg;

        public delegate void EnableAll();
        public EnableAll enableAll;

        public delegate void EnableSendButton(bool flag);
        public EnableSendButton enableSendButton;

        public delegate void SetInfo(string desIP, string desPort, string localIP, string localPort);
        public SetInfo setInfo;

        public Server(byte[] IPaddr, int listenPort)
        {
            this.listenPort = listenPort;
            this.IPaddr = IPaddr;
        }

        public void waitConnect()
        {
            client = null;
            try
            {
                if (listener == null)
                {
                    listener = new TcpListener(new System.Net.IPAddress(IPaddr), listenPort);
                    listener.Start();
                }
            }
            catch
            {
                changeStatusTextBlock(string.Format("{0}：连接错误", DateTime.Now));
                enableAll();
                return;
            }

            while (client == null)
            {
                try
                {
                    client = listener.AcceptTcpClient();
                    IPEndPoint clientDesIP = (IPEndPoint)client.Client.RemoteEndPoint;
                    IPEndPoint clientLocalIP = (IPEndPoint)client.Client.LocalEndPoint;
                    changeStatusTextBlock(string.Format("{1}：{0}已连接", clientDesIP.Address.ToString(), DateTime.Now));
                    setInfo(clientDesIP.Address.ToString(), clientDesIP.Port.ToString(), clientLocalIP.Address.ToString(), clientLocalIP.Port.ToString());
                    enableSendButton(true);
                    if (threadIfOnline == null || threadIfOnline.IsCompleted == true)
                    {
                        threadIfOnline = new Task(IfOnline);
                        threadIfOnline.Start();
                    }
                }
                catch
                {
                    continue;
                }
                System.Threading.Thread.Sleep(100);
            }
            br = new BinaryReader(client.GetStream());
            bw = new BinaryWriter(client.GetStream());
            threadRead = new Task(read);
            threadRead.Start();
        }

        private void read()
        {
            while(true)
            {
                if (br == null) return;
                try
                {
                    var msg = br.ReadString();
                    printMsg(msg);
                }
                catch
                {
                    continue;
                }
                System.Threading.Thread.Sleep(100);
            }
        }

        public void write(string msg)
        {
            bw.Write(msg);
        }

        public void stopListen()
        {
            closeConnect();
            listener.Stop();
        }

        public void closeConnect()
        {
            if (br != null) br.Close();
            if (bw != null) bw.Close();
            if (client != null) client.Close();
            br = null;
            bw = null;
            client = null;
        }

        private void IfOnline()
        {
            while (true)
            {
                try
                {
                    if (((client.Client.Poll(1000, SelectMode.SelectRead) && (client.Client.Available == 0)) || !client.Client.Connected))
                    {
                        changeStatusTextBlock(string.Format("{0}：等待连接中", DateTime.Now));
                        closeConnect();
                        enableSendButton(false);
                        waitConnect();
                    }
                }
                catch
                {
                    return;
                }
                System.Threading.Thread.Sleep(100);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                client.Close();
                br.Close();
                br.Dispose();
                bw.Close();
                bw.Dispose();
                threadRead.Dispose();
                threadIfOnline.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
