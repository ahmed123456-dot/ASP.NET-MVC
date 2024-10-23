using System;
using System.Collections.Generic;

namespace BlossomCart.Models;

public partial class PaymentTable
{
    public int PaymentId { get; set; }

    public string HolderName { get; set; } = null!;

    public int OrderId { get; set; }

    public long CardNumber { get; set; }

    public int Cvc { get; set; }

    public DateTime Date { get; set; }

    public int UserId { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual Signup User { get; set; } = null!;
}
