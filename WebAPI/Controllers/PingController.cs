using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PingController : ControllerBase
    { 

        [HttpGet(Name = "GetPing")]
        public String Get()
        {
            string currentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            LogDateTime(currentTime);
            return currentTime;
        }
        private void LogDateTime(string dateTime)
        {
              string logFilePath = "logdeneme.txt";                //log dosyasının yolu ve adı
           // string logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log.txt"); //Projenin kök dizininde log.txt dosyası oluşturulacak

            try
                {
                    using (StreamWriter writer = new StreamWriter(logFilePath, true)) // dosyalara yazı yazmak için
                    {
                        writer.WriteLine($"{dateTime} - Ping");
                    }
                }
                catch (Exception ex)
                {
                   
                    Console.WriteLine($"Loglama sırasında bir hata oluştu: {ex.Message}");
                }
        }
        }
}
