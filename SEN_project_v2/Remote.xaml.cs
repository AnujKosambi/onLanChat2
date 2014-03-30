using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Net;
using System.Runtime.InteropServices;
using System.Diagnostics;
namespace SEN_project_v2
{
    /// <summary>
    /// Interaction logic for Remote.xaml
    /// </summary>
    public partial class Remote : Window
    {
        public RTPClient rtpClient;
        public System.Windows.Forms.Timer timer;
        public System.Windows.Forms.Timer mouseTimer;
        private ScreenCapture sc;
        public static int MouseFlag=1;
        public static int oldMouseFlag = 1;
        public static User32.POINT mousePos;
        private UDP udp = MainWindow.udp;
        private static IPAddress  remoteIP;
        public static Point Location;
        public Remote(Window parent,IPAddress ip)
        {
            InitializeComponent();
            Dictionary<IPAddress,VideoPreview> vplist=new Dictionary<IPAddress,VideoPreview>();
            
            remoteIP = ip;
            this.Left = Location.X;
            this.Top = Location.Y;
            this.LocationChanged += Remote_LocationChanged;
            rtpClient = new RTPClient(new IPEndPoint(ip, (int)MainWindow.Ports.RTP), Screen, MainWindow.hostIP.ToString(), ip.ToString());
            rtpClient.window = this;
            sc = new ScreenCapture();
            timer = new System.Windows.Forms.Timer();
            mouseTimer = new System.Windows.Forms.Timer();
            timer.Interval = 100;
            timer.Tick += timer_Tick;
            mouseTimer.Interval = 1;
            mouseTimer.Tick += mouseTimer_Tick;
          
        }

        void Remote_LocationChanged(object sender, EventArgs e)
        {
            Location.X = (sender as Window).Left;
            Location.Y = (sender as Window).Top;

        }

        void mouseTimer_Tick(object sender, EventArgs e)
        {
          // 
            if((MouseFlag&1)==1)
            if(oldMouseFlag!=MouseFlag)
                User32.mouse_event(0x00000002, (uint)mousePos.X, (uint)mousePos.Y, 0, UIntPtr.Zero);

            if ((MouseFlag & 1)==0)
                if ((oldMouseFlag & 1)==1)
                User32.mouse_event(0x00000004, (uint)mousePos.X, (uint)mousePos.Y, 0, UIntPtr.Zero);
         
            if ((MouseFlag & 2) == 2)
                if (oldMouseFlag != MouseFlag)
                    User32.mouse_event(0x0008, (uint)mousePos.X, (uint)mousePos.Y, 0, UIntPtr.Zero);

            if ((MouseFlag & 2) == 0)
                if ((oldMouseFlag & 2) == 2)
                    User32.mouse_event(0x0010, (uint)mousePos.X, (uint)mousePos.Y, 0, UIntPtr.Zero);
            User32.SetCursorPos(mousePos.X, mousePos.Y);
            oldMouseFlag = MouseFlag;
        }

        void timer_Tick(object sender, EventArgs e)
        {
            rtpClient.rtpSender.Send(sc.GetDesktopBitmapBytes());

            
        }
        public void Start()
        {
            udp.SendMessageTo(UDP.Remote, remoteIP);
            Hook.Start();
        }
        public void StartSending()
        {
            timer.Start();
            mouseTimer.Start();
            Hook.Stop();
        }
        public void StopSending()
        {
            timer.Stop();
            if (mouseTimer != null)
                mouseTimer.Stop();
            rtpClient.Dispose();

        }
        public class User32
        {

            [DllImport("user32.dll")]   
            public static extern bool GetCursorPos(out POINT lpPoint);
            [StructLayout(LayoutKind.Sequential)]
            public  struct POINT
            {
                public  int X;
                public  int Y;

