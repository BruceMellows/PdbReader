// NO LICENSE
// ==========
// There is no copyright, you can use and abuse this source without limit.
// There is no warranty, you are responsible for the consequences of your use of this source.
// There is no burden, you do not need to acknowledge this source in your use of this source.

namespace PDB
{
    using System;
    using System.IO;

    using PDB.Internal;

    public class StreamHeader
    {
        public enum PdbStreamVersion
        {
            VC2 = 19941610,
            VC4 = 19950623,
            VC41 = 19950814,
            VC50 = 19960307,
            VC98 = 19970604,
            VC70Dep = 19990604,
            VC70 = 20000404,
            VC80 = 20030901,
            VC110 = 20091201,
            VC140 = 20140508,
        }

        private StreamHeader(Stream stream)
        {
            Version = (PdbStreamVersion)stream.ReadUnsignedIntel32();
            Signature = stream.ReadUnsignedIntel32();
            Age = stream.ReadUnsignedIntel32();
            var guidBytes = new byte[128 / 8];
            if (guidBytes.Length == stream.Read(guidBytes, 0, guidBytes.Length) /*&& stream.Length == stream.Position*/)
            {
                UniqueId = new Guid(guidBytes);
            }
        }

        public PdbStreamVersion Version { get; private set; }

        public uint Signature { get; private set; }

        public uint Age { get; private set; }

        public Guid UniqueId { get; private set; }

        public static StreamHeader Create(Reader msf)
        {
            using (var stream = msf.Superblock.GetStream(1))
            {
                var result = new StreamHeader(stream);
                return result.UniqueId != null ? result : null;
            }
        }
    }
}
