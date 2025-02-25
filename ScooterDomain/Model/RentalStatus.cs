using System;
using System.Collections.Generic;

namespace ScooterDomain.Model;

public partial class RentalStatus : Entity
{

    public string Name { get; set; } = null!;

    public virtual ICollection<Rental> Rentals { get; set; } = new List<Rental>();
}
