﻿using Newtonsoft.Json;
using SharedComponents.Entities.DbEntities;
using SharedComponents.Entities.WebEntities.Requests.TenantRequests;
using SharedComponents.Services.APIRequestServices;
using SharedComponents.Services.APIRequestServices.Interfaces;
using System.Text;

namespace App.Services.APIRequestServices
{
    public class APIRequestTenantService : BaseAPIRequestService, IAPIRequestTenantService
    {
        public APIRequestTenantService(IConfiguration config, HttpClient httpClient, IHttpContextAccessor httpContextAccessor) : base(config, httpClient, httpContextAccessor)
        {
            AppendBaseAddress("Tenant/");
        }

        public async Task<Tenant?> CreateAsync(TenantCreateRequest request)
        {
            try
            {
                var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("", content);
                var responseData = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<Tenant>(responseData);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<Tenant?> UpdateAsync(TenantUpdateRequest request)
        {
            try
            {
                var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync("", content);
                var responseData = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<Tenant>(responseData);
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

        public async Task<Tenant?> GetAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{id}");
                var responseData = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<Tenant>(responseData);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<Tenant?> GetRichAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{id}/Rich");
                var responseData = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<Tenant>(responseData);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<ICollection<Tenant>?> GetAllAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("");
                var responseData = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ICollection<Tenant>>(responseData);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}