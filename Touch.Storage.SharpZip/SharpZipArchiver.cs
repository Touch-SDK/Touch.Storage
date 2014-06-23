using System;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;

namespace Touch.Storage
{
    /// <summary>
    /// SharpZipLib archiver.
    /// </summary>
    sealed public class SharpZipArchiver : IArchiver
    {
        #region IArchiver implementation
        public void Extract(Stream archive, string outputPath)
        {
            if (archive == null) throw new ArgumentNullException("archive");
            if (!archive.CanRead) throw new ArgumentException("Cannot write read from a stream.", "archive");
            if (string.IsNullOrEmpty(outputPath)) throw new ArgumentNullException("outputPath");

            var archiver = new FastZip();

            archiver.ExtractZip(archive, outputPath, FastZip.Overwrite.Always, null, null, null, false, false);
        }

        public void Compress(string directoryPath, Stream output)
        {
            if (output == null) throw new ArgumentNullException("output");
            if (!output.CanWrite) throw new ArgumentException("Cannot write into a stream.", "output");
            if (string.IsNullOrEmpty(directoryPath)) throw new ArgumentNullException("directoryPath");

            var archiver = new FastZip();

            archiver.CreateZip(output, directoryPath, true, null, null);
        } 
        #endregion
    }
}
