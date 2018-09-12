using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace OHC2
{
    public static class GetRating
    {
        [FunctionName("GetRating")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "bfyoc/{id}")]HttpRequestMessage req, [CosmosDB(
                databaseName: "BFYOC",
                collectionName: "RATINGS",
                ConnectionStringSetting = "CosmosDBConnection",
                SqlQuery = "select * from BFYOC r where r.id = {id}")]
                IEnumerable<Rating> rate, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            if (rate == null)
            {
                // Get request body
                dynamic data = await req.Content.ReadAsAsync<object>();
                rate = data?.name;
            }

            return rate != null
                ? req.CreateResponse(HttpStatusCode.OK, rate.FirstOrDefault())
                : req.CreateResponse(HttpStatusCode.BadRequest, "bump");
        }
    }
}