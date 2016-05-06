using System;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.IO;
using System.Net;

namespace Chat
{
    class Client : IDisposable
    {
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

        public delegate void SetInfo(string desIP, string desPort, string localIP, string localPort);
        public SetInfo setInfo;

        public Client()
        {
            client = new TcpClient();
        }

        public void connect(byte[] IPaddr, int listenPort)
        {
            try {
                client.Connect(new System.Net.IPAddress(IPaddr), listenPort);
                br = new BinaryReader(client.GetStream());
                bw = new BinaryWriter(client.GetStream());
                IPEndPoint clientDesIP = (IPEndPoint)client.Client.RemoteEndPoint;
                IPEndPoint clientLocalIP = (IPEndPoint)client.Client.LocalEndPoint;
                threadRead = new Task(read);
                threadRead.Start();
                changeStatusTextBlock(string.Format("{1}：已连接{0}", clientDesIP.Address.ToString(), DateTime.Now));
                setInfo(clientDesIP.Address.ToString(), clientDesIP.Port.ToString(), clientLocalIP.Address.ToString(), clientLocalIP.Port.ToString());
                if (threadIfOnline == null || threadIfOnline.IsCompleted == true)
                {
                    threadIfOnline = new Task(IfOnline);
                    threadIfOnline.Start();
                }
            }
            catch
            {
                changeStatusTextBlock(string.Format("{0}：连接错误", DateTime.Now));
                enableAll();
                return;
            }
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
                        changeStatusTextBlock(string.Format("{0}：已断开连接", DateTime.Now));
                        closeConnect();
                        enableAll();
                        return;
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
