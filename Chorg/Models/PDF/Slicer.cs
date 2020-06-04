using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace Chorg.Models.PDF
{
    public static class Slicer
    {
        public static event ProgressChangedEventHandler ProgressChanged;

        /// <summary>
        /// Converts the PDF to charts
        /// </summary>
        /// <param name="pdfPath">Path to the PDF</param>
        /// <returns>Charts</returns>
        public static ICollection<Chart> Slice(string pdfPath)
        {
            PdfReader reader = new PdfReader(pdfPath);
            var result = new List<Chart>();

            for (int i = 0; i < reader.NumberOfPages; i++)
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    Document document = new Document();
                    PdfCopy copy = new PdfCopy(document, stream);
                    document.Open();
                    copy.AddPage(copy.GetImportedPage(reader, i + 1));
                    document.Close();

                    string defaultName = $"{Path.GetFileNameWithoutExtension(pdfPath)} (Page {i + 1})";
                    result.Add( new Chart(i + 1, stream.ToArray()) { Description = defaultName } );
                }

                double prog = (double)(i + 1) / reader.NumberOfPages;
                ProgressChanged?.Invoke(null, new ProgressChangedEventArgs((int)(prog * 100), null));
            }

            return result;
        }

        public async static Task<ICollection<Chart>> SliceAsync(string pdfPath)
        {
            ICollection<Chart> slices = null;
            await Task.Run(() => slices = Slice(pdfPath));
            return slices;
        }
    }
}
