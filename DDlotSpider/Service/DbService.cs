using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using CentaLine.Common;
using CentaLine.DataCommon;
using System.Data.SqlClient;
using Dapper;
using DDlotSpider.Entity;

namespace DDlotSpider.Service
{
    public static class DbService
    {
        public static DataRow GetOrder()
        {

            var sql = "select top 1 * from " + AppSettings.TableSearch + " where status is null or status=0";

            return DbUtility.GetDataRow(sql.ToString(), AppSettings.ConnStr);
        }

        public static bool UpdateOrder(OrderItem item)
        {
            var sql = "update " + AppSettings.TableSearch + " set status=@Status,createdate=getdate(),machine=@Machine where prn=@prn";

            var param = new List<SqlParameter>() {

                new SqlParameter{ ParameterName="@prn",Value=item.Prn},
                  new SqlParameter{ ParameterName="@Machine",Value=item.Machine},
                new SqlParameter{ ParameterName="@Status",Value=ConvertUtility.ToInt(item.Status)}
            };

            return DbUtility.ExecuteNonQuery(sql, AppSettings.ConnStr, param.ToArray()) > 0;
        }

        public static int GetCount()
        {

            var sql = "select count(1) from " + AppSettings.TableSearch;
            var row = DbUtility.GetDataRow(sql, AppSettings.ConnStr);
            return ConvertUtility.ToInt(row[0]);
        }

        public static int GetHandlerCount()
        {

            var sql = "select count(1) from " + AppSettings.TableSearch + " where status =1 or status=-1";
            var row = DbUtility.GetDataRow(sql, AppSettings.ConnStr);
            return ConvertUtility.ToInt(row[0]);
        }

        public static bool Save2Db(OrderItem item)
        {
            var sql = @"INSERT INTO {0}
                        ([PRN]
                       ,[Keyword]
                       ,[FieldName]
                       ,[Block]
                       ,[Lot]
                       ,[BlockDesc]
                       ,[Floor]
                       ,[Flat]
                       ,[AddressLot]
                       ,[IsClosed]
                       ,[CreateDate])
                 VALUES
                       (@PRN
                       ,@Keyword
                       ,@FieldName
                       ,@Block
                       ,@Lot
                       ,@BlockDesc
                       ,@Floor
                       ,@Flat
                       ,@AddressLot
                       ,@IsClosed
                       ,@CreateDate)";

          

            sql = string.Format(sql, AppSettings.TableResult);

            var conn = new SqlConnection(AppSettings.ConnStr);

            return conn.Execute(sql, item) > 0;
        }
    }
}
