---

# 🧝‍♂️ Console RPG

Welcome to **Console RPG**, a turn-based role-playing game built using C# and .NET Core. Players explore, fight monsters, collect loot, and level up through a console-based interface. The game emphasizes tactical combat, character progression, and dynamic world interaction.

---

## 🎮 Features

* **Character Creation**: Choose from multiple archetypes like Warrior, Rogue, Mage, etc.
* **Skills System**: Includes martial, magical, support, and ultimate skills with cooldowns, scaling, and effects.
* **Combat System**: Turn-based battles with enemies, using tactics and abilities.
* **Inventory System**: Equip weapons, armor, and use consumables.
* **Room Navigation**: Explore a connected world via directional room movement.
* **Leveling System**: Gain XP and scale skills dynamically.
* **Monster AI**: Enemies attack and use skills intelligently.
* **Entity Framework Core**: Persistent data for characters, items, and environments.
* **Test Coverage**: Core mechanics are tested using MSTest.

---

## 🛠️ Technologies

* **C# / .NET Core**
* **Entity Framework Core**
* **MSTest** (for unit testing)
* **Console Input/Output abstraction**

---

## 🚀 Getting Started

### Prerequisites

* [.NET 6+ SDK](https://dotnet.microsoft.com/)
* A terminal or IDE (e.g., Visual Studio, JetBrains Rider)

### Setup

1. Clone the repository:

   ```bash
   git clone https://github.com/your-username/console-rpg.git
   cd console-rpg
   ```

2. Restore packages:

   ```bash
   dotnet restore
   ```

3. Apply migrations and seed the database (if applicable):

   ```bash
   dotnet ef database update
   ```

4. Run the game:

   ```bash
   dotnet run
   ```

---

## 🧪 Running Tests

Run unit tests with:

```bash
dotnet test
```

Test coverage includes skill logic, combat resolution, stat updates, and more.

---

## 🤝 Contributing

Have an idea or found a bug? Feel free to submit an issue or open a pull request!

1. Fork the repo
2. Create a new branch: `git checkout -b feature-foo`
3. Commit your changes: `git commit -m "Added foo"`
4. Push to the branch: `git push origin feature-foo`
5. Open a pull request

---
