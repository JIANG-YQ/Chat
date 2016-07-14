using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Chat
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window, IDisposable
    {
        private Server server;
        private Client client;

        public MainWindow()
        {
            InitializeComponent();
            localIPTextBox.Text = "127.0.0.1";
            desIPTextBox.Text = "127.0.0.1";
            sendButton.IsEnabled = false;
            writeTextBox.IsEnabled = false;
            statusTextBlock.Text = string.Format("{0}：就绪", DateTime.Now);
        }

        private void listenButtonClick(object sender, RoutedEventArgs e)
        {
            if (listenButton.Content.ToString() == "等待连接") waitingConnect();
            else if (listenButton.Content.ToString() == "停止等待") stopWaitConnect();
            else if (listenButton.Content.ToString() == "断开") stopWaitConnect();
        }

        private void waitingConnect()
        {
            if (!IPisRight()) return;
            var IPStrArr = localIPTextBox.Text.Split('.');
            var IP = new byte[4];
            for (var i = 0; i < 4; i++) IP[i] = Convert.ToByte(IPStrArr[i]);
            var port = Convert.ToInt32(localPortTextBox.Text);
            server = new Server(IP, port);
            server.changeStatusTextBlock += status => statusTextBlock.Dispatcher.Invoke(new Server.ChangeStatusTextBlock(changeStatusCallback), status); ;
            server.enableAll += () => listenButton.Dispatcher.Invoke(new Server.EnableAll(enableAllCallback));
            server.enableAll += () => connectButton.Dispatcher.Invoke(new Server.EnableAll(enableAllCallback));
            server.enableAll += () => localPortTextBox.Dispatcher.Invoke(new Server.EnableAll(enableAllCallback));
            server.enableAll += () => desIPTextBox.Dispatcher.Invoke(new Server.EnableAll(enableAllCallback));
            server.enableAll += () => desPortTextBox.Dispatcher.Invoke(new Server.EnableAll(enableAllCallback));
            server.enableAll += () => sendButton.Dispatcher.Invoke(new Server.EnableAll(enableAllCallback));
            server.enableAll += () => writeTextBox.Dispatcher.Invoke(new Server.EnableAll(enableAllCallback));
            server.enableSendButton += flag => sendButton.Dispatcher.Invoke(new Server.EnableSendButton(enableSendCallback), flag);
            server.changelistenButton += str => listenButton.Dispatcher.Invoke(new Server.ChangelistenButton(changelistenButtonCallback), str);
            server.printMsg += msg => msgSP.Dispatcher.Invoke(new Server.PrintMsg(printMsgCallback), msg);
            server.setInfo += (desIP, desPort, localIP, localPort) => desIPTextBox.Dispatcher.Invoke(new Server.SetInfo(setDesIPAndPortCallback), desIP, desPort, localIP, localPort);
            connectButton.IsEnabled = false;
            localIPTextBox.IsEnabled = false;
            localPortTextBox.IsEnabled = false;
            desIPTextBox.IsEnabled = false;
            desPortTextBox.IsEnabled = false;
            listenButton.Content = "停止等待";
            statusTextBlock.Text = string.Format("{0}：等待连接中", DateTime.Now);
            Task threadWaitConnect = new Task(server.waitConnect);
            threadWaitConnect.Start();
        }

        private void stopWaitConnect()
        {
            server.stopListen();
            connectButton.IsEnabled = true;
            localIPTextBox.IsEnabled = true;
            localPortTextBox.IsEnabled = true;
            desIPTextBox.IsEnabled = true;
            desPortTextBox.IsEnabled = true;
            writeTextBox.IsEnabled = false;
            sendButton.IsEnabled = false;
            statusTextBlock.Text = string.Format("{0}：已断开连接", DateTime.Now);
            listenButton.Content = "等待连接";
        }

        private void connectButtonClick(object sender, RoutedEventArgs e)
        {
            if (connectButton.Content.ToString() == "连接") connect();
            else if (connectButton.Content.ToString() == "断开") stopConnect();
        }

        private void connect()
        {
            if (!IPisRight()) return;
            var IPStrArr = desIPTextBox.Text.Split('.');
            var IP = new byte[4];
            for (var i = 0; i < 4; i++) IP[i] = Convert.ToByte(IPStrArr[i]);
            var port = Convert.ToInt32(desPortTextBox.Text);
            client = new Client();
            client.changeStatusTextBlock += status => statusTextBlock.Dispatcher.Invoke(new Client.ChangeStatusTextBlock(changeStatusCallback), status);
            client.enableAll += () => listenButton.Dispatcher.Invoke(new Client.EnableAll(enableAllCallback));
            client.enableAll += () => connectButton.Dispatcher.Invoke(new Client.EnableAll(enableAllCallback));
            client.enableAll += () => localPortTextBox.Dispatcher.Invoke(new Client.EnableAll(enableAllCallback));
            client.enableAll += () => desIPTextBox.Dispatcher.Invoke(new Client.EnableAll(enableAllCallback));
            client.enableAll += () => desPortTextBox.Dispatcher.Invoke(new Client.EnableAll(enableAllCallback));
            client.enableAll += () => sendButton.Dispatcher.Invoke(new Client.EnableAll(enableAllCallback));
            client.enableAll += () => writeTextBox.Dispatcher.Invoke(new Client.EnableAll(enableAllCallback));
            client.printMsg += msg => msgSP.Dispatcher.Invoke(new Client.PrintMsg(printMsgCallback), msg);
            client.setInfo += (desIP, desPort, localIP, localPort) => desIPTextBox.Dispatcher.Invoke(new Client.SetInfo(setDesIPAndPortCallback), desIP, desPort, localIP, localPort);
            listenButton.IsEnabled = false;
            localIPTextBox.IsEnabled = false;
            localPortTextBox.IsEnabled = false;
            desIPTextBox.IsEnabled = false;
            desPortTextBox.IsEnabled = false;
            connectButton.Content = "断开";
            sendButton.IsEnabled = true;
            writeTextBox.IsEnabled = true;
            client.connect(IP, port);
        }

        private void stopConnect()
        {
            client.closeConnect();
            listenButton.IsEnabled = true;
            localIPTextBox.IsEnabled = true;
            localPortTextBox.IsEnabled = true;
            desIPTextBox.IsEnabled = true;
            desPortTextBox.IsEnabled = true;
            writeTextBox.IsEnabled = false;
            sendButton.IsEnabled = false;
            statusTextBlock.Text = string.Format("{0}：已断开连接", DateTime.Now);
            connectButton.Content = "连接";
        }

        private bool IPisRight()
        {
            if (!Regex.IsMatch(localIPTextBox.Text, @"((?:(?:25[0-5]|2[0-4]\d|((1\d{2})|([1-9]?\d)))\.){3}(?:25[0-5]|2[0-4]\d|((1\d{2})|([1-9]?\d))))"))
            {
                statusTextBlock.Text = string.Format("{0}：输入IP不合法", DateTime.Now);
                return false;
            }
            else return true;
        }

        private void setDesIPAndPortCallback()
        {
            throw new NotImplementedException();
        }

        private void changeStatusCallback(string status)
        {
            statusTextBlock.Text = status;
        }

        private void enableSendCallback(bool flag)
        {
            sendButton.IsEnabled = flag;
            writeTextBox.IsEnabled = flag;
        }

        private void enableAllCallback()
        {
            listenButton.IsEnabled = true;
            connectButton.IsEnabled = true;
            localIPTextBox.IsEnabled = true;
            localPortTextBox.IsEnabled = true;
            desIPTextBox.IsEnabled = true;
            desPortTextBox.IsEnabled = true;
            writeTextBox.IsEnabled = false;
            sendButton.IsEnabled = false;
            listenButton.Content = "等待连接";
            connectButton.Content = "连接";
        }

        private void changelistenButtonCallback(string str)
        {
            listenButton.Content = str;
        }

        private void sendButtonClick(object sender, RoutedEventArgs e)
        {
            if (!sendButton.IsEnabled) return;
            if (writeTextBox.Text == "") return;
            if (listenButton.IsEnabled) server.write(writeTextBox.Text);
            if (connectButton.IsEnabled) client.write(writeTextBox.Text);
            var msgLabel = new SendMsg();
            msgLabel.Text.Text = writeTextBox.Text;
            msgSP.Children.Add(msgLabel);
            writeTextBox.Text = "";
            statusTextBlock.Text = string.Format("{0}：发送消息", DateTime.Now);
            scrollViewer.ScrollToEnd();
        }

        private void printMsgCallback(string msg)
        {
            if (msg.EndsWith("-img-:"))
            {
                msg = msg.Remove(msg.Length - 6);
                byte[] imgBytes = Convert.FromBase64String(msg);
                BitmapImage bmp = null;
                bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.StreamSource = new MemoryStream(imgBytes);
                bmp.EndInit();
                var img = new RevImg();
                img.image.Source = bmp;
                if (bmp.Width < img.image.MaxWidth && bmp.Width > img.image.MinWidth)
                {
                    img.image.Stretch = Stretch.None;
                }
                else if (bmp.Width <= img.image.MinWidth)
                {
                    img.image.Width = img.image.MinWidth;
                    img.image.Stretch = Stretch.Uniform;
                }
                else
                {
                    img.image.Stretch = Stretch.Uniform;
                }
                msgSP.Children.Add(img);
                scrollViewer.ScrollToEnd();
            }
            else
            {
                var msgLabel = new RevMsg();
                msgLabel.Text.Text = msg;
                msgSP.Children.Add(msgLabel);
                scrollViewer.ScrollToEnd();
            }
            statusTextBlock.Text = string.Format("{0}：收到消息", DateTime.Now);
        }

        private void setDesIPAndPortCallback(string desIP, string desPort, string localIP, string localPort)
        {
            desIPTextBox.Text = desIP;
            desPortTextBox.Text = desPort;
            localIPTextBox.Text = localIP;
            localPortTextBox.Text = localPort;
        }

        private void writeTextBoxPreviewDragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Copy;
            e.Handled = true;
        }

        private void writeTextBoxPreviewDrop(object sender, DragEventArgs e)
        {
            foreach (string f in (string[])e.Data.GetData(DataFormats.FileDrop)) writeTextBox.Text = f;
            BitmapImage image = new BitmapImage(new Uri(writeTextBox.Text));
            var img = new SendImg();
            img.image.Source = image;
            if (image.Width < img.image.MaxWidth && image.Width > img.image.MinWidth)
            {
                img.image.Stretch = Stretch.None;
            }
            else if (image.Width <= img.image.MinWidth)
            {
                img.image.Width = img.image.MinWidth;
                img.image.Stretch = Stretch.Uniform;
            }
            else
            {
                img.image.Stretch = Stretch.Uniform;
            }
            msgSP.Children.Add(img);
            scrollViewer.ScrollToEnd();
            Byte[] bytes = File.ReadAllBytes(@writeTextBox.Text);
            writeTextBox.Text = "";
            MemoryStream ms = new MemoryStream(bytes);
            String imgStr = Convert.ToBase64String(ms.ToArray());
            if (listenButton.IsEnabled) server.write(imgStr + "-img-:");
            if (connectButton.IsEnabled) client.write(imgStr);
            statusTextBlock.Text = string.Format("{0}：发送消息", DateTime.Now);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                client.Dispose();
                server.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}