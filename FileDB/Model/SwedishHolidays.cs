using System.Collections.Generic;

namespace FileDB.Model;

public record FixedHoliday(string Name, int Month, int Day);

public static class SwedishHolidays
{
    public static IReadOnlyList<FixedHoliday> All { get; } =
    [
        new("Nyårsdagen", 1, 1),
        new("Trettondagsafton", 1, 5),
        new("Trettondedag jul", 1, 6),
        new("Valborgsmässoafton", 4, 30),
        new("Första maj", 5, 1),
        new("Sveriges nationaldag", 6, 6),
        new("Julafton", 12, 24),
        new("Juldagen", 12, 25),
        new("Annandag jul", 12, 26),
        new("Nyårsafton", 12, 31),
    ];
}
