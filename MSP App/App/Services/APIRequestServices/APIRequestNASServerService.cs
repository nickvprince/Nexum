﻿using Newtonsoft.Json;
using SharedComponents.Entities.DbEntities;
using SharedComponents.Entities.WebEntities.Requests.NASServerRequests;
using SharedComponents.Services.APIRequestServices;
using SharedComponents.Services.APIRequestServices.Interfaces;
using System.Text;

namespace App.Services.APIRequestServices
{
    public class APIRequestNASServerService : BaseAPIRequestService, IAPIRequestNASServerService
    {
        public APIRequestNASServerService(IConfiguration config, HttpClient httpClient, IHttpContextAccessor httpContextAccessor) : base(config, httpClient, httpContextAccessor)
        {
            AppendBaseAddress("NASServer/");
        }

        public async Task<NASServer?> CreateAsync(NASServerCreateRequest request)
        {
            try
            {
                var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("", content);
                var responseData = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<NASServer>(responseData);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<NASServer?> UpdateAsync(NASServerUpdateRequest request)
        {
            try
            {
                var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync("", content);
                var responseData = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<NASServer>(responseData);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{id}");
                var responseData = await response.Content.ReadAsStringAsync();
                response.EnsureSuccessStatusCode();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<NASServer?> GetAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{id}");
                var responseData = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<NASServer>(responseData);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<ICollection<NASServer>?> GetAllAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("");
                var responseData = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ICollection<NASServer>>(responseData);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<ICollection<NASServer>?> GetAllByTenantIdAsync(int tenantId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"By-Tenant/{tenantId}");
                var responseData = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ICollection<NASServer>>(responseData);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<ICollection<NASServer>?> GetAllByDeviceIdAsync(int deviceId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"By-Device/{deviceId}");
                var responseData = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ICollection<NASServer>>(responseData);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}