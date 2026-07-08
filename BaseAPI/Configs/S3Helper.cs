using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using SMNETCORE.DataType.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using ZstdSharp.Unsafe;

namespace SMNETCORE.BaseAPI.Configs
{
    public class S3ConfigModel  
    {
        public string AccessKey { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public string BucketName { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
    }

    public class S3Helper
    {
        private static Lazy<S3Helper> _instance;
        public static S3Helper Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Lazy<S3Helper>(() => new S3Helper());
                }
                return _instance.Value;
            }
        }
        
        private S3ConfigModel Config { get; set; } = null;
        public S3Helper() {
           
        }

        public void ConfigureS3(WebApplicationBuilder builder)
        {
            Config = new S3ConfigModel()
            {
                AccessKey = builder.Configuration.GetValue<string>("S3:AccessKey").NullableToString(),
                SecretKey = builder.Configuration.GetValue<string>("S3:SecretKey").NullableToString(),
                BucketName = builder.Configuration.GetValue<string>("S3:BucketName").NullableToString(),
                Region = builder.Configuration.GetValue<string>("S3:Region").NullableToString()
            };
        }
    }
}
