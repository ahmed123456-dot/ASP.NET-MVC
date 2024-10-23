using System;
using System.Collections.Generic;

namespace BlossomCart.Models;

public partial class Bouquet
{
    public int BouquetId { get; set; }

    public string BouquetName { get; set; } = null!;

    public string BouquetDescription { get; set; } = null!;

    public int Price { get; set; }

    public string Image { get; set; } = null!;

    public int? CategoryId { get; set; }

    public int? Status { get; set; }

    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();

    public virtual Category? Category { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}
