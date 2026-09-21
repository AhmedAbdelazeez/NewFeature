using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace NewFeature.Services
{
    // Centralizes "did the uploader actually mean this date" parsing for every Excel import path
    // across every department. Templates are filled in by hand, and some departments write dates
    // in the Hijri (Umm al-Qura) calendar rather than Gregorian - e.g. "15/03/1447" or
    // "1447-03-15" - so this accepts either calendar, plus the usual free-text/Arabic-digit/Excel
    // day-serial variations, and always returns a Gregorian DateTime, since that's what every
    // model field stores.
    public static class FlexibleDateParser
    {
        private static readonly string[] NumericFormats =
        {
            "yyyy-MM-dd", "yyyy/MM/dd",
            "dd/MM/yyyy", "d/M/yyyy", "dd-MM-yyyy", "d-M-yyyy",
            "dd.MM.yyyy", "d.M.yyyy",
        };

        private static readonly Regex FourDigitToken = new(@"\b\d{4}\b", RegexOptions.Compiled);

        private static CultureInfo HijriCulture { get; } = BuildHijriCulture();

        private static CultureInfo BuildHijriCulture()
        {
            // The invariant culture only accepts Gregorian-family calendars, so base this on
            // ar-SA (whose optional calendars include UmAlQuraCalendar) instead.
            var culture = (CultureInfo)CultureInfo.GetCultureInfo("ar-SA").Clone();
            culture.DateTimeFormat.Calendar = new UmAlQuraCalendar();
            return culture;
        }

        // Arabic-Indic digits are what a keyboard set to Arabic produces, and they never parse
        // with the invariant culture - fold them to ASCII before anything else looks at the text.
        public static string NormalizeDigits(string? text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;

            var builder = new System.Text.StringBuilder(text.Length);
            foreach (var ch in text.Trim())
            {
                if (ch >= '٠' && ch <= '٩') builder.Append((char)('0' + (ch - '٠')));       // Arabic-Indic
                else if (ch >= '۰' && ch <= '۹') builder.Append((char)('0' + (ch - '۰'))); // Extended Arabic-Indic
                else if (ch == ',' || ch == '٬') continue;                                           // thousands separators
                else builder.Append(ch);
            }
            return builder.ToString().Trim();
        }

        public static DateTime? Parse(string? text)
        {
            var raw = NormalizeDigits(text);
            if (string.IsNullOrEmpty(raw)) return null;

            // Strip common Hijri-calendar markers so the numeric parse below still works:
            // "15/03/1447 هـ", "1447-03-15H".
            var explicitlyHijri = false;
            var cleaned = raw;
            if (cleaned.Contains("هـ") || cleaned.Contains('ه'))
            {
                cleaned = cleaned.Replace("هـ", string.Empty).Replace("ه", string.Empty).Trim();
                explicitlyHijri = true;
            }
            if (cleaned.EndsWith("H", StringComparison.OrdinalIgnoreCase) && cleaned.Length > 1 && !char.IsLetter(cleaned[^2]))
            {
                cleaned = cleaned[..^1].Trim();
                explicitlyHijri = true;
            }

            // Hijri years currently run ~1440-1460; Gregorian business dates are ~1900-2100. That
            // gap means the 4-digit year in the text almost always tells us which calendar was
            // meant, so we try that calendar first and only fall back to the other on failure.
            var preferHijri = explicitlyHijri || LooksLikeHijriYear(cleaned);

            if (preferHijri)
            {
                var hijri = TryParseHijri(cleaned);
                if (hijri != null) return hijri;
            }

            var gregorian = TryParseGregorian(cleaned);
            if (gregorian != null) return gregorian;

            if (!preferHijri)
            {
                var hijri = TryParseHijri(cleaned);
                if (hijri != null) return hijri;
            }

            // Excel stores dates as day serial numbers; a cell formatted as General shows the raw
            // serial instead of a date string.
            if (double.TryParse(cleaned, NumberStyles.Any, CultureInfo.InvariantCulture, out var serial)
                && serial > 0 && serial < 2958466)
            {
                try { return DateTime.FromOADate(serial); } catch (ArgumentException) { return null; }
            }

            return null;
        }

        private static bool LooksLikeHijriYear(string text)
        {
            var match = FourDigitToken.Match(text);
            if (!match.Success) return false;
            var year = int.Parse(match.Value, CultureInfo.InvariantCulture);
            return year is >= 1300 and <= 1500;
        }

        private static DateTime? TryParseGregorian(string text)
        {
            // Explicit day/month/year formats go first: this ERP's templates are all
            // Middle-East-convention (day before month), and an ambiguous "5/3/2026" must resolve
            // to 5 March, not the US-style free-parse reading of May 3rd.
            if (DateTime.TryParseExact(text, NumericFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
                return parsed;

            if (DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsed))
                return parsed;
            if (DateTime.TryParse(text, out parsed))
                return parsed;

            return null;
        }

        private static DateTime? TryParseHijri(string text)
        {
            // .NET resolves the calendar-relative year/month/day against HijriCulture.Calendar and
            // hands back a normal (Gregorian-ticks) DateTime, so no manual calendar conversion is
            // needed once the parse succeeds.
            if (DateTime.TryParseExact(text, NumericFormats, HijriCulture, DateTimeStyles.None, out var parsed))
                return parsed;

            return null;
        }
    }
}
