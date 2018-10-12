// NO LICENSE
// ==========
// There is no copyright, you can use and abuse this source without limit.
// There is no warranty, you are responsible for the consequences of your use of this source.
// There is no burden, you do not need to acknowledge this source in your use of this source.

namespace PDB.Internal
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    internal class MultiBlockStream : Stream
    {
        private readonly Reader msf;
        private readonly long length;

        private uint[] blockArray;

        private uint currentBlock;

        private byte[] currentBlockBytes;

        public override bool CanRead => true;

        public override bool CanSeek => true;

        public override bool CanWrite => false;

        public override long Length { get { return length; } }

        public override long Position { get; set; }

        public IEnumerable<uint> Blocks { get { return blockArray; } }

        public MultiBlockStream(Reader msf, IEnumerable<uint> blocks)
            : this(msf, blocks, blocks.Count() * msf.BlockSize)
        {
        }

        public MultiBlockStream(Reader msf, IEnumerable<uint> blocks, long length)
        {
            this.msf = msf;
            blockArray = blocks.ToArray();

            if ((length + msf.BlockSize - 1) / msf.BlockSize != blockArray.Length)
            {
                throw new ArgumentException(nameof(length));
            }

            if (blockArray.Length == 0)
            {
                currentBlockBytes = new byte[1];
            }

            this.length = length;
            UpdateCurrentBlock();
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var didCopy = 0;
            var blockSize = msf.BlockSize;
            while (count > 0)
            {
                var sourceIndex = Position % blockSize;
                var blockRemaining = Math.Min(Length - Position, blockSize - sourceIndex);
                var canCopy = (int)Math.Min(count, blockRemaining);
                if (canCopy == 0)
                {
                    break;
                }

                Array.Copy(currentBlockBytes, sourceIndex, buffer, offset, canCopy);

                count -= canCopy;
                offset += canCopy;
                Position += canCopy;
                didCopy += canCopy;
                UpdateCurrentBlock();
            }
            return didCopy;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            var result = Position;

            switch (origin)
            {
                case SeekOrigin.Begin:
                    Position = offset;
                    break;
                case SeekOrigin.Current:
                    Position = Position + offset;
                    break;
                case SeekOrigin.End:
                    Position = Length - offset;
                    break;
            }

            return result;
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        private void UpdateCurrentBlock()
        {
            var newCurrentBlock = (uint)(Position / msf.BlockSize);
            if (currentBlockBytes == null || currentBlock != newCurrentBlock)
            {
                currentBlockBytes = msf.ReadBlock(blockArray[currentBlock = newCurrentBlock]);
            }
        }
    }
}
