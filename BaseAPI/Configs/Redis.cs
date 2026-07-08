using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SMNETCORE.BaseAPI.Configs
{
    public class BaseAPIRedis
    {
        public void ConfigureRedis(WebApplicationBuilder builder)
        {

            builder.Services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = builder.Configuration.GetValue<string>("Redis:MyRedisConStr");
                options.InstanceName = builder.Configuration.GetValue<string>("Redis:MyRedisInstanceStr");
            });
        }
    }
}
