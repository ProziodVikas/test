using DataExtraction.Library.Interfaces;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;
using Aspose.Pdf;
using Aspose.Pdf.Text;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataExtraction.Library.Services
{
    public class ExtractPdfService : IExtractPdfService
    {
        private readonly IRetailerSelection _retailerSelection;

        public ExtractPdfService(IRetailerSelection retailerSelection)
        {
            _retailerSelection = retailerSelection;
        }

        //public async Task<List<string>> ExtractTextFromPdf(string filePath)
        //{
        //    var extractedText = new List<string>();

        //    byte[] bytes = System.IO.File.ReadAllBytes(filePath);
        //    using (var memoryStream = new MemoryStream(bytes))
        //    {
        //        memoryStream.Seek(0, SeekOrigin.Begin);
        //        using (var document = PdfDocument.Open(memoryStream))
        //        {
        //            foreach (var page in document.GetPages())
        //            {
        //                extractedText.Add($"Page{page.Text}");
        //                var extractor = ContentOrderTextExtractor.GetText(page);
        //                var lines = extractor.Split("\r\n");
        //                extractedText.AddRange(lines);
        //            }
        //        }
        //    }

        //    // Use Aspose to extract text from the same PDF
        //    using (var asposeStream = new MemoryStream(bytes))
        //    {
        //        var asposePages = ExtractTextFromPdfAspose(asposeStream);
        //        foreach (var page in asposePages)
        //        {
        //            extractedText.Add($"Aspose Page {page.PageNumber}");
        //            extractedText.AddRange(page.IndexedText.Select(t => t.Text));
        //        }
        //    }

        //    await _retailerSelection.ProcessExtractedTextAsync(extractedText);
        //    return extractedText;
        //}

        public async Task<List<string>> ExtractTextFromPdf(string filePath)
        {
            var extractedText = new List<string>();

            byte[] bytes = System.IO.File.ReadAllBytes(filePath);
            using (var memoryStream = new MemoryStream(bytes))
            using (var pdfDocument = new Aspose.Pdf.Document(memoryStream))
            {
                for (int i = 1; i <= pdfDocument.Pages.Count; i++)
                {
                    var page = pdfDocument.Pages[i];
                    var absorber = new TextAbsorber();
                    page.Accept(absorber);

                    var pageText = absorber.Text.Split('\n');
                    extractedText.AddRange(pageText);
                }
            }

            await _retailerSelection.ProcessExtractedTextAsync(extractedText);
            return extractedText;
        }
    }

}
