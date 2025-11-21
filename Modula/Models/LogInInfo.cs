namespace Modula.Models
{
    public class LogInInfo
    {
        public string access_token { get; set; }
        public string expires { get; set; }

        public DateTime? ExpiresDateTime
        {
            get
            {
                if (DateTime.TryParse(
                        expires,
                        null,
                        System.Globalization.DateTimeStyles.RoundtripKind,
                        out var dt))
                {
                    return dt;
                }
                return null;
            }
        }
    }
}