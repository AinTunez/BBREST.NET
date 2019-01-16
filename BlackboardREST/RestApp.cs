using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.IO;
using System.Dynamic;

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

        private async Task MakeRequest(string url, HttpMethod method, params string[] properties)
        {
            if (String.IsNullOrEmpty(bbAccessToken)) await SetToken();

            var content = new List<KeyValuePair<string, string>>();
            for (int i = 0; i < properties.Length; i+=2)
            {
                string property = properties[i];
                string value = properties[i + 1];
                content.Add(new KeyValuePair<string, string>(property, value));
            }

            var requestMessage = new HttpRequestMessage
            {
                Method = method,
                RequestUri = new Uri((bbOrigin + url).Replace("//","/")),
                Headers =
                {
                    { "Authorization", "Bearer " + bbAccessToken },
                    { "content-type", "application/json" },
                },
                Content = new FormUrlEncodedContent(content)
            };
            var response = client.SendAsync(requestMessage).Result;
        }
        
        private async Task SetToken()
        {
            string requestURL = bbOrigin + "/learn/api/public/v1/oauth2/token";
            var requestContent = new FormUrlEncodedContent(new[] {
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
            });

            var requestMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(bbOrigin + "/learn/api/public/v1/oauth2/token"),
                Headers = {{ "Authorization", bbAuth }}
            };

            var response = client.SendAsync(requestMessage).Result;

            using (var reader = new StreamReader(await response.Content.ReadAsStreamAsync()))
            {
                Console.WriteLine(await reader.ReadToEndAsync());
            }

            Console.ReadLine();
        }
    }
}
