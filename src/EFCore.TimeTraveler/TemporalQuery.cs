using System;
using System.Collections.Immutable;
using System.Threading;

namespace EFCore.TimeTraveler
{
    public static class TemporalQuery 
    {
        private static readonly AsyncLocal<ImmutableStack<Context>> ContextStack = new AsyncLocal<ImmutableStack<Context>>();

        private static ImmutableStack<Context> CurrentContext => ContextStack.Value ?? ImmutableStack<Context>.Empty;


        public static IDisposable At(DateTime targetDateTime)
        {
            var stack = CurrentContext;
            var bookmark = new ContextStackBookmark(stack);

            ContextStack.Value = stack.Push(new Context(targetDateTime));

            return bookmark;
        }

        public static DateTime? TargetDateTime => CurrentContext.IsEmpty ? null : CurrentContext.Peek().ContextTargetDateTime;


        private class Context
        {
            public Context(DateTime? contextTargetDateTime)
            {
                ContextTargetDateTime = contextTargetDateTime;
            }

            public DateTime? ContextTargetDateTime { get; }
        }


        private class ContextStackBookmark : IDisposable
        {
            private readonly ImmutableStack<Context> _bookmark;

            public ContextStackBookmark(ImmutableStack<Context> bookmark)
            {
                _bookmark = bookmark;
            }

            public void Dispose()
            {
                ContextStack.Value = _bookmark;
            }
        }
    }
}
