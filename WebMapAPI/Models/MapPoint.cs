using Newtonsoft.Json;
namespace WebMapAPI.Models
{
    //GeoJson形式の，POSTやGETで使用するデータ構造のモデル
    public class MapPoint
    {
        public string type { get; set; }
        public GeoPoint geometry { get; set; }
        public Properties properties { get; set; }
    }

    public class GeoPoint
    {
        public string type { get; set; }
        public double[] coordinates { get; set; }
    }

    public class Properties
    {
        public int id { get; set; }
        public string name { get; set; }
        public string address { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string phone { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string url { get; set; }
    }
}
