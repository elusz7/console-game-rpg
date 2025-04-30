using ConsoleGameEntities.Models.Items;
using ConsoleGameEntities.Models.Monsters;
using Microsoft.EntityFrameworkCore;
using ConsoleGameEntities.Models.Skills;
using ConsoleGameEntities.Models.Entities;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameEntities.Data
{
    public class GameContext : DbContext
    {
        public DbSet<Player>? Players { get; set; }
        public DbSet<Monster>? Monsters { get; set; }
        public DbSet<Skill> Skills { get; set; }
        public DbSet<Item>? Items { get; set; }
        public DbSet<Inventory>? Inventories { get; set; }
        public DbSet<Room>? Rooms { get; set; }
        public DbSet<Archetype> Archetypes { get; set; }

        public GameContext(DbContextOptions<GameContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //set up monsters
            modelBuilder.Entity<Monster>()
                .HasDiscriminator<string>(m => m.MonsterType)
                .HasValue<Goblin>("Goblin");

            //set up inventory      
            modelBuilder.Entity<Item>()
                .HasDiscriminator<string>(i => i.ItemType)
                .HasValue<Weapon>("Weapon")
                .HasValue<Armor>("Armor");

            modelBuilder.Entity<Item>()
                .Property(i => i.Value)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Item>()
                .Property(i => i.Weight)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Inventory>()
                .Property(i => i.Capacity)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Player>()
                .HasOne(p => p.Inventory)
                .WithOne(i => i.Player)
                .HasForeignKey<Inventory>(i => i.PlayerId);

            modelBuilder.Entity<Player>()
                .HasOne(p => p.CurrentRoom)
                .WithMany()
                .HasForeignKey(p => p.RoomId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Monster>()
                .HasOne(m => m.Room)
                .WithMany()
                .HasForeignKey(m => m.RoomId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Inventory>()
                .HasMany(i => i.Items)
                .WithOne(item => item.Inventory)
                .HasForeignKey(item => item.InventoryId);

            modelBuilder.Entity<Room>()
                .Property(r => r.Name)
                .IsRequired()
                .HasColumnName("Name");

            modelBuilder.Entity<Room>()
                .Property(r => r.Description)
                .IsRequired()
                .HasColumnName("Description");

            modelBuilder.Entity<Room>()
                .HasOne(r => r.North)
                .WithMany()
                .HasForeignKey(r => r.NorthId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Room>()
                .HasOne(r => r.South)
                .WithMany()
                .HasForeignKey(r => r.SouthId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Room>()
                .HasOne(r => r.East)
                .WithMany()
                .HasForeignKey(r => r.EastId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Room>()
                .HasOne(r => r.West)
                .WithMany()
                .HasForeignKey(r => r.WestId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Skill>()
                .HasCheckConstraint("CK_Skill_OnlyOneOwner",
                @"((""ArchetypeId"" IS NOT NULL AND ""MonsterId"" IS NULL) OR (""ArchetypeId"" IS NULL AND ""MonsterId"" IS NOT NULL))");

            // Configure relationships

            modelBuilder.Entity<Skill>()
                .HasDiscriminator<string>(i => i.SkillType)
                .HasValue<MartialSkill>("MartialSkill")
                .HasValue<MagicSkill>("MagicSkill")
                .HasValue<SupportSkill>("SupportSkill")
                .HasValue<UltimateSkill>("UltimateSkill");

            modelBuilder.Entity<Skill>()
                .HasOne(s => s.Archetype)
                .WithMany(a => a.Skills)
                .HasForeignKey(s => s.ArchetypeId);

            modelBuilder.Entity<Skill>()
                .HasOne(s => s.Monster)
                .WithMany(m => m.Skills)
                .HasForeignKey(s => s.MonsterId);

            modelBuilder.Entity<Archetype>()
                .Property("AttackMultiplier")
                .HasColumnType("decimal(3,2)");

            modelBuilder.Entity<Archetype>()
                .Property("MagicMultiplier")
                .HasColumnType("decimal(3,2)");

            modelBuilder.Entity<Archetype>()
                .Property("DefenseMultiplier")
                .HasColumnType("decimal(3,2)");

            modelBuilder.Entity<Archetype>()
                .Property("ResistanceMultiplier")
                .HasColumnType("decimal(3,2)");

            modelBuilder.Entity<Archetype>()
                .Property("SpeedMultiplier")
                .HasColumnType("decimal(3,2)");

            modelBuilder.Entity<Archetype>()
                .Property("ResourceMultiplier")
                .HasColumnType("decimal(3,2)");

            base.OnModelCreating(modelBuilder);
        }
    }
}


