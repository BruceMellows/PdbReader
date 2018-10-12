// NO LICENSE
// ==========
// There is no copyright, you can use and abuse this source without limit.
// There is no warranty, you are responsible for the consequences of your use of this source.
// There is no burden, you do not need to acknowledge this source in your use of this source.

namespace PDB.Internal
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading;

    internal static partial class Extensions
    {
        public static void FullDispose<T>(ref T t) where T : class, IDisposable
        {
            FullDispose<T>(ref t, x => x.Dispose());
        }

        public static void FullDispose<T>(ref T t, Action<T> onDispose) where T : class
        {
            T local = t;
            if (local != null && Interlocked.CompareExchange<T>(ref t, null, local) == local)
            {
                onDispose(local);
            }
        }

        public static uint ReadUnsignedIntel32(this Stream stream)
        {
            var bytes = new byte[sizeof(UInt32)];
            stream.Read(bytes, 0, bytes.Length);
            return BitConverter.ToUInt32(bytes, 0);
        }

        public static uint ReadUnsignedNetwork32(this Stream stream)
        {
            var bytes = new byte[sizeof(UInt32)];
            stream.Read(bytes, 0, bytes.Length);
            return BitConverter.ToUInt32(bytes.Reverse().ToArray(), 0);
        }
    }
}
