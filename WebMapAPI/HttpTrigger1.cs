using System.Net;
using System;
using System.IO;
using System.Threading.Tasks;
using Npgsql;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;
using System.Collections.Generic;
using WebMapAPI.Models;

namespace WebMapAPI.Functions
{
    public static class HttpTrigger1
    {
        //データベースへの接続文字列
        private static string connectionString = "Server=localhost;Port=5432;Database=postgres;User ID=postgres;Password=postgres;";
        //データベース上のテーブル名
        private static string tableName = "mappointtable";

        /*POSTの処理*/
        [FunctionName("CreatePoint")]
        public static async Task<IActionResult> CreatePoint(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "mapPoint")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Creating a new Point.");

            //Jsonのデシリアライズ
            var settings = new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Ignore
            };
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<MapPoint>(requestBody,settings);

            //クエリの作成
            var query = new StringBuilder();
            query.Append($"INSERT INTO {tableName} VALUES(");
            query.Append(data.properties.id + ",");
            query.Append($"ST_GeomFromText('POINT({data.geometry.coordinates[0]} {data.geometry.coordinates[1]})'),");
            query.Append($"'{data.properties.name}',");
            query.Append($"'{data.properties.address}',");
            query.Append($"'{data.properties.phone}',");
            query.Append($"'{data.properties.url}'");
            query.Append(");");

            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    //コネクションの開始
                    connection.Open();

