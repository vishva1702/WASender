using System;
using System.Collections.Generic;

namespace WASender.Models;

public partial class Schedulecontact
{
    public ulong Id { get; set; }

    public ulong? ContactId { get; set; }

    public ulong? SchedulemessageId { get; set; }

    public int? StatusCode { get; set; }

    public virtual Contact? Contact { get; set; }

    public virtual Schedulemessage? Schedulemessage { get; set; }
}
