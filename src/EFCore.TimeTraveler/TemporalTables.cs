using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace EFCore.TimeTraveler
{
    // TODO: Build this after the model is built based on the .EnableTemporalQuery() configuration
    public class TemporalTables
    {
        private readonly List<string> _tables = new List<string>();

        public void Add<T>(DbSet<T> table) where T : class
        {
            throw new NotImplementedException();
        }

        public void Add(string tableName)
        {
            _tables.Add($"[{tableName}]");
        }

        /// <summary>
        /// Table names in square brackets
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> Tables()
        {
            return _tables;
        }
    }
}
