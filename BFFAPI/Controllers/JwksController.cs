using Microsoft.AspNetCore.Mvc;

namespace BFFAPI.Controllers
{
    public class JwksController : Controller
    {
        private readonly HttpClient _httpClient;

        public JwksController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [HttpGet("signing-keys")]  // Updated route for clarity
        public async Task<IActionResult> GetSigningKeys()
        {
            try
            {
                var response = await _httpClient.GetAsync("http://localhost:8888/realms/SK/protocol/openid-connect/certs");
                response.EnsureSuccessStatusCode();

                var jwks = await response.Content.ReadFromJsonAsync<Jwks>();

                var signingKeys = jwks.Keys.Where(key => key.Use == "sig").ToList();

                return Ok(new { keys = signingKeys });
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, "Error fetching JWKS");
            }
        }
    }
}
