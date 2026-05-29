namespace BoardingStrike.Core.Data;

/// <summary>
/// Semantic validation of a loaded <see cref="ContentDatabase"/>: required
/// fields, enum membership, value ranges, and cross-references between
/// categories. Pure functions that append human-readable messages to an error
/// list; never throws on bad content (the loader decides what to do with the
/// errors).
/// </summary>
internal static class ContentValidator
{
    public static void Validate(ContentDatabase db, List<string> errors)
    {
        foreach ((string id, ModifierDeckData deck) in db.ModifierDecks)
        {
            ValidateModifierDeck(id, deck, errors);
        }

        foreach ((string id, CardData card) in db.Cards)
        {
            ValidateCard(id, card, db, errors);
        }

        foreach ((string id, ClassData klass) in db.Classes)
        {
            ValidateClass(id, klass, db, errors);
        }

        foreach ((string id, AiCardData aiCard) in db.AiCards)
        {
            ValidateAiCard(id, aiCard, errors);
        }

        foreach ((string id, EnemyData enemy) in db.Enemies)
        {
            ValidateEnemy(id, enemy, db, errors);
        }

        foreach ((string id, MapData map) in db.Maps)
        {
            ValidateMap(id, map, db, errors);
        }

        foreach ((string id, MissionData mission) in db.Missions)
        {
            ValidateMission(id, mission, db, errors);
        }
    }

    private static void ValidateModifierDeck(string id, ModifierDeckData deck, List<string> errors)
    {
        string ctx = $"modifier_deck '{id}'";
        if (deck.Cards is null || deck.Cards.Count == 0)
        {
            errors.Add($"{ctx}: must define at least one 'cards' entry.");
            return;
        }

        for (int i = 0; i < deck.Cards.Count; i++)
        {
            ModifierEntryData entry = deck.Cards[i];
            string entryCtx = $"{ctx}, cards[{i}]";
            RequireEnum(entry.Effect, ContentVocabulary.ModifierEffects, $"{entryCtx}.effect", errors);
            if (entry.Value is null)
            {
                errors.Add($"{entryCtx}: requires 'value'.");
            }

            if (entry.Count is null or < 1)
            {
                errors.Add($"{entryCtx}: 'count' must be >= 1.");
            }
        }
    }

    private static void ValidateCard(string id, CardData card, ContentDatabase db, List<string> errors)
    {
        string ctx = $"card '{id}'";
        RequireRange(card.Initiative, 1, 99, $"{ctx}.initiative", errors);
        RequireReference(card.ClassId, db.Classes, $"{ctx}.class_id", "class", errors);
        ValidateHalf(card.Top, $"{ctx}.top", errors);
        ValidateHalf(card.Bottom, $"{ctx}.bottom", errors);
    }

    private static void ValidateHalf(CardHalfData? half, string ctx, List<string> errors)
    {
        if (half is null)
        {
            errors.Add($"{ctx}: required half is missing.");
            return;
        }

        if (half.Actions is null || half.Actions.Count == 0)
        {
            errors.Add($"{ctx}: must contain at least one action.");
            return;
        }

        for (int i = 0; i < half.Actions.Count; i++)
        {
            ValidateAction(half.Actions[i], $"{ctx}.actions[{i}]", errors);
        }
    }

    private static void ValidateClass(string id, ClassData klass, ContentDatabase db, List<string> errors)
    {
        string ctx = $"class '{id}'";
        RequireRange(klass.Hp, 1, int.MaxValue, $"{ctx}.hp", errors);
        RequireReference(klass.ModifierDeckId, db.ModifierDecks, $"{ctx}.modifier_deck_id", "modifier_deck", errors);

        if (klass.StartingCardIds is null || klass.StartingCardIds.Count == 0)
        {
            errors.Add($"{ctx}: must list at least one 'starting_card_ids'.");
        }
        else
        {
            foreach (string cardId in klass.StartingCardIds)
            {
                if (!db.Cards.ContainsKey(cardId))
                {
                    errors.Add($"{ctx}.starting_card_ids: references unknown card '{cardId}'.");
                }
            }
        }
    }

    private static void ValidateAiCard(string id, AiCardData card, List<string> errors)
    {
        string ctx = $"ai_card '{id}'";
        RequireRange(card.Initiative, 1, 99, $"{ctx}.initiative", errors);
        RequireRange(card.Movement, 0, int.MaxValue, $"{ctx}.movement", errors);
        RequireEnum(card.TargetPriority, ContentVocabulary.TargetPriorities, $"{ctx}.target_priority", errors);
        RequireEnum(card.MovementMode, ContentVocabulary.MovementModes, $"{ctx}.movement_mode", errors);

        if (card.Actions is not null)
        {
            for (int i = 0; i < card.Actions.Count; i++)
            {
                ValidateAction(card.Actions[i], $"{ctx}.actions[{i}]", errors);
            }
        }
    }

