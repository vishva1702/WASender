using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace WASender.Models;

public partial class ApplicationDbContext : DbContext
{
    public ApplicationDbContext()
    {
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<App> Apps { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Categorymeta> Categorymetas { get; set; }

    public virtual DbSet<Contact> Contacts { get; set; }

    public virtual DbSet<Device> Devices { get; set; }

    public virtual DbSet<Deviceorder> Deviceorders { get; set; }

    public virtual DbSet<FailedJob> FailedJobs { get; set; }

    public virtual DbSet<Gateway> Gateways { get; set; }

    public virtual DbSet<Group> Groups { get; set; }

    public virtual DbSet<Groupcontact> Groupcontacts { get; set; }

    public virtual DbSet<Job> Jobs { get; set; }

    public virtual DbSet<Menu> Menus { get; set; }

    public virtual DbSet<Migration> Migrations { get; set; }

    public virtual DbSet<ModelHasPermission> ModelHasPermissions { get; set; }

    public virtual DbSet<ModelHasRole> ModelHasRoles { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Option> Options { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<PasswordReset> PasswordResets { get; set; }

    public virtual DbSet<Permission> Permissions { get; set; }

    public virtual DbSet<PersonalAccessToken> PersonalAccessTokens { get; set; }

    public virtual DbSet<Plan> Plans { get; set; }

    public virtual DbSet<Post> Posts { get; set; }

    public virtual DbSet<Postcategory> Postcategories { get; set; }

    public virtual DbSet<Postmeta> Postmetas { get; set; }

    public virtual DbSet<Reply> Replies { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Schedulecontact> Schedulecontacts { get; set; }

    public virtual DbSet<Schedulemessage> Schedulemessages { get; set; }

    public virtual DbSet<Smstesttransaction> Smstesttransactions { get; set; }

    public virtual DbSet<Smstransaction> Smstransactions { get; set; }

    public virtual DbSet<Support> Supports { get; set; }

    public virtual DbSet<Supportlog> Supportlogs { get; set; }

    public virtual DbSet<Template> Templates { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Webhook> Webhooks { get; set; }
    public object WhyChoose { get; internal set; }
    public IEnumerable<object> Features { get; internal set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseMySql("server=localhost;database=wasender;user=root", Microsoft.EntityFrameworkCore.ServerVersion.Parse("10.4.32-mariadb"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_general_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<App>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("apps")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.DeviceId, "apps_device_id_foreign");

            entity.HasIndex(e => e.Key, "apps_key_unique").IsUnique();

            entity.HasIndex(e => e.UserId, "apps_user_id_foreign");

            entity.HasIndex(e => e.Uuid, "apps_uuid_unique").IsUnique();

            entity.Property(e => e.Id)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.DeviceId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("device_id");
            entity.Property(e => e.Key)
                .HasMaxLength(191)
                .HasColumnName("key");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'1'")
                .HasColumnType("int(11)")
                .HasColumnName("status");
            entity.Property(e => e.Title)
                .HasMaxLength(191)
                .HasColumnName("title");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("user_id");
            entity.Property(e => e.Uuid).HasColumnName("uuid");
            entity.Property(e => e.Website)
                .HasMaxLength(191)
                .HasColumnName("website");

            entity.HasOne(d => d.Device).WithMany(p => p.Apps)
                .HasForeignKey(d => d.DeviceId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("apps_device_id_foreign");

            entity.HasOne(d => d.User).WithMany(p => p.Apps)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("apps_user_id_foreign");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("categories")
                .UseCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.Id)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.IsFeatured)
                .HasDefaultValueSql("'1'")
                .HasColumnType("int(11)")
                .HasColumnName("is_featured");
            entity.Property(e => e.Lang)
                .HasMaxLength(191)
                .HasDefaultValueSql("'en'")
                .HasColumnName("lang");
            entity.Property(e => e.Slug)
                .HasMaxLength(191)
                .HasColumnName("slug");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'1'")
                .HasColumnType("int(11)")
                .HasColumnName("status");
            entity.Property(e => e.Title)
                .HasMaxLength(191)
                .HasColumnName("title");
            entity.Property(e => e.Type)
                .HasMaxLength(191)
                .HasDefaultValueSql("'category'")
                .HasColumnName("type");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<Categorymeta>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("categorymetas")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.CategoryId, "categorymetas_category_id_foreign");

            entity.Property(e => e.Id)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("id");
            entity.Property(e => e.CategoryId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("category_id");
            entity.Property(e => e.Type)
                .HasMaxLength(191)
                .HasColumnName("type");
            entity.Property(e => e.Value)
                .HasColumnType("text")
                .HasColumnName("value");

            entity.HasOne(d => d.Category).WithMany(p => p.Categorymeta)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("categorymetas_category_id_foreign");
        });

        modelBuilder.Entity<Contact>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("contacts")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.UserId, "contacts_user_id_foreign");

            entity.Property(e => e.Id)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.Name)
                .HasMaxLength(191)
                .HasColumnName("name");
            entity.Property(e => e.Phone)
                .HasMaxLength(191)
                .HasColumnName("phone");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Contacts)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("contacts_user_id_foreign");
        });

