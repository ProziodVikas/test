using Aspose.Pdf;
using System.Collections.Generic;

public class BillMetadata
{
    public string billingCurrency { get; set; }
    public string billingAddress { get; set; }
    public string totalAmountDue { get; set; }
    public DateOnly dueDate { get; set; }
    public DateOnly nextBillingDate { get; set; }
    public string customerServiceContact { get; set; }
    public string currentBillAmount { get; set; }
    public string accountNumber { get; set; }
    public string invoiceNumber { get; set; }
    public DateOnly invoiceDate { get; set; }
    public string fixedChargeTotal { get; set; }
    public string ICP { get; set; }
    public string billingPeriod { get; set; }
    public string gst { get; set; }
    public string fixedChargeQuantity { get; set; }
    public string fixedChargeRate { get; set; }
    public string paymentMethods { get; set; }
    public string previousBalance { get; set; }
    public string previousPayment { get; set; }
    public DateOnly meterReadEndDate { get; set; }
    public DateOnly meterReadStartDate { get; set; }
    public List<metersData> metersData { get; set; }
    public string templateId { get; set; }
    public int templateVersion { get; set; }
    public string utilityType { get; set; }
    public string supplierName { get; set; }
    public string customerName { get; set; }
    public string fileName { get; set; }
    public string fileExtension { get; set; }
}


public class metersData
{
    public string meterNumber { get; set; }
    public List<meterType> meterTypes { get; set; } = new List<meterType>();
}
public class meterType
{
    public string type { get; set; }
    public string meterMultiplier { get; set; }
    public string rate { get; set; }
    public string quantity { get; set; }
    public string total { get; set; }
    public string previousReading { get; set; }
    public string currentReading { get; set; }
}