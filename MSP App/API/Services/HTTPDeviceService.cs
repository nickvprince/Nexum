﻿using API.DataAccess;
using API.Services.Interfaces;
using Newtonsoft.Json;
using SharedComponents.Entities;
using SharedComponents.RequestEntities.HTTP;
using SharedComponents.ResponseEntities.HTTP;
using SharedComponents.Utilities;
using System.Text;

namespace API.Services
{
    public class HTTPDeviceService : BaseHTTPService, IHTTPDeviceService
    {
        public HTTPDeviceService(IConfiguration config, HttpClient httpClient, AppDbContext appDbContext) : base(config, httpClient, appDbContext)
        {
            //Use if tenant server api is updated with proper routes
            /*if (_httpClient.BaseAddress != null)
            {
                _httpClient.BaseAddress = new Uri(_httpClient.BaseAddress, "Device/");
            }
            else
            {
                throw new InvalidOperationException("BaseAddress is not set.");
            }*/
        }

        public async Task<bool?> ForceDeviceCheckinAsync(int tenantId, int client_id)
        {
            try
            {
                if (await InitiallizeHttpClient(tenantId))
                {
                    var content = new StringContent(JsonConvert.SerializeObject(client_id), Encoding.UTF8, "application/json");
                    var response = await _httpClient.PostAsync("force_checkin", content);
                    var responseData = await response.Content.ReadAsStringAsync();
                    response.EnsureSuccessStatusCode();
                    return true;
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Error forcing device checkin on the server.");
            }
            return false;
        }

        public async Task<bool?> ForceDeviceUpdateAsync(int tenantId, int client_id)
        {
            try
            {
                if (await InitiallizeHttpClient(tenantId))
                {
                    var content = new StringContent(JsonConvert.SerializeObject(client_id), Encoding.UTF8, "application/json");
                    var response = await _httpClient.PostAsync("force_update", content);
                    var responseData = await response.Content.ReadAsStringAsync();
                    response.EnsureSuccessStatusCode();
                    return true;
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Error forcing device update on the server.");
            }
            return false;
        }

        public async Task<DeviceStatus?> GetDeviceStatusAsync(int tenantId, int clientId)
        {
            try
            {
                if (await InitiallizeHttpClient(tenantId))
                {
                    var response = await _httpClient.GetAsync($"get_status/{clientId}");
                    var responseData = await response.Content.ReadAsStringAsync();
                    response.EnsureSuccessStatusCode();
                    return JsonConvert.DeserializeObject<DeviceStatus>(responseData);
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Error getting device status from the server.");
            }
            return null;
        }

        public async Task<GetDeviceFilesResponse?> GetDeviceFilesAsync(int tenantId, GetDeviceFilesRequest request)
        {
            try
            {
                if (await InitiallizeHttpClient(tenantId))
                {
                    var content = new StringContent(JsonConvert.SerializeObject(request, new InvalidJsonUtilities()), Encoding.UTF8, "application/json");
                    var response = await _httpClient.PostAsync("get_files", content);
                    var responseData = await response.Content.ReadAsStringAsync();
                    response.EnsureSuccessStatusCode();
                    return JsonConvert.DeserializeObject<GetDeviceFilesResponse>(responseData);
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Error getting device files from the server.");
            }
            return null;
        }
    }
}
