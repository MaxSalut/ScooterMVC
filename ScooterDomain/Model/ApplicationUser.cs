using Microsoft.AspNetCore.Identity;

namespace ScooterDomain.Model
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int? RiderId { get; set; }
        public Rider Rider { get; set; }

    }
}