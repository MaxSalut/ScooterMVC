using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ScooterDomain.Model;

public partial class Discount : Entity
{
    [Required(ErrorMessage = "Поле \"Назва\" обов'язкове для заповнення")]
    [Display(Name = "Назва")]
    [StringLength(100, ErrorMessage = "Назва не може бути довшою за 100 символів")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "Поле \"Відсоток знижки\" обов'язкове для заповнення")]
    [Display(Name = "Відсоток знижки")]
    [Range(0, 100, ErrorMessage = "Відсоток знижки має бути від 0 до 100")]
    public decimal Percentage { get; set; }

    [Display(Name = "Опис")]
    public string? Description { get; set; }

    public virtual ICollection<Rider> Riders { get; set; } = new List<Rider>();
}