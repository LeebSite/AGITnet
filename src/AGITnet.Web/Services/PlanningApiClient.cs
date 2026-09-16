using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AGITnet.Application.DTOs;

namespace AGITnet.Web.Services;

public class ApiResponse<T>
{
    public bool IsSuccess { get; set; }
    public T? Data { get; set; }
    public string? ErrorMessage { get; set; }
    public HttpStatusCode StatusCode { get; set; }
}

public interface IPlanningApiClient
{
    Task<ApiResponse<PlanningResponse>> CreatePlanningAsync(CreatePlanningRequest request);
    Task<ApiResponse<List<PlanningResponse>>> GetAllPlanningsAsync();
    Task<ApiResponse<PlanningResponse>> GetPlanningByIdAsync(int id);
}

public class PlanningApiClient : IPlanningApiClient
{
    private readonly HttpClient _httpClient;

    public PlanningApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ApiResponse<PlanningResponse>> CreatePlanningAsync(CreatePlanningRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/plannings", request);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<PlanningResponse>();
                return new ApiResponse<PlanningResponse>
                {
                    IsSuccess = true,
                    Data = data,
                    StatusCode = response.StatusCode
                };
            }

            var errorMsg = await ExtractErrorMessageAsync(response);
            return new ApiResponse<PlanningResponse>
            {
                IsSuccess = false,
                ErrorMessage = errorMsg,
                StatusCode = response.StatusCode
            };
        }
        catch (HttpRequestException)
        {
            return new ApiResponse<PlanningResponse>
            {
                IsSuccess = false,
                ErrorMessage = "Gagal terhubung ke server API backend. Pastikan layanan backend aktif.",
                StatusCode = HttpStatusCode.ServiceUnavailable
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<PlanningResponse>
            {
                IsSuccess = false,
                ErrorMessage = $"Terjadi kesalahan tidak terduga: {ex.Message}",
                StatusCode = HttpStatusCode.InternalServerError
            };
        }
    }

    public async Task<ApiResponse<List<PlanningResponse>>> GetAllPlanningsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/plannings");
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<List<PlanningResponse>>();
                return new ApiResponse<List<PlanningResponse>>
                {
                    IsSuccess = true,
                    Data = data ?? new List<PlanningResponse>(),
                    StatusCode = response.StatusCode
                };
            }

            var errorMsg = await ExtractErrorMessageAsync(response);
            return new ApiResponse<List<PlanningResponse>>
            {
                IsSuccess = false,
                ErrorMessage = errorMsg,
                StatusCode = response.StatusCode
            };
        }
        catch (HttpRequestException)
        {
            return new ApiResponse<List<PlanningResponse>>
            {
                IsSuccess = false,
                ErrorMessage = "Gagal terhubung ke server API backend. Pastikan layanan backend aktif.",
                StatusCode = HttpStatusCode.ServiceUnavailable
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<List<PlanningResponse>>
            {
                IsSuccess = false,
                ErrorMessage = $"Terjadi kesalahan tidak terduga: {ex.Message}",
                StatusCode = HttpStatusCode.InternalServerError
            };
        }
    }

    public async Task<ApiResponse<PlanningResponse>> GetPlanningByIdAsync(int id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/plannings/{id}");
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<PlanningResponse>();
                return new ApiResponse<PlanningResponse>
                {
                    IsSuccess = true,
                    Data = data,
                    StatusCode = response.StatusCode
                };
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return new ApiResponse<PlanningResponse>
                {
                    IsSuccess = false,
                    ErrorMessage = $"Planning dengan ID {id} tidak ditemukan.",
                    StatusCode = HttpStatusCode.NotFound
                };
            }

            var errorMsg = await ExtractErrorMessageAsync(response);
            return new ApiResponse<PlanningResponse>
            {
                IsSuccess = false,
                ErrorMessage = errorMsg,
                StatusCode = response.StatusCode
            };
        }
        catch (HttpRequestException)
        {
            return new ApiResponse<PlanningResponse>
            {
                IsSuccess = false,
                ErrorMessage = "Gagal terhubung ke server API backend. Pastikan layanan backend aktif.",
                StatusCode = HttpStatusCode.ServiceUnavailable
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<PlanningResponse>
            {
                IsSuccess = false,
                ErrorMessage = $"Terjadi kesalahan tidak terduga: {ex.Message}",
                StatusCode = HttpStatusCode.InternalServerError
            };
        }
    }

    private static async Task<string> ExtractErrorMessageAsync(HttpResponseMessage response)
    {
        try
        {
            var content = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(content))
                return $"Error ({response.StatusCode})";

            using var doc = JsonDocument.Parse(content);
            if (doc.RootElement.TryGetProperty("message", out var msgElement))
            {
                return msgElement.GetString() ?? content;
            }
            if (doc.RootElement.TryGetProperty("title", out var titleElement))
            {
                return titleElement.GetString() ?? content;
            }
        }
        catch
        {
            // Fallback jika respon bukan format JSON
        }

        return $"Error HTTP {(int)response.StatusCode}: {response.ReasonPhrase}";
    }
}
