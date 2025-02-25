using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ScooterDomain.Model;

public partial class Rental : Entity
{
    [Required(ErrorMessage = "Поле \"Райдер\" обов'язкове")]
    [Display(Name = "Райдер")]
    public int RiderId { get; set; }

    [Required(ErrorMessage = "Поле \"Скутер\" обов'язкове")]
    [Display(Name = "Скутер")]
    public int ScooterId { get; set; }

    [Required(ErrorMessage = "Поле \"Статус\" обов'язкове")]
    [Display(Name = "Статус")]
    public int StatusId { get; set; }

    [Required(ErrorMessage = "Поле \"Час початку\" обов'язкове")]
    [Display(Name = "Час початку")]
    [DataType(DataType.DateTime, ErrorMessage = "Введіть коректну дату та час")]
    public DateTime StartTime { get; set; }

    [Display(Name = "Час завершення")]
    [DataType(DataType.DateTime, ErrorMessage = "Введіть коректну дату та час")]
    public DateTime? EndTime { get; set; }

    [Required(ErrorMessage = "Поле \"Загальна вартість\" обов'язкове")]
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
}