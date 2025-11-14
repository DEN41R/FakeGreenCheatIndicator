using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace FakeCheatIndicator
{
    public class IndicatorWindow : Window
    {
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

        [DllImport("user32.dll")]
        private static extern uint GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, 
            int X, int Y, int cx, int cy, uint uFlags);

        private const int GWL_EXSTYLE = -20;
        private const uint WS_EX_TRANSPARENT = 0x00000020;
        private const uint WS_EX_TOPMOST = 0x00000008;
        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOACTIVATE = 0x0010;

        private readonly Ellipse indicator;
        private readonly DispatcherTimer updateTimer;
        private bool isDragging = false;
        private Point dragStartPoint;

        public IndicatorWindow()
        {
            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            Background = Brushes.Transparent;
            ShowInTaskbar = false;
            Topmost = true;
            ResizeMode = ResizeMode.NoResize;

            indicator = new Ellipse
            {
                Stroke = Brushes.Black,
                Fill = Brushes.Green
            };

            var canvas = new System.Windows.Controls.Canvas();
            canvas.Children.Add(indicator);
            Content = canvas;

            MouseLeftButtonDown += OnMouseLeftButtonDown;
            MouseLeftButtonUp += OnMouseLeftButtonUp;
            MouseMove += OnMouseMove;

            updateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(16)
            };
            updateTimer.Tick += (s, e) => EnsureTopMost();
            updateTimer.Start();

            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            
            
            uint exStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
            SetWindowLong(hwnd, GWL_EXSTYLE, exStyle | WS_EX_TRANSPARENT | WS_EX_TOPMOST);
            
            EnsureTopMost();
        }

        private void EnsureTopMost()
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            if (hwnd != IntPtr.Zero)
            {
                SetWindowPos(hwnd, HWND_TOPMOST, 0, 0, 0, 0, 
                    SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
            }
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isDragging = true;
            dragStartPoint = e.GetPosition(this);
            
            
            var hwnd = new WindowInteropHelper(this).Handle;
            uint exStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
            SetWindowLong(hwnd, GWL_EXSTYLE, exStyle & ~WS_EX_TRANSPARENT);
            
            CaptureMouse();
        }

        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (isDragging)
            {
                isDragging = false;
                ReleaseMouseCapture();
                
                
                var hwnd = new WindowInteropHelper(this).Handle;
                uint exStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
                SetWindowLong(hwnd, GWL_EXSTYLE, exStyle | WS_EX_TRANSPARENT);

                
                var settings = Settings.Instance;
                if (settings.SavePosition)
                {
                    settings.PositionX = (int)Left;
                    settings.PositionY = (int)Top;
                    settings.Save();
                }
            }
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                var currentPosition = e.GetPosition(this);
                var offset = currentPosition - dragStartPoint;
                
                Left += offset.X;
                Top += offset.Y;

                
                var screen = SystemParameters.WorkArea;
                Left = Math.Max(0, Math.Min(screen.Width - Width, Left));
                Top = Math.Max(0, Math.Min(screen.Height - Height, Top));
            }
        }

        public void UpdateFromSettings()
        {
            var settings = Settings.Instance;
            
            
            int totalSize = settings.Size + settings.BorderThickness * 2 + 4;
            Width = totalSize;
            Height = totalSize;
            
            indicator.Width = settings.Size;
            indicator.Height = settings.Size;
            indicator.StrokeThickness = settings.BorderThickness;
            
            System.Windows.Controls.Canvas.SetLeft(indicator, settings.BorderThickness + 2);
            System.Windows.Controls.Canvas.SetTop(indicator, settings.BorderThickness + 2);

           
            var color = settings.GetIndicatorColor();
            byte alpha = (byte)(settings.Opacity * 255 / 100);
            indicator.Fill = new SolidColorBrush(Color.FromArgb(alpha, color.R, color.G, color.B));

        
            UpdatePosition();
        }

        public void UpdatePosition()
        {
            var settings = Settings.Instance;
            var screen = SystemParameters.WorkArea;
            
            Left = Math.Max(0, Math.Min(screen.Width - Width, settings.PositionX));
            Top = Math.Max(0, Math.Min(screen.Height - Height, settings.PositionY));
        }

        protected override void OnClosed(EventArgs e)
        {
            updateTimer.Stop();
            base.OnClosed(e);
        }
    }
}
