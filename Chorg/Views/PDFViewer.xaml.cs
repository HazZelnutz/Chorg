using PdfiumViewer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Chorg.Views
{
    /// <summary>
    /// Interaction logic for PDFViewer.xaml
    /// </summary>
    public partial class PDFViewer : UserControl
    {
        PdfViewer viewer;

        public MemoryStream PDFStream
        {
            get => (MemoryStream)GetValue(PDFStreamProperty); 
            set { SetValue(PDFStreamProperty, value); }
        }

        static DependencyProperty PDFStreamProperty =
            DependencyProperty.Register("PDFStream", typeof(MemoryStream), typeof(PDFViewer), new PropertyMetadata(default(MemoryStream), StreamChanged));

        public PDFViewer()
        {
            InitializeComponent();

            viewer = new PdfViewer();
            viewer.ShowToolbar = false;
            viewer.ShowBookmarks = false;
            Wrapper.Child = viewer;
        }

        static void StreamChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((PDFViewer)d).RenderPDF();
        }

        private void RenderPDF()
        {
            viewer.Document?.Dispose();
            viewer.Document = PdfDocument.Load(PDFStream);     
        }

        private void MagMinus_Click(object sender, RoutedEventArgs e)
        {
            if(viewer.Document != null)
                viewer.Renderer.ZoomOut();
        }
        
        private void MagPlus_Click(object sender, RoutedEventArgs e)
        {
            if (viewer.Document != null)
                viewer.Renderer.ZoomIn();
        }

        private void RotateLeft_Click(object sender, RoutedEventArgs e)
        {
            if (viewer.Document != null)
                viewer.Renderer.RotateLeft();
        }

        private void RotateRight_Click(object sender, RoutedEventArgs e)
        {
            if (viewer.Document != null)
                viewer.Renderer.RotateRight();
        }

        private void Fit_Click(object sender, RoutedEventArgs e)
        {
            if (viewer.Document != null)
                viewer.Renderer.Zoom = 1;
        }
    }
}
