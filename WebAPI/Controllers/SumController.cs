using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SumController : ControllerBase
    {
        [HttpGet("{x}/{y}", Name = "GetSum")]
        public IActionResult Sum(int x, int y) // toplamı JSON formatına döndürmek için IActionResult 
        {
            int sum = x + y;
            string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - Sum of {x} and {y} is {sum}";
            Log(logMessage);
            return Ok(new { Sum = sum });   // return Ok --> IActionResult ı döndürüyor
        }
        private void Log(string message)
        {
            string logFilePath = "logdeneme.txt"; // log dosyasının yolu ve adı

            try
            {
                using (StreamWriter writer = new StreamWriter(logFilePath, true)) // dosyalara yazı yazmak için
                {
                    writer.WriteLine(message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Loglama sırasında bir hata oluştu: {ex.Message}");
            }
        }
    }

}
