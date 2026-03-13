using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using System.Text;
using System.Net.Http;

namespace MBU_site.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {

        }

        public async Task<IActionResult> OnPostGlonassRedirectAsync()
        {
            try
            {
                // Данные для входа
                var login = "ЧепоровМА";
                var password = "Qwerty54321";

                // Задержка 0.5 секунды (требование API)
                await Task.Delay(500);

                // Создаем HttpClient прямо здесь (простой способ)
                using var httpClient = new HttpClient();

                // Создаем запрос к API ГЛОНАССсофт
                var requestData = new
                {
                    login = login,
                    password = password
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(requestData),
                    Encoding.UTF8,
                    "application/json");

                _logger.LogInformation("Отправка запроса к API ГЛОНАССсофт");

                var response = await httpClient.PostAsync(
                    "https://hosting.glonasssoft.ru/api/v3/auth/login",
                    content);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Ошибка API: {response.StatusCode}");
                    TempData["GlonassError"] = "Ошибка подключения к сервису мониторинга";
                    return RedirectToPage();
                }

                var responseString = await response.Content.ReadAsStringAsync();
                var responseData = JsonSerializer.Deserialize<JsonElement>(responseString);

                if (responseData.TryGetProperty("AuthId", out var authId))
                {
                    var token = authId.GetString();

                    // Определяем базовый URL
                    var baseUrl = "https://hosting.glonasssoft.ru";

                    _logger.LogInformation("Токен успешно получен, выполняем перенаправление");

                    // Перенаправляем пользователя в ГЛОНАССсофт
                    return Redirect($"{baseUrl}/login?authId={token}");
                }

                _logger.LogError("Не удалось получить токен из ответа API");
                TempData["GlonassError"] = "Не удалось получить токен авторизации";
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при подключении к ГЛОНАССсофт");
                TempData["GlonassError"] = "Произошла ошибка при подключении";
                return RedirectToPage();
            }
        }
    }
}