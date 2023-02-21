using System;
namespace WebMapAPI.Models
{
    //データの更新を行う処理で渡されるデータ形式のモデル
    public class UpdateProperties
    {
        public double?[] coordinates { get; set; }
        public string name { get; set; }
        public string address { get; set; }
        public string phone { get; set; }
        public string url { get; set; }
    }
}
