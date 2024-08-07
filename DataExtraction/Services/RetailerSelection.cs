using DataExtraction.Library.Interfaces;
using DataExtraction.Library.Retailers;

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
                if (!string.IsNullOrEmpty(selectedRetailer))
                {
                    IRetailer retailerInstance = null;

                    switch (selectedRetailer.ToLower())
                    {
                        case "agl":
                            retailerInstance = new AglRetailer();
                            break;
                        case "suncorp":
                            retailerInstance = new SuncorpRetailer();
                            break;
                        case "genesis":
                            retailerInstance = new GenesisRetailer();
                            break;
                        case "meridian":
                            retailerInstance = new MeridianRetailer();
                            break;
                        default:
                            throw new ArgumentException("Invalid retailer name");
                    }

                    retailerInstance?.ProcessAsync(groupedText, extractedText);
                }

            }
        }
    }
}
