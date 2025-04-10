using System;
using System.Collections.Generic;

namespace WASender.Models;

public partial class Order
{
    public ulong Id { get; set; }

    public string? InvoiceNo { get; set; }

    public string? PaymentId { get; set; }

    public ulong PlanId { get; set; }

    public ulong UserId { get; set; }

    public ulong GatewayId { get; set; }

    public double? Amount { get; set; }

    public double? Tax { get; set; }

    public int? Status { get; set; } 

    public DateOnly? WillExpire { get; set; }

    public string? Meta { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Gateway Gateway { get; set; } = null!;

    public virtual Plan Plan { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
