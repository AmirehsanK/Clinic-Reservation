using System.Globalization;

namespace Clinic.Application.Utilities
{
    public class DateConverter
    {
        public static DateTime ConvertPersianToGregorian(string persianDate)
        {
            var parts = persianDate.Split('/');
            if (parts.Length != 3)
                throw new FormatException("Invalid Persian date format.");

            var year = int.Parse(parts[0]);
            var month = int.Parse(parts[1]);
            var day = int.Parse(parts[2]);

            var pc = new PersianCalendar();
            return pc.ToDateTime(year, month, day, 0, 0, 0, 0);
        }
    }
}
