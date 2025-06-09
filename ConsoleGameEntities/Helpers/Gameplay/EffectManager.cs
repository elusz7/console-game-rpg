using System.Collections.Concurrent;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameEntities.Helpers.Gameplay;

public class EffectManager
{
    private readonly ConcurrentDictionary<long, StatusRecord> _effects = new();

    public void AddEffect(StatusRecordType source, int type, int duration, int power)
    {
        long key = DateTime.Now.Ticks;
        while (!_effects.TryAdd(key, new StatusRecord(source, type, duration, power)))
            key++;
    }
    public int SumByEffect(StatType stat)
    {
        var baseStat = SumByCondition(StatusRecordType.Skill, (int)stat);

        var elementalEffect = stat switch
        {
            StatType.Defense => ElementalStatusEffectType.Charred,
            StatType.Resistance => ElementalStatusEffectType.Shocked,
            StatType.Speed => ElementalStatusEffectType.Snared,
            _ => throw new ArgumentOutOfRangeException(nameof(stat), "Unsupported stat type for elemental status effects.")
        };

        return baseStat - SumByCondition(StatusRecordType.ElementalStatus, (int)elementalEffect);
    }

    public int SumByCondition(StatusRecordType source, int type) =>
        _effects.Values.Where(e => e.Source == source && e.Type == type).Sum(e => e.Power);

    public void ElapseTime(Action<StatusRecord> onExpire, Action<StatusRecord> onTick)
    {
        var expiredKeys = new List<long>();

        foreach (var kvp in _effects)
        {
            var record = kvp.Value;
            if (record.Duration - 1 <= 0)
            {
                expiredKeys.Add(kvp.Key);
                onExpire?.Invoke(record);
            }
            else
            {
                _effects[kvp.Key] = record with { Duration = record.Duration - 1 };
                onTick?.Invoke(record);
            }
        }

        foreach (var key in expiredKeys)
            _effects.TryRemove(key, out _);
    }

    public void ClearEffects()
    {
        _effects.Clear();
    }
}
