using System;

/// <summary>
/// List of all possible action triggers that justify getting a bonus from a perk
/// Example: COMBAT
/// </summary>
public enum PerkCondition
{
    ANY,
    COMBAT,
    NATURAL,
    SWORD,
    SPEAR,
    HAMMER,
    AXE,
    RAID,
    SNEAK,
    TALK,
    BARTER,
    HUNT,
    FORAGE,
    FISH,
    FARM,
    CRAFT_WEAPON,
    CRAFT_ARMOR,
    CRAFT_TRAP,
    CRAFT_OBJECT,
    TOTAL
}
