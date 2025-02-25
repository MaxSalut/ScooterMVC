using System;
using System.Collections.Generic;

namespace ScooterDomain.Model;

public partial class Discount : Entity
{

    public string Name { get; set; } = null!;

    public decimal Percentage { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<Rider> Riders { get; set; } = new List<Rider>();
}
