using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.IO;


public class ScreenCapture : System.MarshalByRefObject
{
    [DllImport("user32.dll")]
    private static extern IntPtr GetDesktopWindow();

    [DllImport("gdi32.dll")]
    private static extern bool BitBlt(
        IntPtr hdcDest,
        int nXDest, 
        int nYDest, 
        int nWidth, 
        int nHeight, 
        IntPtr hdcSrc,
        int nXSrc, 
        int nYSrc,
        System.Int32 dwRop 
        );
    private const Int32 SRCCOPY = 0xCC0020;
    [DllImport("user32.dll")]
    private static extern int GetSystemMetrics(int nIndex);

    private const int SM_CXSCREEN = 0;
    private const int SM_CYSCREEN = 1;
    public const int WM_CLICK = 0x00F5;

    [DllImport("user32.dll", SetLastError = true)]
    internal static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
    [DllImport("user32.dll", EntryPoint = "GetDC")]
    internal extern static IntPtr GetDC(IntPtr hWnd);
    [DllImport("gdi32.dll", EntryPoint = "CreateCompatibleDC")]
    internal extern static IntPtr CreateCompatibleDC(IntPtr hdc);
    [DllImport("gdi32.dll", EntryPoint = "CreateCompatibleBitmap")]
    internal extern static IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);
    [DllImport("gdi32.dll", EntryPoint = "DeleteDC")]
    internal extern static IntPtr DeleteDC(IntPtr hDc);
    [DllImport("user32.dll", EntryPoint = "ReleaseDC")]
    internal extern static IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDc);

    [DllImport("gdi32.dll", EntryPoint = "SelectObject")]
    internal extern static IntPtr SelectObject(IntPtr hdc, IntPtr bmp);
    [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
    internal extern static IntPtr DeleteObject(IntPtr hDc);
    [DllImport("user32.dll")]
    public static extern int SendMessage(int hWnd, uint Msg, long wParam, long lParam);
    [DllImport("user32.dll")]
    public static extern IntPtr GetWindowDC(IntPtr hWnd);
    [DllImport("user32.dll")]
    public static extern IntPtr GetWindowRect(IntPtr hWnd, ref RECT rect);



    public Size GetDesktopBitmapSize()
    {
        return new Size(GetSystemMetrics(SM_CXSCREEN), GetSystemMetrics(SM_CYSCREEN));
    }
    public byte[] GetDesktopBitmapBytes()
    {
        Size DesktopBitmapSize = GetDesktopBitmapSize();
        Graphics Graphic = Graphics.FromHwnd(GetDesktopWindow());
        Bitmap MemImage = new Bitmap(DesktopBitmapSize.Width, DesktopBitmapSize.Height, Graphic);

        Graphics MemGraphic = Graphics.FromImage(MemImage);
        IntPtr dc1 = Graphic.GetHdc();
        IntPtr dc2 = MemGraphic.GetHdc();
        BitBlt(dc2, 0, 0, DesktopBitmapSize.Width, DesktopBitmapSize.Height, dc1, 0, 0, SRCCOPY);
        Graphic.ReleaseHdc(dc1);
        MemGraphic.ReleaseHdc(dc2);
        Graphic.Dispose();
        MemGraphic.Dispose();

        Graphics g = System.Drawing.Graphics.FromImage(MemImage);
        System.Windows.Forms.Cursor cur = System.Windows.Forms.Cursors.Arrow;
        cur.Draw(g, new Rectangle(System.Windows.Forms.Cursor.Position.X - 10, System.Windows.Forms.Cursor.Position.Y - 10, cur.Size.Width, cur.Size.Height));

        MemoryStream ms = new MemoryStream();
        MemImage.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);


        return ms.GetBuffer();
    }
    public byte[] GetWindowBitmapBytes(String title)
    {
        IntPtr hWnd = FindWindow(null, title);
        MemoryStream ms = new MemoryStream();
        /*  Bitmap bmp = null;
     IntPtr hdcFrom = GetDC(hWnd);
     IntPtr hdcTo = CreateCompatibleDC(hdcFrom);
     int Width = 529;
     int Height = 436;
     IntPtr hBitmap = CreateCompatibleBitmap(hdcFrom, Width, Height);
     if (hBitmap != IntPtr.Zero)
     {
       
         IntPtr hLocalBitmap = SelectObject(hdcTo, hBitmap);
         BitBlt(hdcTo, 0, 0, Width, Height,hdcFrom, 0, 0, SRCCOPY);
         SelectObject(hdcTo, hLocalBitmap);
         DeleteDC(hdcTo);
         ReleaseDC(hWnd, hdcFrom);
         bmp = System.Drawing.Image.FromHbitmap(hBitmap);
         DeleteObject(hBitmap);
         bmp.Save(ms, ImageFormat.Jpeg);
         */
        IntPtr hdcSrc = GetWindowDC(hWnd);
        RECT windowRect = new RECT();
        GetWindowRect(hWnd, ref windowRect);
        int width = windowRect.right - windowRect.left;
        int height = windowRect.bottom - windowRect.top;

        IntPtr hdcDest = CreateCompatibleDC(hdcSrc);

        IntPtr hBitmap = CreateCompatibleBitmap(hdcSrc, width, height);

        IntPtr hOld = SelectObject(hdcDest, hBitmap);
        // bitblt over
        BitBlt(hdcDest, 0, 0, width, height, hdcSrc, 0, 0, SRCCOPY);

        SelectObject(hdcDest, hOld);

        DeleteDC(hdcDest);
        ReleaseDC(hWnd, hdcSrc);

        Image img = Image.FromHbitmap(hBitmap);
        img.Save(ms, ImageFormat.Jpeg);
        DeleteObject(hBitmap);
        //  }


        return ms.GetBuffer();
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }

    public Image Get_Resized_Image(int w, int h, byte[] image)
    {
        MemoryStream ms = new MemoryStream(image);

        Image bt = Image.FromStream(ms);
        try
        {
            Size sizing = new Size(w, h);
            bt = new System.Drawing.Bitmap(bt, sizing);

        }
        catch (Exception) { }
        return bt;

    }

    public float difference(Image OrginalImage, Image SecoundImage)
    {
        float percent = 0;
        try
        {
            float counter = 0;

            Bitmap bt1 = new Bitmap(OrginalImage);
            Bitmap bt2 = new Bitmap(SecoundImage);
            int size_H = bt1.Size.Height;
            int size_W = bt1.Size.Width;

            float total = size_H * size_W;

            Color pixel_image1;
            Color pixel_image2;

            for (int x = 0; x != size_W; x++)
            {

                for (int y = 0; y != size_H; y++)
                {
                    pixel_image1 = bt1.GetPixel(x, y);
                    pixel_image2 = bt2.GetPixel(x, y);

                    if (pixel_image1 != pixel_image2)
                    {
                        counter++;
                    }

                }

            }
            percent = (counter / total) * 100;

        }
        catch (Exception) { percent = 0; }

        return percent;
    }

}