    private static void ValidateEnemy(string id, EnemyData enemy, ContentDatabase db, List<string> errors)
    {
        string ctx = $"enemy '{id}'";
        RequireRange(enemy.Hp, 1, int.MaxValue, $"{ctx}.hp", errors);
        RequireReference(enemy.ModifierDeckId, db.ModifierDecks, $"{ctx}.modifier_deck_id", "modifier_deck", errors);

        if (enemy.AiCardIds is null || enemy.AiCardIds.Count == 0)
        {
            errors.Add($"{ctx}: must list at least one 'ai_card_ids'.");
        }
        else
        {
            foreach (string aiCardId in enemy.AiCardIds)
            {
                if (!db.AiCards.ContainsKey(aiCardId))
                {
                    errors.Add($"{ctx}.ai_card_ids: references unknown ai_card '{aiCardId}'.");
                }
            }
        }
    }

    private static void ValidateMap(string id, MapData map, ContentDatabase db, List<string> errors)
    {
        string ctx = $"map '{id}'";
        BoundsData? bounds = map.Bounds;
        if (bounds is null || bounds.QMin is null || bounds.QMax is null || bounds.RMin is null || bounds.RMax is null)
        {
            errors.Add($"{ctx}: 'bounds' must define q_min, q_max, r_min, r_max.");
        }
        else if (bounds.QMin > bounds.QMax || bounds.RMin > bounds.RMax)
        {
            errors.Add($"{ctx}: 'bounds' min exceeds max.");
        }

        if (map.DefaultHexKind is not null)
        {
            RequireEnum(map.DefaultHexKind, ContentVocabulary.HexKinds, $"{ctx}.default_hex_kind", errors);
        }

        if (map.Hexes is not null)
        {
            for (int i = 0; i < map.Hexes.Count; i++)
            {
                RequireEnum(map.Hexes[i].Kind, ContentVocabulary.HexKinds, $"{ctx}.hexes[{i}].kind", errors);
            }
        }

        ValidateEdges(ctx, map, errors);
        ValidateSpawnPoints(ctx, map, db, errors);
    }

    private static void ValidateEdges(string ctx, MapData map, List<string> errors)
    {
        if (map.Edges is null)
        {
            return;
        }

        for (int i = 0; i < map.Edges.Count; i++)
        {
            EdgeData edge = map.Edges[i];
            string edgeCtx = $"{ctx}.edges[{i}]";
            bool coordsOk = IsCoordPair(edge.A) && IsCoordPair(edge.B);
            if (!coordsOk)
            {
                errors.Add($"{edgeCtx}: 'a' and 'b' must each be a [q, r] pair.");
            }
            else if (!AreAdjacent(edge.A!, edge.B!))
            {
                errors.Add($"{edgeCtx}: 'a' and 'b' must be adjacent hexes.");
            }

            RequireEnum(edge.Kind, ContentVocabulary.EdgeKinds, $"{edgeCtx}.kind", errors);
            if (edge.Kind == "door")
            {
                RequireEnum(edge.InitialState, ContentVocabulary.EdgeStates, $"{edgeCtx}.initial_state", errors);
            }
        }
    }

    private static void ValidateSpawnPoints(string ctx, MapData map, ContentDatabase db, List<string> errors)
    {
        if (map.SpawnPoints is null)
        {
            return;
        }

        for (int i = 0; i < map.SpawnPoints.Count; i++)
        {
            SpawnPointData spawn = map.SpawnPoints[i];
            string spawnCtx = $"{ctx}.spawn_points[{i}]";
            RequireEnum(spawn.Kind, ContentVocabulary.SpawnKinds, $"{spawnCtx}.kind", errors);

            if (!IsCoordPair(spawn.Hex))
            {
                errors.Add($"{spawnCtx}: 'hex' must be a [q, r] pair.");
            }

            switch (spawn.Kind)
            {
                case "marine_slot":
                    if (spawn.Slot is null or < 1)
                    {
                        errors.Add($"{spawnCtx}: marine_slot requires 'slot' >= 1.");
                    }

                    break;
                case "enemy":
                    if (string.IsNullOrWhiteSpace(spawn.EnemyId))
                    {
                        errors.Add($"{spawnCtx}: enemy spawn requires 'enemy_id'.");
                    }
                    else if (!db.Enemies.ContainsKey(spawn.EnemyId))
                    {
                        errors.Add($"{spawnCtx}: references unknown enemy '{spawn.EnemyId}'.");
                    }

                    break;
            }
        }
    }

