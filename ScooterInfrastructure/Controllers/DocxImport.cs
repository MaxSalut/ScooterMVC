using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using ScooterDomain.Model;
using ScooterInfrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace ScooterInfrastructure.Controllers
{
    public partial class ReportsController : Controller
    {
        #region ImportDocx

        /// <summary>
        /// Імпортує дані з .docx-файлу до відповідної таблиці.
        /// </summary>
        /// <param name="file">Завантажений .docx-файл</param>
        /// <param name="tableName">Назва таблиці для імпорту</param>
        /// <returns>Перенаправлення на Index або відображення помилок</returns>
        [HttpPost]
        public async Task<IActionResult> ImportDocx(IFormFile file, string tableName)
        {
            if (!IsFileValid(file))
            {
                ModelState.AddModelError("file", "Будь ласка, виберіть файл для імпорту.");
                return View("Index");
            }

            var savePath = await SaveUploadedFile(file);
            using var doc = WordprocessingDocument.Open(savePath, false);
            var body = doc.MainDocumentPart.Document.Body;

            // Знаходимо першу таблицю
            var table = body.Elements<Table>().FirstOrDefault();
            if (table == null)
            {
                ModelState.AddModelError("file", "Файл не містить таблиці з даними.");
                return View("Index");
            }

            // Пропускаємо заголовки (перший рядок) і отримуємо дані
            var dataRows = table.Elements<TableRow>().Skip(1).ToList();
            if (!dataRows.Any())
            {
                ModelState.AddModelError("file", "Таблиця не містить даних для імпорту.");
                return View("Index");
            }

            switch (tableName)
            {
                case "ChargingStations":
                    await ImportChargingStationsFromDocxTable(dataRows);
                    break;

                case "Scooters":
                    await ImportScootersFromDocxTable(dataRows);
                    break;

                case "Riders":
                    await ImportRidersFromDocxTable(dataRows);
                    break;

                case "Discounts":
                    await ImportDiscountsFromDocxTable(dataRows);
                    break;

                case "Rentals":
                    await ImportRentalsFromDocxTable(dataRows);
                    break;

                default:
                    ModelState.AddModelError("tableName", "Невірно вказана таблиця.");
                    return View("Index");
            }

            if (ModelState.IsValid)
            {
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", new { controller = tableName });
            }

            return View("Index");
        }

        private async Task ImportChargingStationsFromDocxTable(List<TableRow> rows)
        {
            foreach (var row in rows)
            {
                var cells = row.Elements<TableCell>().ToList();
                if (cells.Count < 4) continue;

                var chargingStation = new ChargingStation
                {
                    Name = cells[0].InnerText.Trim(),
                    Location = cells[1].InnerText.Trim(),
                    ChargingSlots = int.TryParse(cells[2].InnerText.Trim(), out int slots) ? slots : 0,
                    CurrentScooterCount = int.TryParse(cells[3].InnerText.Trim(), out int count) ? count : 0
                };
                ProcessEntity(chargingStation, row.InnerText);
            }
        }

        private async Task ImportScootersFromDocxTable(List<TableRow> rows)
        {
            foreach (var row in rows)
            {
                var cells = row.Elements<TableCell>().ToList();
                if (cells.Count < 5) continue;

                var scooter = new Scooter
                {
                    Model = cells[0].InnerText.Trim(),
                    BatteryLevel = int.TryParse(cells[1].InnerText.Trim(), out int level) ? level : 0,
                    StatusId = await GetScooterStatusIdFromName(cells[2].InnerText.Trim()),
                    CurrentLocation = cells[3].InnerText.Trim(),
                    StationId = int.TryParse(cells[4].InnerText.Trim(), out int stationId) ? stationId : null
                };
                ProcessEntity(scooter, row.InnerText);
            }
        }

        private async Task ImportRidersFromDocxTable(List<TableRow> rows)
        {
            foreach (var row in rows)
            {
                var cells = row.Elements<TableCell>().ToList();
                if (cells.Count < 5) continue;

                var rider = new Rider
                {
                    FirstName = cells[0].InnerText.Trim(),
                    LastName = cells[1].InnerText.Trim(),
                    PhoneNumber = cells[2].InnerText.Trim(),
                    RegistrationDate = DateOnly.TryParse(cells[3].InnerText.Trim(), out DateOnly regDate) ? regDate : DateOnly.FromDateTime(DateTime.Now),
                    AccountBalance = decimal.TryParse(cells[4].InnerText.Trim(), out decimal balance) ? balance : 0
                };
                ProcessEntity(rider, row.InnerText);
            }
        }

        private async Task ImportDiscountsFromDocxTable(List<TableRow> rows)
        {
            foreach (var row in rows)
            {
                var cells = row.Elements<TableCell>().ToList();
                if (cells.Count < 3) continue;

                var discount = new Discount
                {
                    Name = cells[0].InnerText.Trim(),
                    Percentage = decimal.TryParse(cells[1].InnerText.Trim(), out decimal percentage) ? percentage : 0,
                    Description = cells[2].InnerText.Trim()
                };
                ProcessEntity(discount, row.InnerText);
            }
        }

        private async Task ImportRentalsFromDocxTable(List<TableRow> rows)
        {
            foreach (var row in rows)
            {
                var cells = row.Elements<TableCell>().ToList();
                if (cells.Count < 9) continue;

                var rental = new Rental
                {
                    RiderId = int.TryParse(cells[0].InnerText.Trim(), out int riderId) ? riderId : 0,
                    ScooterId = int.TryParse(cells[1].InnerText.Trim(), out int scooterId) ? scooterId : 0,
                    StatusId = await GetRentalStatusIdFromName(cells[2].InnerText.Trim()),
                    StartTime = DateTime.TryParse(cells[3].InnerText.Trim(), out DateTime start) ? start : DateTime.Now,
                    EndTime = DateTime.TryParse(cells[4].InnerText.Trim(), out DateTime end) ? end : null,
                    TotalCost = decimal.TryParse(cells[5].InnerText.Trim(), out decimal cost) ? cost : 0,
                    PaymentDate = DateTime.TryParse(cells[6].InnerText.Trim(), out DateTime payDate) ? payDate : null,
                    Amount = decimal.TryParse(cells[7].InnerText.Trim(), out decimal amount) ? amount : null,
                    PaymentMethodId = int.TryParse(cells[8].InnerText.Trim(), out int payMethod) ? payMethod : null
                };
                ProcessEntity(rental, row.InnerText);
            }
        }

        #endregion
    }
}