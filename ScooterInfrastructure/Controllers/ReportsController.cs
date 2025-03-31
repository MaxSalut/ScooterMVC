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
using System.ComponentModel.DataAnnotations;
using OfficeOpenXml.DataValidation;
using NuGet.Packaging;


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
        public async Task<IActionResult> ImportExcel(IFormFile file, string tableName)
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

            using (var package = new ExcelPackage(new FileInfo(savePath)))
            {
                var worksheet = package.Workbook.Worksheets[0];

                // Перевірка структури файлу
                if (worksheet.Dimension == null)
                {
                    ModelState.AddModelError("file", "Файл порожній або пошкоджений.");
                    return View("Index");
                }

                int rowCount = worksheet.Dimension.Rows;
                int colCount = worksheet.Dimension.Columns;

                switch (tableName)
                {
                    case "ChargingStations":
                        if (colCount < 4 || worksheet.Cells[1, 1].Value?.ToString() != "Назва" ||
                            worksheet.Cells[1, 2].Value?.ToString() != "Розташування" ||
                            worksheet.Cells[1, 3].Value?.ToString() != "Кількість слотів" ||
                            worksheet.Cells[1, 4].Value?.ToString() != "Поточна кількість скутерів")
                        {
                            ModelState.AddModelError("file", "Невірна структура файлу. Очікуються стовпці: Назва, Розташування, Кількість слотів, Поточна кількість скутерів.");
                            return View("Index");
                        }
                        for (int row = 2; row <= rowCount; row++)
                        {
                            var chargingStation = new ChargingStation
                            {
                                Name = worksheet.Cells[row, 1].Value?.ToString(),
                                Location = worksheet.Cells[row, 2].Value?.ToString(),
                                ChargingSlots = int.TryParse(worksheet.Cells[row, 3].Value?.ToString(), out int slots) ? slots : 0,
                                CurrentScooterCount = int.TryParse(worksheet.Cells[row, 4].Value?.ToString(), out int count) ? count : 0
                            };

                            var validationResults = new List<ValidationResult>();
                            var validationContext = new ValidationContext(chargingStation);
                            if (Validator.TryValidateObject(chargingStation, validationContext, validationResults, true))
                            {
                                _context.ChargingStations.Add(chargingStation);
                            }
                            else
                            {
                                foreach (var error in validationResults)
                                {
                                    ModelState.AddModelError("", $"Рядок {row}: {error.ErrorMessage}");
                                }
                            }
                        }
                        break;

                    case "Scooters":
                        if (colCount < 5 || worksheet.Cells[1, 1].Value?.ToString() != "Модель" ||
                            worksheet.Cells[1, 2].Value?.ToString() != "Рівень батареї" ||
                            worksheet.Cells[1, 3].Value?.ToString() != "Статус" ||
                            worksheet.Cells[1, 4].Value?.ToString() != "Поточне розташування" ||
                            worksheet.Cells[1, 5].Value?.ToString() != "Станція ID")
                        {
                            ModelState.AddModelError("file", "Невірна структура файлу. Очікуються стовпці: Модель, Рівень батареї, Статус, Поточне розташування, Станція ID.");
                            return View("Index");
                        }
                        for (int row = 2; row <= rowCount; row++)
                        {
                            var scooter = new Scooter
                            {
                                Model = worksheet.Cells[row, 1].Value?.ToString(),
                                BatteryLevel = int.TryParse(worksheet.Cells[row, 2].Value?.ToString(), out int level) ? level : 0,
                                StatusId = await GetScooterStatusIdFromName(worksheet.Cells[row, 3].Value?.ToString()),
                                CurrentLocation = worksheet.Cells[row, 4].Value?.ToString(),
                                StationId = int.TryParse(worksheet.Cells[row, 5].Value?.ToString(), out int stationId) ? stationId : null
                            };

                            var validationResults = new List<ValidationResult>();
                            var validationContext = new ValidationContext(scooter);
                            if (Validator.TryValidateObject(scooter, validationContext, validationResults, true))
                            {
                                _context.Scooters.Add(scooter);
                            }
                            else
                            {
                                foreach (var error in validationResults)
                                {
                                    ModelState.AddModelError("", $"Рядок {row}: {error.ErrorMessage}");
                                }
                            }
                        }
                        break;

                    case "Riders":
                        if (colCount < 5 || worksheet.Cells[1, 1].Value?.ToString() != "Ім'я" ||
                            worksheet.Cells[1, 2].Value?.ToString() != "Прізвище" ||
                            worksheet.Cells[1, 3].Value?.ToString() != "Номер телефону" ||
                            worksheet.Cells[1, 4].Value?.ToString() != "Дата реєстрації" ||
                            worksheet.Cells[1, 5].Value?.ToString() != "Баланс рахунку")
                        {
                            ModelState.AddModelError("file", "Невірна структура файлу. Очікуються стовпці: Ім'я, Прізвище, Номер телефону, Дата реєстрації, Баланс рахунку.");
                            return View("Index");
                        }
                        for (int row = 2; row <= rowCount; row++)
                        {
                            var rider = new Rider
                            {
                                FirstName = worksheet.Cells[row, 1].Value?.ToString(),
                                LastName = worksheet.Cells[row, 2].Value?.ToString(),
                                PhoneNumber = worksheet.Cells[row, 3].Value?.ToString(),
                                RegistrationDate = DateOnly.TryParse(worksheet.Cells[row, 4].Value?.ToString(), out DateOnly regDate) ? regDate : DateOnly.FromDateTime(DateTime.Now),
                                AccountBalance = decimal.TryParse(worksheet.Cells[row, 5].Value?.ToString(), out decimal balance) ? balance : 0
                            };

                            var validationResults = new List<ValidationResult>();
                            var validationContext = new ValidationContext(rider);
                            if (Validator.TryValidateObject(rider, validationContext, validationResults, true))
                            {
                                _context.Riders.Add(rider);
                            }
                            else
                            {
                                foreach (var error in validationResults)
                                {
                                    ModelState.AddModelError("", $"Рядок {row}: {error.ErrorMessage}");
                                }
                            }
                        }
                        break;

                    case "Discounts":
                        if (colCount < 3 || worksheet.Cells[1, 1].Value?.ToString() != "Назва" ||
                            worksheet.Cells[1, 2].Value?.ToString() != "Відсоток знижки" ||
                            worksheet.Cells[1, 3].Value?.ToString() != "Опис")
                        {
                            ModelState.AddModelError("file", "Невірна структура файлу. Очікуються стовпці: Назва, Відсоток знижки, Опис.");
                            return View("Index");
                        }
                        for (int row = 2; row <= rowCount; row++)
                        {
                            var discount = new Discount
                            {
                                Name = worksheet.Cells[row, 1].Value?.ToString(),
                                Percentage = decimal.TryParse(worksheet.Cells[row, 2].Value?.ToString(), out decimal percentage) ? percentage : 0,
                                Description = worksheet.Cells[row, 3].Value?.ToString()
                            };

                            var validationResults = new List<ValidationResult>();
                            var validationContext = new ValidationContext(discount);
                            if (Validator.TryValidateObject(discount, validationContext, validationResults, true))
                            {
                                _context.Discounts.Add(discount);
                            }
                            else
                            {
                                foreach (var error in validationResults)
                                {
                                    ModelState.AddModelError("", $"Рядок {row}: {error.ErrorMessage}");
                                }
                            }
                        }
                        break;

                    case "Rentals":
                        if (colCount < 9 || worksheet.Cells[1, 1].Value?.ToString() != "Rider ID" ||
                            worksheet.Cells[1, 2].Value?.ToString() != "Scooter ID" ||
                            worksheet.Cells[1, 3].Value?.ToString() != "Статус" ||
                            worksheet.Cells[1, 4].Value?.ToString() != "Час початку" ||
                            worksheet.Cells[1, 5].Value?.ToString() != "Час завершення" ||
                            worksheet.Cells[1, 6].Value?.ToString() != "Загальна вартість" ||
                            worksheet.Cells[1, 7].Value?.ToString() != "Дата оплати" ||
                            worksheet.Cells[1, 8].Value?.ToString() != "Сума оплати" ||
                            worksheet.Cells[1, 9].Value?.ToString() != "Payment Method ID")
                        {
                            ModelState.AddModelError("file", "Невірна структура файлу. Очікуються стовпці: Rider ID, Scooter ID, Статус, Час початку, Час завершення, Загальна вартість, Дата оплати, Сума оплати, Payment Method ID.");
                            return View("Index");
                        }
                        for (int row = 2; row <= rowCount; row++)
                        {
                            var rental = new Rental
                            {
                                RiderId = int.TryParse(worksheet.Cells[row, 1].Value?.ToString(), out int riderId) ? riderId : 0,
                                ScooterId = int.TryParse(worksheet.Cells[row, 2].Value?.ToString(), out int scooterId) ? scooterId : 0,
                                StatusId = await GetRentalStatusIdFromName(worksheet.Cells[row, 3].Value?.ToString()),
                                StartTime = DateTime.TryParse(worksheet.Cells[row, 4].Value?.ToString(), out DateTime start) ? start : DateTime.Now,
                                EndTime = DateTime.TryParse(worksheet.Cells[row, 5].Value?.ToString(), out DateTime end) ? end : null,
                                TotalCost = decimal.TryParse(worksheet.Cells[row, 6].Value?.ToString(), out decimal cost) ? cost : 0,
                                PaymentDate = DateTime.TryParse(worksheet.Cells[row, 7].Value?.ToString(), out DateTime payDate) ? payDate : null,
                                Amount = decimal.TryParse(worksheet.Cells[row, 8].Value?.ToString(), out decimal amount) ? amount : null,
                                PaymentMethodId = int.TryParse(worksheet.Cells[row, 9].Value?.ToString(), out int payMethod) ? payMethod : null
                            };

                            var validationResults = new List<ValidationResult>();
                            var validationContext = new ValidationContext(rental);
                            if (Validator.TryValidateObject(rental, validationContext, validationResults, true))
                            {
                                _context.Rentals.Add(rental);
                            }
                            else
                            {
                                foreach (var error in validationResults)
                                {
                                    ModelState.AddModelError("", $"Рядок {row}: {error.ErrorMessage}");
                                }
                            }
                        }
                        break;

                    default:
                        ModelState.AddModelError("tableName", "Невірно вказана таблиця.");
                        return View("Index");
                }

                if (ModelState.IsValid)
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index", tableName);
                }
            }

            return View("Index");
        }

        // GET: Експорт у Excel
        public async Task<IActionResult> ExportExcel(string tableName, int? statusId = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add(tableName);

                switch (tableName)
                {
                    case "ChargingStations":
                        var stations = await _context.ChargingStations.ToListAsync();
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
                        break;

                    case "Scooters":
                        var scootersQuery = _context.Scooters.Include(s => s.Status).AsQueryable();
                        if (statusId.HasValue)
                        {
                            scootersQuery = scootersQuery.Where(s => s.StatusId == statusId.Value);
                        }
                        var scooters = await scootersQuery.ToListAsync();
                        worksheet.Cells[1, 1].Value = "Модель";
                        worksheet.Cells[1, 2].Value = "Рівень батареї";
                        worksheet.Cells[1, 3].Value = "Статус";
                        worksheet.Cells[1, 4].Value = "Поточне розташування";
                        worksheet.Cells[1, 5].Value = "Станція ID";
                        for (int i = 0; i < scooters.Count; i++)
                        {
                            worksheet.Cells[i + 2, 1].Value = scooters[i].Model;
                            worksheet.Cells[i + 2, 2].Value = scooters[i].BatteryLevel;
                            worksheet.Cells[i + 2, 3].Value = scooters[i].Status.Name; // Використовуємо назву статусу
                            worksheet.Cells[i + 2, 4].Value = scooters[i].CurrentLocation;
                            worksheet.Cells[i + 2, 5].Value = scooters[i].StationId;
                        }
                        // Додаємо випадний список для статусу
                        var scooterStatuses = await _context.ScooterStatuses.Select(s => s.Name).ToListAsync();
                        var scooterValidation = worksheet.DataValidations.AddListValidation("C2:C" + (scooters.Count + 1));
                        scooterValidation.Formula.Values.AddRange(scooterStatuses);
                        break;

                    case "Riders":
                        var ridersQuery = _context.Riders.AsQueryable();
                        if (startDate.HasValue)
                        {
                            ridersQuery = ridersQuery.Where(r => r.RegistrationDate >= DateOnly.FromDateTime(startDate.Value));
                        }
                        if (endDate.HasValue)
                        {
                            ridersQuery = ridersQuery.Where(r => r.RegistrationDate <= DateOnly.FromDateTime(endDate.Value));
                        }
                        var riders = await ridersQuery.ToListAsync();
                        worksheet.Cells[1, 1].Value = "Ім'я";
                        worksheet.Cells[1, 2].Value = "Прізвище";
                        worksheet.Cells[1, 3].Value = "Номер телефону";
                        worksheet.Cells[1, 4].Value = "Дата реєстрації";
                        worksheet.Cells[1, 5].Value = "Баланс рахунку";
                        for (int i = 0; i < riders.Count; i++)
                        {
                            worksheet.Cells[i + 2, 1].Value = riders[i].FirstName;
                            worksheet.Cells[i + 2, 2].Value = riders[i].LastName;
                            worksheet.Cells[i + 2, 3].Value = riders[i].PhoneNumber;
                            worksheet.Cells[i + 2, 4].Value = riders[i].RegistrationDate.ToString("yyyy-MM-dd");
                            worksheet.Cells[i + 2, 5].Value = riders[i].AccountBalance;
                        }
                        break;

                    case "Discounts":
                        var discounts = await _context.Discounts.ToListAsync();
                        worksheet.Cells[1, 1].Value = "Назва";
                        worksheet.Cells[1, 2].Value = "Відсоток знижки";
                        worksheet.Cells[1, 3].Value = "Опис";
                        for (int i = 0; i < discounts.Count; i++)
                        {
                            worksheet.Cells[i + 2, 1].Value = discounts[i].Name;
                            worksheet.Cells[i + 2, 2].Value = discounts[i].Percentage;
                            worksheet.Cells[i + 2, 3].Value = discounts[i].Description;
                        }
                        break;

                    case "Rentals":
                        var rentalsQuery = _context.Rentals.Include(r => r.Status).AsQueryable();
                        if (statusId.HasValue)
                        {
                            rentalsQuery = rentalsQuery.Where(r => r.StatusId == statusId.Value);
                        }
                        if (startDate.HasValue)
                        {
                            rentalsQuery = rentalsQuery.Where(r => r.StartTime >= startDate.Value);
                        }
                        if (endDate.HasValue)
                        {
                            rentalsQuery = rentalsQuery.Where(r => r.StartTime <= endDate.Value);
                        }
                        var rentals = await rentalsQuery.ToListAsync();
                        worksheet.Cells[1, 1].Value = "Rider ID";
                        worksheet.Cells[1, 2].Value = "Scooter ID";
                        worksheet.Cells[1, 3].Value = "Статус";
                        worksheet.Cells[1, 4].Value = "Час початку";
                        worksheet.Cells[1, 5].Value = "Час завершення";
                        worksheet.Cells[1, 6].Value = "Загальна вартість";
                        worksheet.Cells[1, 7].Value = "Дата оплати";
                        worksheet.Cells[1, 8].Value = "Сума оплати";
                        worksheet.Cells[1, 9].Value = "Payment Method ID";
                        for (int i = 0; i < rentals.Count; i++)
                        {
                            worksheet.Cells[i + 2, 1].Value = rentals[i].RiderId;
                            worksheet.Cells[i + 2, 2].Value = rentals[i].ScooterId;
                            worksheet.Cells[i + 2, 3].Value = rentals[i].Status.Name; // Використовуємо назву статусу
                            worksheet.Cells[i + 2, 4].Value = rentals[i].StartTime.ToString("yyyy-MM-dd HH:mm");
                            worksheet.Cells[i + 2, 5].Value = rentals[i].EndTime?.ToString("yyyy-MM-dd HH:mm");
                            worksheet.Cells[i + 2, 6].Value = rentals[i].TotalCost;
                            worksheet.Cells[i + 2, 7].Value = rentals[i].PaymentDate?.ToString("yyyy-MM-dd HH:mm");
                            worksheet.Cells[i + 2, 8].Value = rentals[i].Amount;
                            worksheet.Cells[i + 2, 9].Value = rentals[i].PaymentMethodId;
                        }
                        // Додаємо випадний список для статусу
                        var rentalStatuses = await _context.RentalStatuses.Select(s => s.Name).ToListAsync();
                        var rentalValidation = worksheet.DataValidations.AddListValidation("C2:C" + (rentals.Count + 1));
                        rentalValidation.Formula.Values.AddRange(rentalStatuses);
                        break;

                    default:
                        return BadRequest("Невірно вказана таблиця.");
                }

                worksheet.Cells.AutoFitColumns();
                var stream = new MemoryStream(package.GetAsByteArray());
                string fileName = $"{tableName}_Report_{DateTime.Now:yyyyMMdd}.xlsx";
                string savePath = Path.Combine(ReportsDirectory, fileName);
                System.IO.File.WriteAllBytes(savePath, stream.ToArray());

                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }

        // POST: Імпорт з .docx
        [HttpPost]
        public async Task<IActionResult> ImportDocx(IFormFile file, string tableName)
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

                if (lines.Length < 2)
                {
                    ModelState.AddModelError("file", "Файл порожній або не містить даних для імпорту.");
                    return View("Index");
                }

                switch (tableName)
                {
                    case "ChargingStations":
                        foreach (var line in lines.Skip(1))
                        {
                            var parts = line.Split('|');
                            if (parts.Length < 4)
                            {
                                ModelState.AddModelError("file", $"Рядок не містить достатньої кількості полів: {line}");
                                continue;
                            }
                            var chargingStation = new ChargingStation
                            {
                                Name = parts[0].Trim(),
                                Location = parts[1].Trim(),
                                ChargingSlots = int.TryParse(parts[2].Trim(), out int slots) ? slots : 0,
                                CurrentScooterCount = int.TryParse(parts[3].Trim(), out int count) ? count : 0
                            };

                            var validationResults = new List<ValidationResult>();
                            var validationContext = new ValidationContext(chargingStation);
                            if (Validator.TryValidateObject(chargingStation, validationContext, validationResults, true))
                            {
                                _context.ChargingStations.Add(chargingStation);
                            }
                            else
                            {
                                foreach (var error in validationResults)
                                {
                                    ModelState.AddModelError("", $"Рядок '{line}': {error.ErrorMessage}");
                                }
                            }
                        }
                        break;

                    case "Scooters":
                        foreach (var line in lines.Skip(1))
                        {
                            var parts = line.Split('|');
                            if (parts.Length < 5)
                            {
                                ModelState.AddModelError("file", $"Рядок не містить достатньої кількості полів: {line}");
                                continue;
                            }
                            var scooter = new Scooter
                            {
                                Model = parts[0].Trim(),
                                BatteryLevel = int.TryParse(parts[1].Trim(), out int level) ? level : 0,
                                StatusId = await GetScooterStatusIdFromName(parts[2].Trim()),
                                CurrentLocation = parts[3].Trim(),
                                StationId = int.TryParse(parts[4].Trim(), out int stationId) ? stationId : null
                            };

                            var validationResults = new List<ValidationResult>();
                            var validationContext = new ValidationContext(scooter);
                            if (Validator.TryValidateObject(scooter, validationContext, validationResults, true))
                            {
                                _context.Scooters.Add(scooter);
                            }
                            else
                            {
                                foreach (var error in validationResults)
                                {
                                    ModelState.AddModelError("", $"Рядок '{line}': {error.ErrorMessage}");
                                }
                            }
                        }
                        break;

                    case "Riders":
                        foreach (var line in lines.Skip(1))
                        {
                            var parts = line.Split('|');
                            if (parts.Length < 5)
                            {
                                ModelState.AddModelError("file", $"Рядок не містить достатньої кількості полів: {line}");
                                continue;
                            }
                            var rider = new Rider
                            {
                                FirstName = parts[0].Trim(),
                                LastName = parts[1].Trim(),
                                PhoneNumber = parts[2].Trim(),
                                RegistrationDate = DateOnly.TryParse(parts[3].Trim(), out DateOnly regDate) ? regDate : DateOnly.FromDateTime(DateTime.Now),
                                AccountBalance = decimal.TryParse(parts[4].Trim(), out decimal balance) ? balance : 0
                            };

                            var validationResults = new List<ValidationResult>();
                            var validationContext = new ValidationContext(rider);
                            if (Validator.TryValidateObject(rider, validationContext, validationResults, true))
                            {
                                _context.Riders.Add(rider);
                            }
                            else
                            {
                                foreach (var error in validationResults)
                                {
                                    ModelState.AddModelError("", $"Рядок '{line}': {error.ErrorMessage}");
                                }
                            }
                        }
                        break;

                    case "Discounts":
                        foreach (var line in lines.Skip(1))
                        {
                            var parts = line.Split('|');
                            if (parts.Length < 3)
                            {
                                ModelState.AddModelError("file", $"Рядок не містить достатньої кількості полів: {line}");
                                continue;
                            }
                            var discount = new Discount
                            {
                                Name = parts[0].Trim(),
                                Percentage = decimal.TryParse(parts[1].Trim(), out decimal percentage) ? percentage : 0,
                                Description = parts[2].Trim()
                            };

                            var validationResults = new List<ValidationResult>();
                            var validationContext = new ValidationContext(discount);
                            if (Validator.TryValidateObject(discount, validationContext, validationResults, true))
                            {
                                _context.Discounts.Add(discount);
                            }
                            else
                            {
                                foreach (var error in validationResults)
                                {
                                    ModelState.AddModelError("", $"Рядок '{line}': {error.ErrorMessage}");
                                }
                            }
                        }
                        break;

                    case "Rentals":
                        foreach (var line in lines.Skip(1))
                        {
                            var parts = line.Split('|');
                            if (parts.Length < 9)
                            {
                                ModelState.AddModelError("file", $"Рядок не містить достатньої кількості полів: {line}");
                                continue;
                            }
                            var rental = new Rental
                            {
                                RiderId = int.TryParse(parts[0].Trim(), out int riderId) ? riderId : 0,
                                ScooterId = int.TryParse(parts[1].Trim(), out int scooterId) ? scooterId : 0,
                                StatusId = await GetRentalStatusIdFromName(parts[2].Trim()),
                                StartTime = DateTime.TryParse(parts[3].Trim(), out DateTime start) ? start : DateTime.Now,
                                EndTime = DateTime.TryParse(parts[4].Trim(), out DateTime end) ? end : null,
                                TotalCost = decimal.TryParse(parts[5].Trim(), out decimal cost) ? cost : 0,
                                PaymentDate = DateTime.TryParse(parts[6].Trim(), out DateTime payDate) ? payDate : null,
                                Amount = decimal.TryParse(parts[7].Trim(), out decimal amount) ? amount : null,
                                PaymentMethodId = int.TryParse(parts[8].Trim(), out int payMethod) ? payMethod : null
                            };

                            var validationResults = new List<ValidationResult>();
                            var validationContext = new ValidationContext(rental);
                            if (Validator.TryValidateObject(rental, validationContext, validationResults, true))
                            {
                                _context.Rentals.Add(rental);
                            }
                            else
                            {
                                foreach (var error in validationResults)
                                {
                                    ModelState.AddModelError("", $"Рядок '{line}': {error.ErrorMessage}");
                                }
                            }
                        }
                        break;

                    default:
                        ModelState.AddModelError("tableName", "Невірно вказана таблиця.");
                        return View("Index");
                }

                if (ModelState.IsValid)
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index", tableName);
                }
            }

            return View("Index");
        }

        [HttpGet]
        public async Task<IActionResult> ExportDocx(string tableName)
        {
            string fileName = $"{tableName}_Report_{DateTime.Now:yyyyMMdd}.docx";
            string savePath = Path.Combine(ReportsDirectory, fileName);

            using (WordprocessingDocument doc = WordprocessingDocument.Create(savePath, WordprocessingDocumentType.Document))
            {
                MainDocumentPart mainPart = doc.AddMainDocumentPart();
                mainPart.Document = new Document();
                Body body = mainPart.Document.AppendChild(new Body());

                Paragraph title = body.AppendChild(new Paragraph());
                Run titleRun = title.AppendChild(new Run());
                titleRun.AppendChild(new Text($"Звіт по {tableName}"));

                switch (tableName)
                {
                    case "ChargingStations":
                        var stations = await _context.ChargingStations.ToListAsync();
                        foreach (var station in stations)
                        {
                            Paragraph para = body.AppendChild(new Paragraph());
                            Run run = para.AppendChild(new Run());
                            run.AppendChild(new Text($"{station.Name} | {station.Location} | {station.ChargingSlots} | {station.CurrentScooterCount}"));
                        }
                        break;

                    case "Scooters":
                        var scooters = await _context.Scooters.Include(s => s.Status).ToListAsync();
                        foreach (var scooter in scooters)
                        {
                            Paragraph para = body.AppendChild(new Paragraph());
                            Run run = para.AppendChild(new Run());
                            run.AppendChild(new Text($"{scooter.Model} | {scooter.BatteryLevel} | {scooter.Status.Name} | {scooter.CurrentLocation} | {scooter.StationId}"));
                        }
                        break;

                    case "Riders":
                        var riders = await _context.Riders.ToListAsync();
                        foreach (var rider in riders)
                        {
                            Paragraph para = body.AppendChild(new Paragraph());
                            Run run = para.AppendChild(new Run());
                            run.AppendChild(new Text($"{rider.FirstName} | {rider.LastName} | {rider.PhoneNumber} | {rider.RegistrationDate} | {rider.AccountBalance}"));
                        }
                        break;

                    case "Discounts":
                        var discounts = await _context.Discounts.ToListAsync();
                        foreach (var discount in discounts)
                        {
                            Paragraph para = body.AppendChild(new Paragraph());
                            Run run = para.AppendChild(new Run());
                            run.AppendChild(new Text($"{discount.Name} | {discount.Percentage} | {discount.Description}"));
                        }
                        break;

                    case "Rentals":
                        var rentals = await _context.Rentals.Include(r => r.Status).ToListAsync();
                        foreach (var rental in rentals)
                        {
                            Paragraph para = body.AppendChild(new Paragraph());
                            Run run = para.AppendChild(new Run());
                            run.AppendChild(new Text($"{rental.RiderId} | {rental.ScooterId} | {rental.Status.Name} | {rental.StartTime:yyyy-MM-dd HH:mm} | {rental.EndTime:yyyy-MM-dd HH:mm} | {rental.TotalCost} | {rental.PaymentDate:yyyy-MM-dd HH:mm} | {rental.Amount} | {rental.PaymentMethodId}"));
                        }
                        break;

                    default:
                        return BadRequest("Невірно вказана таблиця.");
                }
            }

            var stream = new MemoryStream(System.IO.File.ReadAllBytes(savePath));
            return File(stream, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", fileName);
        }

        // Допоміжний метод для отримання ID статусу скутера за назвою
        private async Task<int> GetScooterStatusIdFromName(string statusName)
        {
            if (string.IsNullOrEmpty(statusName)) return 1; // За замовчуванням "Доступний"
            var status = await _context.ScooterStatuses.FirstOrDefaultAsync(s => s.Name == statusName);
            return status?.Id ?? 1;
        }

        // Допоміжний метод для отримання ID статусу оренди за назвою
        private async Task<int> GetRentalStatusIdFromName(string statusName)
        {
            if (string.IsNullOrEmpty(statusName)) return 1; // За замовчуванням "Активна"
            var status = await _context.RentalStatuses.FirstOrDefaultAsync(s => s.Name == statusName);
            return status?.Id ?? 1;
        }
    }
}