using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;

namespace EFCore.TimeTraveler
{
    public class TimeTravelInterceptor : DbCommandInterceptor
    {
        public static DateTime? TimeTravelDate => TemporalQuery.TargetDateTime;

        internal const string TemporalTablesKey = "TemporalTables";

        public override Task<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<DbDataReader> result,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(ReaderExecuting(command, eventData, result));
        }

        public override InterceptionResult<DbDataReader> ReaderExecuting(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<DbDataReader> result)
        {
            if (IsEnabled())
            {
                var temporalTables = GetTemporalTables(eventData.Context.Model)
                    ?? throw new InvalidOperationException(
                        $"No temporal entities have been configured for {eventData.Context.GetType().Name}.  Please call `entityTypeBuilder.EnableTemporalQuery()` for each entity type that should participate in time travel.");

                UpdateQuery(command, temporalTables);
            }

            return result;
        }

        private bool IsEnabled()
        {
            return TimeTravelDate != null;
        }

        private TemporalTables GetTemporalTables(IModel contextModel)
        {
            return contextModel[TemporalTablesKey] as TemporalTables;
        }

        private void UpdateQuery(DbCommand command, TemporalTables temporalTables)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = "@TimeTravelDate";
            parameter.Value = TimeTravelDate;

            command.Parameters.Add(parameter);

            foreach (var table in temporalTables.Tables())
            {
                command.CommandText =
                    command.CommandText.Replace(table, $"{table} FOR SYSTEM_TIME AS OF @TimeTravelDate");
            }
        }
    }
}

