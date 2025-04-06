using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ScooterDomain.Model;

public partial class Rider : Entity, IValidatableObject
{
    [Required(ErrorMessage = "Поле \"Ім'я\" обов'язкове для заповнення")]
    [Display(Name = "Ім'я")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Ім'я має бути від 2 до 50 символів")]
    public string FirstName { get; set; } = null!;

    [Required(ErrorMessage = "Поле \"Прізвище\" обов'язкове для заповнення")]
    [Display(Name = "Прізвище")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Прізвище має бути від 2 до 50 символів")]
    public string LastName { get; set; } = null!;

    [Required(ErrorMessage = "Поле \"Номер телефону\" обов'язкове для заповнення")]
    [Display(Name = "Номер телефону")]
    [Phone(ErrorMessage = "Введіть коректний номер телефону")]
    [RegularExpression(@"^\+?[1-9]\d{9,14}$", ErrorMessage = "Номер телефону має бути у форматі +380xxxxxxxxx")]
    public string PhoneNumber { get; set; } = null!;

    [Required(ErrorMessage = "Поле \"Дата реєстрації\" обов'язкове для заповнення")]
    [Display(Name = "Дата реєстрації")]
    [DataType(DataType.Date, ErrorMessage = "Введіть коректну дату")]
    public DateOnly RegistrationDate { get; set; }

    [Display(Name = "Баланс рахунку")]
    [Range(0, double.MaxValue, ErrorMessage = "Баланс не може бути від'ємним")]
    public decimal? AccountBalance { get; set; }

    public string? ApplicationUserId { get; set; } // Нове поле для зв'язку з ApplicationUser
    public ApplicationUser? ApplicationUser { get; set; } // Навігаційна властивість

    public virtual ICollection<Rental> Rentals { get; set; } = new List<Rental>();
    public virtual ICollection<Discount> Discounts { get; set; } = new List<Discount>();

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // Визначаємо мінімальну допустиму дату (наприклад, 01.01.2000)
        var minDate = new DateOnly(2000, 1, 1);
        // Поточна дата (на момент валідації)
        var maxDate = DateOnly.FromDateTime(DateTime.Now);

        // Перевірка, чи дата не занадто в минулому
        if (RegistrationDate < minDate)
        {
            yield return new ValidationResult(
                $"Дата реєстрації не може бути раніше {minDate:dd.MM.yyyy}.",
                new[] { nameof(RegistrationDate) });
        }

        // Перевірка, чи дата не в майбутньому
        if (RegistrationDate > maxDate)
        {
            yield return new ValidationResult(
                "Дата реєстрації не може бути в майбутньому.",
                new[] { nameof(RegistrationDate) });
        }
    }
}