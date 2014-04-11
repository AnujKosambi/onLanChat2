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
        public System.Windows.Forms.Timer mousekeyTimer;
        private ScreenCapture sc;
        public static int MouseFlag=1;
        public static int oldMouseFlag = 1;
        public static User32.POINT mousePos;
        private UDP udp = MainWindow.udp;
        private static IPAddress  remoteIP;
        public static Point Location;
        public static int verticalOffset;
        public  List<KeyStatus> waiting;
        public MainWindow mainWindow;
        public struct KeyStatus
        {
          public  Keys code;
          public Byte Flag;
        }
        public Remote(Window parent,IPAddress ip)
        {
            this.Background = MainWindow.brushColor;
            InitializeComponent();
            Dictionary<IPAddress,VideoPreview> vplist=new Dictionary<IPAddress,VideoPreview>();
            waiting = new List<KeyStatus>();
            remoteIP = ip;
            this.Left = Location.X;
            this.Top = Location.Y;
            this.LocationChanged += Remote_LocationChanged;
            rtpClient = new RTPClient(new IPEndPoint(ip, (int)MainWindow.Ports.RTP), Screen, MainWindow.hostIP.ToString(), ip.ToString());
            rtpClient.window = this;
            sc = new ScreenCapture();
            timer = new System.Windows.Forms.Timer();
            mousekeyTimer = new System.Windows.Forms.Timer();
            timer.Interval = 100;
            timer.Tick += timer_Tick;
            mousekeyTimer.Interval = 10;
            mousekeyTimer.Tick += mousekeyTimer_Tick;
            mainWindow = parent as MainWindow;
        }

        void Remote_LocationChanged(object sender, EventArgs e)
        {
            User32.RECT windowRect=new User32.RECT();
            IntPtr handle=System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;
            User32.GetWindowRect(handle,ref windowRect);
             
            Location.X = windowRect.left;
            Location.Y =windowRect.top;
            //Point locationFromScreen = Screen.PointFromScreen(new Point(0, 0));
            //System.Diagnostics.Debug.WriteLine(locationFromScreen.X+" "+locationFromScreen.Y);
        }

        void mousekeyTimer_Tick(object sender, EventArgs e)
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
            while(waiting.Count>0)
            {   KeyStatus ks= waiting.First();
                 User32.keybd_event((Byte)ks.code, (Byte)0x45, ks.Flag, 0);
                 waiting.Remove(waiting.First());
            }
        }

        void timer_Tick(object sender, EventArgs e)
        {
            rtpClient.rtpSender.Send(sc.GetDesktopBitmapBytes());

            
        }
        public void Start()
        {
            udp.SendMessageTo(UDP.Remote, remoteIP);
            HookMouse.Start();
            HookKeyboard.Start();
        }
        public void StartSending()
        {
            timer.Start();
            mousekeyTimer.Start();
            HookMouse.Stop();
            HookKeyboard.Stop();
        }
        public void StopSending()
        {
            timer.Stop();
            if (mousekeyTimer != null)
                mousekeyTimer.Stop();
            rtpClient.Dispose();

        }
        public class User32
        {
            [DllImport("user32.dll", SetLastError = true)]
            public static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);
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
            [DllImport("user32.dll")]
            public static extern IntPtr GetWindowRect(IntPtr hWnd, ref RECT rect);
            [StructLayout(LayoutKind.Sequential)]
            public struct RECT
            {
                public int left;
                public int top;
                public int right;
                public int bottom;
            }
        }

        private void Screen_MouseMove(object sender, MouseEventArgs e)
        {
           UpdateMouseData();
        }
        private void UpdateMouseData()
        {
            User32.GetCursorPos(out mousePos);
         // udp.SendMessageTo(UDP.Mouse + MouseFlag + UDP.Breaker + mousePos.X + UDP.Breaker + mousePos.Y, remoteIP);

        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            mainWindow.IsEnabled = true;
            
            if (mousekeyTimer != null)
                mousekeyTimer.Stop();
            if (timer != null)
                timer.Stop();
            if (rtpClient != null)
                rtpClient.Dispose();
            HookKeyboard.Stop();
            HookMouse.Stop();
        }

    class HookMouse{
     
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
          //  System.Diagnostics.Debug.WriteLine(Convert.ToString(wParam.ToInt32(),16) + " " + mousePos.X + " " + mousePos.Y);
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
    class HookKeyboard
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private static LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;

        public static void Start()
        {
            _hookID = SetHook(_proc);
           
        }
        public static void Stop()
        {
            UnhookWindowsHookEx(_hookID);
        }

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private delegate IntPtr LowLevelKeyboardProc(
            int nCode, IntPtr wParam, IntPtr lParam);

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                MainWindow.udp.SendMessageTo(UDP.Keyboard + vkCode + UDP.Breaker + (Byte)wParam,remoteIP);
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
            LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
            IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
     
    }
           public enum Keys : int
        {
            VK_LBUTTON = 0x01,   //Left mouse button 
            VK_RBUTTON = 0x02,   //Right mouse button 
            VK_CANCEL = 0x03,   //Control-break processing 
            VK_MBUTTON = 0x04,   //Middle mouse button (three-button mouse) 
            VK_BACK = 0x08,   //BACKSPACE key 
            VK_TAB = 0x09,   //TAB key 
            VK_CLEAR = 0x0C,   //CLEAR key 
            VK_RETURN = 0x0D,   //ENTER key 
            VK_SHIFT = 0x10,   //SHIFT key 
            VK_CONTROL = 0x11,   //CTRL key 
            VK_MENU = 0x12,   //ALT key 
            VK_PAUSE = 0x13,   //PAUSE key 
            VK_CAPITAL = 0x14,   //CAPS LOCK key 
            VK_ESCAPE = 0x1B,   //ESC key 
            VK_SPACE = 0x20,   //SPACEBAR 
            VK_PRIOR = 0x21,   //PAGE UP key 
            VK_NEXT = 0x22,   //PAGE DOWN key 
            VK_END = 0x23,   //END key 
            VK_HOME = 0x24,   //HOME key 
            VK_LEFT = 0x25,   //LEFT ARROW key 
            VK_UP = 0x26,   //UP ARROW key 
            VK_RIGHT = 0x27,   //RIGHT ARROW key 
            VK_DOWN = 0x28,   //DOWN ARROW key 
            VK_SELECT = 0x29,   //SELECT key 
            VK_PRINT = 0x2A,   //PRINT key
            VK_EXECUTE = 0x2B,   //EXECUTE key 
            VK_SNAPSHOT = 0x2C,   //PRINT SCREEN key 
            VK_INSERT = 0x2D,   //INS key 
            VK_DELETE = 0x2E,   //DEL key 
            VK_HELP = 0x2F,   //HELP key
            VK_0 = 0x30,   //0 key 
            VK_1 = 0x31,   //1 key 
            VK_2 = 0x32,   //2 key 
            VK_3 = 0x33,   //3 key 
            VK_4 = 0x34,   //4 key 
            VK_5 = 0x35,   //5 key 
            VK_6 = 0x36,    //6 key 
            VK_7 = 0x37,    //7 key 
            VK_8 = 0x38,   //8 key 
            VK_9 = 0x39,    //9 key 
            VK_A = 0x41,   //A key 
            VK_B = 0x42,   //B key 
            VK_C = 0x43,   //C key 
            VK_D = 0x44,   //D key 
            VK_E = 0x45,   //E key 
            VK_F = 0x46,   //F key 
            VK_G = 0x47,   //G key 
            VK_H = 0x48,   //H key 
            VK_I = 0x49,    //I key 
            VK_J = 0x4A,   //J key 
            VK_K = 0x4B,   //K key 
            VK_L = 0x4C,   //L key 
            VK_M = 0x4D,   //M key 
            VK_N = 0x4E,    //N key 
            VK_O = 0x4F,   //O key 
            VK_P = 0x50,    //P key 
            VK_Q = 0x51,   //Q key 
            VK_R = 0x52,   //R key 
            VK_S = 0x53,   //S key 
            VK_T = 0x54,   //T key 
            VK_U = 0x55,   //U key 
            VK_V = 0x56,   //V key 
            VK_W = 0x57,   //W key 
            VK_X = 0x58,   //X key 
            VK_Y = 0x59,   //Y key 
            VK_Z = 0x5A,    //Z key
            VK_NUMPAD0 = 0x60,   //Numeric keypad 0 key 
            VK_NUMPAD1 = 0x61,   //Numeric keypad 1 key 
            VK_NUMPAD2 = 0x62,   //Numeric keypad 2 key 
            VK_NUMPAD3 = 0x63,   //Numeric keypad 3 key 
            VK_NUMPAD4 = 0x64,   //Numeric keypad 4 key 
            VK_NUMPAD5 = 0x65,   //Numeric keypad 5 key 
            VK_NUMPAD6 = 0x66,   //Numeric keypad 6 key 
            VK_NUMPAD7 = 0x67,   //Numeric keypad 7 key 
            VK_NUMPAD8 = 0x68,   //Numeric keypad 8 key 
            VK_NUMPAD9 = 0x69,   //Numeric keypad 9 key 
            VK_SEPARATOR = 0x6C,   //Separator key 
            VK_SUBTRACT = 0x6D,   //Subtract key 
            VK_DECIMAL = 0x6E,   //Decimal key 
            VK_DIVIDE = 0x6F,   //Divide key
            VK_F1 = 0x70,   //F1 key 
            VK_F2 = 0x71,   //F2 key 
            VK_F3 = 0x72,   //F3 key 
            VK_F4 = 0x73,   //F4 key 
            VK_F5 = 0x74,   //F5 key 
            VK_F6 = 0x75,   //F6 key 
            VK_F7 = 0x76,   //F7 key 
            VK_F8 = 0x77,   //F8 key 
            VK_F9 = 0x78,   //F9 key 
            VK_F10 = 0x79,   //F10 key 
            VK_F11 = 0x7A,   //F11 key 
            VK_F12 = 0x7B,   //F12 key
            VK_SCROLL = 0x91,   //SCROLL LOCK key 
            VK_LSHIFT = 0xA0,   //Left SHIFT key
            VK_RSHIFT = 0xA1,   //Right SHIFT key
            VK_LCONTROL = 0xA2,   //Left CONTROL key
            VK_RCONTROL = 0xA3,    //Right CONTROL key
            VK_LMENU = 0xA4,      //Left MENU key
            VK_RMENU = 0xA5,   //Right MENU key
            VK_PLAY = 0xFA,   //Play key
            VK_ZOOM = 0xFB, //Zoom key 
        }
    private void Scroller_ScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        
        verticalOffset =(int) e.VerticalOffset;
    }



    }
}
