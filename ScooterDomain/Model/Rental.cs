using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ScooterDomain.Model;

public partial class Rental : Entity, IValidatableObject
{
    [Required(ErrorMessage = "Поле \"Користувач\" обов'язкове для заповнення")]
    [Display(Name = "Користувач")]
    public int RiderId { get; set; }

    [Required(ErrorMessage = "Поле \"Скутер\" обов'язкове для заповнення")]
    [Display(Name = "Скутер")]
    public int ScooterId { get; set; }

    [Required(ErrorMessage = "Поле \"Статус\" обов'язкове для заповнення")]
    [Display(Name = "Статус")]
    public int StatusId { get; set; }

    [Required(ErrorMessage = "Поле \"Час початку\" обов'язкове для заповнення")]
    [Display(Name = "Час початку")]
    [DataType(DataType.DateTime, ErrorMessage = "Введіть коректну дату та час")]
    public DateTime StartTime { get; set; }

    [Display(Name = "Час завершення")]
    [DataType(DataType.DateTime, ErrorMessage = "Введіть коректну дату та час")]
    public DateTime? EndTime { get; set; }

    [Required(ErrorMessage = "Поле \"Загальна вартість\" обов'язкове для заповнення")]
    [Display(Name = "Загальна вартість")]
    [Range(0, double.MaxValue, ErrorMessage = "Вартість не може бути від'ємною")]
    public decimal TotalCost { get; set; }

    [Display(Name = "Дата оплати")]
    [DataType(DataType.DateTime, ErrorMessage = "Введіть коректну дату та час")]
    public DateTime? PaymentDate { get; set; }

    [Display(Name = "Сума оплати")]
    [Range(0, double.MaxValue, ErrorMessage = "Сума не може бути від'ємною")]
    public decimal? Amount { get; set; }

    [Display(Name = "Спосіб оплати")]
    public int? PaymentMethodId { get; set; }

    public virtual PaymentMethod? PaymentMethod { get; set; }
    public virtual Rider Rider { get; set; } = null!;
    public virtual Scooter Scooter { get; set; } = null!;
    public virtual RentalStatus Status { get; set; } = null!;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // Статуси з RentalStatuses: 1 - Активна, 2 - Завершена, 3 - Скасована
        const int ActiveStatusId = 1;
        const int CompletedStatusId = 2;
        const int CancelledStatusId = 3;

        // Перевірка часу завершення
        if (EndTime.HasValue && EndTime <= StartTime)
        {
            yield return new ValidationResult(
                "Час завершення не може бути меншим або дорівнювати часу початку.",
                new[] { nameof(EndTime) });
        }

        if (StatusId == ActiveStatusId)
        {
            // Для статусу "Активна"
            if (EndTime.HasValue)
            {
                yield return new ValidationResult(
                    "Час завершення має бути відсутнім для активної оренди.",
                    new[] { nameof(EndTime) });
            }

            if (PaymentDate.HasValue)
            {
                yield return new ValidationResult(
                    "Дата оплати має бути відсутньою для активної оренди.",
                    new[] { nameof(PaymentDate) });
            }

            if (Amount.HasValue)
            {
                yield return new ValidationResult(
                    "Сума оплати має бути відсутньою для активної оренди.",
                    new[] { nameof(Amount) });
            }

            if (PaymentMethodId.HasValue)
            {
                yield return new ValidationResult(
                    "Спосіб оплати має бути відсутнім для активної оренди.",
                    new[] { nameof(PaymentMethodId) });
            }
        }
        else if (StatusId == CompletedStatusId)
        {
            // Для статусу "Завершена"
            if (!EndTime.HasValue)
            {
                yield return new ValidationResult(
                    "Час завершення обов'язковий для завершеної оренди.",
                    new[] { nameof(EndTime) });
            }

            if (!PaymentDate.HasValue)
            {
                yield return new ValidationResult(
                    "Дата оплати обов'язкова для завершеної оренди.",
                    new[] { nameof(PaymentDate) });
            }

            if (!Amount.HasValue)
            {
                yield return new ValidationResult(
                    "Сума оплати обов'язкова для завершеної оренди.",
                    new[] { nameof(Amount) });
            }

            if (!PaymentMethodId.HasValue)
            {
                yield return new ValidationResult(
                    "Спосіб оплати обов'язковий для завершеної оренди.",
                    new[] { nameof(PaymentMethodId) });
            }

            if (Amount.HasValue && Amount != TotalCost)
            {
                yield return new ValidationResult(
                    "Сума оплати має дорівнювати загальній вартості для завершеної оренди.",
                    new[] { nameof(Amount) });
            }

            if (TotalCost <= 0)
            {
                yield return new ValidationResult(
                    "Загальна вартість має бути більшою за 0 для завершеної оренди.",
                    new[] { nameof(TotalCost) });
            }
        }
        else if (StatusId == CancelledStatusId)
        {
            // Для статусу "Скасована"
            if (!EndTime.HasValue)
            {
                yield return new ValidationResult(
                    "Час завершення обов'язковий для скасованої оренди.",
                    new[] { nameof(EndTime) });
            }

            if (PaymentDate.HasValue)
            {
                yield return new ValidationResult(
                    "Дата оплати має бути відсутньою для скасованої оренди.",
                    new[] { nameof(PaymentDate) });
            }

            if (Amount.HasValue)
            {
                yield return new ValidationResult(
                    "Сума оплати має бути відсутньою для скасованої оренди.",
                    new[] { nameof(Amount) });
            }

            if (PaymentMethodId.HasValue)
            {
                yield return new ValidationResult(
                    "Спосіб оплати має бути відсутнім для скасованої оренди.",
                    new[] { nameof(PaymentMethodId) });
            }
        }
        else
        {
            yield return new ValidationResult(
                "Невірний статус оренди. Допустимі значення: Активна (1), Завершена (2), Скасована (3).",
                new[] { nameof(StatusId) });
        }
    }
}