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
using MaterialDesignThemes.Wpf;

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

        private bool _IsBusyIndeterminate;
        public bool IsBusyIndeterminate
        {
            get => _IsBusyIndeterminate;
            set {
                _IsBusyIndeterminate = value;
                NotifyOfPropertyChange(() => IsBusyIndeterminate);
                NotifyOfPropertyChange(() => IsBusy);
                NotifyOfPropertyChange(() => CanInteract);
            }
        }

        private bool _IsBusyDeterminate;
        public bool IsBusyDeterminate
        {
            get => _IsBusyDeterminate;
            set
            {
                _IsBusyDeterminate = value;
                NotifyOfPropertyChange(() => IsBusyDeterminate);
                NotifyOfPropertyChange(() => IsBusy);
                NotifyOfPropertyChange(() => CanInteract);
            }
        }

        public bool IsBusy { get => IsBusyIndeterminate || IsBusyDeterminate; }

        private int _DeterminateProgress;
        public int DeterminateProgress
        {
            get => _DeterminateProgress;
            set { _DeterminateProgress = value; NotifyOfPropertyChange(() => DeterminateProgress); }
        }


        public bool CanInteract { get => !IsBusy; }

        public bool UnsavedChanges { get; set; }

        public EditChartsViewModel(Airport airport)
        {
            model = airport;

            // Add copies of the charts
            var clones = airport.Charts.ToList().ConvertAll(x => x.Clone());

            // Render Thumbnails in Background
            BindingOperations.EnableCollectionSynchronization(ChartThumbs, thumbnailsLock);

            Task.Run(() => {
                DeterminateProgress = 75;
                IsBusyIndeterminate = true;
                clones.ForEach(x => ChartThumbs.Add(new ChartThumbnailViewModel(x)));
                IsBusyIndeterminate = false;
            });
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
                DeterminateProgress = 0;
                IsBusyDeterminate = true;
                string pdfPath = dialog.FileName;
                Slicer.ProgressChanged += (o, e) => DeterminateProgress = e.ProgressPercentage;
                var newPdfs = await Slicer.SliceAsync(pdfPath);
                Slicer.ProgressChanged -= (o, e) => DeterminateProgress = e.ProgressPercentage;

                _ = Task.Run(() => {
                    DeterminateProgress = 75;
                    IsBusyIndeterminate = true;
                    newPdfs.ToList().ForEach(x => ChartThumbs.Add(new ChartThumbnailViewModel(x)));
                    IsBusyIndeterminate = false;
                });

                UnsavedChanges = true;
                IsBusyDeterminate = false;
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
        /// Handles closings
        /// </summary>
        public async void Close()
        {
            bool sure = false;

            if (UnsavedChanges)
            {
                await DialogHost.Show(new Views.PromptDiscardChanges(), "EditChartsDialogHost", delegate (object sender, DialogClosingEventArgs e) {
                    sure = (bool)e.Parameter;
                });
            }

            if (!UnsavedChanges || sure)
                DialogHost.CloseDialogCommand.Execute(null, null);
        }

        /// <summary>
        /// Saves the changes to the model and database
        /// </summary>
        public async void Save()
        {
            DeterminateProgress = 75;
            IsBusyIndeterminate = true;

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
            UnsavedChanges = false;
            IsBusyIndeterminate = false;
            DialogHost.CloseDialogCommand.Execute(null, null);
        }
    }
}
