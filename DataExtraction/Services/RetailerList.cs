public class RetailerList
{
    public static List<string> StringList { get; set; }
    static RetailerList()
    {
        StringList = new List<string>();

        StringList.Add("Origin");
        StringList.Add("AGL");
        StringList.Add("agl.com");
        StringList.Add("Xyz");
        StringList.Add("Suncorp");
    }
}