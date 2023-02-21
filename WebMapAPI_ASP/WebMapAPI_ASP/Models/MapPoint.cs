using System.ComponentModel.DataAnnotations;

namespace WebMapAPI_ASP.Models
{
    //GET,POST用のGeoJsonを元にした構造のモデル
    public class MapPoint
    {
        [Required]
        public string type { get; set; }
        [Required]
        public GeoPoint geometry { get; set; }
        [Required]
        public Properties properties { get; set; }
    }

    public class GeoPoint
    {
        [Required]
        public string type { get; set; }
        [Required]
        public double[] coordinates { get; set; }
    }

    public class Properties
    {
        [Required]
        public int id { get; set; }
        [Required]
        public string name { get; set; }
        [Required]
        public string address { get; set; }

        public string phone { get; set; }

        public string url { get; set; }
    }
}
