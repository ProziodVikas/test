using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataExtraction.Library.Interfaces
{
    public interface IExtractPdfService
    {
        Task<List<string>> ExtractTextFromPdf(string filePath, string billsFolderPath);
        //Task<List<string>> ExtractTextFromPdf(Stream pdfstream);
    }
}