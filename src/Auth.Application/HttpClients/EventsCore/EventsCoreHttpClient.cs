using System;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Auth.Application.HttpClients.EventsCore
{
    public class EventsCoreHttpClient
    {
        private readonly HttpClient _httpClient;

        public EventsCoreHttpClient(HttpClient httpClient, IConfiguration config)
        {
            httpClient.BaseAddress = new Uri(config["eventsCoreUrl"]);
            _httpClient = httpClient;
        }


        public async Task Register(RegisterRequestDto dto)
        {
            var res = await _httpClient.PostAsync("/auth/register", Json(dto))
                .ConfigureAwait(false);
            res.EnsureSuccessStatusCode();
        }

        public async Task<TokenResponse> Login(LoginRequestDto dto)
        {
            var res = await _httpClient.PostAsync("/auth/login", Json(dto))
                .ConfigureAwait(false);
            res.EnsureSuccessStatusCode();
            var body = await res.Content.ReadAsStringAsync().ConfigureAwait(false);
            var token = JsonConvert.DeserializeObject<TokenResponse>(body);
            return token;
        }

        private static HttpContent Json(object o)
            => new StringContent(
                JsonConvert.SerializeObject(o),
                Encoding.UTF8,
                MediaTypeNames.Application.Json);
    }
}