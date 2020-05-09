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
        private Airport airportModel;

        public EditAirportViewModel Edit { get => new EditAirportViewModel(airportModel, this); }

        public string ICAO { get => airportModel.ICAO; }
        public string AirportName { get => airportModel.Name ?? "No Name"; }
        public List<ChartViewModel> Charts { get; }

        public AirportViewModel(Airport model)
        {
            airportModel = model;
            airportModel.PropertyChanged += ModelChanged;

            Charts = new List<ChartViewModel>(model.Charts.ToList().ConvertAll(chart => (ChartViewModel)chart));
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

        /// <summary>
        /// Call a PropertyChanged on Edit: Returns a new copy of the Edit ViewModel
        /// </summary>
        public void ResetEdit()
            => NotifyOfPropertyChange(() => Edit);

        private string Stringify()        
            => ICAO + " " + AirportName ?? string.Empty;        

        public bool SearchPredicate(string searchText)
            => Stringify().IndexOf(searchText ?? string.Empty, StringComparison.OrdinalIgnoreCase) >= 0;
    }
}
