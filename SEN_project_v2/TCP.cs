using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Windows.Controls;
using System.IO;
using System.Threading;

namespace SEN_project_v2
{
    public class TCP
    {
        TcpListener tcpRecevingListner;
        TcpClient tcpRecevingClient;
      //  TcpClient tcpSendingClient;
     //   List<string> files;
    //    public List<IPAddress> ips;
        //public Thread recevingThread;
        // public Thread sendingThread;
    //Dictionary<IPAddress, Thread> SendingThreads;
  //      Dictionary<IPAddress, TcpClient> Clients;
        List<Thread> SendingThreads;
        List<TcpClient> Clients;
        TCPServer server;
        public TCP()
        {
            //tcpRecevingListner = new TcpListener((int)MainWindow.Ports.TCP);
            //tcpRecevingListner.Start();
            //files = new List<string>();
            //recevingThread.Start();
          //  SendingThreads = new Dictionary<IPAddress, Thread>();
          //  Clients = new Dictionary<IPAddress, TcpClient>();
            SendingThreads = new List< Thread>();
            Clients = new List< TcpClient>();
            server = new TCPServer();
        }
        public void SendFiles(List<string> list,List<IPAddress> ips)
        {
                SendFiles(list, ips, 0);
        }
        public void SendFile(string file,IPAddress ip,int flag)
        {
            List<string> files = new List<string>();
            files.Add(file);
            List<IPAddress> ips = new List<IPAddress>();
            ips.Add(ip);
            SendFiles(files, ips,flag);
        }

        public void SendFiles(List<string> list, List<IPAddress> ips,int Flags)
        {
            //  files = list;
            //      this.ips = ips;
            foreach (IPAddress ip in ips)
            {
                // if (Clients.Keys.Contains(ip))
                //      Clients.Remove(ip);
                //      Clients.Add(ip, new TcpClient());
                //      if (SendingThreads.ContainsKey(ip))
                //         SendingThreads.Remove(ip);
                TcpClient tcpClient = new TcpClient();
                tcpClient.Connect(ip, (int)MainWindow.Ports.TCP);
                List<string> newList = list;
                Thread thread = new Thread((ThreadStart)delegate { fileSending_proc(newList, tcpClient,Flags); });
                SendingThreads.Add(thread);
                thread.Start();

            }
        }
    /*
        public  void tcpReceving_proc()
        {
          
          
            while (true)
            {
                tcpRecevingClient = tcpRecevingListner.AcceptTcpClient();
                IPAddress ip = ((IPEndPoint)tcpRecevingClient.Client.RemoteEndPoint).Address;
                NetworkStream readStream = tcpRecevingClient.GetStream();
                
                var filesCount=new Byte[4];
                readStream.Read(filesCount, 0, 4);
                for (int i = 0; i < BitConverter.ToInt32(filesCount, 0); i++)
                {
                    Int64 bytesReceived = 0;
                    ProgressBar progress = UserList.Get(ip).userView.Progressbar;
               
                    int count;
                    var buffer = new byte[1024 * 8];
                    readStream.Read(buffer, 0, 8);
                    Int64 numberOfBytes = BitConverter.ToInt64(buffer, 0);
                    progress.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        progress.Value = 0;
                        progress.Visibility = System.Windows.Visibility.Visible;
                        progress.Maximum = numberOfBytes / (1024 * 8);
                    }));
                    System.Diagnostics.Debug.WriteLine(numberOfBytes);
                    String FileName = "file";
                    Microsoft.Win32.SaveFileDialog saveFile = new Microsoft.Win32.SaveFileDialog();
                    if (saveFile.ShowDialog().Value == true)
                    {
                        System.Diagnostics.Debug.WriteLine(saveFile.FileName);
                        FileName = saveFile.FileName;

                    }

                    using (FileStream fileIO = File.Create(FileName))
                    {
                        int l = buffer.Length;
                        while (bytesReceived < numberOfBytes && (count = readStream.Read(buffer, 0, l)) > 0)
                        {
                            fileIO.Write(buffer, 0, count);
                            progress.Dispatcher.BeginInvoke((Action)(() =>
                            {
                                progress.Value++;
                                progress.UpdateLayout();
                            }));
                            bytesReceived += count;
                            if ((numberOfBytes - bytesReceived) < buffer.Length)
                                l = (int)(numberOfBytes - bytesReceived);

                        }
                        fileIO.Close();
                    }
                    progress.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        progress.Visibility = System.Windows.Visibility.Hidden;
                    }));
                 
                }
                 readStream.Close();
            }
        }
        */
        private void fileSending_proc(List<string> files,TcpClient tcpClient,int Flag)
        {
            //foreach (IPAddress ip in ips)
            //{
                TcpClient tcpSendingClient = tcpClient;
               if (tcpSendingClient.Connected)
                {
                    NetworkStream stream = tcpSendingClient.GetStream();
                    stream.Write(BitConverter.GetBytes(files.Count), 0, 4);
                    List<String> newList = new List<string>(files);
                    foreach (string file in newList)
                    {
                       
                        using (FileStream fileIO = File.OpenRead(file))
                        {
                            Byte[] length = BitConverter.GetBytes(fileIO.Length);
                            stream.Write(length, 0, 8);
                            string fileNames=file.Split('\\').Last();
                            stream.Write(BitConverter.GetBytes(fileNames.Length),0,4);
                            stream.Write(Encoding.ASCII.GetBytes(fileNames), 0, fileNames.Length);
                         
                            stream.Write(BitConverter.GetBytes(Flag), 0, 4);
                            Int64 byteSent = 0;
                            var buffer = new byte[1024 * 8];
                            int count;
                            try
                            {

                                while ((count = fileIO.Read(buffer, 0, buffer.Length)) > 0)
                                {
                                stream.Write(buffer, 0, count);
                              
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
               tcpClient.Close();
               Clients.Remove(tcpClient);
               SendingThreads.Remove(System.Threading.Thread.CurrentThread);
               System.Threading.Thread.CurrentThread.Abort();
            //}

        }

        public void Stop()
        {
            //if(recevingThread!=null && recevingThread.IsAlive)
            //recevingThread.Abort();
            //if(tcpRecevingClient!=null)
            //tcpRecevingClient.Close();
            //if (tcpRecevingListner != null)
            //    tcpRecevingListner.Stop();

            foreach (TcpClient client in Clients)
            {
                client.Close();
            }
            foreach (Thread thread in SendingThreads)
            {
                if (thread.IsAlive)
                    thread.Abort();
            }
            server.Stop();
        }
    }
}
