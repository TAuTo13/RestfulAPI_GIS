namespace WebMapAPI_ASP.Models
{
    //GETAll用のモデル
    public class GeoFutures
    {
        public string type { get; set; }
        public MapPoint[] features { get; set; }
    }
}
