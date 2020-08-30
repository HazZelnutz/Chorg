using System;
using System.Collections.Generic;
using System.Data.SQLite;
using Chorg.ViewModels;
using System.IO;
using Caliburn.Micro;
using Chorg.Views;
using System.Linq;

namespace Chorg.Models
{
    public class Chart : PropertyChangedBase
    {
        public int? Id { get; set; } = null;
        public int Page { get; set; }

        private string _Identifier;
        public string Identifier {
            get => _Identifier;
            set { _Identifier = value; NotifyOfPropertyChange(() => Identifier); }
        }

        private string _Description;
        public string Description {
            get => _Description;
            set { _Description = value; NotifyOfPropertyChange(() => Description); }
        }

        private ContentType _Content;
        public ContentType Content {
            get => _Content;
            set { _Content = value; NotifyOfPropertyChange(() => Content); } }

        public List<string> Keywords { get; set; } = new List<string>();
        public SQLiteBlob Blob { get; set; }

        byte[] rawPDF;
        int size;

        public Chart(int page, int size)
        {
            this.Page = page;
            this.size = size;
        }

        public Chart(int page, byte[] pdf)
        {
            Page = page;
            rawPDF = pdf;
            size = sizeof(byte) * pdf.Length;
        }

        public byte[] GetChart()
        {
            if (rawPDF == null)
            {
                try
                {
                    // Read Data from DB
                    return Gateway.GetInstance().GetPDFBytes(this);
                }
                catch (Exception e)
                {
                    MainViewModel.GetInstance().TriggerSnackbar(e);
                    return new byte[0];
                }
            }
            else
                return rawPDF;
        }

        public Chart Clone()
        {
            Chart temp = (Chart)MemberwiseClone();
            temp.Keywords = new List<string>(Keywords);
            return temp;
        }

        public void FreeRawPdf()
            => rawPDF = null;
        
        public MemoryStream GetStream()
            => new MemoryStream(GetChart());

        public override string ToString()
            => Description ?? $"Chart Page {Page}";

        public static explicit operator ChartViewModel(Chart chart)
            => new ChartViewModel(chart);
    }
}
