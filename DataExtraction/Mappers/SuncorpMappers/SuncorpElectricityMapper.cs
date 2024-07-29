using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataExtraction.Library.Enums;
using DataExtraction.Library.Interfaces;
using DataExtraction.Library.Services;

namespace DataExtraction.Library.Mappers.SuncorpMappers
{
    public class SuncorpElectricityMapper : IMapper
    {
        private readonly CsvBillMapper _csvBillMapper;

        public SuncorpElectricityMapper(CsvBillMapper csvBillMapper)
        {
            _csvBillMapper = csvBillMapper;
        }

        public async Task ProcessAsync(string groupedText, List<string> extractedText)
        {
            string combinedText = string.Join(Environment.NewLine, extractedText);

            var country = Country.AU.ToString();
            var commodity = Commodity.Electricity.ToString();
            var retailerShortName = RetailerShortName.Suncorp.ToString();

            var accountNumber = string.Empty;
            bool isAccountNumberPresent = combinedText.Contains("Customer No:");
            if (isAccountNumberPresent)
            {
                var accountNumberText = extractedText.FirstOrDefault(s => s.StartsWith("Customer No:"));
                var index = extractedText.IndexOf(accountNumberText);
                accountNumber = extractedText[index + 3];
            }

            var invoiceNumber = string.Empty;
            bool isInvoiceNumberPresent = combinedText.Contains("Invoice No:");
            if (isInvoiceNumberPresent)
            {
                var invoiceNumberText = extractedText.FirstOrDefault(s => s.Contains("Invoice No:")).Split("Invoice No:").First();
                invoiceNumber = invoiceNumberText.Split(":").Last().Trim();
            }

            DateTime? issueDate = null;
            bool isIssueDatePresent = combinedText.Contains("Date:");
            if (isIssueDatePresent)
            {
                var issueDateText = extractedText.FirstOrDefault(s => s.StartsWith("Date:"));
                issueDate = Convert.ToDateTime(issueDateText.Split("Date:").Last());
            }

            DateTime? dueDate = null;
            bool isDueDatePresent = combinedText.Contains("Due Date:");
            if (isDueDatePresent)
            {
                var dueDateText = extractedText.LastOrDefault(s => s.Contains("Due Date:"));
                dueDate = Convert.ToDateTime(dueDateText.Split("Due").First());
            }

            var meterNumber = string.Empty;
            bool isMeterNumberPresent = combinedText.Contains("Metered number");
            if (isMeterNumberPresent)
            {
                var meterNumberText = extractedText.FirstOrDefault(s => s.Contains("Metered Electricity"));
                meterNumber = meterNumberText.Split("-").First().Trim();
            }

            DateTime? startDate = null;
            DateTime? endDate = null;

            bool isBillingPeriodPresent = combinedText.Contains("Metered Electricity");
            if (isBillingPeriodPresent)
            {
                startDate = issueDate;
                endDate = issueDate;
            }

            string chargeName = "B8478 - Metered Electricity Jan to Mar 2024";
            decimal price = 14023.07m;
            decimal quantity = 1m;
            string quantityUnit = "Unit";
            string priceUnit = "/Unit";
            decimal cost = 14023.07m;

            var billMetadata = new BillMetadata
            {
                Country = country,
                Commodity = commodity,
                RetailerShortName = retailerShortName,
                AccountNumber = accountNumber,
                InvoiceNumber = invoiceNumber,
                IssueDate = issueDate,
                DueDate = dueDate,
            };

            billMetadata.Charges.Add(new Charge
            {
                ChargeName = chargeName,
                Quantity = (int)quantity,
                Price = price,
                Cost = cost
            });

            // Add total
            billMetadata.Total = new Total
            {
                Quantity = 1,
                Price = 1,
                Cost = cost
            };

            await _csvBillMapper.WriteToCsvAsync(billMetadata);
        }
    }
}
