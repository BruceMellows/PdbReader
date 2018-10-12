// NO LICENSE
// ==========
// There is no copyright, you can use and abuse this source without limit.
// There is no warranty, you are responsible for the consequences of your use of this source.
// There is no burden, you do not need to acknowledge this source in your use of this source.

namespace PDB
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using PDB.Internal;

    public class Superblock
    {
        private readonly Reader msf;
        private readonly uint[] streamSizes;
        private readonly uint[][] streamBlocks;

        public Superblock(Reader msf, Stream stream)
        {
            this.msf = msf;
            FreeBlockMapBlock = stream.ReadUnsignedIntel32();
            NumBlocks = stream.ReadUnsignedIntel32();
            NumDirectoryBytes = stream.ReadUnsignedIntel32();
            var unknown = stream.ReadUnsignedIntel32();
            BlockMapAddr = stream.ReadUnsignedIntel32();

            if (NumDirectoryBlocks > msf.BlockSize / sizeof(uint))
            {
                throw new IndexOutOfRangeException("Need to handle larger directory size - update the code");
            }

            if (NumBlocks * msf.BlockSize != stream.Length)
            {
                throw new InvalidDataException("Calculated file size is not equal to actual file size");
            }

            using (var directoryBlockStream = msf.OpenBlock(BlockMapAddr))
            {
                var directoryBlocks = new uint[NumDirectoryBlocks];
                for (var i = 0; i != NumDirectoryBlocks; ++i)
                {
                    directoryBlocks[i] = directoryBlockStream.ReadUnsignedIntel32();
                }
                DirectoryBlocks = directoryBlocks;
            }
            using (var directoryStream = new MultiBlockStream(msf, DirectoryBlocks))
            {
                NumStreams = directoryStream.ReadUnsignedIntel32();
                streamSizes = new uint[NumStreams];
                for (var streamIndex = 0; streamIndex != NumStreams; ++streamIndex)
                {
                    streamSizes[streamIndex] = directoryStream.ReadUnsignedIntel32();
                }
                streamBlocks = new uint[NumStreams][];
                for (var streamIndex = 0; streamIndex != NumStreams; ++streamIndex)
                {
                    var streamSize = streamSizes[streamIndex];
                    var blockCount = (streamSize + msf.BlockSize - 1) / msf.BlockSize;
                    var blocks = new uint[blockCount];
                    for (var blockIndex = 0; blockIndex != blockCount; ++blockIndex)
                    {
                        blocks[blockIndex] = directoryStream.ReadUnsignedIntel32();
                    }
                    streamBlocks[streamIndex] = blocks;
                }
            }
        }

        public uint FreeBlockMapBlock { get; private set; }

        public uint NumBlocks { get; private set; }

        public uint NumDirectoryBytes { get; private set; }

        public uint NumDirectoryBlocks { get { return (NumDirectoryBytes + msf.BlockSize - 1) / msf.BlockSize; } }

        public uint BlockMapAddr { get; private set; }

        public IEnumerable<uint> DirectoryBlocks { get; private set; }

        public uint NumStreams { get; private set; }

        public Stream GetStream(uint streamIndex)
        {
            if (streamIndex >= NumStreams)
            {
                throw new ArgumentOutOfRangeException(nameof(streamIndex));
            }

            return new MultiBlockStream(msf, streamBlocks[streamIndex], streamSizes[streamIndex]);
        }
    }
}
