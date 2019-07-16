using System;
using HealthGateway.Util;
using HealthGateway.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace HealthGateway.Database
{
    public partial class HealthGatewayContext : DbContext
    {
        string host;
        string port;
        string username;
        string password;

        public HealthGatewayContext()
        {
        }

        public HealthGatewayContext(DbContextOptions<HealthGatewayContext> options, IEnvironment environment)
            : base(options)
        {
            host = environment.GetValue("POSTGRES_HOST");
            port = environment.GetValue("POSTGRES_PORT");
            username = environment.GetValue("POSTGRES_USERNAME");
            password = environment.GetValue("POSTGRES_PASSWORD");

        }

        public virtual DbSet<UserPreferences> UserPreferences { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql($"Host={host};Port={port};Database=healthgateway;Username={username};Password={password}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserPreferences>(entity =>
            {
                entity.ToTable("userPreferences", "hgw");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.DisplayName)
                    .HasColumnName("displayName")
                    .HasColumnType("character varying(50)[]");
            });
        }
    }
}
