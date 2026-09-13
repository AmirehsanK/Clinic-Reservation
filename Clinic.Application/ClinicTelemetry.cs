using System.Diagnostics.Metrics;

namespace Clinic.Application;

/// <summary>
/// Business metrics for the booking flow. The meter is always created; values are
/// only exported when the web app has OpenTelemetry configured, otherwise
/// recording them costs next to nothing.
/// </summary>
public static class ClinicTelemetry
{
    public const string MeterName = "Clinic.Bookings";

    private static readonly Meter Meter = new(MeterName);

    public static readonly Counter<long> BookingsCreated =
        Meter.CreateCounter<long>("clinic.bookings.created", description: "Appointments successfully booked.");

    public static readonly Counter<long> BookingConflicts =
        Meter.CreateCounter<long>("clinic.bookings.conflicts",
            description: "Bookings rejected because another request reserved the same slot first.");
}
