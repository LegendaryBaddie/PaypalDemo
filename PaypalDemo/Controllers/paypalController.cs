using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace PaypalDemo.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class paypalController : ControllerBase
    {
        private async Task<HttpContent> getAccessCodeAsync()
        {
            //this function calls the paypal api and returns a client token which represents the start of a transaction (who the money will go to)
            using (var httpClient = new HttpClient())
            {
                // create a new post request to the paypal api, it is a different api when using a live environment vs. a dev env
                using (var request = new HttpRequestMessage(new HttpMethod("POST"), "https://api.sandbox.paypal.com/v1/oauth2/token"))
                {   //required api headers
                    request.Headers.TryAddWithoutValidation("Accept", "application/json");
                    request.Headers.TryAddWithoutValidation("Accept-Language", "en_US");

                    //the values are your paypal developer client id and your secret ("CLIENT_ID:SECRET")

                    var base64authorization = Convert.ToBase64String(Encoding.ASCII.GetBytes("AVZP57SHtLTwirsERs8Slow50abfU2IoU6WnQbFQc_Mqni9UX_W11LHwnTVZZlnZX1fLXv9V46TZlyVt:EBW_wal0WXa7oRmHHDTWo-mBXLny4m_yXTO1YzuVJhDyhGZt5FYCHTCgRMpT0pZg4-QTr_V9_6VpADku"));
                    // more headers
                    request.Headers.TryAddWithoutValidation("Authorization", $"Basic {base64authorization}");
                    // define the post content
                    request.Content = new StringContent("grant_type=client_credentials");

                    request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded");
                    //actually make the http call and await a response
                    var response = await httpClient.SendAsync(request);
                    return response.Content;
                }

            }
        }
        private async Task<HttpContent> getClientTokenAsync(string accessToken)
        {
            using (var httpClient = new HttpClient())
            {
                using (var request = new HttpRequestMessage(new HttpMethod("POST"), "https://api-m.sandbox.paypal.com/v1/identity/generate-token"))
                {
                    request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {accessToken}");
                    request.Headers.TryAddWithoutValidation("Accept-Language", "en_US");

                    var response = await httpClient.SendAsync(request);
                    return response.Content;
                }
            }
        }

        private async Task<HttpContent> createOrder(int cost, string accessToken)
        {
            using (var httpClient = new HttpClient())
            {
                using (var request = new HttpRequestMessage(new HttpMethod("POST"), "https://api.sandbox.paypal.com/v2/checkout/orders"))
                {
                    request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {accessToken}");

                    request.Content = new StringContent("{\n  \"intent\": \"CAPTURE\",\n  \"purchase_units\": [\n    {\n      \"amount\": {\n        \"currency_code\": \"USD\",\n        \"value\": \"" + cost.ToString() + "\"\n      }\n    }\n  ]\n}");
                    request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

                    var response = await httpClient.SendAsync(request);
                    return response.Content;
                }
            }
        }
        private async Task<HttpContent> finalizeOrder(string id, string token)
        {

            using (var httpClient = new HttpClient())
            {
                using (var request = new HttpRequestMessage(new HttpMethod("POST"), $"https://api.sandbox.paypal.com/v2/checkout/orders/{id}/capture"))
                {
                    request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {token}");
                    request.Content = new StringContent("");
                    request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                    var response = await httpClient.SendAsync(request);
                    return response.Content;
                }
            }
        }

        [HttpGet("{cost}")]
        public async Task<string> startOrder(int cost)
        {
            var response = await getAccessCodeAsync();
            // return the response content as a string
            var s_response = await response.ReadAsStringAsync();
            dynamic j_obj = JObject.Parse(s_response);
            string a_token = j_obj.access_token;
            var orderId = await createOrder(cost, a_token);
            var s_orderId = await orderId.ReadAsStringAsync();
            dynamic id_obj = JObject.Parse(s_orderId);
            string a_id = id_obj.id;

            var ctoken = await getClientTokenAsync(a_token);
            var s_ctoken = await ctoken.ReadAsStringAsync();
            string formatted = $"{{\"token\":{s_ctoken},\"orderId\":\"{a_id}\"}}";

            return formatted;
        }
        [HttpGet("/finalize/{id}")]
        public async Task<string> finalize(string id)
        {
            var response = await getAccessCodeAsync();
            // return the response content as a string
            var s_response = await response.ReadAsStringAsync();
            dynamic j_obj = JObject.Parse(s_response);
            string a_token = j_obj.access_token;
            var resp = await finalizeOrder(id, a_token);
            var s_resp = await resp.ReadAsStringAsync();
            return s_resp;
        }

    }
}
