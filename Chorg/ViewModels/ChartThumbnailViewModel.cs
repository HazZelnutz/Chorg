﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chorg.Models;
using Caliburn.Micro;
using System.Windows.Media.Imaging;
using PdfiumViewer;
using System.IO;
using System.Drawing.Imaging;
using System.Drawing;
using System.Windows;
using System.Runtime.InteropServices;

namespace Chorg.ViewModels
{
    public class ChartThumbnailViewModel : ChartViewModel
    {
        public BitmapSource Thumbnail { get; }

        public ChartThumbnailViewModel(Chart chart) : base(chart)
        {
            Thumbnail = Render();
        }

        private BitmapSource Render()
        {
            PdfDocument document = PdfDocument.Load(model.GetStream());
            var image = document.Render(0, 100, 100, false);
            document.Dispose();

            var bitmap = new Bitmap(image);
            IntPtr bmpPt = bitmap.GetHbitmap();
            var bmpSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                bmpPt,
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions()
            );

            bmpSource.Freeze();
            DeleteObject(bmpPt);
            return bmpSource;
        }

        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DeleteObject(IntPtr value);
    }
}
