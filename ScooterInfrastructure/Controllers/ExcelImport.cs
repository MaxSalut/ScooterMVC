using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Authorization; // Додано для авторизації
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
    [Authorize(Roles = "Admin")] // Обмеження доступу до всього контролера для Admin
    public partial class ReportsController : Controller
    {
        #region ImportExcel

        /// <summary>
        /// Імпортує дані з Excel-файлу до відповідної таблиці в базі даних.
        /// </summary>
        /// <param name="file">Завантажений Excel-файл</param>
        /// <param name="tableName">Назва таблиці для імпорту</param>
        /// <returns>Перенаправлення на Index або відображення помилок</returns>
        [HttpPost]
        public async Task<IActionResult> ImportExcel(IFormFile file, string tableName)
        {
            if (!IsFileValid(file))
            {
                ModelState.AddModelError("file", "Будь ласка, виберіть файл для імпорту.");
                return View("Index");
            }

            using var package = new ExcelPackage(file.OpenReadStream());
            var worksheet = package.Workbook.Worksheets.FirstOrDefault();
            if (!IsWorksheetValid(worksheet))
            {
                ModelState.AddModelError("file", "Файл порожній або пошкоджений.");
                return View("Index");
            }

            var rowCount = worksheet.Dimension.Rows;
            switch (tableName)
            {
                case "ChargingStations":
                    if (!ValidateExcelStructure(worksheet, new[] { "Назва", "Розташування", "Кількість слотів", "Поточна кількість скутерів" }))
                    {
                        ModelState.AddModelError("file", "Невірна структура файлу для ChargingStations.");
                        return View("Index");
                    }
                    await ImportChargingStationsFromExcel(worksheet, rowCount);
                    break;

                case "Scooters":
                    if (!ValidateExcelStructure(worksheet, new[] { "Модель", "Рівень батареї", "Статус", "Поточне розташування", "Станція ID" }))
                    {
                        ModelState.AddModelError("file", "Невірна структура файлу для Scooters.");
                        return View("Index");
                    }
                    await ImportScootersFromExcel(worksheet, rowCount);
                    break;

                case "Riders":
                    if (!ValidateExcelStructure(worksheet, new[] { "Ім'я", "Прізвище", "Номер телефону", "Дата реєстрації", "Баланс рахунку" }))
                    {
                        ModelState.AddModelError("file", "Невірна структура файлу для Riders.");
                        return View("Index");
                    }
                    await ImportRidersFromExcel(worksheet, rowCount);
                    break;

                case "Discounts":
                    if (!ValidateExcelStructure(worksheet, new[] { "Назва", "Відсоток знижки", "Опис" }))
                    {
                        ModelState.AddModelError("file", "Невірна структура файлу для Discounts.");
                        return View("Index");
                    }
                    await ImportDiscountsFromExcel(worksheet, rowCount);
                    break;

                case "Rentals":
                    if (!ValidateExcelStructure(worksheet, new[] { "Rider ID", "Scooter ID", "Статус", "Час початку", "Час завершення", "Загальна вартість", "Дата оплати", "Сума оплати", "Payment Method ID" }))
                    {
                        ModelState.AddModelError("file", "Невірна структура файлу для Rentals.");
                        return View("Index");
                    }
                    await ImportRentalsFromExcel(worksheet, rowCount);
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

        private async Task ImportChargingStationsFromExcel(ExcelWorksheet worksheet, int rowCount)
        {
            for (int row = 2; row <= rowCount; row++)
            {
                var chargingStation = new ChargingStation
                {
                    Name = worksheet.Cells[row, 1].Value?.ToString(),
                    Location = worksheet.Cells[row, 2].Value?.ToString(),
                    ChargingSlots = int.TryParse(worksheet.Cells[row, 3].Value?.ToString(), out int slots) ? slots : 0,
                    CurrentScooterCount = int.TryParse(worksheet.Cells[row, 4].Value?.ToString(), out int count) ? count : 0
                };
                ProcessEntity(chargingStation, row);
            }
        }

        private async Task ImportScootersFromExcel(ExcelWorksheet worksheet, int rowCount)
        {
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
                ProcessEntity(scooter, row);
            }
        }

        private async Task ImportRidersFromExcel(ExcelWorksheet worksheet, int rowCount)
        {
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
                ProcessEntity(rider, row);
            }
        }

        private async Task ImportDiscountsFromExcel(ExcelWorksheet worksheet, int rowCount)
        {
            for (int row = 2; row <= rowCount; row++)
            {
                var discount = new Discount
                {
                    Name = worksheet.Cells[row, 1].Value?.ToString(),
                    Percentage = decimal.TryParse(worksheet.Cells[row, 2].Value?.ToString(), out decimal percentage) ? percentage : 0,
                    Description = worksheet.Cells[row, 3].Value?.ToString()
                };
                ProcessEntity(discount, row);
            }
        }

        private async Task ImportRentalsFromExcel(ExcelWorksheet worksheet, int rowCount)
        {
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
                ProcessEntity(rental, row);
            }
        }

        #endregion
    }
}