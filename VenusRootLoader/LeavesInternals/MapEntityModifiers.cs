// ReSharper disable InconsistentNaming

namespace VenusRootLoader.LeavesInternals;

[Flags]
internal enum MapEntityModifiers
{
    // The order is important: it causes the least differences with base game when applied in this order
    None = 0,
    Holo = 1 << 0,
    ALW = 1 << 1,
    COT = 1 << 2,
    ShwKEY = 1 << 3,
    ICE = 1 << 4,
    HIDE = 1 << 5,
    FxdCol = 1 << 6,
    PAU = 1 << 7,
    ALF = 1 << 8,
    TIME = 1 << 9,
    Fixed = 1 << 10,
    ROT = 1 << 11,
    ShwEm = 1 << 12,
    NGS = 1 << 13,
    COG = 1 << 14,

    // TODO: Figure out what this gravity workaround was for
    NGF = 1 << 15,
    ITHD = 1 << 16,
    ITAH = 1 << 17,
    NEAR = 1 << 18,
    NDTCT = 1 << 19,
    DDIST = 1 << 20,
    TOD = 1 << 21,
}