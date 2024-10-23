using System;
using System.Collections.Generic;

namespace BlossomCart.Models;

public partial class Feedback
{
    public int FId { get; set; }

    public string CustomerName { get; set; } = null!;

    public string CustomerEmail { get; set; } = null!;

    public string Feedback1 { get; set; } = null!;

    public DateTime Date { get; set; }
}
