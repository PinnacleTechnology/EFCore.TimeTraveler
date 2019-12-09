using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;

namespace EFCore.TimeTraveler
{
    public class TimeTravelInterceptor : DbCommandInterceptor
    {
        private readonly TemporalTables _temporalTables;

        //TODO: Replace this global static var
        public static DateTime? TimeTravelDate { get; set; } = null;

        public TimeTravelInterceptor()
        {
            _temporalTables = new TemporalTables();
            _temporalTables.Add("Apple");

        }
        public override Task<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<DbDataReader> result,
            CancellationToken cancellationToken = default)
        {
            if (IsEnabled())
            {
                UpdateQuery(command);
            }

            return Task.FromResult(result);
        }

        public override InterceptionResult<DbDataReader> ReaderExecuting(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<DbDataReader> result)
        {
            if (IsEnabled())
            {
                UpdateQuery(command);
            }

            return result;
        }

        private bool IsEnabled()
        {
            return TimeTravelDate != null && _temporalTables.Tables().Any();
        }

        private void UpdateQuery(DbCommand command)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = "@TimeTravelDate";
            parameter.Value = TimeTravelDate;

            command.Parameters.Add(parameter);

            foreach (var table in _temporalTables.Tables())
            {
                command.CommandText =
                    command.CommandText.Replace(table, $"{table} FOR SYSTEM_TIME AS OF @TimeTravelDate");
            }
        }
    }
}

