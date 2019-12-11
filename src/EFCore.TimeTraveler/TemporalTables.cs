using System.Collections.Generic;
using System.Linq;

namespace EFCore.TimeTraveler
{
    public class TemporalTables
    {
        private readonly string[] _tables;

        public TemporalTables(IEnumerable<string> tables)
        {
            _tables = tables.ToArray();
        }

        /// <summary>
        ///     Table names in square brackets
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> Tables()
        {
            return _tables;
        }
    }
}
