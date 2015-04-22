using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Web.Http;
using Newtonsoft.Json;
using TTRider.DynamicDataRecord;

namespace Demo.Controllers
{
    public class ValuesController : ApiController
    {
        public HttpResponseMessage Get()
        {
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new PushStreamContent(async (stream, content, context) =>
                {
                    var textWriter = new StreamWriter(stream);
                    var writer = new JsonTextWriter(textWriter);

                    var sc = WebConfigurationManager.ConnectionStrings["demo"].ConnectionString;

                    var connection = new SqlConnection(sc);
                    await connection.OpenAsync();

                    var cmd = connection.CreateCommand();
                    cmd.CommandText = "SELECT * FROM dbo.[Values]";

                    writer.WriteStartArray();

                    var serializer = JsonSerializer.Create();

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            var binder = DataRecordFactory.GetRecordBinder("ValuesRecord", reader);
                            do
                            {
                                var record = binder.Bind(reader);
                                serializer.Serialize(writer, record);
                            } while (await reader.ReadAsync());
                        }
                    }
                    writer.WriteEndArray();
                    writer.Close();
                    textWriter.Close();
                })
            };
        }


        //public async Task<object> Get()
        //{
        //    var sc = WebConfigurationManager.ConnectionStrings["demo"].ConnectionString;

        //    var connection = new SqlConnection(sc);
        //    await connection.OpenAsync();

        //    var cmd = connection.CreateCommand();
        //    cmd.CommandText = "SELECT * FROM dbo.[Values]";

        //    using (var reader = await cmd.ExecuteReaderAsync())
        //    {
        //        if (await reader.ReadAsync())
        //        {
        //            var binder = DataRecordFactory.GetRecordBinder("ValuesRecord", reader);
        //            var list = binder.CreateCollection();

        //            do
        //            {
        //                var record = binder.Bind(reader);
        //                list.Add(record);

        //            } while (await reader.ReadAsync());
        //            return list;
        //        }
        //    }
        //    return null;
        //}
    }
}
