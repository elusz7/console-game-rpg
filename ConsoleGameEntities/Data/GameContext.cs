using ConsoleGameEntities.Models.Items;
using ConsoleGameEntities.Models.Monsters;
using Microsoft.EntityFrameworkCore;
using ConsoleGameEntities.Models.Skills;
using ConsoleGameEntities.Models.Entities;

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
                .HasValue<EliteMonster>("EliteMonster")
                .HasValue<BossMonster>("BossMonster");

            modelBuilder.Entity<Monster>()
                .Property(m => m.Description)
                .IsRequired()
                .HasColumnName("Description");

            modelBuilder.Entity<Monster>()
                .HasOne(m => m.Treasure)
                .WithOne(i => i.Monster)
                .HasForeignKey<Monster>(m => m.ItemId)
                .OnDelete(DeleteBehavior.SetNull);


            //set up inventory      
            modelBuilder.Entity<Item>()
                .HasDiscriminator<string>(i => i.ItemType)
                .HasValue<Weapon>("Weapon")
                .HasValue<Armor>("Armor")
                .HasValue<Valuable>("Valuable")
                .HasValue<Consumable>("Consumable");

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
                .WithMany(r => r.Monsters)
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
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Room>()
                .HasOne(r => r.South)
                .WithMany()
                .HasForeignKey(r => r.SouthId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Room>()
                .HasOne(r => r.East)
                .WithMany()
                .HasForeignKey(r => r.EastId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Room>()
                .HasOne(r => r.West)
                .WithMany()
                .HasForeignKey(r => r.WestId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure relationships

            modelBuilder.Entity<Skill>()
                .HasDiscriminator<string>(i => i.SkillType)
                .HasValue<MartialSkill>("MartialSkill")
                .HasValue<MagicSkill>("MagicSkill")
                //.HasValue<MagicSkill>("MagicalSkill") // instead of or in addition to "MagicSkill"
                .HasValue<SupportSkill>("SupportSkill")
                .HasValue<UltimateSkill>("UltimateSkill")
                .HasValue<BossSkill>("BossSkill");

            /*modelBuilder.Entity<SupportSkill>()
                .Property(s => s.SupportEffect)
                .HasColumnName("SupportEffect");*/

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


