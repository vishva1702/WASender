﻿using System;
using System.Collections.Generic;

namespace WASender.Models;

public partial class Role
{
    public ulong Id { get; set; }

    public string Name { get; set; } = null!;

    public string GuardName { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<ModelHasRole> ModelHasRoles { get; set; } = new List<ModelHasRole>();

    public virtual ICollection<Permission> Permissions { get; set; } = new List<Permission>();
}
