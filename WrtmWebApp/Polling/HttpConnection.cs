using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Net.Http;
using System.Net;

namespace WrtmWebApp.Polling
{
     public class HttpConnection
    {
        private HttpClient client;
        private CookieContainer cookies;
        private HttpClientHandler handler;
        private Uri baseuri;

        public bool WasError = false;

        public HttpConnection(string addr, int timeout)
        {
            baseuri = new Uri("https://" + addr);
            cookies = new CookieContainer();
            handler = new HttpClientHandler() { CookieContainer = cookies };
            handler.ClientCertificateOptions = ClientCertificateOption.Manual;
            handler.ServerCertificateCustomValidationCallback =
                (httpRequestMessage, cert, cetChain, policyErrors) =>
                {
                    return true;
                };
            client = new HttpClient(handler) { BaseAddress = baseuri };
            client.Timeout = TimeSpan.FromSeconds(timeout);
        }

        public string Get(string rsrc)
        {
            string data = "";
            try
            {
                var resp = client.GetAsync(rsrc).Result;
                if (resp.IsSuccessStatusCode)
                {
                    var respContent = resp.Content;
                    data = respContent.ReadAsStringAsync().Result;
                    WasError = false;
                }
            }
            catch
            {
                WasError = true;
            }
            return data;
        }

        public string Post(string rsrc, string postdata)
        {
            string data = "";
            try
            {
                var content = new StringContent(postdata, Encoding.UTF8, "application/json");
                var resp = client.PostAsync(rsrc, content).Result;
                if (resp.IsSuccessStatusCode)
                {
                    var respContent = resp.Content;
                    data = respContent.ReadAsStringAsync().Result;
                    WasError = false;
                }
            }
            catch
            {
                WasError = true;
            }
            return data;
        }

        public void TokenSet(string token)
        {
            cookies.Add(baseuri, new Cookie("token", token));
        }
    }
}
