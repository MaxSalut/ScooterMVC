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
using NuGet.Packaging;

namespace ScooterInfrastructure.Controllers
{
    public partial class ReportsController : Controller
    {
        #region Helper Methods

        private bool IsFileValid(IFormFile file) => file != null && file.Length > 0;

        private bool IsWorksheetValid(ExcelWorksheet worksheet) => worksheet != null && worksheet.Dimension != null;

        private bool ValidateExcelStructure(ExcelWorksheet worksheet, string[] expectedHeaders)
        {
            if (worksheet.Dimension.Columns < expectedHeaders.Length)
                return false;
            for (int i = 0; i < expectedHeaders.Length; i++)
            {
                if (worksheet.Cells[1, i + 1].Value?.ToString() != expectedHeaders[i])
                    return false;
            }
            return true;
        }

        private void ProcessEntity<T>(T entity, int row) where T : Entity
        {
            if (ValidateModel(entity, out var validationResults))
            {
                _context.Add(entity);
            }
            else
            {
                foreach (var error in validationResults)
                {
                    ModelState.AddModelError("", $"Рядок {row}: {error.ErrorMessage}");
                }
            }
        }

        private void ProcessEntity<T>(T entity, string line) where T : Entity
        {
            if (ValidateModel(entity, out var validationResults))
            {
                _context.Add(entity);
            }
            else
            {
                foreach (var error in validationResults)
                {
                    ModelState.AddModelError("", $"Рядок '{line}': {error.ErrorMessage}");
                }
            }
        }

        private bool ValidateModel<T>(T model, out List<ValidationResult> validationResults)
        {
            validationResults = new List<ValidationResult>();
            var context = new ValidationContext(model);
            return Validator.TryValidateObject(model, context, validationResults, true);
        }

        private async Task<string> SaveUploadedFile(IFormFile file)
        {
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var savePath = Path.Combine(ReportsDirectory, fileName);
            Directory.CreateDirectory(ReportsDirectory);
            using (var stream = new FileStream(savePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            return savePath;
        }

        private void SetExcelHeaders(ExcelWorksheet worksheet, string[] headers)
        {
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cells[1, i + 1].Value = headers[i];
            }
        }

        private void AddStatusValidation(ExcelWorksheet worksheet, int count, List<string> statuses, string column)
        {
            var validation = worksheet.DataValidations.AddListValidation($"{column}2:{column}{count + 1}");
            validation.Formula.Values.AddRange(statuses);
        }

        private void AddDocxTitle(Body body, string titleText)
        {
            var title = body.AppendChild(new Paragraph());
            var run = title.AppendChild(new Run());
            run.AppendChild(new Text(titleText));
        }

        private Table CreateDocxTable(string[] headers)
        {
            var table = new Table();
            var headerRow = new TableRow();
            foreach (var header in headers)
            {
                headerRow.Append(new TableCell(new Paragraph(new Run(new Text(header)))));
            }
            table.Append(headerRow);
            return table;
        }

        private TableRow CreateDocxRow(string[] values)
        {
            var row = new TableRow();
            foreach (var value in values)
            {
                row.Append(new TableCell(new Paragraph(new Run(new Text(value ?? "")))));
            }
            return row;
        }

        private async Task<int> GetScooterStatusIdFromName(string statusName)
        {
            if (string.IsNullOrEmpty(statusName)) return 1; // "Доступний" за замовчуванням
            var status = await _context.ScooterStatuses.FirstOrDefaultAsync(s => s.Name == statusName);
            return status?.Id ?? 1;
        }

        private async Task<int> GetRentalStatusIdFromName(string statusName)
        {
            if (string.IsNullOrEmpty(statusName)) return 1; // "Активна" за замовчуванням
            var status = await _context.RentalStatuses.FirstOrDefaultAsync(s => s.Name == statusName);
            return status?.Id ?? 1;
        }

        #endregion
    }
}