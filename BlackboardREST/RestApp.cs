using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.IO;
using Newtonsoft.Json;

namespace BlackboardREST
{
    public class RestApp
    {
        string bbOrigin = null;
        string bbKey = null;
        string bbSecret = null;
        string bbAccessToken = null;
        string bbAuth = null;
        HttpClient client = new HttpClient();

        public RestApp(string domain, string pkey, string psecret)
        {
            bbOrigin = domain + "/learn/api/public/";
            bbKey = pkey;
            bbSecret = psecret;
            bbAuth = "Basic " + Base64Encode(bbKey + ":" + bbSecret);

            var handler = new HttpClientHandler();
            handler.Credentials = new NetworkCredential(bbKey, bbSecret);
            handler.UseDefaultCredentials = true;
        }

        private static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public async Task<HttpResponseMessage> Request(string method, string url, object jsonObject, bool hasFailed = false)
        {
            return await Request(method, url, JsonConvert.SerializeObject(jsonObject));
        }

        public async Task<HttpResponseMessage> Request(string method, string url, string jsonString, bool hasFailed = false)
        {
            if (String.IsNullOrEmpty(bbAccessToken)) await SetToken();
            
            var requestMessage = new HttpRequestMessage
            {
                Method = new HttpMethod(method.ToUpper()),
                RequestUri = new Uri(bbOrigin + url),
                Headers =
                {
                    { "Authorization", "Bearer " + bbAccessToken},
                },
            };

            if (method.ToUpper() != "GET") requestMessage.Content = new StringContent(jsonString, Encoding.UTF8, "application/json");
            var response = client.SendAsync(requestMessage).Result;
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                bbAccessToken = null;
                if (!hasFailed) return await Request(method, url, jsonString, true);
                else return response;
            } else
            {
                return response;
            }
        }
        
        public async Task SetToken()
        {
            var requestContent = new FormUrlEncodedContent(new[] {
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
            });

            var requestMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(bbOrigin + "/v1/oauth2/token"),
                Headers = { { "Authorization", bbAuth } },
                Content = requestContent
            };

            var response = client.SendAsync(requestMessage).Result;

            using (var reader = new StreamReader(await response.Content.ReadAsStreamAsync()))
            {
                dynamic o = JsonConvert.DeserializeObject(await reader.ReadToEndAsync());
                bbAccessToken = o.access_token;
            }
        }
    }
}
