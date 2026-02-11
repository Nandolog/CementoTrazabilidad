namespace CementoTrazabilidad.API.Helpers
{
    public static class DateTimeHelper
    {
        // ✅ CAMBIAR: De Brasil a Paraguay
        private static readonly TimeZoneInfo ParaguayTimeZone = 
            TimeZoneInfo.FindSystemTimeZoneById("Paraguay Standard Time");

        /// <summary>
        /// Convierte UTC a hora de Paraguay
        /// </summary>
        public static DateTime ToLocalTime(DateTime utcDateTime)
        {
            if (utcDateTime.Kind == DateTimeKind.Unspecified)
                utcDateTime = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);

            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, ParaguayTimeZone);
        }

        /// <summary>
        /// Convierte hora de Paraguay a UTC
        /// </summary>
        public static DateTime ToUtcTime(DateTime localDateTime)
        {
            if (localDateTime.Kind == DateTimeKind.Unspecified)
                localDateTime = DateTime.SpecifyKind(localDateTime, DateTimeKind.Local);

            return TimeZoneInfo.ConvertTimeToUtc(localDateTime, ParaguayTimeZone);
        }

        /// <summary>
        /// Obtiene la hora actual de Paraguay
        /// </summary>
        public static DateTime NowParaguay()
        {
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, ParaguayTimeZone);
        }

        /// <summary>
        /// Formatea una fecha/hora para mostrar en Paraguay
        /// </summary>
        public static string FormatDateTime(DateTime dateTime)
        {
            var localTime = ToLocalTime(dateTime);
            return localTime.ToString("dd/MM/yyyy HH:mm");
        }
    }
}