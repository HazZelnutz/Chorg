using System;
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

namespace Chorg.ViewModels
{
    public class ChartThumbnailViewModel : Screen
    {
        public BitmapImage Thumbnail { get; }

        private Chart model;

        public ChartThumbnailViewModel(Chart chart)
        {
            model = chart;
            Thumbnail = Render();
            Thumbnail.Freeze();
        }

        public Chart GetModel() => model;

        private BitmapImage Render()
        {
            PdfDocument document = PdfDocument.Load(model.GetStream());
            var image = document.Render(0, 100, 100, false);
            document.Dispose();

            MemoryStream stream = new MemoryStream();
            image.Save(stream, ImageFormat.Bmp);

            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = stream;
            bitmapImage.EndInit();
            return bitmapImage;
        }
    }
}
