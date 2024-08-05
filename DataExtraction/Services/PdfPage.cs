public class PdfPage
{
    public int PageNumber { get; set; }
    public List<(int Index, string Text)> IndexedText { get; set; }
}