using System.ComponentModel.DataAnnotations;

namespace ScooterInfrastructure.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Поле \"Email\" обов'язкове")]
        [EmailAddress(ErrorMessage = "Введіть коректний email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Поле \"Пароль\" обов'язкове")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Запам'ятати мене")]
        public bool RememberMe { get; set; }
    }
}