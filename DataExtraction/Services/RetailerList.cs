public class RetailerList
{
    public static List<string> StringList { get; set; }
    static RetailerList()
    {
        StringList = new List<string>();

        StringList.Add("Origin");
        StringList.Add("AGL");
        StringList.Add("Meridian");
        StringList.Add("Genesis");
        StringList.Add("Suncorp");
    }
}