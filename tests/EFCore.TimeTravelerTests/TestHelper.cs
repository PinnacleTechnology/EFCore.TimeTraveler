using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac.Extensions.DependencyInjection;
using EFCore.TimeTravelerTests.DataAccess;
using Microsoft.Extensions.DependencyInjection;

namespace EFCore.TimeTravelerTests
{
    internal static class TestHelper
    {
        public static AutofacServiceProvider ServiceProvider => DatabaseSetupFixture.ServiceProvider;

        public static async Task ResetDb() => await DatabaseSetupFixture.ResetDb();

        public static ApplicationDbContext GetNewDbContext()
        {
            return ServiceProvider.GetService<ApplicationDbContext>();
        }

        public static DateTime UtcNow
        {
            get
            {
                var current = DateTime.UtcNow;

                // Account for accuracy discrepancies with DateTime.UtcNow
                // https://www.nimaara.com/high-resolution-clock-in-net/
                Thread.Sleep(CurrentMachineTimerAccuracy);

                return current;
            }
        }

        public static readonly TimeSpan CurrentMachineTimerAccuracy = GetCurrentMachineTimerAccuracy();

        private static TimeSpan GetCurrentMachineTimerAccuracy()
        {
            var duration = TimeSpan.FromMilliseconds(75);
            var distinctValues = new HashSet<DateTime>();
            var stopWatch = Stopwatch.StartNew();

            while (stopWatch.Elapsed < duration)
            {
                distinctValues.Add(DateTime.UtcNow);
            }

            return TimeSpan.FromMilliseconds((duration.TotalMilliseconds / distinctValues.Count) + 1);
        }
    }
}
