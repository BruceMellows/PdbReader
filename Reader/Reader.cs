// NO LICENSE
// ==========
// There is no copyright, you can use and abuse this source without limit.
// There is no warranty, you are responsible for the consequences of your use of this source.
// There is no burden, you do not need to acknowledge this source in your use of this source.

namespace PDB
{
    using System;
    using System.IO;
    using System.Linq;

    using PDB.Internal;

    public class Reader : IDisposable
    {
        private static byte[] pdbMagicBytes = new byte[]
        {
            0x4d, 0x69, 0x63, 0x72, 0x6f, 0x73, 0x6f, 0x66,
            0x74, 0x20, 0x43, 0x2f, 0x43, 0x2b, 0x2b, 0x20,
            0x4d, 0x53, 0x46, 0x20, 0x37, 0x2e, 0x30, 0x30,
            0x0d, 0x0a, 0x1A, 0x44, 0x53, 0x00, 0x00, 0x00
        };

        private FileStream msfStream;

        private Reader(string path, byte[] magicBytes)
        {
            Path = path;

            msfStream = File.Open(Path, FileMode.Open, FileAccess.Read);
            if (msfStream != null)
            {
                var magicCheck = new byte[magicBytes.Length];
                msfStream.Read(magicCheck, 0, magicCheck.Length);
                if (magicBytes.SequenceEqual(magicCheck))
                {
                    BlockSize = msfStream.ReadUnsignedIntel32();
                    Superblock = new Superblock(this, msfStream);
                }
            }
        }

        public uint BlockSize { get; private set; }

        public Superblock Superblock { get; private set; }

        public string Path { get; private set; }

        public static Reader Create(string path)
        {
            if (!File.Exists(path))
            {
                return null;
            }

            var result = new Reader(path, pdbMagicBytes);

            return result.Superblock != null ? result : null;
        }

        public void Dispose()
        {
            Extensions.FullDispose(ref msfStream);
        }

        internal byte[] ReadBlock(uint block)
        {
            using (var s = OpenBlock(block))
            {
                var bytes = new byte[BlockSize];
                s.Read(bytes, 0, bytes.Length);
                return bytes;
            }
        }

        internal Stream OpenBlock(uint block)
        {
            using (OnEnterExit.Create(() => msfStream.Seek((long)block * BlockSize, SeekOrigin.Begin), x => msfStream.Seek(x, SeekOrigin.Begin)))
            {
                var bytes = new byte[BlockSize];
                msfStream.Read(bytes, 0, bytes.Length);
                return new MemoryStream(bytes, false);
            }
        }
    }
}
