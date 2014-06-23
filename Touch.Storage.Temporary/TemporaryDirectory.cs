using System;
using System.IO;

namespace Touch.Storage
{
    sealed internal class TemporaryDirectory : ITemporaryDirectory
    {
        #region Data
        private readonly string _path;
        private readonly bool _deleteOnClose;
        #endregion;

        public TemporaryDirectory(string path)
            : this(path, false)
        { }

        public TemporaryDirectory(string path, bool deleteOnClose)
        {
            if (string.IsNullOrEmpty(path)) throw new ArgumentException("Invalid directory path.", "path");
            _path = path;

            _deleteOnClose = deleteOnClose;

            if (!Directory.Exists(path)) throw new ArgumentException("Directory does not exists.", "path");
        }

        public string Path { get { return _path; } }

        public void Dispose()
        {
            if (_deleteOnClose) Directory.Delete(_path);
        }
    }
}
