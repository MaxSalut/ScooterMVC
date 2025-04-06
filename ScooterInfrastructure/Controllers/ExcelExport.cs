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
        #region ExportExcel

        /// <summary>
        /// Експортує дані з бази в Excel-файл із застосуванням фільтрів.
        /// </summary>
        /// <param name="tableName">Назва таблиці</param>
        /// <param name="statusId">Фільтр за статусом (опціонально)</param>
        /// <param name="startDate">Початкова дата (опціонально)</param>
        /// <param name="endDate">Кінцева дата (опціонально)</param>
        /// <returns>Файл Excel</returns>
        [HttpGet]
        public async Task<IActionResult> ExportExcel(string tableName, int? statusId = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add(tableName);

            switch (tableName)
            {
                case "ChargingStations":
                    await ExportChargingStationsToExcel(worksheet);
                    break;

                case "Scooters":
                    await ExportScootersToExcel(worksheet, statusId);
                    break;

                case "Riders":
                    await ExportRidersToExcel(worksheet, startDate, endDate);
                    break;

                case "Discounts":
                    await ExportDiscountsToExcel(worksheet);
                    break;

                case "Rentals":
                    await ExportRentalsToExcel(worksheet, statusId, startDate, endDate);
                    break;

                default:
                    return BadRequest("Невірно вказана таблиця.");
            }

            worksheet.Cells.AutoFitColumns();
            var stream = new MemoryStream(package.GetAsByteArray());
            var fileName = $"{tableName}_Report_{DateTime.Now:yyyyMMdd}.xlsx";
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        private async Task ExportChargingStationsToExcel(ExcelWorksheet worksheet)
        {
            var stations = await _context.ChargingStations.AsNoTracking().ToListAsync();
            SetExcelHeaders(worksheet, new[] { "Назва", "Розташування", "Кількість слотів", "Поточна кількість скутерів" });
            for (int i = 0; i < stations.Count; i++)
            {
                worksheet.Cells[i + 2, 1].Value = stations[i].Name;
                worksheet.Cells[i + 2, 2].Value = stations[i].Location;
                worksheet.Cells[i + 2, 3].Value = stations[i].ChargingSlots;
                worksheet.Cells[i + 2, 4].Value = stations[i].CurrentScooterCount;
            }
        }

        private async Task ExportScootersToExcel(ExcelWorksheet worksheet, int? statusId)
        {
            var query = _context.Scooters.Include(s => s.Status).AsNoTracking();
            if (statusId.HasValue)
                query = query.Where(s => s.StatusId == statusId.Value);
            var scooters = await query.ToListAsync();
            SetExcelHeaders(worksheet, new[] { "Модель", "Рівень батареї", "Статус", "Поточне розташування", "Станція ID" });
            for (int i = 0; i < scooters.Count; i++)
            {
                worksheet.Cells[i + 2, 1].Value = scooters[i].Model;
                worksheet.Cells[i + 2, 2].Value = scooters[i].BatteryLevel;
                worksheet.Cells[i + 2, 3].Value = scooters[i].Status.Name;
                worksheet.Cells[i + 2, 4].Value = scooters[i].CurrentLocation;
                worksheet.Cells[i + 2, 5].Value = scooters[i].StationId;
            }
            AddStatusValidation(worksheet, scooters.Count, await _context.ScooterStatuses.Select(s => s.Name).ToListAsync(), "C");
        }

        private async Task ExportRidersToExcel(ExcelWorksheet worksheet, DateTime? startDate, DateTime? endDate)
        {
            var query = _context.Riders.AsNoTracking();
            if (startDate.HasValue)
                query = query.Where(r => r.RegistrationDate >= DateOnly.FromDateTime(startDate.Value));
            if (endDate.HasValue)
                query = query.Where(r => r.RegistrationDate <= DateOnly.FromDateTime(endDate.Value));
            var riders = await query.ToListAsync();
            SetExcelHeaders(worksheet, new[] { "Ім'я", "Прізвище", "Номер телефону", "Дата реєстрації", "Баланс рахунку" });
            for (int i = 0; i < riders.Count; i++)
            {
                worksheet.Cells[i + 2, 1].Value = riders[i].FirstName;
                worksheet.Cells[i + 2, 2].Value = riders[i].LastName;
                worksheet.Cells[i + 2, 3].Value = riders[i].PhoneNumber;
                worksheet.Cells[i + 2, 4].Value = riders[i].RegistrationDate.ToString("yyyy-MM-dd");
                worksheet.Cells[i + 2, 5].Value = riders[i].AccountBalance;
            }
        }

        private async Task ExportDiscountsToExcel(ExcelWorksheet worksheet)
        {
            var discounts = await _context.Discounts.AsNoTracking().ToListAsync();
            SetExcelHeaders(worksheet, new[] { "Назва", "Відсоток знижки", "Опис" });
            for (int i = 0; i < discounts.Count; i++)
            {
                worksheet.Cells[i + 2, 1].Value = discounts[i].Name;
                worksheet.Cells[i + 2, 2].Value = discounts[i].Percentage;
                worksheet.Cells[i + 2, 3].Value = discounts[i].Description;
            }
        }

        private async Task ExportRentalsToExcel(ExcelWorksheet worksheet, int? statusId, DateTime? startDate, DateTime? endDate)
        {
            var query = _context.Rentals.Include(r => r.Status).AsNoTracking();
            if (statusId.HasValue)
                query = query.Where(r => r.StatusId == statusId.Value);
            if (startDate.HasValue)
                query = query.Where(r => r.StartTime >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(r => r.StartTime <= endDate.Value);
            var rentals = await query.ToListAsync();
            SetExcelHeaders(worksheet, new[] { "Rider ID", "Scooter ID", "Статус", "Час початку", "Час завершення", "Загальна вартість", "Дата оплати", "Сума оплати", "Payment Method ID" });
            for (int i = 0; i < rentals.Count; i++)
            {
                worksheet.Cells[i + 2, 1].Value = rentals[i].RiderId;
                worksheet.Cells[i + 2, 2].Value = rentals[i].ScooterId;
                worksheet.Cells[i + 2, 3].Value = rentals[i].Status.Name;
                worksheet.Cells[i + 2, 4].Value = rentals[i].StartTime.ToString("yyyy-MM-dd HH:mm");
                worksheet.Cells[i + 2, 5].Value = rentals[i].EndTime?.ToString("yyyy-MM-dd HH:mm");
                worksheet.Cells[i + 2, 6].Value = rentals[i].TotalCost;
                worksheet.Cells[i + 2, 7].Value = rentals[i].PaymentDate?.ToString("yyyy-MM-dd HH:mm");
                worksheet.Cells[i + 2, 8].Value = rentals[i].Amount;
                worksheet.Cells[i + 2, 9].Value = rentals[i].PaymentMethodId;
            }
            AddStatusValidation(worksheet, rentals.Count, await _context.RentalStatuses.Select(s => s.Name).ToListAsync(), "C");
        }

        #endregion
    }
}