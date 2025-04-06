using Microsoft.AspNetCore.Authorization; // Додано для авторизації
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using ScooterInfrastructure;

namespace ScooterInfrastructure.Controllers
{
    [Authorize(Roles = "Admin")] // Обмеження доступу до всього контролера для Admin
    public partial class ReportsController : Controller
    {
        private readonly ScootersContext _context;
        protected const string ReportsDirectory = @"C:\Users\mbezv\Desktop\ISTPLAB\DOCX";

        public ReportsController(ScootersContext context)
        {
            _context = context;
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial; // Налаштування ліцензії для EPPlus
        }

        // GET: Reports
        public IActionResult Index()
        {
            return View();
        }
    }
}