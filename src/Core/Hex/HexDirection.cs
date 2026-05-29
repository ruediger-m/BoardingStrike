namespace BoardingStrike.Core.Hex;

/// <summary>
/// The six axial neighbor directions, in the order defined by the design doc
/// (docs/design/hex-grid.md). The index of each value is its position in
/// <see cref="HexCoord.Directions"/>.
///
/// Naming note: the labels (E/NE/NW/W/SW/SE) come from the design doc's table,
/// which uses pointy-top directional names. The map renders as flat-top
/// (see docs/technical/art-pipeline.md), where these axial offsets actually
/// point N/S plus four diagonals. The names are therefore nominal — all
/// geometry derives from the axial offsets and the flat-top pixel layout, not
/// from these labels, so the discrepancy is cosmetic. Tracked for a doc fix.
/// </summary>
public enum HexDirection
{
    E = 0,
    NE = 1,
    NW = 2,
    W = 3,
    SW = 4,
    SE = 5,
}
