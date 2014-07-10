using System.Data.Common;

namespace Touch.Storage
{
    sealed public class AwsEncoderConnectionStringBuilder : DbConnectionStringBuilder
    {
        #region Properties
        public string Path
        {
            get { return ContainsKey("Path") ? (string)this["Path"] : string.Empty; }
            set { this["Path"] = value ?? string.Empty; }
        }

        public string Region
        {
            get { return ContainsKey("Region") ? this["Region"] as string : null; }
            set { this["Region"] = value; }
        }

        public string PipelineId
        {
            get { return ContainsKey("PipelineId") ? this["PipelineId"] as string : null; }
            set { this["PipelineId"] = value; }
        }

        public string PresetId
        {
            get { return ContainsKey("PresetId") ? this["PresetId"] as string : null; }
            set { this["PresetId"] = value; }
        }
        #endregion
    }
}
