// NO LICENSE
// ==========
// There is no copyright, you can use and abuse this source without limit.
// There is no warranty, you are responsible for the consequences of your use of this source.
// There is no burden, you do not need to acknowledge this source in your use of this source.

namespace PDB.Internal
{
    using System;

    internal class OnEnterExit
    {
        public static IDisposable Create(Action onEnter, Action onExit)
        {
            return new VoidAction(onEnter, onExit);
        }

        public static IDisposable Create<T>(Func<T> onEnter, Action<T> onExit)
        {
            return new ValueAction<T>(onEnter, onExit);
        }

        private class VoidAction : IDisposable
        {
            Action onExit;

            public VoidAction(Action onEnter, Action onExit)
            {
                onEnter();
                this.onExit = onExit;
            }

            public void Dispose()
            {
                Extensions.FullDispose(ref onExit, x => x());
            }
        }

        private class ValueAction<T> : IDisposable
        {
            T value;
            Action<T> onExit;

            public ValueAction(Func<T> onEnter, Action<T> onExit)
            {
                value = onEnter();
                this.onExit = onExit;
            }
            public void Dispose()
            {
                Extensions.FullDispose(ref onExit, x => x(value));
            }
        }
    }
}
