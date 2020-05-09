using System.Collections.Generic;
using System.Linq;
using Chorg.ViewModels;
using Caliburn.Micro;

namespace Chorg.Models
{
    public class Airport : PropertyChangedBase
    {
        public int? Id { get; set; } = null;

        private string _Name;
        public string Name {
            get => _Name;
            set { _Name = value;  NotifyOfPropertyChange(() => Name); } 
        }

        public string ICAO { get; }
        public ICollection<Chart> Charts { get; set; }

        public Airport(string icao)
        {
            ICAO = icao;
        }

        public Chart this[int i]
        {
            get => Charts.ElementAt(i);
        }

        public Chart GetChartByPage(int page)
            => this[page - 1];

        public override string ToString() => $"Airport: {ICAO}";

        public static explicit operator AirportViewModel(Airport airport)
            => new AirportViewModel(airport);
    }

}
