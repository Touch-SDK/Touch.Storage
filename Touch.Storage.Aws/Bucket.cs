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

        #region IStorage implementation
        public override bool HasFile(string token, out Metadata metadata)
        {
            if (string.IsNullOrEmpty(token)) throw new ArgumentException("token");
            token = GetBucketKey(token);

            using (var client = GetClient())
            {
                var request = new GetObjectMetadataRequest
                                  {
                                      BucketName = ConnectionString.Bucket,
                                      Key = token
                                  };

                GetObjectMetadataResponse response;

                try
                {
                    response = client.GetObjectMetadata(request);
                }
                catch
                {
                    metadata = null;
                    return false;
                }                

                metadata = new Metadata
                {
                    ContentType = response.Headers.ContentType,
                    LastModified = response.LastModified,
                    ETag = response.ETag
                };

                foreach (var key in response.Metadata.Keys)
                    metadata[key] = response.Metadata[key];

                return true;
            }
        }

        public override bool HasFile(string token)
        {
            if (string.IsNullOrEmpty(token)) throw new ArgumentException("token");

            Metadata metadata;
            return HasFile(token, out metadata);
        }

        public override void PutFile(Stream file, string token)
        {
            if (string.IsNullOrEmpty(token)) throw new ArgumentException("token");

            var metadata = new Metadata();
            PutFile(file, token, metadata);
        }

        public override void PutFile(Stream file, string token, Metadata metadata)
        {
            if (file == null) throw new ArgumentNullException("file");
            if (string.IsNullOrEmpty(token)) throw new ArgumentException("token");
            if (metadata == null) throw new ArgumentNullException("metadata");

            token = GetBucketKey(token);

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

        public override void PutMetadata(string token, Metadata metadata)
        {
            if (string.IsNullOrEmpty(token)) throw new ArgumentException("token");
            if (metadata == null) throw new ArgumentNullException("metadata");

            token = GetBucketKey(token);

            using (var client = GetClient())
            {
                var request = new CopyObjectRequest
                {
                    SourceBucket = ConnectionString.Bucket,
                    DestinationBucket = ConnectionString.Bucket,
                    SourceKey = token,
                    DestinationKey = token,
                    ContentType = metadata.ContentType, 
                    MetadataDirective = S3MetadataDirective.REPLACE
                };

                request.Headers.ContentType = metadata.ContentType;

                foreach (var pair in metadata)
                    request.Metadata.Add(pair.Key, pair.Value);

                client.CopyObject(request);
            }
        }

        public override Metadata GetMetadata(string token)
        {
            if (string.IsNullOrEmpty(token)) throw new ArgumentException("token");
            token = GetBucketKey(token);

            var metadata = new Metadata();

            using (var client = GetClient())
            {
                var request = new GetObjectMetadataRequest
                {
                    BucketName = ConnectionString.Bucket,
                    Key = token
                };

                var response = client.GetObjectMetadata(request);

                metadata.ContentType = response.Headers.ContentType;
                metadata.LastModified = response.LastModified;
                metadata.ETag = response.ETag;

                foreach (var key in response.Metadata.Keys)
                    metadata[key] = response.Metadata[key];
            }

            return metadata;
        }

        public override Stream GetFile(string token)
        {
            if (string.IsNullOrEmpty(token)) throw new ArgumentException("token");
            token = GetBucketKey(token);

            using (var client = GetClient())
            {
                var request = new GetObjectRequest
                {
                    BucketName = ConnectionString.Bucket,
                    Key = token
                };

                return client.GetObject(request).ResponseStream;
            }
        }

        public override Stream GetFile(string token, out Metadata metadata)
        {
            if (string.IsNullOrEmpty(token)) throw new ArgumentException("token");
            token = GetBucketKey(token);

            metadata = new Metadata();

            using (var client = GetClient())
            {
                var request = new GetObjectRequest
                {
                    BucketName = ConnectionString.Bucket,
                    Key = token
                };

                var response = client.GetObject(request);

                metadata.ContentType = response.Headers.ContentType;
                metadata.LastModified = response.LastModified;
                metadata.ETag = response.ETag;

                foreach (var key in response.Metadata.Keys)
                    metadata[key] = response.Metadata[key];

                return response.ResponseStream;
            }
        }

        public override void RemoveFile(string token)
        {
            if (string.IsNullOrEmpty(token)) throw new ArgumentException("token");
            token = GetBucketKey(token);

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
            if (!IsPublic) throw new InvalidOperationException("Storage is not public.");
            if (string.IsNullOrEmpty(token)) throw new ArgumentException("token");

            return ConnectionString.PublicUrl + token;
        }

        public override bool IsPublic { get { return !string.IsNullOrEmpty(ConnectionString.PublicUrl); } }
        #endregion
    }
}
