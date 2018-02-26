using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Conductor.Core.Models
{
    public class Cron
    {
        public int? Second { get; set; }
        public int? Minute { get; set; }
        public int? Hour { get; set; }
        public int? Day { get; set; }
        public int? Month { get; set; }
        public int? DayOfWeek { get; set; }
        public bool Match(DateTimeOffset date)
        {
            var sec = Second.HasValue ? date.Second == Second.Value : true;
            var min = Minute.HasValue ? date.Minute == Minute.Value : true;
            var hour = Hour.HasValue ? date.Hour == Hour.Value : true;
            var day = Day.HasValue ? date.Day == Day.Value : true;
            var mon = Month.HasValue ? date.Month == Month.Value : true;
            var week = DayOfWeek.HasValue ? date.DayOfWeek == (DayOfWeek)DayOfWeek.Value : true;

            return sec && min && hour && day && mon && week;
        }
        public static IEnumerable<Cron> Parse(string format)
        {
            if (string.IsNullOrEmpty(format))
                throw new ArgumentNullException(format);

            var columns = format.Split(' ').Select(_ => _.Trim()).ToArray();
            if (columns.Length != 6)
                throw new ArgumentOutOfRangeException(nameof(format), "format should be `s m h d M w`");

            var sec = _parse(columns[0]);
            var min = _parse(columns[1]);
            var hour = _parse(columns[2]);
            var day = _parse(columns[3], 1, 31);
            var mon = _parse(columns[4], 1, 12);
            var week = _parse(columns[5], 0, 6);

            var crons = sec.Select(s => new Cron { Second = s });
            crons = min.SelectMany(m => crons.Select(c => new Cron { Second = c.Second, Minute = m })).ToArray();
            crons = hour.SelectMany(h => crons.Select(c => new Cron { Second = c.Second, Minute = c.Minute, Hour = h })).ToArray();
            crons = day.SelectMany(d => crons.Select(c => new Cron { Second = c.Second, Minute = c.Minute, Hour = c.Hour, Day = d })).ToArray();
            crons = mon.SelectMany(m => crons.Select(c => new Cron { Second = c.Second, Minute = c.Minute, Hour = c.Hour, Day = c.Day, Month = m })).ToArray();
            crons = week.SelectMany(w => crons.Select(c => new Cron { Second = c.Second, Minute = c.Minute, Hour = c.Hour, Day = c.Day, Month = c.Month, DayOfWeek = w })).ToArray();

            return crons.ToArray();
        }
        private static Regex _simple = new Regex(@"^(\d+)$");
        private static Regex _range = new Regex(@"^(\d+)-(\d+)$");
        private static Regex _selection = new Regex(@"^(\d+)(,\d+)*$");
        private static Regex _ratio = new Regex(@"^([\d\-\*]+)/(\d+)$");

        internal static int?[] _parse(string column, int min = 0, int max = 59)
        {
            if (column == "*")
                return new int?[]{null};

            var simple = _simple.Match(column);
            if (simple.Success)
            {
                return new int?[] { int.Parse(simple.Groups[1].Value) };
            }

            var range = _range.Match(column);
            if (range.Success)
            {
                var from = int.Parse(range.Groups[1].Captures[0].Value);
                var to = int.Parse(range.Groups[2].Captures[0].Value);
                return Enumerable.Range(from, to - from + 1).Select(_ => (int?)_).ToArray();
            }

            var selection = _selection.Match(column);
            if (selection.Success)
            {
                return column.Split(',').Select(c => (int?)int.Parse(c)).ToArray();
            }

            var ratio = _ratio.Match(column);
            if (ratio.Success)
            {
                var result = _parse(ratio.Groups[1].Captures[0].Value, min, max);
                if (result.First() == null)
                    result = Enumerable.Range(min, max - min + 1).Select(_ => (int?)_).ToArray();

                var den = int.Parse(ratio.Groups[2].Captures[0].Value);

                return result.Where((r, i) => i % den == 0).ToArray();
            }

            throw new ArgumentException(nameof(column), $"invalid format: {column}");
        }
    }

    public static class CronExtensions
    {
        public static bool Match(this IEnumerable<Cron> crons, DateTimeOffset date)
        {
            return crons.Any(cron => cron.Match(date));
        }

        public static TimeSpan CalcNext(this IEnumerable<Cron> crons, DateTimeOffset date)
        {
            var ms = TimeSpan.FromMilliseconds(date.Millisecond);
            
            var plans = crons.Select(cron => {
                // *
                if (!cron.Second.HasValue)
                    return TimeSpan.FromSeconds(1) - ms;
                // s *
                if (!cron.Minute.HasValue)
                    return TimeSpan.FromSeconds(cron.Second.Value) - TimeSpan.FromSeconds(date.Second) - ms;
                // s m *
                if (!cron.Hour.HasValue)
                    return TimeSpan.FromMinutes(cron.Minute.Value) - TimeSpan.FromMinutes(date.Minute) - TimeSpan.FromSeconds(date.Second) - ms;
                
                return TimeSpan.FromHours(cron.Hour.Value) - TimeSpan.FromHours(date.Hour) - TimeSpan.FromMinutes(date.Minute) - TimeSpan.FromSeconds(date.Second) - ms;
            });

            var filtered = plans.Where(t => t > TimeSpan.FromTicks(0)).ToArray();

            if (filtered.Length > 0)
            {
                return filtered.Min();
            }

            var c = crons.Last();
            
            if (!c.Second.HasValue)
                return TimeSpan.FromSeconds(1) - ms;
            // s *
            if (!c.Minute.HasValue)
                return TimeSpan.FromMinutes(1) - TimeSpan.FromSeconds(c.Second.Value) + plans.Last();
            // s m *
            if (!c.Hour.HasValue)
                return TimeSpan.FromHours(1) - TimeSpan.FromMinutes(c.Minute.Value) + plans.Last();
            // s m h *
            return TimeSpan.FromDays(1) - TimeSpan.FromHours(c.Hour.Value) + plans.Last();

        }
    }
}
