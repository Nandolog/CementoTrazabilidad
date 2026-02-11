namespace CementoTrazabilidad.Blazor.Services
{
    public interface IDateTimeService
    {
        DateTime ToLocalTime(DateTime utcDateTime);
        string FormatDateTime(DateTime dateTime);
        string FormatDate(DateOnly date);
    }

    public class DateTimeService : IDateTimeService
    {
        // ✅ CAMBIAR: Paraguay en lugar de Brasil
        private readonly TimeZoneInfo _paraguayTimeZone;

        public DateTimeService()
        {
            try
            {
                _paraguayTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Paraguay Standard Time");
            }
            catch
            {
                // Fallback: Paraguay suele estar en UTC-4 (horario estándar) o UTC-3 (horario de verano)
                _paraguayTimeZone = TimeZoneInfo.CreateCustomTimeZone(
                    "Paraguay",
                    new TimeSpan(-4, 0, 0), // UTC-4
                    "Paraguay Standard Time",
                    "Paraguay Standard Time"
                );
            }
        }

        public DateTime ToLocalTime(DateTime utcDateTime)
        {
            if (utcDateTime.Kind == DateTimeKind.Unspecified)
                utcDateTime = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);

            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, _paraguayTimeZone);
        }

        public string FormatDateTime(DateTime dateTime)
        {
            var localTime = ToLocalTime(dateTime);
            return localTime.ToString("dd/MM/yyyy HH:mm");
        }

        public string FormatDate(DateOnly date)
        {
            return date.ToString("dd/MM/yyyy");
        }
    }
}