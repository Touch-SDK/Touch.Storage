using System;
using System.IO;

namespace Touch.Storage
{
    public class TemporaryFile : ITemporaryFile
    {
        #region Data
        private readonly string _filePath;
        private readonly bool _deleteOnClose;
        private readonly FileStream _stream;
        #endregion;

        public TemporaryFile(string filePath)
            : this(filePath, false)
        { }

        public TemporaryFile(string filePath, bool deleteOnClose)
        {
            if (string.IsNullOrEmpty(filePath)) throw new ArgumentException("Invalid file path.", "filePath");
            _filePath = filePath;

            _deleteOnClose = deleteOnClose;

            if (!File.Exists(filePath)) throw new ArgumentException("File does not exists.", "filePath");

            _stream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
        }

        public void Dispose()
        {
            if (_stream != null) _stream.Dispose();
            if (_deleteOnClose) File.Delete(_filePath);
        }

        public Stream Stream { get { return _stream; } }
    }
}
