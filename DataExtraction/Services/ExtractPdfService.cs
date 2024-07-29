using DataExtraction.Library.Interfaces;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace DataExtraction.Library.Services
{
    public class ExtractPdfService : IExtractPdfService
    {
        private readonly IRetailerSelection _retailerSelection;

        public ExtractPdfService(IRetailerSelection retailerSelection)
        {
            _retailerSelection = retailerSelection;
        }
        public async Task<List<string>> ExtractTextFromPdf(string filePath)
        {
            var extractedText = new List<string>();
            
            byte[] bytes = System.IO.File.ReadAllBytes(filePath);
            using (var memoryStream = new MemoryStream(bytes))
            {
                memoryStream.Seek(0, SeekOrigin.Begin);
                using (var document = PdfDocument.Open(memoryStream))
                {
                    foreach (var page in document.GetPages())
                    {
                        extractedText.Add($"Page{page.Text}");
                        var extractor = ContentOrderTextExtractor.GetText(page);
                        var lines = extractor.Split("\r\n");
                        extractedText.AddRange(lines);
                    }
                    await _retailerSelection.ProcessExtractedTextAsync(extractedText);
                }
            }
            return extractedText;
        }
    }
}