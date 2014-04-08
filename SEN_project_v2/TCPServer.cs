using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Threading;
namespace SEN_project_v2
{
    class TCPServer
    {
        TcpListener Listner;
    //    Dictionary<IPAddress, TcpClient> Clients;
    //    Dictionary<IPAddress, Thread> Threads;
        List<Thread> Threads;
        List<TcpClient> Clients;
        Thread recievingThread;
        TcpClient currentClient;
        Dictionary<string, string> folderPath;
        public TCPServer()
        {
            Clients = new List<TcpClient>();
            Threads = new List< Thread>();
            folderPath = new Dictionary<string, string>();
            recievingThread = new Thread(new ThreadStart(recievingThread_proc));
            recievingThread.Start();
        }
        public void recievingThread_proc()
        {
            Listner = new TcpListener((int)MainWindow.Ports.TCP);
            Listner.Start();
            try
            {
                while (true)
                {
                    currentClient = Listner.AcceptTcpClient();
                    TcpClient client = currentClient;
                    Clients.Add(client);
                    Thread thread = new Thread((ThreadStart)delegate { tcpReceving_proc(client); });
                    Threads.Add(thread);
                    thread.Start();
                    System.Diagnostics.Debug.WriteLine("Recieving Thread is Started ...");
                }
            }catch(ThreadAbortException e)
            {
              //  System.Windows.MessageBox.Show(e.Message);
            }
        }
        
        public void tcpReceving_proc(TcpClient Client)
        {

                TcpClient tcpRecevingClient = Client;
                IPAddress ip = ((IPEndPoint)tcpRecevingClient.Client.RemoteEndPoint).Address;
             
                NetworkStream readStream = tcpRecevingClient.GetStream();

                var filesCount = new Byte[4];
                readStream.Read(filesCount, 0, 4);
                for (int i = 0; i < BitConverter.ToInt32(filesCount, 0); i++)
                {
                    Int64 bytesReceived = 0;
                    ProgressBar progress = UserList.Get(ip).userView.Progressbar;
                    
                    string filename="";
                    int count;
                    var buffer = new byte[1024 * 8];
                    byte isDir=(byte)readStream.ReadByte();

                    int Flag=0;
                     Int64 numberOfBytes =0;
              
                        readStream.Read(buffer, 0, 8);
                        numberOfBytes = BitConverter.ToInt64(buffer, 0);

                        readStream.Read(buffer, 0, 4);
                        int stringLength = BitConverter.ToInt32(buffer, 0);
                        readStream.Read(buffer, 0, stringLength);
                        filename = Encoding.ASCII.GetString(buffer, 0, stringLength);
                        string Path="";
                        if (isDir == 0xD)
                        {
                            readStream.Read(buffer, 0, 4);
                            int pathLength = BitConverter.ToInt32(buffer, 0);
                            readStream.Read(buffer, 0, pathLength);
                            Path = Encoding.ASCII.GetString(buffer, 0, pathLength);
                        }
                        readStream.Read(buffer, 0, 4);
                        Flag = BitConverter.ToInt32(buffer, 0);
                    
               //         System.Diagnostics.Debug.WriteLine(numberOfBytes);
                        String FileName = filename;
                    
                    if (Flag == 0)
                    {

                        progress.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            progress.Value = 0;
                            progress.Visibility = System.Windows.Visibility.Visible;
                            progress.Maximum = numberOfBytes / (1024 * 8);
                        }));
                        Microsoft.Win32.SaveFileDialog saveFile = new Microsoft.Win32.SaveFileDialog();
                        saveFile.Filter = "("+filename.Split('.').Last()+") Files|*." + filename.Split('.').Last() + "";
                        saveFile.Title = filename;
                        saveFile.FileName = filename;
                        if (saveFile.ShowDialog().Value == true)
                        {

                            FileName = saveFile.FileName;
                           

                        }
                        else
                        {
                            readStream.Close();
                            progress.Dispatcher.BeginInvoke((Action)(() =>
                            {
                                progress.Visibility = System.Windows.Visibility.Hidden;
                            }));
                            break;
                        }
                    }
                    else if(Flag==1)
                    {
                        FileName = AppDomain.CurrentDomain.BaseDirectory + ip.ToString().Replace('.', '\\') + "\\" + UserList.xml[ip].CountMessages+"."+
                            string.Join(".",filename.Split('.').Skip(1).ToArray());
                    }
                    else if(Flag==2)
                    {
                        FileName = AppDomain.CurrentDomain.BaseDirectory + ip.ToString().Replace('.', '\\') + "\\" + filename;
                    }
                    else if(Flag==3)
                    {
                        progress.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            progress.Value = 0;
                            progress.Visibility = System.Windows.Visibility.Visible;
                            progress.Maximum = numberOfBytes / (1024 * 8);
                        }));
                        if (isDir == 0xD)
                            FileName = Path + "\\" + filename;
                    }
                   
                   using (FileStream fileIO = File.Open(FileName,FileMode.Create))
                   // FileStream fileIO = File.Create(FileName);
                    {
                        int l = buffer.Length;
                        while (bytesReceived < numberOfBytes && (count = readStream.Read(buffer, 0, l)) > 0)
                        {
                            fileIO.Write(buffer, 0, count);
                   //         progress.Dispatcher.BeginInvoke((Action)(() =>
                          //  {
                                //progress.Value++;
                              //  progress.UpdateLayout();
                   //         }));
                            

                                progress.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render,
                                    new DispatcherOperationCallback(delegate { progress.Value++; return null; }), null);
                     //       progress.Dispatcher.Invoke((()=> {progress.Value++; progress.UpdateLayout()}));
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
                Clients.Remove(Client);
                Client.Close();

                
              //  Clients.Remove(ip);
              //  Threads.Remove(ip);
        }

        public void Stop()
        {
            if(Listner!=null)
            {
                Listner.Stop();
            }
            if(recievingThread!=null && recievingThread.IsAlive)
            {
                recievingThread.Interrupt();
                recievingThread.Abort();
            }

            foreach(TcpClient client in Clients)
            {
                client.Close();
            }
            foreach (Thread thread in Threads)
            {
                if (thread.IsAlive)
                    thread.Abort();
            }
        }
    }
}
