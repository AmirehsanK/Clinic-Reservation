using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Clinic.Data.Entities;

namespace Clinic.Application.Utilities;

/// <summary>
/// Maps domain enums and values to Tabler UI CSS classes (badges, status dots)
/// and provides small formatting helpers used across the Razor views.
/// </summary>
public static class TablerExtensions
{
    #region Badge Class Mapping

    public static string ToBadgeClass(this Gender gender) => gender switch
    {
        Gender.Male => "bg-blue-lt",
        Gender.Female => "bg-pink-lt",
        Gender.NotSpecified => "bg-secondary-lt",
        _ => "bg-secondary-lt"
    };

    public static string ToBadgeClass(this ReserveStatus status) => status switch
    {
        ReserveStatus.Reserved => "bg-warning-lt",
        ReserveStatus.Attended => "bg-success-lt",
        ReserveStatus.Cancelled => "bg-danger-lt",
        _ => "bg-secondary-lt"
    };

    public static string ToBadgeClass(this PaymentType paymentType) => paymentType switch
    {
        PaymentType.Cash => "bg-green-lt",
        PaymentType.CreditCard => "bg-purple-lt",
        PaymentType.NotPaid => "bg-red-lt",
        _ => "bg-secondary-lt"
    };

    #endregion

    #region Reservation Status Dot Mapping

    /// <summary>bg-red for a reserved slot, bg-green for a free slot.</summary>
    public static string ToStatusDotClass(this bool reserved) => reserved ? "bg-red" : "bg-green";

    public static string ToStatusDotLabel(this bool reserved) => reserved ? "Reserved" : "Free";

    #endregion

    #region Enum Display Helpers

    /// <summary>Reads the [Display(Name = "...")] attribute of an enum value, falling back to its name.</summary>
    public static string GetDisplayName(this Enum value)
    {
        var field = value.GetType().GetField(value.ToString());
        var attribute = field?.GetCustomAttribute<DisplayAttribute>();
        return attribute?.Name ?? value.ToString();
    }

    #endregion

    #region HTML5 Date Formatting

    /// <summary>Formats a DateTime for an HTML5 input[type=datetime-local] value (yyyy-MM-ddTHH:mm).</summary>
    public static string ToHtmlDateTimeLocal(this DateTime value) => value.ToString("yyyy-MM-ddTHH:mm");

    public static string ToHtmlDateTimeLocal(this DateTime? value) => value?.ToString("yyyy-MM-ddTHH:mm") ?? string.Empty;

    /// <summary>Formats a DateTime for an HTML5 input[type=date] value (yyyy-MM-dd).</summary>
    public static string ToHtmlDate(this DateTime value) => value.ToString("yyyy-MM-dd");

    public static string ToHtmlDate(this DateTime? value) => value?.ToString("yyyy-MM-dd") ?? string.Empty;

    #endregion
}
