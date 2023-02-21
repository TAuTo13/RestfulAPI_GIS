using NetTopologySuite.Geometries;

namespace WebMapAPI_ASP.Models
{
    //UPDATE(PUT)リクエスト用のモデル
    public class UpdateProperties
    {
        public double[] coordinates { get; set; }
        public string name { get; set; }
        public string address { get; set; }
        public string phone { get; set; }
        public string url { get; set; }
    }
}
