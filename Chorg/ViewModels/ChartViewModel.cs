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
        private Chart model;

        public string Identifier
        {
            get => model.Identifier ?? "No Identifier";
        }

        public string Description
        {
            get => model.Description ?? "No Description";
        }

        public MemoryStream PDFStream
        {
            get => model.GetStream();
        }

        public ContentType Content
        {
            get => model.Content;
        }

        public ChartViewModel(Chart model)
        {
            this.model = model;
        }

        public Chart GetModel()
            => model;

        private string Stringify()
            => (Identifier ?? string.Empty) + " " + (Description ?? string.Empty) + " " 
                + model.Keywords?.Aggregate(string.Empty, (acc, current) => acc += current + " ");
        
        public bool SearchPredicate(string searchText)    
            => Stringify().IndexOf(searchText ?? string.Empty, StringComparison.OrdinalIgnoreCase) >= 0;
    }
}
