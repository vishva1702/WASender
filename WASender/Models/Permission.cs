using System;
using System.Collections.Generic;

namespace WASender.Models;

public partial class Permission
{
    public ulong Id { get; set; }

    public string Name { get; set; } = null!;

    public string GuardName { get; set; } = null!;

    public string GroupName { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<ModelHasPermission> ModelHasPermissions { get; set; } = new List<ModelHasPermission>();

    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();
}