    private static void ValidateMission(string id, MissionData mission, ContentDatabase db, List<string> errors)
    {
        string ctx = $"mission '{id}'";
        RequireReference(mission.MapId, db.Maps, $"{ctx}.map_id", "map", errors);

        if (mission.Victory is null)
        {
            errors.Add($"{ctx}: requires a 'victory' condition.");
        }
        else
        {
            RequireEnum(mission.Victory.Kind, ContentVocabulary.VictoryKinds, $"{ctx}.victory.kind", errors);
        }

        if (mission.Failure is null)
        {
            errors.Add($"{ctx}: requires a 'failure' condition.");
        }
        else
        {
            RequireEnum(mission.Failure.Kind, ContentVocabulary.FailureKinds, $"{ctx}.failure.kind", errors);
        }

        if (mission.RngSeedSource is not null
            && !ContentVocabulary.RngSeedSources.Contains(mission.RngSeedSource)
            && !long.TryParse(mission.RngSeedSource, out _))
        {
            errors.Add($"{ctx}.rng_seed_source: must be 'scenario_id', 'random', or an integer literal.");
        }
    }

    private static void ValidateAction(ActionData action, string ctx, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(action.Kind))
        {
            errors.Add($"{ctx}: action requires 'kind'.");
            return;
        }

        if (!ContentVocabulary.ActionKinds.Contains(action.Kind))
        {
            errors.Add($"{ctx}: unknown action kind '{action.Kind}'.");
            return;
        }

        switch (action.Kind)
        {
            case "move":
                RequireRange(action.Distance, 0, int.MaxValue, $"{ctx}.distance", errors);
                OptionalEnum(action.Type, ContentVocabulary.MoveTypes, $"{ctx}.type", errors);
                break;
            case "attack_melee":
                RequireRange(action.Damage, 0, int.MaxValue, $"{ctx}.damage", errors);
                OptionalEnum(action.OnHitCondition, ContentVocabulary.Conditions, $"{ctx}.on_hit_condition", errors);
                break;
            case "attack_ranged":
                RequireRange(action.Damage, 0, int.MaxValue, $"{ctx}.damage", errors);
                RequireRange(action.Range, 1, int.MaxValue, $"{ctx}.range", errors);
                OptionalEnum(action.OnHitCondition, ContentVocabulary.Conditions, $"{ctx}.on_hit_condition", errors);
                break;
            case "attack_area":
                RequireRange(action.Damage, 0, int.MaxValue, $"{ctx}.damage", errors);
                RequireRange(action.Range, 1, int.MaxValue, $"{ctx}.range", errors);
                break;
            case "heal":
                RequireRange(action.Amount, 0, int.MaxValue, $"{ctx}.amount", errors);
                OptionalEnum(action.Target, ContentVocabulary.HealTargets, $"{ctx}.target", errors);
                break;
            case "apply_condition":
                RequireEnum(action.Condition, ContentVocabulary.Conditions, $"{ctx}.condition", errors);
                break;
            case "door":
                RequireEnum(action.Operation, ContentVocabulary.DoorOperations, $"{ctx}.operation", errors);
                break;
            case "loot":
                break;
        }
    }

    // ----- shared helpers -----

    private static void RequireEnum(string? value, IReadOnlySet<string> allowed, string field, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errors.Add($"{field}: required value is missing.");
        }
        else if (!allowed.Contains(value))
        {
            errors.Add($"{field}: '{value}' is not one of [{string.Join(", ", allowed.Order(StringComparer.Ordinal))}].");
        }
    }

    private static void OptionalEnum(string? value, IReadOnlySet<string> allowed, string field, List<string> errors)
    {
        if (value is not null && !allowed.Contains(value))
        {
            errors.Add($"{field}: '{value}' is not one of [{string.Join(", ", allowed.Order(StringComparer.Ordinal))}].");
        }
    }

    private static void RequireRange(int? value, int min, int max, string field, List<string> errors)
    {
        if (value is null)
        {
            errors.Add($"{field}: required value is missing.");
        }
        else if (value < min || value > max)
        {
            errors.Add($"{field}: {value} is out of range [{min}, {max}].");
        }
    }

    private static void RequireReference<T>(
        string? value, IReadOnlyDictionary<string, T> table, string field, string targetKind, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errors.Add($"{field}: required reference is missing.");
        }
        else if (!table.ContainsKey(value))
        {
            errors.Add($"{field}: references unknown {targetKind} '{value}'.");
        }
    }

    private static bool IsCoordPair(IReadOnlyList<int>? coord) => coord is { Count: 2 };

    private static bool AreAdjacent(IReadOnlyList<int> a, IReadOnlyList<int> b)
    {
        var hexA = new Hex.HexCoord(a[0], a[1]);
        var hexB = new Hex.HexCoord(b[0], b[1]);
        return hexA.IsAdjacentTo(hexB);
    }
}
