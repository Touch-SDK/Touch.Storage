using System;
using System.IO;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;

namespace Touch.Storage
{
    /// <summary>
    /// S3 bucket.
    /// </summary>
    public class Bucket : AbstractBucket
    {
        #region .ctor
        public Bucket(string connectionString, AWSCredentials credentials)
            : base(new AwsStorageConnectionStringBuilder { ConnectionString = connectionString }, credentials)
        {
        }
        #endregion

        #region Protected methods
        /// <summary>
        /// Get full blob name.
        /// </summary>
        /// <param name="folder">Folder name.</param>
        /// <param name="token">Blob token.</param>
        /// <returns>Full blob name.</returns>
        protected string GetBlobName(string folder, string token)
        {
            return folder + "/" + token;
        }
        #endregion

        #region IStorage implementation
        public override bool HasFile(string token, out Metadata metadata)
        {
            if (string.IsNullOrEmpty(token)) throw new ArgumentException("token");

            using (var client = GetClient())
            {
                var request = new GetObjectMetadataRequest
                                  {
                                      BucketName = ConnectionString.Bucket,
                                      Key = token
                                  };

                var response = client.GetObjectMetadata(request);

                metadata = new Metadata { ContentType = response.Metadata["Content-Type"] };

                foreach (var key in response.Metadata.Keys)
                    metadata[key] = response.Metadata[key];

                return true;
            }
        }

        public override bool HasFile(string token)
        {
            Metadata metadata;
            return HasFile(token, out metadata);
        }

        public override void PutFile(Stream file, string token)
        {
            var metadata = new Metadata();
            PutFile(file, token, metadata);
        }

        public override void PutFile(Stream file, string token, Metadata metadata)
        {
            if (file == null) throw new ArgumentNullException("file");
            if (string.IsNullOrEmpty(token)) throw new ArgumentException("token");
            if (metadata == null) throw new ArgumentNullException("metadata");

            using (var client = GetClient())
            {
                var request = new PutObjectRequest
                {
                    BucketName = ConnectionString.Bucket,
                    Key = token,
                    InputStream = file,
                    StorageClass = ConnectionString.ReducedRedundancy ? S3StorageClass.ReducedRedundancy : S3StorageClass.Standard,
                    ContentType = metadata.ContentType
                };

                foreach (var pair in metadata)
                    request.Metadata.Add(pair.Key, pair.Value);

                client.PutObject(request);
            }
        }

        public override Stream GetFile(string token)
        {
            Metadata metadata;
            return GetFile(token, out metadata);
        }

        public override Stream GetFile(string token, out Metadata metadata)
        {
            if (string.IsNullOrEmpty(token)) throw new ArgumentNullException("token");

            metadata = new Metadata();

            using (var client = GetClient())
            {
                var request = new GetObjectRequest
                {
                    BucketName = ConnectionString.Bucket,
                    Key = token
                };

                var response = client.GetObject(request);

                metadata.ContentType = response.Metadata["Content-Type"];

                foreach (var key in response.Metadata.Keys)
                    metadata[key] = response.Metadata[key];

                return response.ResponseStream;
            }
        }

        public override void RemoveFile(string token)
        {
            if (string.IsNullOrEmpty(token))
                throw new ArgumentException("token");

            using (var client = GetClient())
            {
                var request = new DeleteObjectRequest
                {
                    BucketName = ConnectionString.Bucket,
                    Key = token
                };

                client.DeleteObject(request);
            }
        }

        public override string GetPublicUrl(string token)
        {
            if (string.IsNullOrEmpty(token)) throw new ArgumentNullException("token");
            if (!IsPublic) throw new InvalidOperationException("Storage is not public.");

            return "http:" + ConnectionString.PublicEndpointUrl + token;
        }

        public override bool IsPublic { get { return !string.IsNullOrEmpty(ConnectionString.PublicEndpointUrl); } }
        #endregion
    }
}
