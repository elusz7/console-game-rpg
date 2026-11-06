using ConsoleGameEntities.Models.Items;
using ConsoleGameEntities.Models.Monsters;
using Microsoft.EntityFrameworkCore;
using ConsoleGameEntities.Models.Skills;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Entities.Inventories;
using ConsoleGameEntities.Models.Runes;
using ConsoleGameEntities.Models.Runes.Recipes;

namespace ConsoleGameEntities.Data
{
    public class GameContext : DbContext
    {
        public DbSet<Player> Players { get; set; } = null!;
        public DbSet<Monster> Monsters { get; set; } = null!;
        public DbSet<Skill> Skills { get; set; } = null!;
        public DbSet<Item> Items { get; set; } = null!;
        public DbSet<Inventory> Inventories { get; set; } = null!;
        public DbSet<Room> Rooms { get; set; } = null!;
        public DbSet<Archetype> Archetypes { get; set; } = null!;
        public DbSet<InventoryRune> InventoryRunes { get; set; } = null!;
        public DbSet<InventoryIngredient> InventoryIngredients { get; set; } = null!;
        public DbSet<Rune> Runes { get; set; } = null!;
        public DbSet<Ingredient> Ingredients { get; set; } = null!;
        public DbSet<Recipe> Recipes { get; set; } = null!;
        public DbSet<RecipeIngredient> RecipeIngredients { get; set; } = null!;
        public DbSet<MonsterDrop> MonsterDrops { get; set; } = null!;
        public DbSet<EquippedRune> EquippedRunes { get; set; } = null!;

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
                .HasForeignKey<Item>(i => i.MonsterId)
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
            
            modelBuilder.Entity<Weapon>()
                .Property<bool>("Equipped")
                .HasColumnName("Equipped");

            modelBuilder.Entity<Armor>()
                .Property<bool>("Equipped")
                .HasColumnName("Equipped");

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
                .HasForeignKey(item => item.InventoryId)
                .OnDelete(DeleteBehavior.SetNull);
            
            // Configure persistent rune inventory.
            modelBuilder.Entity<InventoryRune>()
                .HasKey(ir => new { ir.InventoryId, ir.RuneId, ir.Tier });

            modelBuilder.Entity<InventoryRune>()
                .HasOne(ir => ir.Inventory)
                .WithMany(i => i.Runes)
                .HasForeignKey(ir => ir.InventoryId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<InventoryRune>()
                .HasOne(ir => ir.Rune)
                .WithMany()
                .HasForeignKey(ir => ir.RuneId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<InventoryRune>()
                .Property(ir => ir.Quantity)
                .IsRequired();

            // Configure persistent ingredient inventory.
            modelBuilder.Entity<InventoryIngredient>()
                .HasKey(ii => new { ii.InventoryId, ii.IngredientId });

            modelBuilder.Entity<InventoryIngredient>()
                .HasOne(ii => ii.Inventory)
                .WithMany(i => i.Ingredients)
                .HasForeignKey(ii => ii.InventoryId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<InventoryIngredient>()
                .HasOne(ii => ii.Ingredient)
                .WithMany()
                .HasForeignKey(ii => ii.IngredientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<InventoryIngredient>()
                .Property(ii => ii.Quantity)
                .IsRequired();
            
            // Configure rune inheritance.
            modelBuilder.Entity<Rune>()
                .HasDiscriminator<string>(r => r.RuneType)
                .HasValue<WeaponRune>("WeaponRune")
                .HasValue<ArmorRune>("ArmorRune");

// Configure Recipe-to-Rune relationship.
            modelBuilder.Entity<Recipe>()
                .HasOne(r => r.Rune)
                .WithOne(r => r.Recipe)
                .HasForeignKey<Recipe>(r => r.RuneId)
                .OnDelete(DeleteBehavior.Restrict);

// Configure RecipeIngredient relationships.
            modelBuilder.Entity<RecipeIngredient>()
                .HasOne(ri => ri.Recipe)
                .WithMany(r => r.Ingredients)
                .HasForeignKey(ri => ri.RecipeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RecipeIngredient>()
                .HasOne(ri => ri.Ingredient)
                .WithMany()
                .HasForeignKey(ri => ri.IngredientId)
                .OnDelete(DeleteBehavior.Restrict);

            // Prevent duplicate ingredient entries within a recipe.
            modelBuilder.Entity<RecipeIngredient>()
                .HasIndex(ri => new { ri.RecipeId, ri.IngredientId })
                .IsUnique();

            // Configure MonsterDrop composite key and ingredient relationship.
            modelBuilder.Entity<MonsterDrop>()
                .HasKey(md => new
                {
                    md.Element,
                    md.ThreatLevel,
                    md.IngredientId
                });

            modelBuilder.Entity<MonsterDrop>()
                .HasOne(md => md.Ingredient)
                .WithMany()
                .HasForeignKey(md => md.IngredientId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure drop rate precision.
            modelBuilder.Entity<MonsterDrop>()
                .Property(md => md.DropRate)
                .HasColumnType("decimal(5,2)");
            
            // Configure EquippedRune relationships
            modelBuilder.Entity<EquippedRune>()
                .HasOne(e => e.Item)
                .WithOne(i => i.EquippedRune)
                .HasForeignKey<EquippedRune>(e => e.ItemId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<EquippedRune>()
                .HasOne(e => e.InventoryRune)
                .WithMany(ir => ir.EquippedRunes)
                .HasForeignKey(e => new { e.InventoryId, e.RuneId, e.Tier })
                .OnDelete(DeleteBehavior.Restrict);
            
            modelBuilder.Entity<EquippedRune>()
                .HasIndex(e => new { e.InventoryId, e.RuneId, e.Tier })
                .IsUnique();

            //configure room relationships
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
                .HasForeignKey(s => s.ArchetypeId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Skill>()
                .HasOne(s => s.Monster)
                .WithMany(m => m.Skills)
                .HasForeignKey(s => s.MonsterId)
                .OnDelete(DeleteBehavior.SetNull);

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