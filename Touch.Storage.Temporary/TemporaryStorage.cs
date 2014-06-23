using System.IO;

namespace Touch.Storage
{
    /// <summary>
    /// ITemporaryStorage implementation based on local file system and system temp directory.
    /// </summary>
    public class TemporaryStorage : ITemporaryStorage
    {

        #region ITemporaryStorage implementation
        public ITemporaryFile CreateFile()
        {
            return new TemporaryFile(GetTempFileName());
        }

        public ITemporaryDirectory CreateDirectory()
        {
            return new TemporaryDirectory(GetTempDirectoryName());
        } 
        #endregion

        #region Helper methods
        /// <summary>
        /// Create new empty file and return it's path.
        /// </summary>
        /// <returns>Temporary file path.</returns>
        private static string GetTempFileName()
        {
            return Path.GetTempFileName();
        }

        /// <summary>
        /// Create new empty directory and return it's path.
        /// </summary>
        /// <returns>Temporary directory path.</returns>
        private static string GetTempDirectoryName()
        {
            var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(path);
            return path;
        } 
        #endregion
    }
}
