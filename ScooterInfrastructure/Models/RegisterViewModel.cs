using System.ComponentModel.DataAnnotations;

namespace ScooterInfrastructure.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Поле \"Email\" обов'язкове")]
        [EmailAddress(ErrorMessage = "Введіть коректний email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Поле \"Пароль\" обов'язкове")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Пароль має бути від 8 до 100 символів")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Паролі не збігаються")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Поле \"Ім'я\" обов'язкове")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Ім'я має бути від 2 до 50 символів")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Поле \"Прізвище\" обов'язкове")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Прізвище має бути від 2 до 50 символів")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Поле \"Номер телефону\" обов'язкове")]
        [Phone(ErrorMessage = "Введіть коректний номер телефону")]
        public string PhoneNumber { get; set; }
    }
}