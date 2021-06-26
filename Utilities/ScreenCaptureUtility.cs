using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;

namespace UmaFanCountChecker
{
    public static class ScreenCaptureUtility
    {

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr GetDesktopWindow();

        [StructLayout(LayoutKind.Sequential)]
        private struct Rect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowRect(IntPtr hWnd, ref Rect rect);

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

        public static Image CaptureDesktop()
        {
            return CaptureWindow(GetDesktopWindow());
        }

        public static Bitmap CaptureActiveWindow()
        {
            return CaptureWindow(GetForegroundWindow());
        }

        public static BitmapSource ConvertBitmapToBitmapSource(Bitmap bitmap)
        {
            IntPtr hbitmap = bitmap.GetHbitmap();
            var bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                hbitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            DeleteObject(hbitmap);
            return bitmapSource;
        }

        public static Bitmap CaptureWindow(IntPtr handle,
            float xStart=0,
            float xEnd=1,
            float yStart = 0,
            float yEnd = 1)
        {
            var rect = new Rect();
            GetWindowRect(handle, ref rect);
            {
                int left = rect.Left;
                int right = rect.Right;
                int w = right - left;
                int leftOffset = (int)(xStart * w);
                int rightOffset = (int)(xEnd * w);
                rect.Left = left + leftOffset;
                rect.Right = left + rightOffset;
            }

            if ((rect.Right - rect.Left) <= 0)
            {
                return null;
            }

            {
                int top = rect.Top;
                int bottom = rect.Bottom;
                int h = bottom - top;
                int topOffset = (int)(yStart * h);
                int bottomOffset = (int)(yEnd * h);
                rect.Top = top + topOffset;
                rect.Bottom = top + bottomOffset;
            }
            if ((rect.Bottom - rect.Top) <= 0)
            {
                return null;
            }


            var bounds = new Rectangle(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);
            var result = new Bitmap(bounds.Width, bounds.Height);

            using (var graphics = Graphics.FromImage(result))
            {
                graphics.CopyFromScreen(
                    new System.Drawing.Point(bounds.Left, bounds.Top),
                    System.Drawing.Point.Empty, bounds.Size);
            }

            return result;
        }
    }
}
