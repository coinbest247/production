
using Core.Data.EF.Configurations;
using Core.Data.EF.Extensions;
using Core.Data.Entities;
using Core.Data.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;

namespace Core.Data.EF
{
    public class AppDbContext : IdentityDbContext<AppUser, AppRole, Guid>
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Language> Languages { set; get; }
        public DbSet<Function> Functions { get; set; }
        public DbSet<AppUser> AppUsers { get; set; }
        public DbSet<AppRole> AppRoles { get; set; }
        public DbSet<MenuGroup> MenuGroups { set; get; }
        public DbSet<MenuItem> MenuItems { set; get; }
        public DbSet<BlogCategory> BlogCategories { set; get; }
        public DbSet<Blog> Blogs { set; get; }
        public DbSet<BlogTag> BlogTags { set; get; }
        public DbSet<Feedback> Feedbacks { set; get; }
        public DbSet<Tag> Tags { set; get; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<TicketTransaction> TicketTransactions { get; set; }
        public DbSet<Staking> Stakings { get; set; }
        public DbSet<InvestPackageReward> InvestPackageRewards { get; set; }
        public DbSet<StakingAffiliate> StakingAffiliates { get; set; }
        public DbSet<Support> Supports { get; set; }
        public DbSet<Notify> Notifies { get; set; }
        public DbSet<WalletTransfer> WalletTransfers { get; set; }
        public DbSet<WalletTransaction> WalletTransactions { get; set; }
        public DbSet<Config> Configs { get; set; }
        public DbSet<InvestBotConfig> InvestBotConfigs { get; set; }
        public DbSet<InvestProfitHistory> InvestProfitHistories { get; set; }
        public DbSet<Airdrop> Airdrops { get; set; }

        public DbSet<InvestPackage> InvestPackages { get; set; }

        public DbSet<InvestPackageAffiliate> InvestPackageAffiliates { get; set; }

        public DbSet<AgentCommission> AgentCommissions { get; set; }

        public DbSet<QueueTask> QueueTasks { get; set; }

        public DbSet<SaleDefi> SaleDefis { get; set; }

        public DbSet<SaleAffiliate> SaleAffiliates { get; set; }

        public DbSet<StakingReward> StakingRewards { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            #region Identity Config

            builder.Entity<IdentityUserClaim<Guid>>().ToTable("AppUserClaims")
                .HasKey(x => x.Id);

            builder.Entity<IdentityRoleClaim<Guid>>().ToTable("AppRoleClaims")
                .HasKey(x => x.Id);

            builder.Entity<IdentityUserLogin<Guid>>().ToTable("AppUserLogins")
                .HasKey(x => x.UserId);

            builder.Entity<IdentityUserRole<Guid>>().ToTable("AppUserRoles")
                .HasKey(x => new { x.UserId, x.RoleId, });



            builder.Entity<IdentityUserToken<Guid>>().ToTable("AppUserTokens")
                .HasKey(x => new { x.UserId, x.LoginProvider, x.Name });

            #endregion Identity Config

            builder.AddConfiguration(new TagConfiguration());
            builder.AddConfiguration(new BlogTagConfiguration());
            builder.AddConfiguration(new FunctionConfiguration());
            builder.AddConfiguration(new BlogConfiguration());
            builder.AddConfiguration(new BlogCategoryConfiguration());

            builder.Entity<AppUser>()
                .Property(p => p.BCAmount).HasColumnType("decimal(18,4)");
            builder.Entity<AppUser>()
                .Property(p => p.USDTAmount).HasColumnType("decimal(18,4)");
            builder.Entity<AppUser>()
                .Property(p => p.BUSDAmount).HasColumnType("decimal(18,4)");
            builder.Entity<AppUser>()
                .Property(p => p.ShibaAmount).HasColumnType("decimal(18,4)");
            builder.Entity<AppUser>()
                .Property(p => p.BNBAmount).HasColumnType("decimal(18,4)");
            builder.Entity<AppUser>()
                .Property(p => p.BotTradeAmount).HasColumnType("decimal(18,4)");
            builder.Entity<AppUser>()
                .Property(p => p.BCFutureAmount).HasColumnType("decimal(18,4)");
            builder.Entity<AppUser>()
                .Property(p => p.USDTFutureAmount).HasColumnType("decimal(18,4)");
            builder.Entity<AppUser>()
                .Property(p => p.BNBFutureAmount).HasColumnType("decimal(18,4)");
            builder.Entity<AppUser>()
                .Property(p => p.StakingPIAmount).HasColumnType("decimal(18,4)");
            builder.Entity<AppUser>()
                .Property(p => p.StakingPIAffiliateAmount).HasColumnType("decimal(18,4)");

            builder.Entity<AppUser>()
                .Property(p => p.PiNetworkFutureAmount).HasColumnType("decimal(18,4)");

            builder.Entity<AppUser>()
                .Property(p => p.PINetworkAmount).HasColumnType("decimal(18,4)");

            builder.Entity<WalletTransaction>()
                .Property(p => p.Amount).HasColumnType("decimal(18,4)");
            builder.Entity<WalletTransaction>()
                .Property(p => p.AmountReceive).HasColumnType("decimal(18,4)");
            builder.Entity<WalletTransaction>()
                .Property(p => p.FeeAmount).HasColumnType("decimal(18,4)");

            builder.Entity<TicketTransaction>()
                .Property(p => p.FeeAmount).HasColumnType("decimal(18,4)");
            builder.Entity<TicketTransaction>()
                .Property(p => p.FeeAmount).HasColumnType("decimal(18,4)");
            
            builder.Entity<InvestProfitHistory>()
                .Property(p => p.ProfitAmount).HasColumnType("decimal(18,4)");
            builder.Entity<InvestProfitHistory>()
                .Property(p => p.ProfitPercent).HasColumnType("decimal(18,4)");
            builder.Entity<InvestProfitHistory>()
                .Property(p => p.StartPrice).HasColumnType("decimal(18,4)");
            builder.Entity<InvestProfitHistory>()
                .Property(p => p.StopPrice).HasColumnType("decimal(18,4)");

            builder.Entity<TokenPriceHistory>()
                .Property(p => p.Price).HasColumnType("decimal(18,4)");

            builder.Entity<Staking>()
                .Property(p => p.ReceiveAmount).HasColumnType("decimal(18,4)");
            builder.Entity<Staking>()
                .Property(p => p.StakingAmount).HasColumnType("decimal(18,4)");
            builder.Entity<Staking>()
                .Property(p => p.StakingAmountUSDT).HasColumnType("decimal(18,4)");

            builder.Entity<InvestPackageReward>()
                .Property(p => p.Amount).HasColumnType("decimal(18,4)");

            builder.Entity<StakingAffiliate>()
                .Property(p => p.Amount).HasColumnType("decimal(18,4)");

            builder.Entity<InvestPackageAffiliate>()
                .Property(p => p.InvestAmountInUSDT).HasColumnType("decimal(18,4)");

            builder.Entity<SaleDefi>()
                .Property(p => p.BNBAmount).HasColumnType("decimal(18,4)");
            builder.Entity<SaleDefi>()
                .Property(p => p.USDAmount).HasColumnType("decimal(18,4)");
            builder.Entity<SaleDefi>()
                .Property(p => p.TokenAmount).HasColumnType("decimal(18,4)");

            builder.Entity<SaleAffiliate>()
                .Property(p => p.USDAmount).HasColumnType("decimal(18,4)");
            builder.Entity<SaleAffiliate>()
                .Property(p => p.Amount).HasColumnType("decimal(18,4)");

            builder.Entity<StakingReward>()
                .Property(p => p.Amount).HasColumnType("decimal(18,4)");
        }

        public override int SaveChanges()
        {
            var modified = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Modified || e.State == EntityState.Added);

            foreach (EntityEntry item in modified)
            {
                var changedOrAddedItem = item.Entity as IDateTracking;
                if (changedOrAddedItem != null)
                {
                    if (item.State == EntityState.Added)
                        changedOrAddedItem.DateCreated = DateTime.UtcNow;

                    changedOrAddedItem.DateModified = DateTime.UtcNow;
                }
            }

            return base.SaveChanges();
        }
    }

    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json").Build();

            var builder = new DbContextOptionsBuilder<AppDbContext>();
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            builder.UseSqlServer(connectionString);

            return new AppDbContext(builder.Options);
        }
    }
}
