using Conductor.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Conductor.Tests
{
    public class CronTest
    {
        [Fact]
        public void Match_EverySeconds()
        {
            var cron = new Cron();
            Assert.True(cron.Match(DateTimeOffset.Now));
        }
        [Fact]
        public void Match_EveryMinutes()
        {
            var cron = new Cron { Second = 0 };
            Assert.True(cron.Match(DateTimeOffset.Parse("2018-01-01T00:00:00")));
            Assert.True(cron.Match(DateTimeOffset.Parse("2018-01-01T00:01:00")));
            Assert.True(cron.Match(DateTimeOffset.Parse("2018-01-01T00:01:00")));
            Assert.True(cron.Match(DateTimeOffset.Parse("2018-01-01T00:03:00")));
            Assert.True(cron.Match(DateTimeOffset.Parse("2018-01-01T00:04:00")));

            Assert.False(cron.Match(DateTimeOffset.Parse("2018-01-01T00:00:01")));
        }
        [Fact]
        public void Match_EveryHours()
        {
            var cron = new Cron { Second = 0, Minute = 0 };
            Assert.True(cron.Match(DateTimeOffset.Parse("2018-01-01T00:00:00")));
            Assert.True(cron.Match(DateTimeOffset.Parse("2018-01-01T01:00:00")));
            Assert.True(cron.Match(DateTimeOffset.Parse("2018-01-01T02:00:00")));
            Assert.True(cron.Match(DateTimeOffset.Parse("2018-01-01T03:00:00")));
            Assert.True(cron.Match(DateTimeOffset.Parse("2018-01-01T04:00:00")));

            Assert.False(cron.Match(DateTimeOffset.Parse("2018-01-01T00:00:01")));
            Assert.False(cron.Match(DateTimeOffset.Parse("2018-01-01T00:01:00")));
        }
        [Fact]
        public void Match_EveryDays()
        {
            var cron = new Cron { Second = 0, Minute = 0, Hour = 0 };
            Assert.True(cron.Match(DateTimeOffset.Parse("2018-01-01T00:00:00")));
            Assert.True(cron.Match(DateTimeOffset.Parse("2018-01-02T00:00:00")));
            Assert.True(cron.Match(DateTimeOffset.Parse("2018-01-03T00:00:00")));
            Assert.True(cron.Match(DateTimeOffset.Parse("2018-01-04T00:00:00")));
            Assert.True(cron.Match(DateTimeOffset.Parse("2018-01-05T00:00:00")));

            Assert.False(cron.Match(DateTimeOffset.Parse("2018-01-01T00:00:01")));
            Assert.False(cron.Match(DateTimeOffset.Parse("2018-01-01T00:01:00")));
            Assert.False(cron.Match(DateTimeOffset.Parse("2018-01-01T01:00:00")));
        }
        [Fact]
        public void Match_EveryMonths()
        {
            var cron = new Cron { Second = 0, Minute = 0, Hour = 0, Day = 1 };
            Assert.True(cron.Match(DateTimeOffset.Parse("2018-01-01T00:00:00")));
            Assert.True(cron.Match(DateTimeOffset.Parse("2018-02-01T00:00:00")));
            Assert.True(cron.Match(DateTimeOffset.Parse("2018-03-01T00:00:00")));
            Assert.True(cron.Match(DateTimeOffset.Parse("2018-04-01T00:00:00")));
            Assert.True(cron.Match(DateTimeOffset.Parse("2018-05-01T00:00:00")));

            Assert.False(cron.Match(DateTimeOffset.Parse("2018-01-01T00:00:01")));
            Assert.False(cron.Match(DateTimeOffset.Parse("2018-01-01T00:01:00")));
            Assert.False(cron.Match(DateTimeOffset.Parse("2018-01-01T01:00:00")));
            Assert.False(cron.Match(DateTimeOffset.Parse("2018-01-02T00:00:00")));
        }

        [Fact]
        public void Match_Sunday_ap0()
        {
            var cron = new Cron { Second = 0, Minute = 0, Hour = 0, DayOfWeek = 0 };
            Assert.True(cron.Match(DateTimeOffset.Parse("2018-02-04T00:00:00")));
            Assert.True(cron.Match(DateTimeOffset.Parse("2018-02-11T00:00:00")));
            Assert.True(cron.Match(DateTimeOffset.Parse("2018-02-18T00:00:00")));
            Assert.True(cron.Match(DateTimeOffset.Parse("2018-02-25T00:00:00")));

            Assert.False(cron.Match(DateTimeOffset.Parse("2018-02-05T00:00:00")));
            Assert.False(cron.Match(DateTimeOffset.Parse("2018-02-12T00:00:00")));
            Assert.False(cron.Match(DateTimeOffset.Parse("2018-02-20T01:00:00")));
            Assert.False(cron.Match(DateTimeOffset.Parse("2018-02-28T00:00:00")));
        }

        [Fact]
        public void Parse_Column()
        {
            int?[] values;

            values = Cron._parse("*").ToArray();
            Assert.NotNull(values);
            Assert.Single(values);
            Assert.Null(values[0]);            

            values = Cron._parse("0").ToArray();
            Assert.NotNull(values);
            Assert.Single(values);
            Assert.Equal(0, values[0]);

            values = Cron._parse("0,1").ToArray();
            Assert.NotNull(values);
            Assert.Equal(2, values.Length);
            Assert.Equal(0, values[0]);
            Assert.Equal(1, values[1]);

            values = Cron._parse("0-59").ToArray();
            Assert.NotNull(values);
            Assert.Equal(60, values.Length);
            Assert.Equal(0, values[0]);
            Assert.Equal(59, values[59]);

            values = Cron._parse("*/30").ToArray();
            Assert.NotNull(values);
            Assert.Equal(2, values.Length);
            Assert.Equal(0, values[0]);
            Assert.Equal(30, values[1]);

            values = Cron._parse("*/3", 1, 12).ToArray();
            Assert.NotNull(values);
            Assert.Equal(4, values.Length);
            Assert.Equal(1, values[0]);
            Assert.Equal(4, values[1]);
            Assert.Equal(7, values[2]);
            Assert.Equal(10, values[3]);

            values = Cron._parse("10-16/2").ToArray();
            Assert.NotNull(values);
            Assert.Equal(4, values.Length);
            Assert.Equal(10, values[0]);
            Assert.Equal(12, values[1]);
            Assert.Equal(14, values[2]);
            Assert.Equal(16, values[3]);
        }

        [Fact]
        public void Parse_EverySeconds() 
        {
            var crons = Cron.Parse("* * * * * *");

            Assert.NotNull(crons);
            Assert.Single(crons);

            var cron = crons.First();
            
            Assert.Null(cron.Second);
            Assert.Null(cron.Minute);
            Assert.Null(cron.Hour);
            Assert.Null(cron.Day);
            Assert.Null(cron.Month);
            Assert.Null(cron.DayOfWeek);
        }

        [Fact]
        public void Parse_EveryMinutes() 
        {
            var crons = Cron.Parse("0 * * * * *");

            Assert.NotNull(crons);
            Assert.Single(crons);

            var cron = crons.First();
            
            Assert.Equal(0, cron.Second);
            Assert.Null(cron.Minute);
            Assert.Null(cron.Hour);
            Assert.Null(cron.Day);
            Assert.Null(cron.Month);
            Assert.Null(cron.DayOfWeek);
        }

        [Fact]
        public void Parse_MinutesPer20() 
        {
            var crons = Cron.Parse("0 */20 * * * *").ToArray();

            Assert.NotNull(crons);
            Assert.Equal(3, crons.Count());
            
            Assert.Equal(0, crons[0].Second);
            Assert.Equal(0, crons[0].Minute);
            Assert.Null(crons[0].Hour);
            Assert.Null(crons[0].Day);
            Assert.Null(crons[0].Month);
            Assert.Null(crons[0].DayOfWeek);

            Assert.Equal(0, crons[1].Second);
            Assert.Equal(20, crons[1].Minute);
            Assert.Null(crons[1].Hour);
            Assert.Null(crons[1].Day);
            Assert.Null(crons[1].Month);
            Assert.Null(crons[1].DayOfWeek);

            Assert.Equal(0, crons[2].Second);
            Assert.Equal(40, crons[2].Minute);
            Assert.Null(crons[2].Hour);
            Assert.Null(crons[2].Day);
            Assert.Null(crons[2].Month);
            Assert.Null(crons[2].DayOfWeek);
        }

        [Fact]
        public void Parse_MinutesPer5() 
        {
            var crons = Cron.Parse("0 */5 * * * *").ToArray();

            Assert.NotNull(crons);
            Assert.Equal(12, crons.Count());
            
            var waitTime = crons.CalcNext(DateTimeOffset.Parse("2018-01-01T00:56:30.300000+09:00"));
            Assert.Equal(TimeSpan.FromMinutes(5) - TimeSpan.FromMinutes(1) - TimeSpan.FromSeconds(30) -  TimeSpan.FromMilliseconds(300), waitTime);
            Assert.False(crons.Match(DateTimeOffset.Parse("2018-01-01T00:01:30.300000+09:00")));
            Assert.True(crons.Match(DateTimeOffset.Parse("2018-01-01T00:05:00.300000+09:00")));        
        }
    }
}
