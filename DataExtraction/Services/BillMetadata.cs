using System.Collections.Generic;

public class BillMetadata
{
    // Global fields
    public string billingCurrency { get; set; }
    public string billingAddress { get; set; }
    public decimal totalAmountDue { get; set; }
    public DateTime dueDate { get; set; }
    public DateTime nextBillingDate { get; set; }
    public string customerServiceContact { get; set; }
    public decimal currentBillAmount { get; set; }
    public string accountNumber { get; set; }
    public string invoiceNumber { get; set; }
    public DateTime invoiceDate { get; set; }
    public decimal fixedChargeTotal { get; set; }
    public string ICP { get; set; }
    public string billingPeriod { get; set; }
    public decimal gst { get; set; }
    public decimal fixedChargeQuantity { get; set; }
    public decimal fixedChargeRate { get; set; }
    public string paymentMethods { get; set; }
    public decimal previousBalance { get; set; }
    public decimal previousPayment { get; set; }
    public  DateTime meterReadEndDate { get; set; }
    public DateTime meterReadStartDate { get; set; }
    public List<metersData> metersData { get; set; } = new List<metersData>();
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
    public decimal meterMultiplier { get; set; }
    public string type { get; set; }
    public decimal rate { get; set; }
    public decimal quantity { get; set; }
    public decimal total { get; set; }
    public string previousReading { get; set; }
    public string currentReading { get; set; }
}