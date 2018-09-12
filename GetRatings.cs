
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System;

namespace OHC2
{
    public static class GetRatings
    {

        [FunctionName("GetRatings")]
        public static IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)]HttpRequest req,
            [CosmosDB(
                databaseName: "BFYOC",
                collectionName: "RATINGS",
                ConnectionStringSetting = "CosmosDBConnection")] IEnumerable<Rating> ratings,
            TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            try
            {
                string userId = req.Query["userId"];

                string requestBody = new StreamReader(req.Body).ReadToEnd();
                dynamic data = JsonConvert.DeserializeObject(requestBody);
                userId = userId ?? data?.userId;
                List<Rating> filteredRatings = null;
                if (userId != null)
                {
                    bool result = ValidationHelper.ValidateId(userId, ValidationType.UserId);
                    if (result)
                    {
                        filteredRatings = ratings.Where(x => string.Compare(x.userId, userId) == 0).ToList();
                    }
                    else
                    {
                        return new NotFoundObjectResult("userId entered does not exist in the database");
                    }
                }
                return filteredRatings != null && filteredRatings.Count > 0
                    ? (ActionResult)new OkObjectResult(JsonConvert.SerializeObject(filteredRatings, Formatting.Indented))
                    : new NotFoundObjectResult("No matching ratings found");
            }
            catch (Exception ex)
            {
                var msg = $"Error occured:{Environment.NewLine}{ex.Message}{Environment.NewLine}{ex.StackTrace}";
                if (ex.InnerException != null)
                {
                    msg += $"{Environment.NewLine}{ex.InnerException.Message}{Environment.NewLine}{ex.InnerException.StackTrace}";
                }
                return new BadRequestObjectResult(msg);
            }
           
        }
    }
}