                public static implicit operator Point(POINT point)
                {
                    return new Point(point.X, point.Y);
                }
            }
            [DllImportAttribute("user32.dll", EntryPoint = "SetCursorPos")]
            [return: MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.Bool)]
            public static extern bool SetCursorPos(int X, int Y);

            [DllImport("user32.dll")]
            public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, UIntPtr dwExtraInfo);
        }

        private void Screen_MouseMove(object sender, MouseEventArgs e)
        {
           // MouseFlag |= 0x0001;
            UpdateMouseData();
        }
        private void UpdateMouseData()
        {
            User32.GetCursorPos(out mousePos);
         //   udp.SendMessageTo(UDP.Mouse + MouseFlag + UDP.Breaker + mousePos.X + UDP.Breaker + mousePos.Y, remoteIP);
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (mouseTimer != null)
                mouseTimer.Stop();
            if (timer != null)
                timer.Stop();
            if (rtpClient != null)
                rtpClient.Dispose();
            
            Hook.Stop();
        }

        private void Screen_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
         //   MOUSEEVENTF_RIGHTDOWN 0x0008
         //   MouseFlag |= Convert.ToByte("00000010", 2);
       //   UpdateMouseData();
        }

        private void Screen_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            //MOUSEEVENTF_RIGHTUP 0x0010
     //       MouseFlag &= Convert.ToByte("11111101", 2); ;
        //    UpdateMouseData();
        }

        private void Screen_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //MOUSEEVENTF_LEFTDOWN 0x00024
        //    MouseFlag |= Convert.ToByte("00000100", 2);
         //   UpdateMouseData();
        }

        private void Screen_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //MOUSEEVENTF_LEFTUP 0x0004
         //   MouseFlag &= Convert.ToByte("11111011", 2); ;
        //    UpdateMouseData();
        }

    class Hook{
     
         private static LowLevelMouseProc _proc = HookCallback;
         private static IntPtr _hookID = IntPtr.Zero;
         private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);
         public static void Start()
         {

            _hookID = SetHook(_proc);
         }
         public static void Stop()
         {
              UnhookWindowsHookEx(_hookID);
         }
          private static IntPtr SetHook(LowLevelMouseProc proc)
         {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_MOUSE_LL, proc,
                GetModuleHandle(curModule.ModuleName), 0);
            }
        }

       public static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
        if (nCode >= 0)
       {
            MSLLHOOKSTRUCT hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));

            if ((wParam.ToInt32() & 0xF) == 1)
            {
                MouseFlag |= 1;
            }
            else if ((wParam.ToInt32() & 0xF) == 2)
            {
               MouseFlag &=~1;
            }
            else if ((wParam.ToInt32() & 0xF) == 4)
            {
                MouseFlag |= 2;
            }
            else if ((wParam.ToInt32() & 0xF) == 5)
            {
                MouseFlag &= ~2;
            }


            MainWindow.udp.SendMessageTo(UDP.Mouse + MouseFlag + UDP.Breaker +(mousePos.X-Location.X) + UDP.Breaker + (mousePos.Y-Location.Y), remoteIP);
            System.Diagnostics.Debug.WriteLine(Convert.ToString(wParam.ToInt32(),16) + " " + mousePos.X + " " + mousePos.Y);
          }
        return CallNextHookEx(_hookID, nCode, wParam, lParam);
       }
          private const int WH_MOUSE_LL = 14;

          #region DLL
          private enum MouseMessages
    {
        WM_LBUTTONDOWN = 0x0201,
        WM_LBUTTONUP = 0x0202,
        WM_MOUSEMOVE = 0x0200,
        WM_MOUSEWHEEL = 0x020A,
        WM_RBUTTONDOWN = 0x0204,
        WM_RBUTTONUP = 0x0205
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int x;
        public int y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MSLLHOOKSTRUCT
    {
        public POINT pt;
        public uint mouseData;
        public uint flags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook,
        LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
        IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);
          #endregion
    }

    private void Scroller_ScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine(e.VerticalOffset);
        
    }


    }
}
