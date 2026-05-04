using System.Reflection;
using BlueCommand.Domain.Entities;

namespace BlueCommand.Infrastructure.Utils;

public static class ChangeTracker
{
    public static IEnumerable<(string field, string? oldValue, string? newValue)> Diff<T>(T original, T updated, params string[] excludedProperties)
    {
        var excluded = new HashSet<string>(excludedProperties ?? Array.Empty<string>(), StringComparer.OrdinalIgnoreCase);
        foreach (var prop in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!prop.CanRead) continue;
            if (excluded.Contains(prop.Name)) continue;

            var oldVal = prop.GetValue(original);
            var newVal = prop.GetValue(updated);
            if (Equals(oldVal, newVal)) continue;

            yield return (prop.Name, oldVal?.ToString(), newVal?.ToString());
        }
    }

    public static IstoricSectie ToIstoricSectie(int sectieId, string field, string? oldVal, string? newVal, int modifiedById)
        => new()
        {
            SectieId = sectieId,
            CampModificat = field,
            ValoareVeche = oldVal,
            ValoareNoua = newVal,
            ModificatDe = modifiedById,
            ModificatLa = DateTime.UtcNow
        };

    public static IstoricUtilizator ToIstoricUtilizator(int utilizatorId, string field, string? oldVal, string? newVal, int modifiedById)
        => new()
        {
            UtilizatorId = utilizatorId,
            CampModificat = field,
            ValoareVeche = oldVal,
            ValoareNoua = newVal,
            ModificatDe = modifiedById,
            ModificatLa = DateTime.UtcNow
        };

    public static IstoricDosar ToIstoricDosar(int dosarId, string field, string? oldVal, string? newVal, int modifiedById)
        => new()
        {
            DosarId = dosarId,
            CampModificat = field,
            ValoareVeche = oldVal,
            ValoareNoua = newVal,
            ModificatDe = modifiedById,
            ModificatLa = DateTime.UtcNow
        };
}

