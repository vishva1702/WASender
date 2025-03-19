using System;
using System.Collections.Generic;

namespace WASender.Models;

public partial class Schedulemessage
{
    public ulong Id { get; set; }

    public ulong? UserId { get; set; }

    public ulong? DeviceId { get; set; }

    public ulong? TemplateId { get; set; }

    public string? Title { get; set; }

    public string? Body { get; set; }

    public DateTime? ScheduleAt { get; set; }

    public string? Zone { get; set; }

    public DateOnly? Date { get; set; }

    public DateOnly? Time { get; set; }

    public string Status { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Device? Device { get; set; }

    public virtual ICollection<Schedulecontact> Schedulecontacts { get; set; } = new List<Schedulecontact>();

    public virtual Template? Template { get; set; }

    public virtual User? User { get; set; }
}
