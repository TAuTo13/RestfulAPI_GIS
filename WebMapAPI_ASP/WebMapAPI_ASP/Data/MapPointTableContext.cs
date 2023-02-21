using Microsoft.EntityFrameworkCore;
using WebMapAPI_ASP.Models;

namespace WebMapAPI_ASP.Data
{
    public class MapPointTableContext:DbContext
    {
        public MapPointTableContext(DbContextOptions<MapPointTableContext> options):base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder model)
        {
            //データベースモデルのプライマリーキーを設定
            model.Entity<MapPointTable>().HasKey(c => new { c.Id });
        }

        //データベースに対応するモデルを設定
        public DbSet<MapPointTable> MapPointTable { get; set; }
    }
}
