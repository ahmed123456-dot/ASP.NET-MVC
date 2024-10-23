using System;
using System.Collections.Generic;

namespace BlossomCart.Models;

public partial class Cart
{
    public int Id { get; set; }

    public int BouquetId { get; set; }

    public int Price { get; set; }

    public int UserId { get; set; }

    public int Qty { get; set; }

    public int TotalPrice { get; set; }

    public virtual Bouquet Bouquet { get; set; } = null!;

    public virtual Signup User { get; set; } = null!;
}
