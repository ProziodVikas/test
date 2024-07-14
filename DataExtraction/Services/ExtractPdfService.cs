using DataExtraction.Library.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;
using UglyToad.PdfPig.Util;

namespace DataExtraction.Library.Services
{
    public class ExtractPdfService : IExtractPdfService
    {
        public async Task<List<string>> ExtractTextFromPdf(string filePath)
        {
            var extractedText = new List<string>();
            byte[] bytes = System.IO.File.ReadAllBytes(filePath);
            using (var memoryStream = new MemoryStream(bytes))
            {
                memoryStream.Seek(0, SeekOrigin.Begin);
                using (var document = PdfDocument.Open(memoryStream))
                {
                    foreach(var page in document.GetPages())
                    {
                        extractedText.Add($"Page{page.Number}");
                        var extractor = ContentOrderTextExtractor.GetText(page);
                        var lines = extractor.Split("\r\n");
                        extractedText.AddRange(lines);
                    }
                }
            }
            return extractedText;
        }
    }
}
