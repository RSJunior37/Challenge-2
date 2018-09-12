using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace OHC2
{
    public static class ValidationHelper
    {
        public static bool ValidateId(string id, ValidationType type)
        {
            if (id != null)
            {
                string queryUrl = string.Empty;
                switch(type)
                {
                    case ValidationType.UserId:
                        queryUrl = $"{CommonString.getUserByIdUrl}?userId={id}";
                        break;
                    case ValidationType.ProductId:
                        queryUrl = $"{CommonString.getProductByIdUrl}?productId={id}";
                        break;
                    default:
                        break;
                }
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(queryUrl);
                webRequest.AllowAutoRedirect = false;
                HttpWebResponse response = null;
                try
                {
                    response = (HttpWebResponse)webRequest.GetResponse();
                }
                catch (WebException e)
                {
                    HttpWebResponse httpResponse = (HttpWebResponse)e.Response;
                    if (httpResponse.StatusCode != HttpStatusCode.BadRequest)
                    {
                        throw;
                    }
                    else
                    {
                        return false;
                    }

                }
                return true;
            }
            return false;
        }
    }
    public enum ValidationType
    {
        UserId, ProductId
    };

}
