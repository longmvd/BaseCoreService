using BaseCoreService.Entities.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseCoreService.HTTP
{
    public interface IHttpRequest
    {
        string GetHeader(string name);

        Task<ServiceResponse> GetAsync<T>(string url);

        Task<ServiceResponse> GetAsync<T>(string url, Dictionary<string, string> headers = null);

        Task<ServiceResponse> PostAsync<T>(string url, HttpContent requestContent);
        Task<ServiceResponse> PutAsync<T>();
        Task<ServiceResponse> DeleteAsync<T>(HttpRequestMessage httpRequestMessage);
        Task<ServiceResponse> DeleteAsync<T>(string url, string content, string mediatype = "application/json", Dictionary<string, string> headers = null);
        Task<ServiceResponse> PatchAsync<T>();


    }
}
