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
        #region ExportDocx

        /// <summary>
        /// Експортує дані в структурований .docx-файл у вигляді таблиці.
        /// </summary>
        /// <param name="tableName">Назва таблиці</param>
        /// <returns>Файл Word</returns>
        [HttpGet]
        public async Task<IActionResult> ExportDocx(string tableName)
        {
            var fileName = $"{tableName}_Report_{DateTime.Now:yyyyMMdd}.docx";
            var stream = new MemoryStream();

            using (var doc = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document, true))
            {
                var mainPart = doc.AddMainDocumentPart();
                mainPart.Document = new Document();
                var body = mainPart.Document.AppendChild(new Body());

                AddDocxTitle(body, $"Report for {tableName}");

                switch (tableName)
                {
                    case "ChargingStations":
                        await ExportChargingStationsToDocx(body);
                        break;
                    case "Scooters":
                        await ExportScootersToDocx(body);
                        break;
                    case "Riders":
                        await ExportRidersToDocx(body);
                        break;
                    case "Discounts":
                        await ExportDiscountsToDocx(body);
                        break;
                    case "Rentals":
                        await ExportRentalsToDocx(body);
                        break;
                    default:
                        return BadRequest("Invalid table name provided.");
                }

                doc.Save();
            }

            stream.Position = 0;
            return File(stream, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", fileName);
        }

        private async Task ExportChargingStationsToDocx(Body body)
        {
            var stations = await _context.ChargingStations.AsNoTracking().ToListAsync();
            var table = CreateDocxTable(new[] { "Назва", "Розташування", "Кількість слотів", "Поточна кількість скутерів" });
            foreach (var station in stations)
            {
                table.Append(CreateDocxRow(new[] { station.Name, station.Location, station.ChargingSlots.ToString(), station.CurrentScooterCount.ToString() }));
            }
            body.Append(table);
        }

        private async Task ExportScootersToDocx(Body body)
        {
            var scooters = await _context.Scooters.Include(s => s.Status).AsNoTracking().ToListAsync();
            var table = CreateDocxTable(new[] { "Модель", "Рівень батареї", "Статус", "Поточне розташування", "Станція ID" });
            foreach (var scooter in scooters)
            {
                table.Append(CreateDocxRow(new[] { scooter.Model, scooter.BatteryLevel.ToString(), scooter.Status.Name, scooter.CurrentLocation, scooter.StationId?.ToString() }));
            }
            body.Append(table);
        }

        private async Task ExportRidersToDocx(Body body)
        {
            var riders = await _context.Riders.AsNoTracking().ToListAsync();
            var table = CreateDocxTable(new[] { "Ім'я", "Прізвище", "Номер телефону", "Дата реєстрації", "Баланс рахунку" });
            foreach (var rider in riders)
            {
                table.Append(CreateDocxRow(new[] { rider.FirstName, rider.LastName, rider.PhoneNumber, rider.RegistrationDate.ToString("yyyy-MM-dd"), rider.AccountBalance?.ToString() }));
            }
            body.Append(table);
        }

        private async Task ExportDiscountsToDocx(Body body)
        {
            var discounts = await _context.Discounts.AsNoTracking().ToListAsync();
            var table = CreateDocxTable(new[] { "Назва", "Відсоток знижки", "Опис" });
            foreach (var discount in discounts)
            {
                table.Append(CreateDocxRow(new[] { discount.Name, discount.Percentage.ToString(), discount.Description }));
            }
            body.Append(table);
        }

        private async Task ExportRentalsToDocx(Body body)
        {
            var rentals = await _context.Rentals.Include(r => r.Status).AsNoTracking().ToListAsync();
            var table = CreateDocxTable(new[] { "Rider ID", "Scooter ID", "Статус", "Час початку", "Час завершення", "Загальна вартість", "Дата оплати", "Сума оплати", "Payment Method ID" });
            foreach (var rental in rentals)
            {
                table.Append(CreateDocxRow(new[] { rental.RiderId.ToString(), rental.ScooterId.ToString(), rental.Status.Name, rental.StartTime.ToString("yyyy-MM-dd HH:mm"), rental.EndTime?.ToString("yyyy-MM-dd HH:mm"), rental.TotalCost.ToString(), rental.PaymentDate?.ToString("yyyy-MM-dd HH:mm"), rental.Amount?.ToString(), rental.PaymentMethodId?.ToString() }));
            }
            body.Append(table);
        }

        #endregion
    }
}