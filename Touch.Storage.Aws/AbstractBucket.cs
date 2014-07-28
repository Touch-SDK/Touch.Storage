using System;
using System.IO;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;

namespace Touch.Storage
{
    /// <summary>
    /// Abstract S3 bucket.
    /// </summary>
    abstract public class AbstractBucket : IStorage
    {
        #region .ctor
        protected AbstractBucket(AwsStorageConnectionStringBuilder connectionString, AWSCredentials credentials)
        {
            if (connectionString == null) throw new ArgumentNullException("connectionString");
            _connectionString = connectionString;

            if (credentials == null) throw new ArgumentNullException("credentials");
            _credentials = credentials;

            _config = new AmazonS3Config
            {
                RegionEndpoint = RegionEndpoint.GetBySystemName(_connectionString.Region)
            };
        } 
        #endregion

        #region Data
        private readonly AwsStorageConnectionStringBuilder _connectionString;
        private readonly AWSCredentials _credentials;
        private readonly AmazonS3Config _config;
        #endregion

        #region Properties
        protected AwsStorageConnectionStringBuilder ConnectionString { get { return _connectionString; } }
        #endregion

        #region Protected methods
        protected IAmazonS3 GetClient()
        {
            return AWSClientFactory.CreateAmazonS3Client(_credentials, _config);
        }

        protected string GetBucketKey(string token)
        {
            return _connectionString.Path + token;
        }
        #endregion

        #region IStorage methods
        public abstract bool HasFile(string token);

        public abstract bool HasFile(string token, out Metadata metadata);

        public abstract void PutFile(Stream file, string token);

        public abstract void PutFile(Stream file, string token, Metadata metadata);

        public abstract Stream GetFile(string token);

        public abstract Stream GetFile(string token, out Metadata metadata);

        public abstract void RemoveFile(string token);

        public abstract string GetPublicUrl(string token);

        public abstract bool IsPublic { get; }
        #endregion
    }
}
