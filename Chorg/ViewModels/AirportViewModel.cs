using Chorg.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using System.ComponentModel;

namespace Chorg.ViewModels
{
    public class AirportViewModel : Screen, ITextSearch
    {
        private Airport model;

        public string ICAO { get => model.ICAO; }
        public string AirportName { get => model.Name ?? "No Name"; }
        public List<ChartViewModel> Charts { get => model.Charts.ToList().ConvertAll(chart => (ChartViewModel)chart); }

        public AirportViewModel(Airport model)
        {
            this.model = model;
            model.PropertyChanged += ModelChanged;
        }

        /// <summary>
        /// Informs the view when the model changes (e.g. because of edit)
        /// </summary>
        private void ModelChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Name":
                    NotifyOfPropertyChange(() => AirportName);
                    break;

                default:
                    break;
            }
        }

        public Airport GetModel()
            => model;

        private string Stringify()        
            => ICAO + " " + AirportName ?? string.Empty;        

        public bool SearchPredicate(string searchText)
            => Stringify().IndexOf(searchText ?? string.Empty, StringComparison.OrdinalIgnoreCase) >= 0;
    }
}
