using BoardingStrike.Core.Hex;
using BoardingStrike.Game.Cards;
using BoardingStrike.Game.Combat;
using BoardingStrike.Game.Content;

namespace BoardingStrike.Game.Units;

public enum Faction
{
    Marine,
    Hostile,
}

/// <summary>Common state for any unit on the board.</summary>
public abstract class Unit
{
    protected Unit(string id, string name, Faction faction, int maxHp, HexCoord position)
    {
        Id = id;
        Name = name;
        Faction = faction;
        MaxHp = maxHp;
        Hp = maxHp;
        Position = position;
    }

    public string Id { get; }

    public string Name { get; }

    public Faction Faction { get; }

    public int MaxHp { get; }

    public int Hp { get; private set; }

    public HexCoord Position { get; set; }

    /// <summary>The unit's attack-modifier deck (per-marine, or the shared hostile deck).</summary>
    public abstract ModifierDeck Modifiers { get; }

    public ConditionSet Conditions { get; } = new();

    /// <summary>True while the unit still has HP. Marines also leave play on exhaustion.</summary>
    public bool IsAlive => Hp > 0;

    /// <summary>Applies damage (clamped at 0); returns the amount actually dealt.</summary>
    public int TakeDamage(int amount)
    {
        int dealt = Math.Max(0, amount);
        Hp = Math.Max(0, Hp - dealt);
        return dealt;
    }

    /// <summary>Restores HP up to max and clears <see cref="ConditionKind.Wounded"/>.</summary>
    public void Heal(int amount)
    {
        if (amount > 0)
        {
            Hp = Math.Min(MaxHp, Hp + amount);
        }

        Conditions.Remove(ConditionKind.Wounded);
    }
}

/// <summary>A player-controlled marine with a card hand and a personal modifier deck.</summary>
public sealed class Marine : Unit
{
    public Marine(string id, string name, ClassDef classDef, HexCoord position, ModifierDeck modifiers)
        : base(id, name, Faction.Marine, classDef.MaxHp, position)
    {
        Class = classDef;
        Modifiers = modifiers;
        Hand = [.. classDef.StartingCards];
    }

    public ClassDef Class { get; }

    public override ModifierDeck Modifiers { get; }

    /// <summary>Cards available to play this scenario.</summary>
    public List<Card> Hand { get; }

    /// <summary>Cards spent but recoverable by a refresh.</summary>
    public List<Card> Discard { get; } = [];

    /// <summary>Cards burned for the rest of the scenario.</summary>
    public List<Card> Burned { get; } = [];

    /// <summary>True once the marine is out of the scenario (0 HP or no cards left to play).</summary>
    public bool IsExhausted { get; set; }

    /// <summary>A marine is active if alive and not exhausted.</summary>
    public bool IsActive => IsAlive && !IsExhausted;
}

/// <summary>A hostile instance of an enemy type.</summary>
public sealed class Hostile : Unit
{
    public Hostile(string id, EnemyDef enemy, HexCoord position, ModifierDeck modifiers, int spawnOrder)
        : base(id, enemy.Name, Faction.Hostile, enemy.MaxHp, position)
    {
        Enemy = enemy;
        Modifiers = modifiers;
        SpawnOrder = spawnOrder;
    }

    public EnemyDef Enemy { get; }

    public override ModifierDeck Modifiers { get; }

    /// <summary>Spawn index, used to order same-type units within a turn.</summary>
    public int SpawnOrder { get; }

    /// <summary>Bonus damage queued onto the next attack (e.g. the Spitter's Inhale).</summary>
    public int PendingBonusDamage { get; set; }
}
