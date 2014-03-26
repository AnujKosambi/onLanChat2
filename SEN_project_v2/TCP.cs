using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Windows.Controls;
using System.IO;
namespace SEN_project_v2
{
    class TCP
    {
        TcpListener tcpRecevingListner;
        TcpClient tcpRecevingClient;
        TcpClient tcpSendingClient;
        public TCP()
        {
            tcpRecevingListner = new TcpListener((int)MainWindow.Ports.TCP);
            tcpRecevingListner.Start();
       
        }
        private void accept()
        {
            tcpRecevingClient = tcpRecevingListner.AcceptTcpClient();
           }
        public  void tcpReceving_proc()
        {

            ProgressBar pb = new ProgressBar();
            while (true)
            {

                
                IPAddress ip = ((IPEndPoint)tcpRecevingClient.Client.RemoteEndPoint).Address;
                NetworkStream readStream = tcpRecevingClient.GetStream();
                Int64 bytesReceived = 0;
                int count;
                var buffer = new byte[1024 * 8];
                readStream.Read(buffer, 0, 8);
                Int64 numberOfBytes = BitConverter.ToInt64(buffer, 0);
                using (FileStream fileIO = File.Create("anuj.mkv"))
                {
                    while (bytesReceived < numberOfBytes && (count = readStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        fileIO.Write(buffer, 0, count);
                        bytesReceived += count;
                        pb.Dispatcher.BeginInvoke((Action)(() => { pb.Value = (int)(bytesReceived / (1024 * 8)); }));
                    }
                    fileIO.Close();
                }
                 readStream.Close();
            }
        }

        private void fileSending_proc(IPAddress[] ips)
        {
            foreach (IPAddress ip in ips)
            {

                tcpSendingClient = new TcpClient();
                tcpSendingClient.Connect(ip, (int)MainWindow.Ports.TCP);
                string filePath = null;
                ProgressBar pb = null;
                 while (pb == null || filePath == null)
                    System.Threading.Thread.Sleep(10);
                if (tcpSendingClient.Connected)
                {

                    using (FileStream fileIO = File.OpenRead(filePath))
                    {
                        NetworkStream stream = tcpSendingClient.GetStream();
                        Byte[] length = BitConverter.GetBytes(fileIO.Length);

                        stream.Write(length, 0, 8);

                        var buffer = new byte[1024 * 8];
                       pb.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            pb.Minimum = 0;
                            pb.Maximum = (int)(fileIO.Length / (1024 * 8));
                            pb.SmallChange = 1;
                        }));
                        int progress=0;
                        int count;
                        try
                        {
                            while ((count = fileIO.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                progress=+1;
                                stream.Write(buffer, 0, count);
                                UpdateProgressBarDelegate upbd = new UpdateProgressBarDelegate(pb.SetValue);
                                pb.Dispatcher.Invoke(upbd, System.Windows.Threading.DispatcherPriority.Background,
                                new object[] { ProgressBar.ValueProperty, progress });
                            }
                        }
                        catch (Exception e)
                        {
                            System.Windows.Forms.MessageBox.Show(e.Message, "Error in sending File", 
                                System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);

                        }
                        fileIO.Close();
                    }
          
                }

            }

        }
        private delegate void UpdateProgressBarDelegate(System.Windows.DependencyProperty dp, Object value);   
    }
}
