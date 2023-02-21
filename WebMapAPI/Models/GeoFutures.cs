using System;
namespace WebMapAPI.Models
{
    //GETで複数のレコードを一括で取得する際のモデル
    public class GeoFutures
    {
        public string type { get; set; }
        public MapPoint[] features { get; set; }
    }
}
