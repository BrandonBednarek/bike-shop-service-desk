using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BikeShop.Api.Infrastructure.Persistence;

// SQLite stores times as text with no time zone, so they come back as "Unspecified". Marking
// them UTC on the way out keeps the "Z" in the JSON, so browsers show the shop's local time.
public sealed class UtcDateTimeConverter() : ValueConverter<DateTime, DateTime>(
    utc => utc,
    stored => DateTime.SpecifyKind(stored, DateTimeKind.Utc));