        modelBuilder.Entity<Device>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("devices")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.UserId, "devices_user_id_foreign");

            entity.HasIndex(e => e.Uuid, "devices_uuid_unique").IsUnique();

            entity.Property(e => e.Id)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.HookUrl)
                .HasColumnType("text")
                .HasColumnName("hook_url");
            entity.Property(e => e.Meta)
                .HasColumnType("text")
                .HasColumnName("meta");
            entity.Property(e => e.Name)
                .HasMaxLength(191)
                .HasColumnName("name");
            entity.Property(e => e.Phone)
                .HasMaxLength(191)
                .HasColumnName("phone");
            entity.Property(e => e.Qr)
                .HasColumnType("text")
                .HasColumnName("qr");
            entity.Property(e => e.Status)
                .HasColumnType("int(11)")
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("user_id");
            entity.Property(e => e.UserName)
                .HasMaxLength(191)
                .HasColumnName("user_name");
            entity.Property(e => e.Uuid).HasColumnName("uuid");

            entity.HasOne(d => d.User).WithMany(p => p.Devices)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("devices_user_id_foreign");
        });

        modelBuilder.Entity<Deviceorder>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("deviceorders")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.UserId, "deviceorders_user_id_foreign");

            entity.Property(e => e.Id)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("id");
            entity.Property(e => e.Amount).HasColumnName("amount");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.Phone)
                .HasColumnType("int(11)")
                .HasColumnName("phone");
            entity.Property(e => e.Status)
                .HasMaxLength(191)
                .HasDefaultValueSql("'pending'")
                .HasColumnName("status");
            entity.Property(e => e.Trx)
                .HasMaxLength(191)
                .HasColumnName("trx");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Deviceorders)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("deviceorders_user_id_foreign");
        });

        modelBuilder.Entity<FailedJob>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("failed_jobs")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.Uuid, "failed_jobs_uuid_unique").IsUnique();

            entity.Property(e => e.Id)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("id");
            entity.Property(e => e.Connection)
                .HasColumnType("text")
                .HasColumnName("connection");
            entity.Property(e => e.Exception).HasColumnName("exception");
            entity.Property(e => e.FailedAt)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("timestamp")
                .HasColumnName("failed_at");
            entity.Property(e => e.Payload).HasColumnName("payload");
            entity.Property(e => e.Queue)
                .HasColumnType("text")
                .HasColumnName("queue");
            entity.Property(e => e.Uuid)
                .HasMaxLength(191)
                .HasColumnName("uuid");
        });

        modelBuilder.Entity<Gateway>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("gateways")
                .UseCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.Id)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("id");
            entity.Property(e => e.Charge).HasColumnName("charge");
            entity.Property(e => e.Comment)
                .HasColumnType("text")
                .HasColumnName("comment");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.Currency)
                .HasMaxLength(191)
                .HasColumnName("currency");
            entity.Property(e => e.Data)
                .HasColumnType("text")
                .HasColumnName("data");
            entity.Property(e => e.ImageAccept)
                .HasColumnType("int(11)")
                .HasColumnName("image_accept");
            entity.Property(e => e.IsAuto)
                .HasColumnType("int(11)")
                .HasColumnName("is_auto");
            entity.Property(e => e.Logo)
                .HasMaxLength(191)
                .HasColumnName("logo");
            entity.Property(e => e.MaxAmount)
                .HasDefaultValueSql("'1000'")
                .HasColumnName("max_amount");
            entity.Property(e => e.MinAmount)
                .HasDefaultValueSql("'1'")
                .HasColumnName("min_amount");
            entity.Property(e => e.Multiply)
                .HasDefaultValueSql("'1'")
                .HasColumnName("multiply");
            entity.Property(e => e.Name)
                .HasMaxLength(191)
                .HasColumnName("name");
            entity.Property(e => e.Namespace)
                .HasMaxLength(191)
                .HasColumnName("namespace");
            entity.Property(e => e.PhoneRequired)
                .HasColumnType("int(11)")
                .HasColumnName("phone_required");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'1'")
                .HasColumnType("int(11)")
                .HasColumnName("status");
            entity.Property(e => e.TestMode)
                .HasColumnType("int(11)")
                .HasColumnName("test_mode");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<Group>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("groups")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.UserId, "groups_user_id_foreign");

            entity.Property(e => e.Id)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.Name)
                .HasMaxLength(191)
                .HasColumnName("name");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Groups)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("groups_user_id_foreign");
        });

        modelBuilder.Entity<Groupcontact>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("groupcontacts")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.ContactId, "groupcontacts_contact_id_foreign");

            entity.HasIndex(e => e.GroupId, "groupcontacts_group_id_foreign");

            entity.Property(e => e.ContactId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("contact_id");
            entity.Property(e => e.GroupId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("group_id");

            entity.HasOne(d => d.Contact).WithMany()
                .HasForeignKey(d => d.ContactId)
                .HasConstraintName("groupcontacts_contact_id_foreign");

            entity.HasOne(d => d.Group).WithMany()
                .HasForeignKey(d => d.GroupId)
                .HasConstraintName("groupcontacts_group_id_foreign");
        });

        modelBuilder.Entity<Job>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("jobs")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.Queue, "jobs_queue_index");

            entity.Property(e => e.Id)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("id");
            entity.Property(e => e.Attempts)
                .HasColumnType("tinyint(3) unsigned")
                .HasColumnName("attempts");
            entity.Property(e => e.AvailableAt)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("available_at");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("created_at");
            entity.Property(e => e.Payload).HasColumnName("payload");
            entity.Property(e => e.Queue)
                .HasMaxLength(191)
                .HasColumnName("queue");
            entity.Property(e => e.ReservedAt)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("reserved_at");
        });

        modelBuilder.Entity<Menu>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("menus")
                .UseCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.Id)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.Data)
                .HasColumnType("text")
                .HasColumnName("data");
            entity.Property(e => e.Lang)
                .HasMaxLength(191)
                .HasDefaultValueSql("'en'")
                .HasColumnName("lang");
            entity.Property(e => e.Name)
                .HasMaxLength(191)
                .HasColumnName("name");
            entity.Property(e => e.Position)
                .HasMaxLength(191)
                .HasColumnName("position");
            entity.Property(e => e.Status)
                .HasMaxLength(191)
                .HasDefaultValueSql("'1'")
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<Migration>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("migrations")
                .UseCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.Id)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("id");
            entity.Property(e => e.Batch)
                .HasColumnType("int(11)")
                .HasColumnName("batch");
            entity.Property(e => e.Migration1)
                .HasMaxLength(191)
                .HasColumnName("migration");
        });

        modelBuilder.Entity<ModelHasPermission>(entity =>
        {
            entity.HasKey(e => new { e.PermissionId, e.ModelId, e.ModelType })
                .HasName("PRIMARY")
                .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0, 0 });

            entity
                .ToTable("model_has_permissions")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => new { e.ModelId, e.ModelType }, "model_has_permissions_model_id_model_type_index");

            entity.Property(e => e.PermissionId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("permission_id");
            entity.Property(e => e.ModelId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("model_id");
            entity.Property(e => e.ModelType)
                .HasMaxLength(191)
                .HasColumnName("model_type");

            entity.HasOne(d => d.Permission).WithMany(p => p.ModelHasPermissions)
                .HasForeignKey(d => d.PermissionId)
                .HasConstraintName("model_has_permissions_permission_id_foreign");
        });

        modelBuilder.Entity<ModelHasRole>(entity =>
        {
            entity.HasKey(e => new { e.RoleId, e.ModelId, e.ModelType })
                .HasName("PRIMARY")
                .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0, 0 });

            entity
                .ToTable("model_has_roles")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => new { e.ModelId, e.ModelType }, "model_has_roles_model_id_model_type_index");

            entity.Property(e => e.RoleId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("role_id");
            entity.Property(e => e.ModelId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("model_id");
            entity.Property(e => e.ModelType)
                .HasMaxLength(191)
                .HasColumnName("model_type");

            entity.HasOne(d => d.Role).WithMany(p => p.ModelHasRoles)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("model_has_roles_role_id_foreign");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("notifications")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.UserId, "notifications_user_id_foreign");

            entity.Property(e => e.Id)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("id");
            entity.Property(e => e.Comment)
                .HasColumnType("text")
                .HasColumnName("comment");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.IsAdmin)
                .HasColumnType("int(11)")
                .HasColumnName("is_admin");
            entity.Property(e => e.Seen)
                .HasColumnType("int(11)")
                .HasColumnName("seen");
            entity.Property(e => e.Title)
                .HasMaxLength(191)
                .HasColumnName("title");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");
            entity.Property(e => e.Url)
                .HasMaxLength(191)
                .HasColumnName("url");
            entity.Property(e => e.UserId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("notifications_user_id_foreign");
        });

        modelBuilder.Entity<Option>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("options")
                .UseCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.Id)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("id");
            entity.Property(e => e.Key)
                .HasMaxLength(191)
                .HasColumnName("key");
            entity.Property(e => e.Lang)
                .HasMaxLength(191)
                .HasDefaultValueSql("'en'")
                .HasColumnName("lang");
            entity.Property(e => e.Value)
                .HasColumnType("text")
                .HasColumnName("value");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("orders")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.GatewayId, "orders_gateway_id_foreign");

            entity.HasIndex(e => e.PlanId, "orders_plan_id_foreign");

            entity.HasIndex(e => e.UserId, "orders_user_id_foreign");

            entity.Property(e => e.Id)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("id");
            entity.Property(e => e.Amount).HasColumnName("amount");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.GatewayId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("gateway_id");
            entity.Property(e => e.InvoiceNo)
                .HasMaxLength(191)
                .HasColumnName("invoice_no");
            entity.Property(e => e.Meta)
                .HasColumnType("text")
                .HasColumnName("meta");
            entity.Property(e => e.PaymentId)
                .HasMaxLength(191)
                .HasColumnName("payment_id");
            entity.Property(e => e.PlanId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("plan_id");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'1'")
                .HasColumnType("int(11)")
                .HasColumnName("status");
            entity.Property(e => e.Tax).HasColumnName("tax");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("user_id");
            entity.Property(e => e.WillExpire).HasColumnName("will_expire");

            entity.HasOne(d => d.Gateway).WithMany(p => p.Orders)
                .HasForeignKey(d => d.GatewayId)
                .HasConstraintName("orders_gateway_id_foreign");

            entity.HasOne(d => d.Plan).WithMany(p => p.Orders)
                .HasForeignKey(d => d.PlanId)
                .HasConstraintName("orders_plan_id_foreign");

            entity.HasOne(d => d.User).WithMany(p => p.Orders)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("orders_user_id_foreign");
        });

        modelBuilder.Entity<PasswordReset>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("password_resets")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.Email, "password_resets_email_index");

            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(191)
                .HasColumnName("email");
            entity.Property(e => e.Token)
                .HasMaxLength(191)
                .HasColumnName("token");
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("permissions")
                .UseCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.Id)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.GroupName)
                .HasMaxLength(191)
                .HasColumnName("group_name");
            entity.Property(e => e.GuardName)
                .HasMaxLength(191)
                .HasColumnName("guard_name");
            entity.Property(e => e.Name)
                .HasMaxLength(191)
                .HasColumnName("name");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");

            entity.HasMany(d => d.Roles).WithMany(p => p.Permissions)
                .UsingEntity<Dictionary<string, object>>(
                    "RoleHasPermission",
                    r => r.HasOne<Role>().WithMany()
                        .HasForeignKey("RoleId")
                        .HasConstraintName("role_has_permissions_role_id_foreign"),
                    l => l.HasOne<Permission>().WithMany()
                        .HasForeignKey("PermissionId")
                        .HasConstraintName("role_has_permissions_permission_id_foreign"),
                    j =>
                    {
                        j.HasKey("PermissionId", "RoleId")
                            .HasName("PRIMARY")
                            .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });
                        j
                            .ToTable("role_has_permissions")
                            .UseCollation("utf8mb4_unicode_ci");
                        j.HasIndex(new[] { "RoleId" }, "role_has_permissions_role_id_foreign");
                        j.IndexerProperty<ulong>("PermissionId")
                            .HasColumnType("bigint(20) unsigned")
                            .HasColumnName("permission_id");
                        j.IndexerProperty<ulong>("RoleId")
                            .HasColumnType("bigint(20) unsigned")
                            .HasColumnName("role_id");
                    });
        });

        modelBuilder.Entity<PersonalAccessToken>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("personal_access_tokens")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.Token, "personal_access_tokens_token_unique").IsUnique();

            entity.HasIndex(e => new { e.TokenableType, e.TokenableId }, "personal_access_tokens_tokenable_type_tokenable_id_index");

            entity.Property(e => e.Id)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("id");
            entity.Property(e => e.Abilities)
                .HasColumnType("text")
                .HasColumnName("abilities");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.ExpiresAt)
                .HasColumnType("timestamp")
                .HasColumnName("expires_at");
            entity.Property(e => e.LastUsedAt)
                .HasColumnType("timestamp")
                .HasColumnName("last_used_at");
            entity.Property(e => e.Name)
                .HasMaxLength(191)
                .HasColumnName("name");
            entity.Property(e => e.Token)
                .HasMaxLength(64)
                .HasColumnName("token");
            entity.Property(e => e.TokenableId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("tokenable_id");
            entity.Property(e => e.TokenableType)
                .HasMaxLength(191)
                .HasColumnName("tokenable_type");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<Plan>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("plans")
                .UseCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.Id)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.Data)
                .HasColumnType("text")
                .HasColumnName("data");
            entity.Property(e => e.Days)
                .HasColumnType("int(11)")
                .HasColumnName("days");
            entity.Property(e => e.Iconname)
                .HasMaxLength(191)
                .HasColumnName("iconname");
            entity.Property(e => e.IsFeatured)
                .HasColumnType("int(11)")
                .HasColumnName("is_featured");
            entity.Property(e => e.IsRecommended)
                .HasColumnType("int(11)")
                .HasColumnName("is_recommended");
            entity.Property(e => e.IsTrial)
                .HasColumnType("int(11)")
                .HasColumnName("is_trial");
            entity.Property(e => e.Labelcolor)
                .HasMaxLength(191)
                .HasColumnName("labelcolor");
            entity.Property(e => e.Price).HasColumnName("price");
            entity.Property(e => e.Status)
                .HasColumnType("int(11)")
                .HasColumnName("status");
            entity.Property(e => e.Title)
                .HasMaxLength(191)
                .HasColumnName("title");
            entity.Property(e => e.TrialDays)
                .HasColumnType("int(11)")
                .HasColumnName("trial_days");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<Post>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("posts")
                .UseCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.Id)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.Featured)
                .HasDefaultValueSql("'1'")
                .HasColumnType("int(11)")
                .HasColumnName("featured");
            entity.Property(e => e.Lang)
                .HasMaxLength(191)
                .HasDefaultValueSql("'en'")
                .HasColumnName("lang");
            entity.Property(e => e.Slug)
                .HasMaxLength(191)
                .HasColumnName("slug");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'1'")
                .HasColumnType("int(11)")
                .HasColumnName("status");
            entity.Property(e => e.Title)
                .HasMaxLength(191)
                .HasColumnName("title");
            entity.Property(e => e.Type)
                .HasMaxLength(191)
                .HasDefaultValueSql("'blog'")
                .HasColumnName("type");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<Postcategory>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("postcategories")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.CategoryId, "postcategories_category_id_foreign");

            entity.HasIndex(e => e.PostId, "postcategories_post_id_foreign");

            entity.Property(e => e.CategoryId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("category_id");
            entity.Property(e => e.PostId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("post_id");

            entity.HasOne(d => d.Category).WithMany()
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("postcategories_category_id_foreign");

            entity.HasOne(d => d.Post).WithMany()
                .HasForeignKey(d => d.PostId)
                .HasConstraintName("postcategories_post_id_foreign");
        });

        modelBuilder.Entity<Postmeta>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("postmetas")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.PostId, "postmetas_post_id_foreign");

            entity.Property(e => e.Id)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("id");
            entity.Property(e => e.Key)
                .HasMaxLength(191)
                .HasColumnName("key");
            entity.Property(e => e.PostId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("post_id");
            entity.Property(e => e.Value)
                .HasColumnType("text")
                .HasColumnName("value");

            entity.HasOne(d => d.Post).WithMany(p => p.Postmetas)
                .HasForeignKey(d => d.PostId)
                .HasConstraintName("postmetas_post_id_foreign");
        });

        modelBuilder.Entity<Reply>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("replies")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.DeviceId, "replies_device_id_foreign");

            entity.HasIndex(e => e.TemplateId, "replies_template_id_foreign");

            entity.HasIndex(e => e.UserId, "replies_user_id_foreign");

            entity.Property(e => e.Id)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("id");
            entity.Property(e => e.ApiKey)
                .HasMaxLength(191)
                .HasColumnName("api_key");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.DeviceId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("device_id");
            entity.Property(e => e.Keyword)
                .HasMaxLength(191)
                .HasColumnName("keyword");
            entity.Property(e => e.MatchType)
                .HasMaxLength(191)
                .HasDefaultValueSql("'equal'")
                .HasColumnName("match_type");
            entity.Property(e => e.Reply1)
                .HasColumnType("text")
                .HasColumnName("reply");
            entity.Property(e => e.ReplyType)
                .HasMaxLength(191)
                .HasDefaultValueSql("'text'")
                .HasColumnName("reply_type");
            entity.Property(e => e.TemplateId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("template_id");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("user_id");

            entity.HasOne(d => d.Device).WithMany(p => p.Replies)
                .HasForeignKey(d => d.DeviceId)
                .HasConstraintName("replies_device_id_foreign");

            entity.HasOne(d => d.Template).WithMany(p => p.Replies)
                .HasForeignKey(d => d.TemplateId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("replies_template_id_foreign");

            entity.HasOne(d => d.User).WithMany(p => p.Replies)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("replies_user_id_foreign");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("roles")
                .UseCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.Id)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.GuardName)
                .HasMaxLength(191)
                .HasColumnName("guard_name");
            entity.Property(e => e.Name)
                .HasMaxLength(191)
                .HasColumnName("name");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<Schedulecontact>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("schedulecontacts")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.ContactId, "schedulecontacts_contact_id_foreign");

            entity.HasIndex(e => e.SchedulemessageId, "schedulecontacts_schedulemessage_id_foreign");

            entity.Property(e => e.Id)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("id");
            entity.Property(e => e.ContactId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("contact_id");
            entity.Property(e => e.SchedulemessageId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("schedulemessage_id");
            entity.Property(e => e.StatusCode)
                .HasColumnType("int(11)")
                .HasColumnName("status_code");

            entity.HasOne(d => d.Contact).WithMany(p => p.Schedulecontacts)
                .HasForeignKey(d => d.ContactId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("schedulecontacts_contact_id_foreign");

            entity.HasOne(d => d.Schedulemessage).WithMany(p => p.Schedulecontacts)
                .HasForeignKey(d => d.SchedulemessageId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("schedulecontacts_schedulemessage_id_foreign");
        });

        modelBuilder.Entity<Schedulemessage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("schedulemessages")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.DeviceId, "schedulemessages_device_id_foreign");

            entity.HasIndex(e => e.TemplateId, "schedulemessages_template_id_foreign");

            entity.HasIndex(e => e.UserId, "schedulemessages_user_id_foreign");

            entity.Property(e => e.Id)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("id");
            entity.Property(e => e.Body)
                .HasColumnType("text")
                .HasColumnName("body");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.Date).HasColumnName("date");
            entity.Property(e => e.DeviceId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("device_id");
            entity.Property(e => e.ScheduleAt)
                .HasColumnType("datetime")
                .HasColumnName("schedule_at");
            entity.Property(e => e.Status)
                .HasMaxLength(191)
                .HasDefaultValueSql("'pending'")
                .HasColumnName("status");
            entity.Property(e => e.TemplateId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("template_id");
            entity.Property(e => e.Time).HasColumnName("time");
            entity.Property(e => e.Title)
                .HasMaxLength(191)
                .HasColumnName("title");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("user_id");
            entity.Property(e => e.Zone)
                .HasMaxLength(191)
                .HasColumnName("zone");

            entity.HasOne(d => d.Device).WithMany(p => p.Schedulemessages)
                .HasForeignKey(d => d.DeviceId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("schedulemessages_device_id_foreign");

            entity.HasOne(d => d.Template).WithMany(p => p.Schedulemessages)
                .HasForeignKey(d => d.TemplateId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("schedulemessages_template_id_foreign");

            entity.HasOne(d => d.User).WithMany(p => p.Schedulemessages)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("schedulemessages_user_id_foreign");
        });

        modelBuilder.Entity<Smstesttransaction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("smstesttransactions")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.AppId, "smstesttransactions_app_id_foreign");

            entity.HasIndex(e => e.DeviceId, "smstesttransactions_device_id_foreign");

            entity.HasIndex(e => e.UserId, "smstesttransactions_user_id_foreign");

            entity.Property(e => e.Id)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("id");
            entity.Property(e => e.AppId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("app_id");
            entity.Property(e => e.Body)
                .HasColumnType("text")
                .HasColumnName("body");
            entity.Property(e => e.Charge).HasColumnName("charge");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.DeviceId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("device_id");
            entity.Property(e => e.MessagingId)
                .HasMaxLength(191)
                .HasColumnName("messaging_id");
            entity.Property(e => e.Phone)
                .HasMaxLength(191)
                .HasColumnName("phone");
            entity.Property(e => e.StatusCode)
                .HasColumnType("int(11)")
                .HasColumnName("status_code");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("user_id");

            entity.HasOne(d => d.App).WithMany(p => p.Smstesttransactions)
                .HasForeignKey(d => d.AppId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("smstesttransactions_app_id_foreign");

            entity.HasOne(d => d.Device).WithMany(p => p.Smstesttransactions)
                .HasForeignKey(d => d.DeviceId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("smstesttransactions_device_id_foreign");

            entity.HasOne(d => d.User).WithMany(p => p.Smstesttransactions)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("smstesttransactions_user_id_foreign");
        });

        modelBuilder.Entity<Smstransaction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("smstransactions")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.AppId, "smstransactions_app_id_foreign");

            entity.HasIndex(e => e.DeviceId, "smstransactions_device_id_foreign");

            entity.HasIndex(e => e.TemplateId, "smstransactions_template_id_foreign");

            entity.HasIndex(e => e.UserId, "smstransactions_user_id_foreign");

            entity.Property(e => e.Id)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("id");
            entity.Property(e => e.AppId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("app_id");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.DeviceId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("device_id");
            entity.Property(e => e.From)
                .HasMaxLength(191)
                .HasColumnName("from");
            entity.Property(e => e.TemplateId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("template_id");
            entity.Property(e => e.To)
                .HasMaxLength(191)
                .HasColumnName("to");
            entity.Property(e => e.Type)
                .HasMaxLength(191)
                .HasDefaultValueSql("'from_api'")
                .HasColumnName("type");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("user_id");

            entity.HasOne(d => d.App).WithMany(p => p.Smstransactions)
                .HasForeignKey(d => d.AppId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("smstransactions_app_id_foreign");

            entity.HasOne(d => d.Device).WithMany(p => p.Smstransactions)
                .HasForeignKey(d => d.DeviceId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("smstransactions_device_id_foreign");

            entity.HasOne(d => d.Template).WithMany(p => p.Smstransactions)
                .HasForeignKey(d => d.TemplateId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("smstransactions_template_id_foreign");

            entity.HasOne(d => d.User).WithMany(p => p.Smstransactions)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("smstransactions_user_id_foreign");
        });

        modelBuilder.Entity<Support>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("supports")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.UserId, "supports_user_id_foreign");

            entity.Property(e => e.Id)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'2'")
                .HasColumnType("int(11)")
                .HasColumnName("status");
            entity.Property(e => e.Subject)
                .HasMaxLength(191)
                .HasColumnName("subject");
            entity.Property(e => e.TicketNo)
                .HasColumnType("int(11)")
                .HasColumnName("ticket_no");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Supports)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("supports_user_id_foreign");
        });

        modelBuilder.Entity<Supportlog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("supportlogs")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.SupportId, "supportlogs_support_id_foreign");

            entity.HasIndex(e => e.UserId, "supportlogs_user_id_foreign");

            entity.Property(e => e.Id)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("id");
            entity.Property(e => e.Comment)
                .HasColumnType("text")
                .HasColumnName("comment");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.IsAdmin)
                .HasColumnType("int(11)")
                .HasColumnName("is_admin");
            entity.Property(e => e.Seen)
                .HasColumnType("int(11)")
                .HasColumnName("seen");
            entity.Property(e => e.SupportId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("support_id");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("user_id");

            entity.HasOne(d => d.Support).WithMany(p => p.Supportlogs)
                .HasForeignKey(d => d.SupportId)
                .HasConstraintName("supportlogs_support_id_foreign");

            entity.HasOne(d => d.User).WithMany(p => p.Supportlogs)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("supportlogs_user_id_foreign");
        });

        modelBuilder.Entity<Template>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("templates")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.UserId, "templates_user_id_foreign");

            entity.HasIndex(e => e.Uuid, "templates_uuid_unique").IsUnique();

            entity.Property(e => e.Id)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("id");
            entity.Property(e => e.Body)
                .HasColumnType("text")
                .HasColumnName("body");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'1'")
                .HasColumnType("int(11)")
                .HasColumnName("status");
            entity.Property(e => e.Title)
                .HasMaxLength(191)
                .HasColumnName("title");
            entity.Property(e => e.Type)
                .HasMaxLength(191)
                .HasColumnName("type");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("user_id");
            entity.Property(e => e.Uuid).HasColumnName("uuid");

            entity.HasOne(d => d.User).WithMany(p => p.Templates)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("templates_user_id_foreign");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("users")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.Email, "users_email_unique").IsUnique();

            entity.Property(e => e.Id)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("id");
            entity.Property(e => e.Address)
                .HasMaxLength(191)
                .HasColumnName("address");
            entity.Property(e => e.Authkey)
                .HasMaxLength(191)
                .HasColumnName("authkey");
            entity.Property(e => e.Avatar)
                .HasMaxLength(191)
                .HasColumnName("avatar");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(191)
                .HasColumnName("email");
            entity.Property(e => e.EmailVerifiedAt)
                .HasColumnType("timestamp")
                .HasColumnName("email_verified_at");
            entity.Property(e => e.Meta)
                .HasColumnType("text")
                .HasColumnName("meta");
            entity.Property(e => e.Name)
                .HasMaxLength(191)
                .HasColumnName("name");
            entity.Property(e => e.Password)
                .HasMaxLength(191)
                .HasColumnName("password");
            entity.Property(e => e.Phone)
                .HasMaxLength(191)
                .HasColumnName("phone");
            entity.Property(e => e.Plan)
                .HasColumnType("text")
                .HasColumnName("plan");
            entity.Property(e => e.PlanId)
                .HasColumnType("int(11)")
                .HasColumnName("plan_id");
            entity.Property(e => e.RememberToken)
                .HasMaxLength(100)
                .HasColumnName("remember_token");
            entity.Property(e => e.Role)
                .HasMaxLength(191)
                .HasDefaultValueSql("'user'")
                .HasColumnName("role");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'1'")
                .HasColumnType("int(11)")
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");
            entity.Property(e => e.Wallet)
                .HasMaxLength(191)
                .HasColumnName("wallet");
            entity.Property(e => e.WillExpire).HasColumnName("will_expire");
        });

        modelBuilder.Entity<Webhook>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("webhooks")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.DeviceId, "webhooks_device_id_foreign");

            entity.HasIndex(e => e.UserId, "webhooks_user_id_foreign");

            entity.Property(e => e.Id)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.DeviceId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("device_id");
            entity.Property(e => e.Hook)
                .HasColumnType("text")
                .HasColumnName("hook");
            entity.Property(e => e.Payload)
                .HasColumnType("text")
                .HasColumnName("payload");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'2'")
                .HasColumnType("int(11)")
                .HasColumnName("status");
            entity.Property(e => e.StatusCode)
                .HasColumnType("int(11)")
                .HasColumnName("status_code");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("user_id");

            entity.HasOne(d => d.Device).WithMany(p => p.Webhooks)
                .HasForeignKey(d => d.DeviceId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("webhooks_device_id_foreign");

            entity.HasOne(d => d.User).WithMany(p => p.Webhooks)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("webhooks_user_id_foreign");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
