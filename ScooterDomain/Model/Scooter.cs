using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ScooterDomain.Model;

public partial class Scooter : Entity
{

    [Required(ErrorMessage = "Модель обов'язкова")]
    public string Model { get; set; }

    [Range(0, 100, ErrorMessage = "Рівень батареї повинен бути від 0 до 100")]
    public int BatteryLevel { get; set; }

    [Required(ErrorMessage = "Статус обов'язковий")]
    public int StatusId { get; set; }

    public string? CurrentLocation { get; set; }

    public int? StationId { get; set; }

    public virtual ICollection<Rental> Rentals { get; set; } = new List<Rental>();

    public virtual ChargingStation? Station { get; set; }

    public virtual ScooterStatus Status { get; set; } = null!;
}
