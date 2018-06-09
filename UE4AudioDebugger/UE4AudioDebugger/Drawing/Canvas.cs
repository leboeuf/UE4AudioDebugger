using System;
using System.Drawing;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace UE4AudioDebugger.Drawing
{
    public class Canvas : System.Windows.Controls.Canvas
    {
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        private readonly Pen _pen = new Pen(Color.Blue, 2);

        protected override void OnRender(System.Windows.Media.DrawingContext dc)
        {
            if (ActualWidth <= 0 || ActualHeight <= 0)
            {
                return;
            }

            using (var bitmap = new Bitmap((int)ActualWidth, (int)ActualHeight))
            {
                using (var g = Graphics.FromImage(bitmap))
                {
                    g.DrawEllipse(_pen, 10, 10, 100, 100);
                }

                // Copy GDI bitmap to WPF bitmap.
                var hBitmap = bitmap.GetHbitmap();
                var image = Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                image.Freeze();
                dc.DrawImage(image, new Rect(RenderSize));

                DeleteObject(hBitmap);
            }
        }
    }
}
