using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace BlossomCart.Models;

public partial class BlossomCartsContext : DbContext
{
    public BlossomCartsContext()
    {
    }

    public BlossomCartsContext(DbContextOptions<BlossomCartsContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Bouquet> Bouquets { get; set; }

    public virtual DbSet<Cart> Carts { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Feedback> Feedbacks { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderDetail> OrderDetails { get; set; }

    public virtual DbSet<PaymentTable> PaymentTables { get; set; }

    public virtual DbSet<Signup> Signups { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("data source=HP-PC\\SQLEXPRESS;initial catalog=BlossomCarts;Integrated Security=True; TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Bouquet>(entity =>
        {
            entity.HasKey(e => e.BouquetId).HasName("PK__Bouquets__DEF2F11A3C54FA43");

            entity.Property(e => e.BouquetDescription)
                .HasMaxLength(500)
                .HasColumnName("Bouquet_Description");
            entity.Property(e => e.BouquetName)
                .HasMaxLength(50)
                .HasColumnName("Bouquet_Name");
            entity.Property(e => e.CategoryId).HasColumnName("Category_Id");
            entity.Property(e => e.Status).HasDefaultValueSql("((1))");

            entity.HasOne(d => d.Category).WithMany(p => p.Bouquets)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK_Bouquets_ToTable");
        });

        modelBuilder.Entity<Cart>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Cart__3214EC07533CCED1");

            entity.ToTable("Cart");

            entity.Property(e => e.TotalPrice).HasColumnName("Total_Price");

            entity.HasOne(d => d.Bouquet).WithMany(p => p.Carts)
                .HasForeignKey(d => d.BouquetId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Cart_ToTable");

            entity.HasOne(d => d.User).WithMany(p => p.Carts)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Cart_ToTable_1");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__Category__6DB38D6EF6F38609");

            entity.ToTable("Category");

            entity.Property(e => e.CategoryId).HasColumnName("Category_Id");
            entity.Property(e => e.CategoryName)
                .HasMaxLength(50)
                .HasColumnName("Category_Name");
        });

        modelBuilder.Entity<Feedback>(entity =>
        {
            entity.HasKey(e => e.FId).HasName("PK__Feedback__2C6EC723291E072B");

            entity.ToTable("Feedback");

            entity.Property(e => e.FId).HasColumnName("F_Id");
            entity.Property(e => e.CustomerEmail)
                .HasMaxLength(50)
                .HasColumnName("Customer_Email");
            entity.Property(e => e.CustomerName)
                .HasMaxLength(50)
                .HasColumnName("Customer_Name");
            entity.Property(e => e.Date)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("date");
            entity.Property(e => e.Feedback1).HasColumnName("Feedback");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PK__Order__F1E4607BDFB25F83");

            entity.ToTable("Order");

            entity.Property(e => e.OrderId).HasColumnName("Order_Id");
            entity.Property(e => e.OrderDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("date")
                .HasColumnName("Order_Date");
            entity.Property(e => e.PaymentStatus)
                .HasMaxLength(50)
                .HasDefaultValueSql("('Not Paid')")
                .HasColumnName("Payment_Status");
            entity.Property(e => e.PaymentType)
                .HasMaxLength(50)
                .HasColumnName("Payment_Type");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValueSql("('Pending')");
            entity.Property(e => e.Time).HasDefaultValueSql("(CONVERT([time],getdate()))");
            entity.Property(e => e.UserId).HasColumnName("User_Id");

            entity.HasOne(d => d.User).WithMany(p => p.Orders)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Order_ToTable");
        });

        modelBuilder.Entity<OrderDetail>(entity =>
        {
            entity.HasKey(e => e.OrderDetailsId).HasName("PK__Order_De__F6868F123F6F77CA");

            entity.ToTable("Order_Details");

            entity.Property(e => e.OrderDetailsId).HasColumnName("Order_Details_Id");
            entity.Property(e => e.BouquetId).HasColumnName("Bouquet_Id");
            entity.Property(e => e.OrderId).HasColumnName("Order_Id");
            entity.Property(e => e.TotalPrice).HasColumnName("Total_Price");
            entity.Property(e => e.UserId).HasColumnName("User_Id");

            entity.HasOne(d => d.Bouquet).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.BouquetId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Order_Details_ToTable_1");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Order_Details_ToTable");

            entity.HasOne(d => d.User).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Order_Details_ToTable_2");
        });

        modelBuilder.Entity<PaymentTable>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("PK__Payment___9B556A38A996E5B1");

            entity.ToTable("Payment_Table");

            entity.Property(e => e.Cvc).HasColumnName("CVC");
            entity.Property(e => e.Date)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("date");
            entity.Property(e => e.HolderName).HasMaxLength(50);
            entity.Property(e => e.UserId).HasColumnName("User_Id");

            entity.HasOne(d => d.Order).WithMany(p => p.PaymentTables)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Payment_Table_ToTable");

            entity.HasOne(d => d.User).WithMany(p => p.PaymentTables)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Payment_Table_ToTable_1");
        });

        modelBuilder.Entity<Signup>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Signup__3214EC0746BDCEBF");

            entity.ToTable("Signup");

            entity.Property(e => e.CustamerName)
                .HasMaxLength(50)
                .HasColumnName("Custamer_Name");
            entity.Property(e => e.EMail)
                .HasMaxLength(50)
                .HasColumnName("E_Mail");
            entity.Property(e => e.Password).HasMaxLength(250);
            entity.Property(e => e.PhoneNumber).HasColumnName("Phone_Number");
            entity.Property(e => e.Role).HasDefaultValueSql("((1))");
            entity.Property(e => e.Token)
                .HasMaxLength(50)
                .HasDefaultValueSql("((0))")
                .HasColumnName("token");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
