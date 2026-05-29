using System.Text.Json;

namespace BoardingStrike.Core.Data;

/// <summary>
/// Loads and validates all game content from a directory tree of JSON files
/// (docs/technical/data-model.md). Each category lives in its own subfolder:
/// <c>cards/</c>, <c>classes/</c>, <c>modifier_decks/</c>, <c>enemies/</c>,
/// <c>ai_cards/</c>, <c>maps/</c>, <c>missions/</c>. A missing subfolder is
/// treated as "no content of that kind", not an error.
///
/// <para>
/// The loader produces a validated <see cref="ContentDatabase"/> of raw DTOs;
/// mapping DTOs to Game-layer domain entities happens in the Game layer (Step 5)
/// to preserve the Game → Core dependency direction. The loader never silently
/// defaults bad content: it aggregates every problem and surfaces them together
/// (<see cref="ContentValidationException"/>).
/// </para>
/// </summary>
public static class JsonContentLoader
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = false,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    /// <summary>
    /// Loads and validates content, throwing <see cref="ContentValidationException"/>
    /// if anything is wrong.
    /// </summary>
    public static ContentDatabase Load(string rootDirectory)
    {
        if (!TryLoad(rootDirectory, out ContentDatabase? database, out IReadOnlyList<string> errors))
        {
            throw new ContentValidationException(errors);
        }

        return database!;
    }

    /// <summary>
    /// Attempts to load and validate content. Returns false (with a populated
    /// <paramref name="errors"/> list) instead of throwing. <paramref name="database"/>
    /// is still produced so callers can inspect partially loaded content.
    /// </summary>
    public static bool TryLoad(string rootDirectory, out ContentDatabase? database, out IReadOnlyList<string> errors)
    {
        ArgumentNullException.ThrowIfNull(rootDirectory);

        var messages = new List<string>();

        if (!Directory.Exists(rootDirectory))
        {
            messages.Add($"content root directory does not exist: '{rootDirectory}'.");
            database = null;
            errors = messages;
            return false;
        }

        var cards = LoadCategory<CardData>(rootDirectory, "cards", c => c.Id, messages);
        var classes = LoadCategory<ClassData>(rootDirectory, "classes", c => c.Id, messages);
        var modifierDecks = LoadCategory<ModifierDeckData>(rootDirectory, "modifier_decks", d => d.Id, messages);
        var enemies = LoadCategory<EnemyData>(rootDirectory, "enemies", e => e.Id, messages);
        var aiCards = LoadCategory<AiCardData>(rootDirectory, "ai_cards", a => a.Id, messages);
        var maps = LoadCategory<MapData>(rootDirectory, "maps", m => m.Id, messages);
        var missions = LoadCategory<MissionData>(rootDirectory, "missions", m => m.Id, messages);

        var db = new ContentDatabase(cards, classes, modifierDecks, enemies, aiCards, maps, missions);

        ContentValidator.Validate(db, messages);

        database = db;
        errors = messages;
        return messages.Count == 0;
    }

    private static Dictionary<string, T> LoadCategory<T>(
        string rootDirectory, string category, Func<T, string?> idSelector, List<string> errors)
    {
        var result = new Dictionary<string, T>(StringComparer.Ordinal);
        string directory = Path.Combine(rootDirectory, category);
        if (!Directory.Exists(directory))
        {
            return result;
        }

        // Ordinal ordering makes the load (and thus any error ordering) deterministic.
        foreach (string file in Directory.EnumerateFiles(directory, "*.json").OrderBy(f => f, StringComparer.Ordinal))
        {
            string fileName = Path.GetFileName(file);
            T? dto;
            try
            {
                dto = JsonSerializer.Deserialize<T>(File.ReadAllText(file), Options);
            }
            catch (JsonException ex)
            {
                errors.Add($"{category}: failed to parse '{fileName}': {ex.Message}");
                continue;
            }

            if (dto is null)
            {
                errors.Add($"{category}: '{fileName}' deserialized to null.");
                continue;
            }

            string? id = idSelector(dto);
            if (string.IsNullOrWhiteSpace(id))
            {
                errors.Add($"{category}: '{fileName}' is missing the required 'id' field.");
                continue;
            }

            if (!result.TryAdd(id, dto))
            {
                errors.Add($"{category}: duplicate id '{id}' (offending file '{fileName}').");
            }
        }

        return result;
    }
}
