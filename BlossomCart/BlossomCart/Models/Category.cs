using System;
using System.Collections.Generic;

namespace BlossomCart.Models;

public partial class Category
{
    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = null!;

    public virtual ICollection<Bouquet> Bouquets { get; set; } = new List<Bouquet>();
}
