using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.IO;
using Newtonsoft.Json;

namespace BBREST
{
    public class RestApp
    {
        string bbOrigin = null;
        string bbAccessToken = null;
        string bbAuth = null;
        HttpClient client = new HttpClient();

        public RestApp(string origin, string bbKey, string bbSecret)
        {
            if (!origin.EndsWith("/")) origin += "/";
            bbOrigin = origin + "learn/api/public/";
            bbAuth = "Basic " + Base64Encode(bbKey + ":" + bbSecret);
        }

        private string Base64Encode(string plainText)
        {
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        public async Task<BlackboardResponse> POST(string url, dynamic json = null) => await Request("POST", url, json);
        public async Task<BlackboardResponse> PATCH(string url, dynamic json = null) => await Request("PATCH", url, json);
        public async Task<BlackboardResponse> PUT(string url, dynamic json = null) => await Request("PUT", url, json);
        public async Task<BlackboardResponse> DELETE(string url, dynamic json = null) => await Request("DELETE", url, json);
        public async Task<BlackboardResponse> GET(string url, dynamic json = null) => await Request("GET", url, json);

        public async Task<BlackboardResponse> Request(string method, string url, object jsonObject = null, bool hasFailed = false)
        {
            return await Request(method, url, jsonObject ?? "{}");
        }

        public async Task<BlackboardResponse> Request(string method, string url, string jsonString, bool hasFailedOnce = false)
        {
            if (string.IsNullOrEmpty(bbAccessToken)) await SetToken();
            
            var requestMessage = new HttpRequestMessage
            {
                Method = new HttpMethod(method.ToUpper()),
                RequestUri = new Uri(bbOrigin + url),
                Headers = {{ "Authorization", "Bearer " + bbAccessToken}},
            };

            if (method.ToUpper() != "GET") requestMessage.Content = new StringContent(jsonString, Encoding.UTF8, "application/json");
            var response = client.SendAsync(requestMessage).Result;
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                bbAccessToken = null;
                if (!hasFailedOnce) return await Request(method, url, jsonString, true);
                else throw new Exception("Unable to retrieve access token.");
            }
            return new BlackboardResponse(response);
        }
        
        private async Task SetToken()
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

    public class BlackboardResponse
    {
        public HttpResponseMessage Response;
        public HttpContent Content { get => Response.Content; }

        public BlackboardResponse(HttpResponseMessage responseMessage) => Response = responseMessage;

        public async Task<string> ReadContentAsync()
        {
            using (var reader = new StreamReader(await Content.ReadAsStreamAsync()))
            {
                return await reader.ReadToEndAsync();
            }
        }
        
    }
}
