using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nexum_WebApp_Class_Library.Services
{
    public abstract class BaseService
    {
        public readonly HttpClient _httpClient;
        public readonly IConfiguration _config;
        public BaseService(IConfiguration config, HttpClient httpClient)
        {
            _config = config;
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(_config["ApiSettings:ApiBaseUrl"]);
        }
    }
}
