using DataExtraction.Library.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace DataExtraction.Library.Services
{
    public class RetailerSelection : IRetailerSelection
    {

        public async Task ProcessExtractedTextAsync(List<string> extractedText)
        {
            var selectedRetailer = string.Empty;
            string groupedText = string.Empty;
            await Task.Run(() =>
            {
                foreach (var text in extractedText)
                {
                    groupedText = string.Join(" ", extractedText);
                }
            });
            List<string> retailerList = RetailerList.StringList;
            retailerList.Sort();
            if (groupedText.ToString().Length != 0 && retailerList.ToList() != null)
            {

                foreach (var retailer in retailerList)
                {
                    if (!string.IsNullOrEmpty(selectedRetailer)) continue;

                    if (groupedText.Contains(retailer))
                        selectedRetailer = retailer.ToString();

                }

            }

        }

    }
}
