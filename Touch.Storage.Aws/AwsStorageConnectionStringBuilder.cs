using System;
using System.Data.Common;
using Amazon;

namespace Touch.Storage
{
    sealed public class AwsStorageConnectionStringBuilder : DbConnectionStringBuilder
    {
        #region Properties
        public string PublicUrl
        {
            get { return ContainsKey("PublicUrl") ? this["PublicUrl"] as string : null; }
            set { this["PublicUrl"] = value; }
        }

        public string Path
        {
            get { return ContainsKey("Path") ? (string)this["Path"] : string.Empty; }
            set { this["Path"] = value ?? string.Empty; }
        }

        public string Bucket
        {
            get { return ContainsKey("Bucket") ? this["Bucket"] as string : null; }
            set { this["Bucket"] = value; }
        }

        public string Region
        {
            get { return ContainsKey("Region") ? this["Region"] as string : null; }
            set { this["Region"] = value; }
        }

        public bool ReducedRedundancy
        {
            get { return ContainsKey("ReducedRedundancy") && Convert.ToBoolean(this["ReducedRedundancy"]); }
            set { this["ReducedRedundancy"] = value; }
        }
        #endregion
    }
}
