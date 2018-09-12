
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Net;
using System;

namespace OHC2
{
    public static class CreatingRating
    {
        [FunctionName("CreatingRating")]
        public static IActionResult Run([HttpTrigger(AuthorizationLevel.Function,"post", Route = null)]HttpRequest req,
            [CosmosDB(
                databaseName: "BFYOC",
                collectionName: "RATINGS",
                ConnectionStringSetting = "CosmosDBConnection")]
                IAsyncCollector<Rating> ratingOut,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            try
            {
                string requestBody = new StreamReader(req.Body).ReadToEnd();
                dynamic data = JsonConvert.DeserializeObject(requestBody);

                string userId = data?.userId;
                string productId = data?.productId;
                string locationName = data?.locationName;
                int rating = data?.rating;
                string userNotes = data?.userNotes;

                bool userValidation = ValidationHelper.ValidateId(userId, ValidationType.UserId);
                bool productValidation = ValidationHelper.ValidateId(productId, ValidationType.ProductId);

                bool validationSuccess = userValidation && productValidation ;

                Rating newRating = null;
                if (validationSuccess)
                {
                    newRating = new Rating
                    {
                        id = Guid.NewGuid().ToString(),
                        userId = userId,
                        productId = productId,
                        timestamp = DateTime.Now,
                        locationName = locationName,
                        rating = rating,
                        userNotes = userNotes
                    };
                    ratingOut.AddAsync(newRating);
                    return (ActionResult)new OkObjectResult(JsonConvert.SerializeObject(newRating, Formatting.Indented));
                }
                else
                {
                    string errorMessage = string.Empty;
                    if (!userValidation && !productValidation)
                    {
                        errorMessage = "Both userId and productId entered do not exist in database.";
                    }
                    else if (!userValidation)
                    {
                        errorMessage = "userId does not exists in database.";
                    }
                    else
                    {
                        errorMessage = "productId does not exists in database.";
                    }
                    return new BadRequestObjectResult(errorMessage);
                }
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
