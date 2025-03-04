using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ScooterDomain.Model;

public partial class Scooter : Entity
{
    [Required(ErrorMessage = "Поле \"Модель\" обов'язкове для заповнення")]
    [Display(Name = "Модель")]
    [StringLength(100, ErrorMessage = "Модель не може бути довшою за 100 символів")]
    public string Model { get; set; } = null!;

    [Required(ErrorMessage = "Поле \"Рівень батареї\" обов'язкове для заповнення")]
    [Display(Name = "Рівень батареї")]
    [Range(0, 100, ErrorMessage = "Рівень батареї повинен бути від 0 до 100")]
    public int BatteryLevel { get; set; }

    [Required(ErrorMessage = "Поле \"Статус\" обов'язкове для заповнення")]
    [Display(Name = "Статус")]
    public int StatusId { get; set; }

    [Display(Name = "Поточне розташування")]
    [StringLength(255, ErrorMessage = "Розташування не може бути довшим за 255 символів")]
    public string? CurrentLocation { get; set; }

    [Display(Name = "Станція")]
    public int? StationId { get; set; }

    public virtual ICollection<Rental> Rentals { get; set; } = new List<Rental>();
    public virtual ChargingStation? Station { get; set; }
    public virtual ScooterStatus Status { get; set; } = null!;
}