namespace SMNETCORE.JWTAuthentication.Configs
{
    public class Redis
    {
        public void ConfigureRedis(WebApplicationBuilder builder)
        {

            builder.Services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = builder.Configuration.GetConnectionString("Redis:MyRedisConStr");
                options.InstanceName = builder.Configuration.GetConnectionString("Redis:MyRedisInstanceStr");
            });
        }
    }
}
