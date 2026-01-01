using Microsoft.EntityFrameworkCore;
using YouSpent.Models;

namespace YouSpent.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Expense> Expenses { get; set; }
        public DbSet<Day> Days { get; set; }
        public DbSet<Week> Weeks { get; set; }
        public DbSet<Month> Months { get; set; }
        public DbSet<Year> Years { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Expense
            modelBuilder.Entity<Expense>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Description).IsRequired().HasMaxLength(500);
                entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Category).HasMaxLength(100);
                entity.Property(e => e.Date).IsRequired();
            });

            // Configure Day
            modelBuilder.Entity<Day>(entity =>
            {
                entity.HasKey(d => d.Id);
                entity.Property(d => d.Date).IsRequired();
                entity.HasMany(d => d.Expenses)
                      .WithOne()
                      .HasForeignKey("DayId")
                      .OnDelete(DeleteBehavior.Cascade);
                entity.Ignore(d => d.TotalSpent);
                entity.Ignore(d => d.DayOfWeek);
                entity.Ignore(d => d.DayNumber);
            });

            // Configure Week
            modelBuilder.Entity<Week>(entity =>
            {
                entity.HasKey(w => w.Id);
                entity.Property(w => w.StartDate).IsRequired();
                entity.Property(w => w.EndDate).IsRequired();
                entity.Property(w => w.WeekNumber).IsRequired();
                entity.Property(w => w.Year).IsRequired();
                entity.HasMany(w => w.Days)
                      .WithOne()
                      .HasForeignKey("WeekId")
                      .OnDelete(DeleteBehavior.Cascade);
                entity.Ignore(w => w.TotalSpent);
            });

            // Configure Month
            modelBuilder.Entity<Month>(entity =>
            {
                entity.HasKey(m => m.Id);
                entity.Property(m => m.MonthNumber).IsRequired();
                entity.Property(m => m.Year).IsRequired();
                entity.HasMany(m => m.Days)
                      .WithOne()
                      .HasForeignKey("MonthId")
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(m => m.Weeks)
                      .WithOne()
                      .HasForeignKey("MonthId")
                      .OnDelete(DeleteBehavior.Cascade);
                entity.Ignore(m => m.TotalSpent);
                entity.Ignore(m => m.MonthName);
            });

            // Configure Year
            modelBuilder.Entity<Year>(entity =>
            {
                entity.HasKey(y => y.Id);
                entity.Property(y => y.YearNumber).IsRequired();
                entity.HasMany(y => y.Months)
                      .WithOne()
                      .HasForeignKey("YearId")
                      .OnDelete(DeleteBehavior.Cascade);
                entity.Ignore(y => y.TotalSpent);
                entity.Ignore(y => y.IsLeapYear);
            });
        }
    }
}
