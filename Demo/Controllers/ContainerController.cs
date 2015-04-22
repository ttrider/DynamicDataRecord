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
    public class ContainerController : ApiController
    {
        public async Task<DataRecordFactory.RecordsContainer> Get()
        {
            var sc = WebConfigurationManager.ConnectionStrings["demo"].ConnectionString;

            var connection = new SqlConnection(sc);
            await connection.OpenAsync();

            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM dbo.[Values]";

            using (var reader = await cmd.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                {
                    var binder = DataRecordFactory.GetRecordBinder("ValuesRecord", reader);
        
                    var container = binder.CreateContainer();

                    do
                    {
                        var record = binder.Bind(reader);
                        container.Add(record);

                    } while (await reader.ReadAsync());
                    return container;
                }
            }
            return DataRecordFactory.RecordsContainer.Empty();
        }
    }
}
