using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConsoleGameEntities.Migrations;

public partial class SeedMonsterDrops : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Seed MonsterDrops table with essence drops
        var elementTypes = Enumerable.Range(0, 6); // 0 to 5
        var essenceDropTable = new List<object[]>();

        var essenceDropRatesByThreat = new Dictionary<int, Dictionary<int, decimal>> // new Dictionary<threatLevel, Dictionary<essenceId, dropRate>>
        {
            [0] = new() { [1] = 25.0M, [2] = 10.0M, [3] = 1.0M },
            [1] = new() { [1] = 30.0M, [2] = 15.0M, [3] = 5.0M, [4] = 1.0M },
            [2] = new() { [1] = 35.0M, [2] = 20.0M, [3] = 10.0M, [4] = 5.0M, [5] = 1.0M },
            [3] = new() { [1] = 40.0M, [2] = 25.0M, [3] = 15.0M, [4] = 10.0M, [5] = 5.0M, [6] = 1.0M },
            [4] = new() { [2] = 35.0M, [3] = 25.0M, [4] = 20.0M, [5] = 10.0M, [6] = 5.0M }
        };

        foreach (var threatLevel in essenceDropRatesByThreat.Keys)
        {
            foreach (var element in elementTypes)
            {
                foreach (var kvp in essenceDropRatesByThreat[threatLevel])
                {
                    essenceDropTable.Add(new object[] { element, threatLevel, kvp.Key, kvp.Value });
                }
            }
        }

        // Seed MonsterDrops table with core drops
        var coreDropTable = new List<object[]>();

        var coreDropRatesByThreat = new List<decimal>()
        {
            1.0M, // ThreatLevel 0 = 1.0%
            5.0M, // ThreatLevel 1 = 5.0%
            10.0M, // ThreatLevel 2 = 10.0%
            15.0M, // ThreatLevel 3 = 15.0%
            20.0M, // ThreatLevel 4 = 20.0%
        };

        for (int threatLevel = 0; threatLevel < coreDropRatesByThreat.Count; threatLevel++)
        {
            foreach (var element in elementTypes)
            {
                for (int id = 7; id <= 12; id++) // Core ingredients from 7 to 12
                {
                    coreDropTable.Add(new object[] { element, threatLevel, id, coreDropRatesByThreat[threatLevel] });
                }
            }
        }

        //Seed MonsterDrops table with monster part drops
        // Threat Level 0, Common Ingredients (13-29), drop rate 30%
        // Threat Level 0, Uncommon Ingredients (30-46), drop rate 15%
        // Threat Level 0, Rare Ingredients (47-60), drop rate 5%

        // Threat Level 1, Common Ingredients (13-29), drop rate 40%
        // Threat Level 1, Uncommon Ingredients (30-46), drop rate 30%
        // Threat Level 1, Rare Ingredients (47-60), drop rate 15%
        // Threat Level 1, Epic Ingredients (61-75), drop rate 5%

        // Threat Level 2, Common Ingredients (13-29), drop rate 20%
        // Threat Level 2, Uncommon Ingredients (30-46), drop rate 40%
        // Threat Level 2, Rare Ingredients (47-60), drop rate 30%
        // Threat Level 2, Epic Ingredients (61-75), drop rate 15%
        // Threat Level 2, Legendary Ingredients (76-90), drop rate 5%

        // Threat Level 3, Uncommon Ingredients (30-46), drop rate 20%
        // Threat Level 3, Rare Ingredients (47-60), drop rate 40%
        // Threat Level 3, Epic Ingredients (61-75), drop rate 30%
        // Threat Level 3, Legendary Ingredients (76-90), drop rate 10%
        // Threat Level 3, Mythic Ingredients (91-105), drop rate 1%

        // Threat Level 4, Rare Ingredients (47-60), drop rate 20%
        // Threat Level 4, Epic Ingredients (61-75), drop rate 40%
        // Threat Level 4, Legendary Ingredients (76-90), drop rate 30%
        // Threat Level 4, Mythic Ingredients (91-102), drop rate 5%

        var ingredientTiers = new Dictionary<string, (int start, int end)>
        {
            { "Common", (13, 29) },
            { "Uncommon", (30, 46) },
            { "Rare", (47, 60) },
            { "Epic", (61, 75) },
            { "Legendary", (76, 90) },
            { "Mythic", (91, 102) },
        };

        var monsterPartDropsByThreat = new Dictionary<int, List<(string rarity, decimal dropRate)>>()
        {
            [0] = new() {
                ("Common", 30.0M),
                ("Uncommon", 15.0M),
                ("Rare", 5.0M),
            },
            [1] = new() {
                ("Common", 40.0M),
                ("Uncommon", 30.0M),
                ("Rare", 15.0M),
                ("Epic", 5.0M),
            },
            [2] = new() {
                ("Common", 20.0M),
                ("Uncommon", 40.0M),
                ("Rare", 30.0M),
                ("Epic", 15.0M),
                ("Legendary", 5.0M),
            },
            [3] = new() {
                ("Uncommon", 20.0M),
                ("Rare", 40.0M),
                ("Epic", 30.0M),
                ("Legendary", 10.0M),
                ("Mythic", 1.0M),
            },
            [4] = new() {
                ("Rare", 20.0M),
                ("Epic", 40.0M),
                ("Legendary", 30.0M),
                ("Mythic", 5.0M),
            },
        };

        var monsterPartDropTable = new List<object[]>();
        var baseElements = elementTypes.ToList();

        foreach (var threatEntry in monsterPartDropsByThreat)
        {
            int threatLevel = threatEntry.Key;
            var rarityDrops = threatEntry.Value;

            foreach (var (rarity, dropRate) in rarityDrops)
            {
                var (startId, endId) = ingredientTiers[rarity];
                int count = endId - startId + 1;

                // Repeat element list to match ingredient count
                var expandedElements = Enumerable.Range(0, count)
                .Select(i => baseElements[i % baseElements.Count])
                .OrderBy(_ => Random.Shared.Next())
                .ToList();

                int index = 0;
                for (int id = startId; id <= endId; id++)
                {
                    var element = expandedElements[index++];
                    monsterPartDropTable.Add(new object[] { element, threatLevel, id, dropRate });
                }
            }
        }

        // Combine all drop tables
        var combinedDropTable = essenceDropTable
            .Concat(coreDropTable)
            .Concat(monsterPartDropTable)
            .ToList();

        // Insert into MonsterDrops table
        migrationBuilder.InsertData(
            table: "MonsterDrops",
            columns: new[] { "Element", "ThreatLevel", "IngredientId", "DropRate" },
            values: To2DArray(combinedDropTable));
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {

    }

    private static object[,] To2DArray(List<object[]> data)
    {
        int rows = data.Count;
        int cols = data[0].Length;
        var result = new object[rows, cols];
        for (int i = 0; i < rows; i++)
            for (int j = 0; j < cols; j++)
                result[i, j] = data[i][j];
        return result;
    }
}
