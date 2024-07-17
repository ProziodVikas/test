using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataExtraction.Library.Interfaces
{
    public interface IRetailerSelection
    {
        Task ProcessExtractedTextAsync(List<string> extractedText);

    }
}