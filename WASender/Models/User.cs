using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace WASender.Models;

public partial class User
{
    public ulong Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Avatar { get; set; }

    public string? Authkey { get; set; }

    public string? Wallet { get; set; }

    public string? Role { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? Phone { get; set; }

    public string? Address { get; set; }

    public DateTime? EmailVerifiedAt { get; set; }

    public string Password { get; set; } = null!;

    public int? Status { get; set; }

    public string? Meta { get; set; }

    public string? Plan { get; set; }

    public int? PlanId { get; set; }

    public DateOnly? WillExpire { get; set; }

    public string? RememberToken { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<App> Apps { get; set; } = new List<App>();

    public virtual ICollection<Contact> Contacts { get; set; } = new List<Contact>();

    public virtual ICollection<Deviceorder> Deviceorders { get; set; } = new List<Deviceorder>();

    public virtual ICollection<Device> Devices { get; set; } = new List<Device>();

    public virtual ICollection<Group> Groups { get; set; } = new List<Group>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<Reply> Replies { get; set; } = new List<Reply>();

    public virtual ICollection<Schedulemessage> Schedulemessages { get; set; } = new List<Schedulemessage>();

    public virtual ICollection<Smstesttransaction> Smstesttransactions { get; set; } = new List<Smstesttransaction>();

    public virtual ICollection<Smstransaction> Smstransactions { get; set; } = new List<Smstransaction>();

    public virtual ICollection<Supportlog> Supportlogs { get; set; } = new List<Supportlog>();

    public virtual ICollection<Support> Supports { get; set; } = new List<Support>();

    public virtual ICollection<Template> Templates { get; set; } = new List<Template>();

    public virtual ICollection<Webhook> Webhooks { get; set; } = new List<Webhook>();
}