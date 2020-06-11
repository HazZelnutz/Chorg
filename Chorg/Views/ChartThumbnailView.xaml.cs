using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MaterialDesignThemes.Wpf;

namespace Chorg.Views
{
    /// <summary>
    /// Interaction logic for ChartPreviewView.xaml
    /// </summary>
    public partial class ChartThumbnailView : UserControl
    {
        public ChartThumbnailView()
        {
            InitializeComponent();
        }

        public bool Magnifiable 
        {
            get => (bool)GetValue(MagnifiableProperty);
            set => SetValue(MagnifiableProperty, value);
        }

        static readonly DependencyProperty MagnifiableProperty =
            DependencyProperty.Register("Magnifiable", typeof(bool), typeof(ChartThumbnailView), new PropertyMetadata(true, MagnifiableChangedCallback));

        private static void MagnifiableChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as ChartThumbnailView).MagnifiableChanged();
        }

        private void MagnifiableChanged()
        {
            if (Magnifiable)
            {
                Binding binding = new Binding("MainControl.IsMouseOver");
                binding.Converter = new BooleanToVisibilityConverter();
                Magnifier.SetBinding(VisibilityProperty, binding);
            }
            else
                Magnifier.Visibility = Visibility.Collapsed;
        }
    }
}
