using System;

namespace EFCore.TimeTraveler
{
    public class TemporalQuery : IDisposable
    {
        // TODO: Replace with AsyncLocal of ImmutableStack
        public TemporalQuery(in DateTime targetDateTime)
        {
            TimeTravelInterceptor.TimeTravelDate = targetDateTime;
        }

        public void Dispose()
        {
            TimeTravelInterceptor.TimeTravelDate = null;
        }
    }
}
