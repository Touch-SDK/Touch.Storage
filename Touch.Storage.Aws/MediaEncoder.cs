using System;
using Amazon;
using Amazon.ElasticTranscoder;
using Amazon.ElasticTranscoder.Model;
using Amazon.Runtime;

namespace Touch.Storage
{
    public sealed class MediaEncoder : IMediaEncoder
    {
        public MediaEncoder(AWSCredentials credentials, string connectionString)
        {
            _credentials = credentials;
            _connectionString = new AwsEncoderConnectionStringBuilder{ ConnectionString = connectionString };
            _config = new AmazonElasticTranscoderConfig
            {
                RegionEndpoint = RegionEndpoint.GetBySystemName(_connectionString.Region)
            };
        }

        private readonly AWSCredentials _credentials;
        private readonly AwsEncoderConnectionStringBuilder _connectionString;
        private readonly AmazonElasticTranscoderConfig _config;

        public MediaEncoderJob Encode(string source, string output)
        {
            return Encode(source, output, null);
        }

        public MediaEncoderJob Encode(string source, string output, string thumbnailsOutput)
        {
            if (string.IsNullOrWhiteSpace(source)) throw new ArgumentNullException("source");
            if (string.IsNullOrWhiteSpace(output)) throw new ArgumentNullException("output");

            source = GetPath(source);
            output = GetPath(output);

            if (!string.IsNullOrWhiteSpace(thumbnailsOutput))
                thumbnailsOutput = GetPath(thumbnailsOutput);

            using (var client = GetClient())
            {
                var inputJob = new JobInput
                {
                    Key = source
                };

                var outputJob = new CreateJobOutput
                {
                    PresetId = _connectionString.PresetId,
                    Key = output,
                    ThumbnailPattern = !string.IsNullOrWhiteSpace(thumbnailsOutput) ? thumbnailsOutput + "-{count}" : null
                };

                var response = client.CreateJob(new CreateJobRequest { Input = inputJob, Output = outputJob, PipelineId = _connectionString.PipelineId });

                return new AwsMediaEncoderJob(response.Job, GetClient);
            }
        }

        private IAmazonElasticTranscoder GetClient()
        {
            return AWSClientFactory.CreateAmazonElasticTranscoderClient(_credentials, _config);
        }

        private string GetPath(string path)
        {
            return _connectionString.Path + path;
        }
    }

    internal sealed class AwsMediaEncoderJob : MediaEncoderJob
    {
        public AwsMediaEncoderJob(Job job, Func<IAmazonElasticTranscoder> clientFactory)
        {
            _job = job;
            _clientFactory = clientFactory;

            Token = Guid.NewGuid();
            Started = DateTime.UtcNow;
            Source = job.Input.Key;
            Output = job.Output.Key;
        }

        private readonly Job _job;
        private readonly Func<IAmazonElasticTranscoder> _clientFactory;

        public override Guid Token { get; protected set; }

        public override string Source { get; protected set; }

        public override string Output { get; protected set; }

        public override DateTime Started { get; protected set; }

        public override MediaEncoderJobStatus Status
        {
            get
            {
                using (var client = _clientFactory())
                {
                    var response = client.ReadJob(new ReadJobRequest{ Id = _job.Id });

                    switch (response.Job.Status)
                    {
                        case "Progressing":
                            return MediaEncoderJobStatus.Started;

                        case "Complete":
                            return MediaEncoderJobStatus.Complete;

                        default:
                            return MediaEncoderJobStatus.Failed;
                    }
                }
            }
            protected set {}
        }
    }
}
