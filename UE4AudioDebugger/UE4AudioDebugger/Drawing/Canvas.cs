using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using UE4AudioDebugger.Models;

namespace UE4AudioDebugger.Drawing
{
    public class Canvas : System.Windows.Controls.Canvas
    {
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

        private readonly Pen _pen = new Pen(Color.Blue, 1);
        private decimal _minX;
        private decimal _maxX;
        private decimal _minY;
        private decimal _maxY;

        public List<UActor> Actors { get; set; }
        public Pen ActorPen { get; set; }

        public bool IsGridVisible { get; set; } = true;
        public Pen GridPen { get; set; }

        public Canvas()
        {
            ActorPen = new Pen(Color.Blue, 1);

            GridPen = new Pen(Color.DimGray, 1);
            GridPen.DashPattern = new[] { 5f, 3f };
        }

        protected override void OnRender(System.Windows.Media.DrawingContext dc)
        {
            if (ActualWidth <= 0 || ActualHeight <= 0 || Actors == null || Actors.Count() < 1)
            {
                return;
            }

            using (var bitmap = new Bitmap((int)ActualWidth, (int)ActualHeight))
            {
                using (var g = Graphics.FromImage(bitmap))
                {
                    PrepareFrame(g);

                    if (IsGridVisible)
                    {
                        DrawGrid(g);
                        DrawPoints(g);
                    }
                }

                // Copy GDI bitmap to WPF bitmap
                var hBitmap = bitmap.GetHbitmap();
                var image = Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                image.Freeze();
                dc.DrawImage(image, new Rect(RenderSize));

                DeleteObject(hBitmap);
            }
        }

        private void PrepareFrame(Graphics g)
        {
            g.SmoothingMode = SmoothingMode.HighSpeed;

            foreach (var actor in Actors)
            {
                if (actor.Location.X < _minX)
                    _minX = actor.Location.X;
                else if (actor.Location.X > _maxX)
                    _maxX = actor.Location.X;

                if (actor.Location.Y < _minY)
                    _minY = actor.Location.Y;
                else if (actor.Location.Y > _maxY)
                    _maxY = actor.Location.Y;
            }

            // Make symmetrical
            if (Math.Abs(_minX) > _maxX)
                _maxX = -_minX;
            else _minX = -_maxX;

            if (Math.Abs(_minY) > _maxY)
                _maxY = -_minY;
            else _minY = -_maxY;

            var halfWidth = (int)(ActualWidth / 2);
            var halfHeight = (int)(ActualHeight / 2);
        }

        private void DrawPoints(Graphics g)
        {
            // Flip Y axis
            var m = new Matrix(1, 0, 0, -1, 0, 0);

            // Bring center to half/half of canvas
            var halfWidth = (float)ActualWidth / 2;
            var halfHeight = (float)ActualHeight / 2;
            m.Translate(halfWidth, halfHeight, MatrixOrder.Append);

            g.Transform = m;
            g.ScaleTransform(0.05f, 0.05f);

            // Draw points
            foreach (var actor in Actors)
            {
                //g.FillEllipse(Brushes.Blue, (int)actor.Location.X, (int)actor.Location.Y, 50, 50);
                g.DrawRectangle(ActorPen, (int)actor.Location.X, (int)actor.Location.Y, (int)actor.Size.X, (int)actor.Size.Y);
            }
        }

        private void DrawGrid(Graphics g)
        {
            var halfWidth = (int)(ActualWidth / 2);
            var halfHeight = (int)(ActualHeight / 2);

            var m = new Matrix();
            m.Translate(halfWidth, halfHeight);
            g.Transform = m;

            g.DrawLine(GridPen, -halfWidth, 0, halfWidth, 0);
            g.DrawLine(GridPen, 0, -halfHeight, 0, halfHeight);
        }
    }
}
