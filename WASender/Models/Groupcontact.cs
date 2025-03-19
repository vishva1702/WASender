using System;
using System.Collections.Generic;

namespace WASender.Models;

public partial class Groupcontact
{
    public ulong GroupId { get; set; }

    public ulong ContactId { get; set; }

    public virtual Contact Contact { get; set; } = null!;

    public virtual Group Group { get; set; } = null!;
}
