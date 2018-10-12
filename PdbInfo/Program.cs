// NO LICENSE
// ==========
// There is no copyright, you can use and abuse this source without limit.
// There is no warranty, you are responsible for the consequences of your use of this source.
// There is no burden, you do not need to acknowledge this source in your use of this source.

namespace PdbInfo
{
    using System;
    using System.IO;

    class Program
    {
        static void Main(string[] args)
        {
            foreach (var arg in args)
            {
                var pdbReader = PDB.Reader.Create(arg);
                if (pdbReader != null)
                {
                    var pdbSuperblock = pdbReader.Superblock;
                    Console.WriteLine("Path                 {0}", pdbReader.Path);
                    Console.WriteLine("BlockSize            {0}", pdbReader.BlockSize);
                    Console.WriteLine("FreeBlockMapBlock    {0}", pdbSuperblock.FreeBlockMapBlock);
                    Console.WriteLine("NumBlocks            {0} (calculated file size {1}, actual file size {2})", pdbSuperblock.NumBlocks, pdbSuperblock.NumBlocks * pdbReader.BlockSize, new FileInfo(arg).Length);
                    Console.WriteLine("NumDirectoryBytes    {0}", pdbSuperblock.NumDirectoryBytes);
                    Console.WriteLine("NumDirectoryBlocks   {0}", pdbSuperblock.NumDirectoryBlocks);
                    Console.WriteLine("BlockMapAddr         {0} (at offset {1}/0x{1:X})", pdbSuperblock.BlockMapAddr, pdbSuperblock.BlockMapAddr * pdbReader.BlockSize);
                    //Console.WriteLine("DirectoryBlocks      [ {0} ]", string.Join(", ", pdbSuperblock.DirectoryBlocks.Select(x => x.ToString())));
                    Console.WriteLine("NumStreams           {0}", pdbSuperblock.NumStreams);
                    for (uint streamIndex = 0; streamIndex != pdbSuperblock.NumStreams; ++streamIndex)
                    {
                        using (var stream = pdbSuperblock.GetStream(streamIndex))
                        {
                            Console.WriteLine("Stream{0,-14} {1}", string.Format("[{0}].Length", streamIndex), stream.Length);
                        }
                    }
                    var pdbStreamHeader = PDB.StreamHeader.Create(pdbReader);
                    if (pdbStreamHeader != null)
                    {
                        Console.WriteLine("Version              {0}", pdbStreamHeader.Version);
                        Console.WriteLine("Signature            {0:X}", pdbStreamHeader.Signature);
                        Console.WriteLine("Age                  {0}", pdbStreamHeader.Age);
                        Console.WriteLine("UniqueId             {0}", pdbStreamHeader.UniqueId);
                        Console.WriteLine("SymbolPath           {0}{1:X}", pdbStreamHeader.UniqueId.ToString("N"), pdbStreamHeader.Age);
                    }
                }
            }
        }
    }
}
