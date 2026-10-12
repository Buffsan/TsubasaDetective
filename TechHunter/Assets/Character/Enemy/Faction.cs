using System;

[Flags]
public enum Faction
{
    None = 0,

    // 国
    Ferust   = 1 << 0,
    Diana    = 1 << 1,
    Garewo   = 1 << 2,
    Kardia   = 1 << 3,
    Pastal   = 1 << 4,
    Teruseto = 1 << 5,
    Shuela   = 1 << 6,
    Gratohto = 1 << 7,
    Himawari = 1 << 8,
    LAST     = 1 << 9,

    // 種族
    DemiHuman = 1 << 10,
    Monster   = 1 << 11,
    SeaBeast  = 1 << 12,
    Kingyo    = 1 << 13,
}
