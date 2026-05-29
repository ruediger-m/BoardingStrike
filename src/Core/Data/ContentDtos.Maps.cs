namespace BoardingStrike.Core.Data;

/// <summary>Axial bounds of a map in (q, r) space.</summary>
public sealed record BoundsData
{
    public int? QMin { get; init; }
    public int? QMax { get; init; }
    public int? RMin { get; init; }
    public int? RMax { get; init; }
}

/// <summary>An axial coordinate expressed as an object (used by void_hexes).</summary>
public sealed record CoordData
{
    public int? Q { get; init; }
    public int? R { get; init; }
}

/// <summary>A per-hex entry with an explicit kind.</summary>
public sealed record HexData
{
    public int? Q { get; init; }
    public int? R { get; init; }
    public string? Kind { get; init; }
}

/// <summary>
/// A non-default edge override. <see cref="A"/> and <see cref="B"/> are
/// two-element [q, r] arrays identifying the adjacent hexes the edge sits
/// between. <see cref="InitialState"/> applies to doors.
/// </summary>
public sealed record EdgeData
{
    public IReadOnlyList<int>? A { get; init; }
    public IReadOnlyList<int>? B { get; init; }
    public string? Kind { get; init; }
    public string? InitialState { get; init; }
}

/// <summary>
/// A spawn point. For marine slots, <see cref="Slot"/> is required. For enemies,
/// <see cref="EnemyId"/> is required and must resolve to an enemy type.
/// </summary>
public sealed record SpawnPointData
{
    public string? Kind { get; init; }
    public IReadOnlyList<int>? Hex { get; init; }
    public int? Slot { get; init; }
    public string? EnemyId { get; init; }
}

/// <summary>A playfield map (docs/technical/data-model.md "Maps").</summary>
public sealed record MapData
{
    public int? Version { get; init; }
    public string? Id { get; init; }
    public string? Name { get; init; }
    public BoundsData? Bounds { get; init; }
    public string? DefaultHexKind { get; init; }
    public IReadOnlyList<HexData>? Hexes { get; init; }
    public IReadOnlyList<CoordData>? VoidHexes { get; init; }
    public IReadOnlyList<EdgeData>? Edges { get; init; }
    public IReadOnlyList<SpawnPointData>? SpawnPoints { get; init; }
}

/// <summary>
/// A victory or failure condition. Only <see cref="Kind"/> is validated in the
/// MVP; the remaining fields are reserved for later condition kinds
/// (reach-hex, hold-hex, escort, defend) so the schema does not have to migrate.
/// </summary>
public sealed record ConditionData
{
    public string? Kind { get; init; }
    public IReadOnlyList<int>? Hex { get; init; }
    public int? Radius { get; init; }
    public int? Rounds { get; init; }
}

/// <summary>A scenario definition (docs/technical/data-model.md "Scenarios").</summary>
public sealed record MissionData
{
    public int? Version { get; init; }
    public string? Id { get; init; }
    public string? Name { get; init; }
    public string? MapId { get; init; }
    public ConditionData? Victory { get; init; }
    public ConditionData? Failure { get; init; }
    public int? RoundLimit { get; init; }
    public string? RngSeedSource { get; init; }
}
