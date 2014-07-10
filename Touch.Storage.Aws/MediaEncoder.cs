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
            _config = new AwsEncoderConnectionStringBuilder{ ConnectionString = connectionString };
        }

        private readonly AWSCredentials _credentials;
        private readonly AwsEncoderConnectionStringBuilder _config;

        public MediaEncoderJob Encode(string source, string output)
        {
            using (var client = GetClient())
            {
                var inputJob = new JobInput
                {
                    Key = source
                };

                var outputJob = new CreateJobOutput
                {
                    PresetId = _config.PresetId,
                    Key = output
                };

                var response = client.CreateJob(new CreateJobRequest { Input = inputJob, Output = outputJob, PipelineId = _config.PipelineId });
                
                return new AwsMediaEncoderJob(response.Job);
            }
        }

        private IAmazonElasticTranscoder GetClient()
        {
            return AWSClientFactory.CreateAmazonElasticTranscoderClient(_credentials);
        }
    }

    internal sealed class AwsMediaEncoderJob : MediaEncoderJob
    {
        public AwsMediaEncoderJob(Job job)
        {
            _job = job;

            Token = Guid.NewGuid();
            Started = DateTime.UtcNow;
            Source = job.Input.Key;
            Output = job.Output.Key;
        }

        private readonly Job _job;

        public override Guid Token { get; protected set; }

        public override string Source { get; protected set; }

        public override string Output { get; protected set; }

        public override DateTime Started { get; protected set; }

        public override MediaEncoderJobStatus Status
        {
            get
            {
                switch (_job.Status)
                {
                    default:
                        return MediaEncoderJobStatus.Started;
                }
            }
            protected set {}
        }
    }
}
