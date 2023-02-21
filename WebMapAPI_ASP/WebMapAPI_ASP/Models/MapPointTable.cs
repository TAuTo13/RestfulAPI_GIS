using System.ComponentModel.DataAnnotations.Schema;
using NetTopologySuite.Geometries;

namespace WebMapAPI_ASP.Models
{
    //データベースとの連携用のモデル
    [Table("mappointtable")]
    public class MapPointTable
    {
        [Column("geoid")]
        public int Id { get; set; }
        [Column("geom")]
        public Point Point { get; set; }
        [Column("name")]
        public string Name { get; set; }
        [Column("address")]
        public string Address { get; set; }
        [Column("phone")]
        public string Phone { get; set; }
        [Column("url")]
        public string Url { get; set; }
    }
}
