using ConsoleGameEntities.Models.Characters;
using ConsoleGameEntities.Models.Items;
using ConsoleGameEntities.Models.Characters.Monsters;
using Microsoft.EntityFrameworkCore;
using ConsoleGameEntities.Models.Abilities;
using ConsoleGameEntities.Models.Rooms;

namespace ConsoleGameEntities.Data
{
    public class GameContext : DbContext
    {
        public DbSet<Player>? Players { get; set; }
        public DbSet<Monster>? Monsters { get; set; }
        public DbSet<Ability>? Abilities { get; set; }
        public DbSet<Item>? Items { get; set; }
        public DbSet<Inventory>? Inventories { get; set; }
        public DbSet<Room>? Rooms { get; set; }

        public GameContext(DbContextOptions<GameContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure TPH for Character hierarchy
            modelBuilder.Entity<Monster>()
                .HasDiscriminator<string>(m=> m.MonsterType)
                .HasValue<Goblin>("Goblin");

            // Configure TPH for Ability hierarchy
            modelBuilder.Entity<Ability>()
                .HasDiscriminator<string>(pa=>pa.AbilityType)
                .HasValue<ShoveAbility>("ShoveAbility");

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

            modelBuilder.Entity<Player>()
                .HasOne(p => p.Inventory)
                .WithOne(i => i.Player)
                .HasForeignKey<Inventory>(i => i.PlayerId);

            //modelBuilder.Entity<Inventory>()
            //    .Property(i => i.Capacity)
            //    .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Inventory>()
                .HasMany(i => i.Items)
                .WithOne(item => item.Inventory)
                .HasForeignKey(item => item.InventoryId);

            // Configure many-to-many relationship
            modelBuilder.Entity<Player>()
                .HasMany(p => p.Abilities)
                .WithMany(a => a.Players)
                .UsingEntity(j => j.ToTable("PlayerAbilities"));

            //set up rooms
            modelBuilder.Entity<Room>()
                .HasMany(r => r.Monsters)
                .WithOne(m => m.Room)
                .HasForeignKey(m => m.RoomId);

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

            base.OnModelCreating(modelBuilder);
        }

        public void UpdatePlayer(Player player)
        {
            Players.Update(player);
            SaveChanges();
        }
        public void UpdateMonster(Monster monster)
        {
            Monsters.Update(monster);
            SaveChanges();
        }
        public void UpdateAbility(Ability ability)
        {
            Abilities.Update(ability);
            List<Player> players = Players.Where(p => p.Abilities.Contains(ability)).ToList();
            Players.UpdateRange(players);
            SaveChanges();
        }
        public void UpdateItem(Item item)
        {
            Items.Update(item);
            SaveChanges();
        }
        public void UpdateInventory(Inventory inventory)
        {
            Inventories.Update(inventory);
            SaveChanges();
        }
    }
}


