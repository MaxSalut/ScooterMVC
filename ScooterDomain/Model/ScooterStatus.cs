using System;
using System.Collections.Generic;

namespace ScooterDomain.Model;

public partial class ScooterStatus : Entity
{
    public string Name { get; set; } = null!;

    public virtual ICollection<Scooter> Scooters { get; set; } = new List<Scooter>();
}
