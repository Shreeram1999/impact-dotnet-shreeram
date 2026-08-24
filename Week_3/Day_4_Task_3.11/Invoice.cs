// A plain, ordinary class - nothing reflection-specific about it. The
// interesting part is entirely in Program.cs, which INSPECTS this class
// from the outside using reflection instead of just using it normally.
public class Invoice
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime IssuedDate { get; set; }

    // A parameterless constructor is required for the
    // Activator.CreateInstance() call later in Program.cs to work.
    public Invoice()
    {
    }

    public Invoice(string invoiceNumber, decimal amount)
    {
        InvoiceNumber = invoiceNumber;
        Amount = amount;
        IssuedDate = DateTime.Today;
    }

    public decimal CalculateTax(decimal taxRate) => Amount * taxRate;

    public string Summarize() => $"{InvoiceNumber}: {Amount:C} (issued {IssuedDate:d})";
}
