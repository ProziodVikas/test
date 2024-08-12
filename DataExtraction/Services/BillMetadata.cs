using System.Collections.Generic;

public class BillMetadata
{
    // Global fields
    public string AccountNumber { get; set; }
    public string InvoiceNumber { get; set; }
    public string IssueDate { get; set; }
    public string DueDate { get; set; }
    public string NextBillingDate { get; set; }
    public string TotalAmountDue { get; set; }
    public string PaymentMethod { get; set; }
    public string OpeningBalance { get; set; }
    public string PreviousPayment { get; set; }
    public string CustomerServiceContact { get; set; }
    public string CurrentBillAmount { get; set; }
    public string DiscountAmount { get; set; }

    // ICP Data
    public List<ICP> ICPS { get; set; } = new List<ICP>();
}

public class ICP
{
    public string ICPCode { get; set; }
    public string ServiceDescription { get; set; }
    public string BillingAddress { get; set; }
    public string BillingPeriod { get; set; }
    public string ReadStartDate { get; set; }
    public string ReadEndDate { get; set; }
    public List<Meter> Meters { get; set; } = new List<Meter>();

}
public class Meter
{
    public string MeterNumber { get; set; }
    public string FixedChargeQuantity { get; set; }
    public string FixedChargeRate { get; set; }
    public string FixedChargeTotal { get; set; }
    public string GST { get; set; }
    public List<Type> Types { get; set; } = new List<Type>();
}

public class Type
{
    public string TypeName { get; set; }
    public string Multiplier { get; set; }
    public string PreviousReading { get; set; }
    public string CurrentReading { get; set; }
    public string Rate { get; set; }
    public string Quantity { get; set; }
    public string Total { get; set; }
}


