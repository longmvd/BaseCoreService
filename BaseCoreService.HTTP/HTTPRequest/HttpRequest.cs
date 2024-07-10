using BaseCoreService.Entities.DTO;
using BaseCoreService.Entities.Extension;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseCoreService.HTTP
{
    public class HttpRequest : IHttpRequest
    {
        private string _url = string.Empty;

        private HttpClient _httpClient => new HttpClient();

        public HttpRequest() { }


        public string Url { get { return _url; } set { _url = value; } }

        public string GetHeader(string name)
        {
            throw new NotImplementedException();
        }

        public async Task<ServiceResponse> GetAsync<T>(string url = "")
        {
            var response = new ServiceResponse();
            try
            {
                var res = await _httpClient.GetAsync(string.IsNullOrEmpty(_url) ? url : _url);
                if (res != null)
                {
                    var content = await res.Content.ReadAsStringAsync();
                    if (res.StatusCode == System.Net.HttpStatusCode.OK || res.StatusCode == System.Net.HttpStatusCode.Accepted
                        || res.StatusCode == System.Net.HttpStatusCode.Created)
                    {
                        var responseContent = JsonConvert.DeserializeObject<T>(content);

                        response.OnSuccess(responseContent);
                    }
                    else
                    {
                        response.OnError(new ErrorResponse()
                        {
                            ErrorCode = (int)res.StatusCode,
                            Data = content
                        });
                    }

                }
                else
                {
                    response.OnError(new ErrorResponse());
                }

            }catch (Exception ex)
            {
                response.OnException(new ExceptionResponse() { Data = JsonConvert.SerializeObject(ex), ExceptionMessage = ex.Message });
            }
            return response;
        }

        public async Task<ServiceResponse> GetAsync<T>(string url, Dictionary<string, string> headers = null)
        {
            var requestMessage = new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(url)

            };
            if (headers != null)
            {
                foreach (var key in headers.Keys)
                {
                    requestMessage.Headers.Add(key, headers[key]);
                }
            }
            return await this.SendAsync<T>(requestMessage);
        }

        public async Task<ServiceResponse> PostAsync<T>(string url, HttpContent requestContent)
        {
            var response = new ServiceResponse();
            var res = await _httpClient.PostAsync(url, requestContent);
            if (res != null)
            {
                var content = await res.Content.ReadAsStringAsync();
                if (res.StatusCode == System.Net.HttpStatusCode.OK || res.StatusCode == System.Net.HttpStatusCode.Accepted
                    || res.StatusCode == System.Net.HttpStatusCode.Created)
                {
                    var responseContent = JsonConvert.DeserializeObject<T>(content);

                    response.OnSuccess(responseContent);
                }
                else
                {
                    response.OnError(new ErrorResponse()
                    {
                        ErrorCode = (int)res.StatusCode,
                        Data = content
                    });
                }

            }
            else
            {
                response.OnError(new ErrorResponse());
            }
            return response;
        }

        public async Task<ServiceResponse> PutAsync<T>()
        {
            throw new NotImplementedException();
        }

        public async Task<ServiceResponse> SendAsync<T>(HttpRequestMessage httpRequest)
        {
            var response = new ServiceResponse();
            var res = await _httpClient.SendAsync(httpRequest);
            if (res != null)
            {
                var content = await res.Content.ReadAsStringAsync();
                if (res.StatusCode == System.Net.HttpStatusCode.OK || res.StatusCode == System.Net.HttpStatusCode.Accepted
                    || res.StatusCode == System.Net.HttpStatusCode.Created)
                {
                    var responseContent = JsonConvert.DeserializeObject<T>(content);

                    response.OnSuccess(responseContent);
                }
                else
                {
                    response.OnError(new ErrorResponse()
                    {
                        ErrorCode = (int)res.StatusCode,
                        Data = content
                    });
                }

            }
            else
            {
                response.OnError(new ErrorResponse());
            }
            return response;
        }

        public async Task<ServiceResponse> DeleteAsync<T>(string url, string content, string mediatype = "application/json", Dictionary<string, string> headers = null)
        {
            var requestMessage = new HttpRequestMessage()
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri(url),
                Content = new StringContent(content, Encoding.UTF8, mediatype),
                
            };
            if(headers != null)
            {
                foreach( var key in headers.Keys)
                {
                    requestMessage.Headers.Add(key, headers[key]);
                }
            }
            

            return await this.SendAsync<T>(requestMessage);
        }

        public async Task<ServiceResponse> DeleteAsync<T>(HttpRequestMessage httpRequestMessage)
        {
            httpRequestMessage.Method = HttpMethod.Delete;
            var response = await this.SendAsync<T>(httpRequestMessage);
            return response;
        }

        public Task<ServiceResponse> PatchAsync<T>()
        {
            throw new NotImplementedException();
        }
    }
}
