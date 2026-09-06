using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CoreGym.Infrastructure.Configurations;

/// <summary>
/// Replaces Postgres text[] columns with NVARCHAR(MAX) JSON arrays
/// (decision 2026-09-05), exposed as List&lt;string&gt; on entities.
/// </summary>
internal static class JsonArrayListConversion
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public static PropertyBuilder<List<string>?> AsJsonArray(this PropertyBuilder<List<string>?> property)
    {
        var converter = new ValueConverter<List<string>?, string>(
            v => Serialize(v),
            v => string.IsNullOrWhiteSpace(v) ? null : JsonSerializer.Deserialize<List<string>>(v, SerializerOptions));

        var comparer = new ValueComparer<List<string>?>(
            (a, b) => Serialize(a) == Serialize(b),
            v => v == null ? 0 : Serialize(v)!.GetHashCode(),
            v => string.IsNullOrWhiteSpace(Serialize(v))
                ? null
                : JsonSerializer.Deserialize<List<string>>(Serialize(v)!, SerializerOptions));

        return property
            .HasColumnType("nvarchar(max)")
            .HasConversion(converter, comparer);
    }

    private static string? Serialize(List<string>? value) =>
        value is null ? null : JsonSerializer.Serialize(value, SerializerOptions);
}
