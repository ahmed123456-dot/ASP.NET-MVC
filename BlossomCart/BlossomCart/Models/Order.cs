using System;
using System.Collections.Generic;

namespace BlossomCart.Models;

public partial class Order
{
    public int OrderId { get; set; }

    public int UserId { get; set; }

    public string Address { get; set; } = null!;

    public DateTime OrderDate { get; set; }

    public TimeSpan Time { get; set; }

    public string Status { get; set; } = null!;

    public string PaymentType { get; set; } = null!;

    public int Price { get; set; }

    public string PaymentStatus { get; set; } = null!;

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual ICollection<PaymentTable> PaymentTables { get; set; } = new List<PaymentTable>();

    public virtual Signup User { get; set; } = null!;
}
