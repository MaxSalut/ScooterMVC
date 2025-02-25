using System;
using System.Collections.Generic;

namespace ScooterDomain.Model;

public partial class ChargingStation : Entity
{
   

    public string Name { get; set; } = null!;

    public string Location { get; set; } = null!;

    public int ChargingSlots { get; set; }

    public int CurrentScooterCount { get; set; }

    public virtual ICollection<Scooter> Scooters { get; set; } = new List<Scooter>();
}
