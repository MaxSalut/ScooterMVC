using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ScooterDomain.Model;

public partial class PaymentMethod : Entity
{
    [Required(ErrorMessage = "Поле \"Назва\" обов'язкове для заповнення")]
    [Display(Name = "Назва")]
    [StringLength(100, ErrorMessage = "Назва не може бути довшою за 100 символів")]
    public string Name { get; set; } = null!;

    public virtual ICollection<Rental> Rentals { get; set; } = new List<Rental>();
}