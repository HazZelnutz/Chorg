using System.Collections.Generic;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace Chorg.Models.PDF
{
    public static class Slicer
    {
        /// <summary>
        /// Converts the PDF to charts
        /// </summary>
        /// <param name="pdfPath">Path to the PDF</param>
        /// <returns>Charts</returns>
        public static ICollection<Chart> Slice(string pdfPath)
        {
            PdfReader reader = new PdfReader(pdfPath);
            ICollection<Chart> result = new List<Chart>();

            for (int i = 0; i < reader.NumberOfPages; i++)
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    Document document = new Document();
                    PdfCopy copy = new PdfCopy(document, stream);
                    document.Open();
                    copy.AddPage(copy.GetImportedPage(reader, i + 1));
                    document.Close();

                    result.Add( new Chart(i + 1, stream.ToArray()) );
                }
            }

            return result;
        }
    }
}
