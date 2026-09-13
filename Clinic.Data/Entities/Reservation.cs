using System.ComponentModel.DataAnnotations;

namespace Clinic.Data.Entities;

public class Reservation : BaseEntity
{
    public DateTime ReserveTime { get; set; }
    public DateTime EndReserveTime { get; set; }

    // Concurrency token: EF adds "AND Reserved = <value when loaded>" to every
    // UPDATE of this row. Two requests that both read a free slot and both try to
    // book it can no longer both succeed - the second UPDATE matches zero rows
    // and SaveChanges throws DbUpdateConcurrencyException instead of silently
    // double-booking. Needs no schema change, and works on SQL Server and SQLite.
    [ConcurrencyCheck]
    public bool Reserved { get; set; }
}
