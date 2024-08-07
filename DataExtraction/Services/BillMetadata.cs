using System;
using System.Collections.Generic;

public class BillMetadata
{
    public string BillIdentifier { get; set; }
    public string AccountNumber { get; set; }
    public string InvoiceNumber { get; set; }
    public string BillingPeriod { get; set; }
    public string IssueDate { get; set; }
    public string DueDate { get; set; }
    public string NextBillingDate { get; set; }
    public string ServiceDescription { get; set; }
    public string TotalAmountDue { get; set; }
    public string PaymentMethod { get; set; }
    public string OpeningBalance { get; set; }
    public string PreviousPayment { get; set; }
    public string CustomerServiceContact { get; set; }
    public string BillingAddress { get; set; }
    public string CurrentBillAmount { get; set; }
    public string DiscountAmount { get; set; }
    public string CustomerNumber { get; set; }



    public string ICP { get; set; }
    public string ReadStartDate { get; set; }
    public string ReadEndDate { get; set; }
    public string FixedChargeQuantity { get; set; }
    public string FixedChargeRate { get; set; }
    public string FixedChargeTotal { get; set; }
    public string GST { get; set; }




    public string MeterNumber { get; set; }
    public string Multiplier { get; set; }
    public string Type { get; set; }
    public string PreviousReading { get; set; }
    public string CurrentReading { get; set; }
    public string Rate { get; set; }
    public string Quantity { get; set; }
    public string Total { get; set; }

    //public string Country { get; set; }//global
    //public string Commodity { get; set; }
    //public string RetailerShortName { get; set; }
    //public string Address { get; set; }
    //public string City { get; set; }
    //public string Postcode { get; set; }
    //public DateOnly? PeriodFrom { get; set; }
    //public DateOnly? PeriodTo { get; set; }



    // Total
    //public Total Total { get; set; }

    //public BillMetadata()
    //{
    //    Charges = new List<Charge>();
    //    Total = new Total();
    //}
}

//public class Charge
//{
    
//}

//public class FinalTotal
//{
    
//}
