using Microsoft.AspNetCore.Mvc;

namespace SistemaVentaBlazor.WhatsAppBot.Controllers
{
    public class WhatsAppController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
