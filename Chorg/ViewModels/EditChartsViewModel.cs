using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chorg.Models;
using Chorg.Models.PDF;
using Caliburn.Micro;
using System.Collections.ObjectModel;
using PdfiumViewer;
using Microsoft.Win32;
using System.Windows.Threading;
using System.Windows.Data;
using Org.BouncyCastle.Bcpg;

namespace Chorg.ViewModels
{
    public class EditChartsViewModel : Conductor<EditChartViewModel>
    {
        private Airport model;

        private object thumbnailsLock = new object();
        public ObservableCollection<ChartThumbnailViewModel> ChartThumbs { get; private set; }
            = new ObservableCollection<ChartThumbnailViewModel>();

        private ChartThumbnailViewModel _SelectedChartThumb;
        public ChartThumbnailViewModel SelectedChartThumb
        {
            get => _SelectedChartThumb;
            set {
                _SelectedChartThumb = value;
                if (SelectedChartThumb != null)
                    ActivateItem(new EditChartViewModel(SelectedChartThumb.GetModel()));
                else
                    ActivateItem(null);
            }
        }

        private bool _IsBusySaving;
        public bool IsBusySaving
        {
            get => _IsBusySaving;
            set {
                _IsBusySaving = value;
                NotifyOfPropertyChange(() => IsBusySaving);
                NotifyOfPropertyChange(() => IsBusy);
                NotifyOfPropertyChange(() => CanInteract);
            }
        }

        private bool _IsBusySlicing;
        public bool IsBusySlicing
        {
            get => _IsBusySlicing;
            set
            {
                _IsBusySlicing = value;
                NotifyOfPropertyChange(() => IsBusySlicing);
                NotifyOfPropertyChange(() => IsBusy);
                NotifyOfPropertyChange(() => CanInteract);
            }
        }

        public bool IsBusy { get => IsBusySaving || IsBusySlicing; }

        private int _SliceProgress;
        public int SliceProgress
        {
            get => _SliceProgress;
            set { _SliceProgress = value; NotifyOfPropertyChange(() => SliceProgress); }
        }


        public bool CanInteract { get => !IsBusy; }

        public EditChartsViewModel(Airport airport)
        {
            model = airport;

            // Slice Progress
            Slicer.ProgressChanged += (o, e) => SliceProgress = e.ProgressPercentage;

            // Add copies of the charts
            var clones = airport.Charts.ToList().ConvertAll(x => x.Clone());

            // Render Thumbnails in Background
            BindingOperations.EnableCollectionSynchronization(ChartThumbs, thumbnailsLock);
            Task.Run(() => clones.ForEach(x => ChartThumbs.Add(new ChartThumbnailViewModel(x))));
        }


        /// <summary>
        /// Opens a dialog to add a PDF
        /// </summary>
        public async void AddPDF()
        {
            OpenFileDialog dialog = new OpenFileDialog()
            {
                CheckFileExists = true,
                Filter = "PDF Document (*.pdf)|*.pdf;*.PDF"
            };

            if ((bool)dialog.ShowDialog())
            {
                SliceProgress = 0;
                IsBusySlicing = true;
                string pdfPath = dialog.FileName;
                var newPdfs = await Slicer.SliceAsync(pdfPath);
                _ = Task.Run(() => newPdfs.ToList().ForEach(x => ChartThumbs.Add(new ChartThumbnailViewModel(x))));
                IsBusySlicing = false;
            }
        }

        /// <summary>
        /// Returns all clone models
        /// </summary>
        /// <returns></returns>
        private IEnumerable<Chart> GetModels()
        {
            foreach (var thumb in ChartThumbs)
                yield return thumb.GetModel();
        }

        /// <summary>
        /// Saves the changes to the model and database
        /// </summary>
        public async void Save()
        {
            SliceProgress = 75;
            IsBusySaving = true;

            foreach (Chart chart in GetModels())
            {
                // New Chart --> Must be inserted in Database first
                if(chart.Id == null)
                {
                    await Gateway.GetInstance().AddChartToAirportAsync(chart, model);
                }
                else
                {
                    await Gateway.GetInstance().UpdateChartAsync(chart);
                }
            }

            IsBusySaving = false;
        }
    }
}
