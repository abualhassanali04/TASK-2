using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ProductCatalogApi.Data.Conventions
{
    /// <summary>
    /// SQL Server's datetime2 has no time zone, so EF reads values back with Kind = Unspecified
    /// and the API would serialize them without a "Z". All our times are stored in UTC,
    /// so mark them as UTC on the way out (and convert to UTC on the way in).
    /// </summary>
    public sealed class UtcDateTimeConverter : ValueConverter<DateTime, DateTime>
    {
        public UtcDateTimeConverter()
            : base(
                v => v.Kind == DateTimeKind.Utc ? v : v.ToUniversalTime(),
                v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
        {
        }
    }

    public sealed class NullableUtcDateTimeConverter : ValueConverter<DateTime?, DateTime?>
    {
        public NullableUtcDateTimeConverter()
            : base(
                v => v.HasValue ? (v.Value.Kind == DateTimeKind.Utc ? v : v.Value.ToUniversalTime()) : v,
                v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v)
        {
        }
    }
}
