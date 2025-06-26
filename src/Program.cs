using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;

class Program
{
    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

    [DllImport("user32.dll")]
    static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll")]
    static extern bool UpdateLayeredWindow(IntPtr hwnd, IntPtr hdcDst,
        ref Point pptDst, ref Size psize, IntPtr hdcSrc, ref Point pptSrc,
        int crKey, ref BLENDFUNCTION pblend, uint dwFlags);

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct BLENDFUNCTION
    {
        public byte BlendOp;
        public byte BlendFlags;
        public byte SourceConstantAlpha;
        public byte AlphaFormat;
    }

    private const int GWL_EXSTYLE = -20;
    private const int GWL_STYLE = -16;
    private const uint WS_EX_LAYERED = 0x80000;
    private const uint WS_EX_TRANSPARENT = 0x20;
    private const uint WS_EX_TOOLWINDOW = 0x80;
    private const uint WS_EX_TOPMOST = 0x8;
    private const uint WS_VISIBLE = 0x10000000;
    private const uint WS_POPUP = 0x80000000;
    private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
    private const uint SWP_NOSIZE = 0x1;
    private const uint SWP_NOMOVE = 0x2;
    private const uint SWP_NOACTIVATE = 0x10;
    private const uint SWP_SHOWWINDOW = 0x40;
    private const uint ULW_ALPHA = 0x00000002;

    static volatile bool shouldExit = false;

    class TransparentForm : Form
    {
        private readonly Bitmap buffer;
        private readonly System.Windows.Forms.Timer updateTimer;
        private readonly Graphics bufferGraphics;

        public TransparentForm()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer | 
                    ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.SupportsTransparentBackColor, true);
            
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            TopMost = true;
            Size = new Size(50, 50);
            StartPosition = FormStartPosition.Manual;
            Location = new Point(25, 15);

            buffer = new Bitmap(50, 50, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            bufferGraphics = Graphics.FromImage(buffer);
            
            const int CIRCLE_SIZE = 21;
            bufferGraphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using (var brush = new SolidBrush(Color.FromArgb(255, 0, 255, 0)))
            using (var pen = new Pen(Color.Black, 3))
            {
                bufferGraphics.FillEllipse(brush, 3, 3, CIRCLE_SIZE, CIRCLE_SIZE);
                bufferGraphics.DrawEllipse(pen, 3, 3, CIRCLE_SIZE, CIRCLE_SIZE);
            }

            updateTimer = new System.Windows.Forms.Timer { Interval = 16 };
            updateTimer.Tick += (s, e) =>
            {
                if (shouldExit)
                {
                    Close();
                    return;
                }
                UpdateWindow();
                EnsureTopMost();
            };
        }

        private void EnsureTopMost()
        {
            SetWindowPos(Handle, HWND_TOPMOST, 0, 0, 0, 0, 
                (uint)(SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE | SWP_SHOWWINDOW));
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

    
            uint style = (uint)(WS_POPUP | WS_VISIBLE);
            SetWindowLong(Handle, GWL_STYLE, style);

        
            uint exStyle = (uint)(WS_EX_LAYERED | WS_EX_TRANSPARENT | WS_EX_TOPMOST | WS_EX_TOOLWINDOW);
            SetWindowLong(Handle, GWL_EXSTYLE, exStyle);

            EnsureTopMost();
            UpdateWindow();
            updateTimer.Start();
        }

        private void UpdateWindow()
        {
            IntPtr screenDc = IntPtr.Zero;
            IntPtr memDc = IntPtr.Zero;
            IntPtr hBitmap = IntPtr.Zero;
            IntPtr oldBitmap = IntPtr.Zero;

            try
            {
                screenDc = User32.GetDC(IntPtr.Zero);
                memDc = Gdi32.CreateCompatibleDC(screenDc);
                hBitmap = buffer.GetHbitmap(Color.FromArgb(0));
                oldBitmap = Gdi32.SelectObject(memDc, hBitmap);

                var size = new Size(buffer.Width, buffer.Height);
                var pointSource = new Point(0, 0);
                var topPos = new Point(Left, Top);
                var blend = new BLENDFUNCTION
                {
                    BlendOp = 0,
                    BlendFlags = 0,
                    SourceConstantAlpha = 255,
                    AlphaFormat = 1
                };

                UpdateLayeredWindow(Handle, screenDc, ref topPos, ref size, memDc, ref pointSource, 0, ref blend, ULW_ALPHA);
            }
            finally
            {
                if (oldBitmap != IntPtr.Zero)
                    Gdi32.SelectObject(memDc, oldBitmap);
                if (hBitmap != IntPtr.Zero)
                    Gdi32.DeleteObject(hBitmap);
                if (memDc != IntPtr.Zero)
                    Gdi32.DeleteDC(memDc);
                if (screenDc != IntPtr.Zero)
                    User32.ReleaseDC(IntPtr.Zero, screenDc);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                updateTimer.Dispose();
                bufferGraphics.Dispose();
                buffer.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    static class User32
    {
        [DllImport("user32.dll")]
        public static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);
    }

    static class Gdi32
    {
        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        public static extern IntPtr CreateCompatibleDC(IntPtr hDC);

        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        public static extern bool DeleteDC(IntPtr hdc);

        [DllImport("gdi32.dll", ExactSpelling = true)]
        public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);

        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        public static extern bool DeleteObject(IntPtr hObject);
    }

    [STAThread]
    static void Main()
    {
        Console.Title = "FakeCheatIndicator";
        Console.WriteLine("Индикатор активирован!");
        Console.WriteLine("Нажмите Enter для выхода из программы");

        Thread inputThread = new Thread(() =>
        {
            Console.ReadLine();
            shouldExit = true;
        });
        inputThread.IsBackground = true;
        inputThread.Start();

        Application.EnableVisualStyles();
        Application.Run(new TransparentForm());

        if (inputThread.IsAlive)
            inputThread.Join();
    }
} 