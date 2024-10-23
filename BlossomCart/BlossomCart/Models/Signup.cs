using System;
using System.Collections.Generic;

namespace BlossomCart.Models;

public partial class Signup
{
    public int Id { get; set; }

    public string CustamerName { get; set; } = null!;

    public string EMail { get; set; } = null!;

    public string Password { get; set; } = null!;

    public long PhoneNumber { get; set; }

    public string Token { get; set; } = null!;

    public int Role { get; set; }

    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<PaymentTable> PaymentTables { get; set; } = new List<PaymentTable>();
}
