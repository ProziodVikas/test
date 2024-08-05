using System;
using System.Collections.Generic;

public class BillMetadata
{
    public string AccountNumber { get; set; }
    public string InvoiceNumber { get; set; }
    public string Country { get; set; }//global
    public string Commodity { get; set; }
    public string RetailerShortName { get; set; }
    public string Address { get; set; }
    public string City { get; set; }
    public string Postcode { get; set; }
    public DateOnly? PeriodFrom { get; set; }
    public DateOnly? PeriodTo { get; set; }
    public DateTime? IssueDate { get; set; }
    public DateTime? DueDate { get; set; }

    // List to store charges
    public List<Charge> Charges { get; set; }

    // Total
    public Total Total { get; set; }

    public BillMetadata()
    {
        Charges = new List<Charge>();
        Total = new Total();
    }
}

public class Charge
{
    public string ChargeName { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal Cost { get; set; }
}

public class Total
{
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal Cost { get; set; }
}
