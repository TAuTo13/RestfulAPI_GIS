using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WebMapAPI_ASP.Models;
using WebMapAPI_ASP.Data;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Cors;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebMapAPI_ASP.Controllers
{
    //ルートを固定してRESTにする
    [Route("api/MapPoint")]
    [ApiController]
    [EnableCors("CorsPolicy")]
    public class APIController : ControllerBase
    {
        private readonly MapPointTableContext context;//データベースのContext

        public APIController(MapPointTableContext context)
        {
            this.context = context;//Contextをセット
        }

        /*GETの処理*/
        [HttpGet("{id}")]
        public async Task<ActionResult<MapPoint>> Get(int id) {
            //データベースからレコードを取得
            var mapT = await context.MapPointTable.FindAsync(id);

            if (mapT == null)
                return NotFound();

            //データモデルを変更
            var mapPoint = Translate(mapT);

            return mapPoint;
        }

        /*GET(GETALL)の処理*/
        [HttpGet]
        public async Task<ActionResult<GeoFutures>> GetAll()
        {
            //データベースからレコードを全て取得
            var mapTList = await context.MapPointTable.ToListAsync();
            //GETAll用のモデルにセット
            var geoFutures = GetGeoFutures(mapTList);
            if (geoFutures == null)
                return NotFound();

            return geoFutures;
        }

        /*POSTの処理*/
        [HttpPost]
        public async Task<IActionResult> Create(MapPoint mapPoint)
        {
            if (mapPoint == null)
                return BadRequest();

            //モデルを変更
            MapPointTable mapT = Translate(mapPoint);

            //データベースにレコードを登録
            context.MapPointTable.Add(mapT);
            await context.SaveChangesAsync();

            return CreatedAtAction(nameof(Create),new { id = mapPoint.properties.id },mapPoint);
        }

        /*UPDATE(PUT)の処理*/
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id,UpdateProperties update)
        {
            //PUT処理用にモデルを作成
            var mapPoint = new MapPoint()
            {
                type = "Feature",
                geometry = new GeoPoint()
                {
                    type = "Point",
                    coordinates = update.coordinates
                },
                properties = new Properties()
                {
                    id = id,
                    name = update.name,
                    address = update.address,
                    phone = update.phone,
                    url = update.url
                }
            };
            var mapT = Translate(mapPoint);

            //データベース内のレコードを更新
            context.Entry(mapT).State = EntityState.Modified;

            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PointExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        /*DELETEの処理*/
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            //idでレコードを取得
            var mapT = await context.MapPointTable.FindAsync(id);
            if (mapT == null)
                return NotFound();

            //取得したレコードを削除
            context.MapPointTable.Remove(mapT);
            await context.SaveChangesAsync();

            return NoContent();
        }


        /*データモデルの読み替え用メソッド*/
        #region Translate
        /*MapPointTableをMapPointに読み替える*/
        private MapPoint Translate(MapPointTable mapT)
        {
            return new MapPoint
            {
                type = "Feature",
                geometry = new GeoPoint()
                {
                    type = "Point",
                    coordinates = new double[] {
                        mapT.Point.X,mapT.Point.Y
                    }

                },
                properties = new Properties()
                {
                    id = mapT.Id,
                    name = mapT.Name,
                    address = mapT.Address,
                    phone = mapT.Phone,
                    url = mapT.Url
                }
            };
        }

        /*MapPointをMapPointTableに読み替える*/
        private MapPointTable Translate(MapPoint mapPoint)
        {
            var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4612);
            var x = mapPoint.geometry.coordinates[0];
            var y = mapPoint.geometry.coordinates[1];

            return new MapPointTable()
            {
                Id = mapPoint.properties.id,
                Point = geometryFactory.CreatePoint(new Coordinate(x, y)),
                Name = mapPoint.properties.name,
                Address = mapPoint.properties.address,
                Phone = mapPoint.properties.phone,
                Url = mapPoint.properties.url
            };
        }

        #endregion

        //MapPointをGETAll用モデルに格納するメソッド
        private GeoFutures GetGeoFutures(List<MapPointTable> mapTList)
        {
            if (mapTList == null)
                return null;

            var features = new List<MapPoint>();
            foreach (var mapT in mapTList)
            {
                features.Add(Translate(mapT));
            }

            return new GeoFutures()
            {
                type = "FeatureCollection",
                features = features.ToArray()
            };
        }

        /*指定したidのレコードがデータベース内に存在するか判定するメソッド*/
        private bool PointExists(int id)
        {
            return context.MapPointTable.Any(e => e.Id == id);
        }


    }
}
