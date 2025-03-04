using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ScooterDomain.Model;

public partial class ChargingStation : Entity
{
    [Required(ErrorMessage = "Поле \"Назва\" обов'язкове для заповнення")]
    [Display(Name = "Назва")]
    [StringLength(100, ErrorMessage = "Назва не може бути довшою за 100 символів")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "Поле \"Розташування\" обов'язкове для заповнення")]
    [Display(Name = "Розташування")]
    [StringLength(255, ErrorMessage = "Розташування не може бути довшим за 255 символів")]
    public string Location { get; set; } = null!;

    [Required(ErrorMessage = "Поле \"Кількість слотів\" обов'язкове для заповнення")]
    [Display(Name = "Кількість слотів")]
    [Range(1, int.MaxValue, ErrorMessage = "Кількість слотів має бути більшою за 0")]
    public int ChargingSlots { get; set; }

    [Required(ErrorMessage = "Поле \"Поточна кількість скутерів\" обов'язкове для заповнення")]
    [Display(Name = "Поточна кількість скутерів")]
    [Range(0, int.MaxValue, ErrorMessage = "Кількість скутерів не може бути від'ємною")]
    public int CurrentScooterCount { get; set; }

    public virtual ICollection<Scooter> Scooters { get; set; } = new List<Scooter>();
}