                    using (var command = new NpgsqlCommand())
                    {
                        var transaction = connection.BeginTransaction();
                        try
                        {
                            //コマンドのセット
                            command.Connection = connection;
                            command.CommandText = query.ToString();

                            //コマンドの送信
                            var result = command.ExecuteNonQuery();

                            if (result != 1)
                            {
                                transaction.Rollback();
                                return new BadRequestResult();
                            }
                            transaction.Commit();
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                            transaction.Rollback();
                        }
                        finally
                        {
                            transaction.Dispose();
                        }
                    }
                }
            } catch (Exception e) {
                Console.WriteLine(e.Message);
                return new BadRequestResult();
            }

            return new OkObjectResult(HttpStatusCode.OK);
        }

        /*PUTの処理*/
        [FunctionName("UpdatePoint")]
        public static async Task<IActionResult> UpdatePoint(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = "mapPoint/{id}")] HttpRequest req,
            ILogger log,string id)
        {
            log.LogInformation("Updating a Point.");

            //Jsonのデシリアライズ
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<UpdateProperties>(requestBody);

            //クエリの作成
            var query = new StringBuilder();
            query.Append($"UPDATE {tableName} SET ");
            query.Append($"geom=ST_GeomFromText('POINT({data.coordinates[0]} {data.coordinates[1]})')");
            query.Append($",name='{data.name}'");
            query.Append($",address='{data.address}'");
            query.Append($",phone='{data.phone}'");
            query.Append($",url='{data.url}'");
            query.Append($" WHERE geoid={id};");

            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    //コネクションの開始
                    connection.Open();

                    using (var command = new NpgsqlCommand())
                    {
                        var transaction = connection.BeginTransaction();
                        try
                        {
                            //コマンドをセット
                            command.Connection = connection;
                            command.CommandText = query.ToString();

                            //コマンドを送信
                            var result = command.ExecuteNonQuery();

                            if (result != 1)
                            {
                                transaction.Rollback();
                                return new BadRequestResult();
                            }
                            transaction.Commit();
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                            transaction.Rollback();
                        }
                        finally
                        {
                            transaction.Dispose();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return new BadRequestResult();

            }

            return new OkObjectResult(HttpStatusCode.OK);
        }


        /*PATCHの処理*/
        [FunctionName("UpdatePartialPoint")]
        public static async Task<IActionResult> UpdatePartialPoint(
            [HttpTrigger(AuthorizationLevel.Function, "patch", Route = "mapPoint/{id}")] HttpRequest req,
            ILogger log, string id)
        {
            log.LogInformation("Updating a part of Point.");

            //Jsonのデシリアライズ
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var options = new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };
            var data = JsonConvert.DeserializeObject<UpdateProperties>(requestBody,options);

            //クエリの作成，nullになっている要素を判定して，nullでないもののみクエリに加えて更新を行う
            bool hasVal = false;
            var query = new StringBuilder();
            query.Append($"UPDATE {tableName} SET ");
            if (data.coordinates[0].HasValue)
            {
                query.Append($"geom=ST_GeomFromText('POINT({data.coordinates[0]} {data.coordinates[1]})')");
                hasVal = true;
            }
            if (!string.IsNullOrEmpty(data.name))
            {
                if (hasVal)
                    query.Append(",");
                else
                    hasVal = true;

                query.Append($"name='{data.name}'");
            }
            if (!string.IsNullOrEmpty(data.address))
            {
                if (hasVal)
                    query.Append(",");
                else
                    hasVal = true;

                query.Append($"address='{data.address}'");
            }
            if (!string.IsNullOrEmpty(data.phone))
            {
                if (hasVal)
                    query.Append(",");
                else
                    hasVal = true;

                query.Append($"phone='{data.phone}'");
            }
            if (!string.IsNullOrEmpty(data.url))
            {
                if (hasVal)
                    query.Append(",");
                else
                    hasVal = true;

                query.Append($"url='{data.url}'");
            }
            query.Append($" WHERE geoid={id};");

            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    //コネクションの開始
                    connection.Open();

                    using (var command = new NpgsqlCommand())
                    {
                        var transaction = connection.BeginTransaction();
                        try
                        {
                            //コマンドのセット
                            command.Connection = connection;
                            command.CommandText = query.ToString();

                            //コマンドの送信
                            var result = command.ExecuteNonQuery();

                            if (result != 1)
                            {
                                transaction.Rollback();
                                return new BadRequestResult();
                            }
                            transaction.Commit();
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                            transaction.Rollback();
                        }
                        finally
                        {
                            transaction.Dispose();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return new BadRequestResult();

            }

            return new OkObjectResult(HttpStatusCode.OK);
        }


        /*GET{id}の処理*/
        [FunctionName("GetPoint")]
        public static IActionResult GetPoint(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "mapPoint/{id}")] HttpRequest req,
            ILogger log, string id)
        {
            log.LogInformation("Getting a Point.");

            string data = string.Empty;
            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    //sqlconnection open
                    connection.Open();

                    using (var command = new NpgsqlCommand())
                    {
                        try
                        {
                            //コマンドの作成
                            command.Connection = connection;
                            command.CommandText = $"select geoid , ST_X(geom) as p_x , ST_Y(geom) as p_y,name,address,phone,url from {tableName} where geoid={id};";

                            //コマンドの送信
                            var dr = command.ExecuteReader();

                            //返ってきたレコードをモデルに格納
                            if (dr.HasRows)
                            {
                                dr.Read();
                                var mapPoint = new MapPoint()
                                {
                                    type = "Feature",
                                    geometry = new GeoPoint()
                                    {
                                        type = "Point",
                                        coordinates = new double[] { (double)dr["p_x"], (double)dr["p_y"] }
                                    },
                                    properties = new Properties
                                    {
                                        id = (int)dr["geoid"],
                                        name = (string)dr["name"],
                                        address = (string)dr["address"],
                                        //nullが許容されているカラムはnull判定を行う
                                        phone = dr["phone"] is DBNull ? string.Empty : (string)dr["phone"],
                                        url = dr["url"] is DBNull ? string.Empty : (string)dr["url"]
                                    }
                                };
                                //取得したレコードをjson形式にする
                                data = JsonConvert.SerializeObject(mapPoint);
                            }else{
                                return new NotFoundResult();
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                            return new BadRequestResult();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return new BadRequestResult();
            }

            return new OkObjectResult(data);
        }


        /*GETの処理*/
        [FunctionName("GetPoints")]
        public static IActionResult GetPoints(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "mapPoint")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Getting a Points.");

            string data = string.Empty;
            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    //sqlconnection open
                    connection.Open();

                    using (var command = new NpgsqlCommand())
                    {
                        try
                        {
                            //コマンドの作成
                            command.Connection = connection;
                            command.CommandText = $"select geoid,ST_X(geom) as p_x,ST_Y(geom) as p_y,name,address,phone,url from {tableName};";

                            //コマンドの送信
                            var dr = command.ExecuteReader();

                            //返ってきたレコードをモデルに格納
                            var features = new List<MapPoint>(); 
                            while (dr.Read())
                            {
                                features.Add(
                                    new MapPoint()
                                    {
                                        type = "Feature",
                                        geometry = new GeoPoint()
                                        {
                                            type = "Point",
                                            coordinates = new double[] { (double)dr["p_x"], (double)dr["p_y"] }
                                        },
                                        properties = new Properties
                                        {
                                            id = (int)dr["geoid"],
                                            name = (string)dr["name"],
                                            address = (string)dr["address"],
                                            //nullが許容されているカラムはnull判定を行う
                                            phone = dr["phone"] is DBNull ? string.Empty:(string)dr["phone"],
                                            url = dr["url"] is DBNull ? string.Empty : (string)dr["url"]
                                        }
                                    }
                                );
                            }

                            //GETAll用のモデルに格納
                            var geoFutures = new GeoFutures()
                            {
                                type = "FeatureCollection",
                                features = features.ToArray()
                            };

                            //取得したレコードをJson形式にする
                            data = JsonConvert.SerializeObject(geoFutures);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                            return new BadRequestResult();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return new BadRequestResult();
            }

            return new OkObjectResult(data);
        }

        /*DELETEの処理*/
        [FunctionName("DeletePoint")]
        public static IActionResult DeletePoint(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "mapPoint/{id}")] HttpRequest req,
            ILogger log,string id)
        {
            log.LogInformation("delete a Point.");

            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    //コネクションの開始
                    connection.Open();
                    var transaction = connection.BeginTransaction();
                    using (var command = new NpgsqlCommand())
                    {
                        try
                        {
                            //コマンドのセット
                            command.Connection = connection;
                            command.CommandText = $"delete from {tableName} where geoid={id};";

                            //コマンドの送信
                            var result = command.ExecuteNonQuery();
                            if (result != 1) {
                                transaction.Rollback();
                                return new BadRequestResult();
                            }
                            transaction.Commit();
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                            transaction.Rollback();
                        }
                        finally
                        {
                            transaction.Dispose();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return new BadRequestResult();
            }

            return new OkObjectResult(HttpStatusCode.OK);
        }
    }
}
