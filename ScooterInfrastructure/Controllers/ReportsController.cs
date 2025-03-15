// ScooterInfrastructure\Controllers\ReportsController.cs
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using ScooterDomain.Model;
using ScooterInfrastructure;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ScooterInfrastructure.Controllers
{
    public class ReportsController : Controller
    {
        private readonly ScootersContext _context;
        private const string ReportsDirectory = @"C:\Users\mbezv\Desktop\ISTPLAB\DOCX\";

        public ReportsController(ScootersContext context)
        {
            _context = context;
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial; // Для EPPlus у некомерційних проєктах
        }

        // GET: Reports
        public IActionResult Index()
        {
            return View();
        }

        // POST: Імпорт з Excel
        [HttpPost]
        public async Task<IActionResult> ImportExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("file", "Будь ласка, виберіть файл для імпорту.");
                return View("Index");
            }

            string savePath = Path.Combine(ReportsDirectory, file.FileName);
            Directory.CreateDirectory(ReportsDirectory); // Створюємо директорію, якщо її немає

            using (var stream = new FileStream(savePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            using (var package = new ExcelPackage(new FileInfo(savePath)))
            {
                var worksheet = package.Workbook.Worksheets[0];
                int rowCount = worksheet.Dimension.Rows;

                for (int row = 2; row <= rowCount; row++) // Пропускаємо заголовки
                {
                    var chargingStation = new ChargingStation
                    {
                        Name = worksheet.Cells[row, 1].Value?.ToString(),
                        Location = worksheet.Cells[row, 2].Value?.ToString(),
                        ChargingSlots = int.TryParse(worksheet.Cells[row, 3].Value?.ToString(), out int slots) ? slots : 0,
                        CurrentScooterCount = int.TryParse(worksheet.Cells[row, 4].Value?.ToString(), out int count) ? count : 0
                    };

                    if (!string.IsNullOrEmpty(chargingStation.Name) && !string.IsNullOrEmpty(chargingStation.Location))
                    {
                        _context.ChargingStations.Add(chargingStation);
                    }
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index", "ChargingStations");
        }

        // GET: Експорт у Excel
        public async Task<IActionResult> ExportExcel()
        {
            var stations = await _context.ChargingStations.ToListAsync();
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("ChargingStations");
                worksheet.Cells[1, 1].Value = "Назва";
                worksheet.Cells[1, 2].Value = "Розташування";
                worksheet.Cells[1, 3].Value = "Кількість слотів";
                worksheet.Cells[1, 4].Value = "Поточна кількість скутерів";

                for (int i = 0; i < stations.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = stations[i].Name;
                    worksheet.Cells[i + 2, 2].Value = stations[i].Location;
                    worksheet.Cells[i + 2, 3].Value = stations[i].ChargingSlots;
                    worksheet.Cells[i + 2, 4].Value = stations[i].CurrentScooterCount;
                }

                worksheet.Cells.AutoFitColumns();
                var stream = new MemoryStream(package.GetAsByteArray());
                string fileName = $"ChargingStationsReport_{DateTime.Now:yyyyMMdd}.xlsx";
                string savePath = Path.Combine(ReportsDirectory, fileName);
                System.IO.File.WriteAllBytes(savePath, stream.ToArray());

                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }

        // POST: Імпорт з .docx
        [HttpPost]
        public async Task<IActionResult> ImportDocx(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("file", "Будь ласка, виберіть файл для імпорту.");
                return View("Index");
            }

            string savePath = Path.Combine(ReportsDirectory, file.FileName);
            Directory.CreateDirectory(ReportsDirectory);

            using (var stream = new FileStream(savePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            using (WordprocessingDocument doc = WordprocessingDocument.Open(savePath, false))
            {
                string text = doc.MainDocumentPart.Document.Body.InnerText;
                var lines = text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var line in lines.Skip(1)) // Пропускаємо заголовок
                {
                    var parts = line.Split('|');
                    if (parts.Length >= 4)
                    {
                        var chargingStation = new ChargingStation
                        {
                            Name = parts[0].Trim(),
                            Location = parts[1].Trim(),
                            ChargingSlots = int.TryParse(parts[2].Trim(), out int slots) ? slots : 0,
                            CurrentScooterCount = int.TryParse(parts[3].Trim(), out int count) ? count : 0
                        };

                        if (!string.IsNullOrEmpty(chargingStation.Name) && !string.IsNullOrEmpty(chargingStation.Location))
                        {
                            _context.ChargingStations.Add(chargingStation);
                        }
                    }
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index", "ChargingStations");
        }

        // GET: Експорт у .docx
        public async Task<IActionResult> ExportDocx()
        {
            var stations = await _context.ChargingStations.ToListAsync();
            string fileName = $"ChargingStationsReport_{DateTime.Now:yyyyMMdd}.docx";
            string savePath = Path.Combine(ReportsDirectory, fileName);

            using (WordprocessingDocument doc = WordprocessingDocument.Create(savePath, WordprocessingDocumentType.Document))
            {
                MainDocumentPart mainPart = doc.AddMainDocumentPart();
                mainPart.Document = new Document();
                Body body = mainPart.Document.AppendChild(new Body());

                Paragraph title = body.AppendChild(new Paragraph());
                Run titleRun = title.AppendChild(new Run());
                titleRun.AppendChild(new Text("Звіт про станції зарядки"));

                foreach (var station in stations)
                {
                    Paragraph para = body.AppendChild(new Paragraph());
                    Run run = para.AppendChild(new Run());
                    run.AppendChild(new Text($"{station.Name} | {station.Location} | {station.ChargingSlots} | {station.CurrentScooterCount}"));
                }
            }

            var stream = new MemoryStream(System.IO.File.ReadAllBytes(savePath));
            return File(stream, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", fileName);
        }
    }
}