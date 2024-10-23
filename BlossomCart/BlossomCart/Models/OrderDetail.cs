using System;
using System.Collections.Generic;

namespace BlossomCart.Models;

public partial class OrderDetail
{
    public int OrderDetailsId { get; set; }

    public int OrderId { get; set; }

    public int BouquetId { get; set; }

    public int Price { get; set; }

    public int Qty { get; set; }

    public long TotalPrice { get; set; }

    public int UserId { get; set; }

    public virtual Bouquet Bouquet { get; set; } = null!;

    public virtual Order Order { get; set; } = null!;

    public virtual Signup User { get; set; } = null!;
}
