namespace BoardingStrike.Core.Data;

/// <summary>
/// An immutable, validated in-memory catalog of all content DTOs, keyed by id
/// within each category. Produced by <see cref="JsonContentLoader"/>. The Game
/// layer (Step 5) reads from this to build domain entities; nothing here knows
/// about the engine.
/// </summary>
public sealed class ContentDatabase
{
    public ContentDatabase(
        IReadOnlyDictionary<string, CardData> cards,
        IReadOnlyDictionary<string, ClassData> classes,
        IReadOnlyDictionary<string, ModifierDeckData> modifierDecks,
        IReadOnlyDictionary<string, EnemyData> enemies,
        IReadOnlyDictionary<string, AiCardData> aiCards,
        IReadOnlyDictionary<string, MapData> maps,
        IReadOnlyDictionary<string, MissionData> missions)
    {
        Cards = cards;
        Classes = classes;
        ModifierDecks = modifierDecks;
        Enemies = enemies;
        AiCards = aiCards;
        Maps = maps;
        Missions = missions;
    }

    public IReadOnlyDictionary<string, CardData> Cards { get; }

    public IReadOnlyDictionary<string, ClassData> Classes { get; }

    public IReadOnlyDictionary<string, ModifierDeckData> ModifierDecks { get; }

    public IReadOnlyDictionary<string, EnemyData> Enemies { get; }

    public IReadOnlyDictionary<string, AiCardData> AiCards { get; }

    public IReadOnlyDictionary<string, MapData> Maps { get; }

    public IReadOnlyDictionary<string, MissionData> Missions { get; }

    /// <summary>Total number of content items loaded across all categories.</summary>
    public int TotalCount =>
        Cards.Count + Classes.Count + ModifierDecks.Count + Enemies.Count
        + AiCards.Count + Maps.Count + Missions.Count;
}
