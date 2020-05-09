using Chorg.Models;
using System;
using System.IO;
using System.Linq;
using System.Windows.Media;
using Caliburn.Micro;
using System.ComponentModel;

namespace Chorg.ViewModels
{
    public class ChartViewModel : Screen, ITextSearch
    {
        private Chart chartModel;

        public EditChartViewModel Edit { get => new EditChartViewModel(chartModel, this); } 

        public string Identifier
        {
            get => chartModel.Identifier ?? "No Identifier";
        }

        public string Description
        {
            get => chartModel.Description ?? "No Description";
        }

        public MemoryStream PDFStream
        {
            get => chartModel.GetStream();
        }

        public ContentType Content
        {
            get => chartModel.Content;
        }

        public string ContentString
        {
            get 
            {
                switch (chartModel.Content)
                {
                    case ContentType.GENERAL:
                        return "GEN";
                    case ContentType.TAXI:
                        return "TAXI";
                    case ContentType.SID:
                        return "SID";
                    case ContentType.STAR:
                        return "STAR";
                    case ContentType.APP:
                        return "APP";
                    case ContentType.UNDEFINED:
                        return "N.D.";
                    default:
                        throw new Exception();
                }
            }
        }

        public SolidColorBrush Accent 
        {
            get {
                switch (chartModel.Content)
                {
                    case ContentType.GENERAL:
                        return (SolidColorBrush)App.Current.FindResource("Color_GEN");
                    case ContentType.TAXI:
                        return (SolidColorBrush)App.Current.FindResource("Color_TAXI");
                    case ContentType.SID:
                        return (SolidColorBrush)App.Current.FindResource("Color_SID");
                    case ContentType.STAR:
                        return (SolidColorBrush)App.Current.FindResource("Color_STAR");
                    case ContentType.APP:
                        return (SolidColorBrush)App.Current.FindResource("Color_APP");
                    case ContentType.UNDEFINED:
                        return (SolidColorBrush)App.Current.FindResource("Color_ND");
                    default:
                        throw new Exception();
                }
            }
        }

        public ChartViewModel(Chart model)
        {
            chartModel = model;
            chartModel.PropertyChanged += ModelChanged;
        }

        /// <summary>
        /// Informs the view when the model changes (e.g. because of edit)
        /// </summary>
        private void ModelChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Identifier":
                    NotifyOfPropertyChange(() => Identifier);
                    break;

                case "Description":
                    NotifyOfPropertyChange(() => Description);
                    break;

                case "Content":
                    NotifyOfPropertyChange(() => Accent);
                    NotifyOfPropertyChange(() => ContentString);
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// Call a PropertyChanged on Edit: Returns a new copy of the Edit ViewModel
        /// </summary>
        public void ResetEdit()
            => NotifyOfPropertyChange(() => Edit);

        private string Stringify()
            => (Identifier ?? string.Empty) + " " + (Description ?? string.Empty) + " " 
                + chartModel.Keywords?.Aggregate(string.Empty, (acc, current) => acc += current + " ");
        
        public bool SearchPredicate(string searchText)    
            => Stringify().IndexOf(searchText ?? string.Empty, StringComparison.OrdinalIgnoreCase) >= 0;
    }
}
