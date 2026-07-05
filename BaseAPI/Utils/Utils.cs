using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SMNETCORE.BaseAPI.Utils
{
    public static class Utils
    {

        public static IServiceCollection AddAllImplementations<TInterface>(
            this IServiceCollection services,
            ServiceLifetime lifetime = ServiceLifetime.Scoped)
        {
            var interfaceType = typeof(TInterface);

            var implementations = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t =>
                    interfaceType.IsAssignableFrom(t) &&
                    t.IsClass &&
                    !t.IsAbstract);

            foreach (var implementation in implementations)
            {
                var descriptor = new ServiceDescriptor(
                    interfaceType,
                    implementation,
                    lifetime);

                services.Add(descriptor);
            }

            return services;
        }

        public static IServiceCollection AddDALImplementations(
            this IServiceCollection services,
            ServiceLifetime lifetime = ServiceLifetime.Scoped)
        {
            var allInterfaces =  AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => t.IsInterface && t.Namespace.StartsWith("SMNETCORE.DAL")  && t.Namespace.EndsWith("Interface"));

            foreach (var itype in allInterfaces)
            {
                var implementations = AppDomain.CurrentDomain
                    .GetAssemblies().SelectMany(a => a.GetTypes())
                    .Where(d => d.FullName.StartsWith("SMNETCORE.DAL") && !d.IsInterface && !d.FullName.EndsWith("Interface") &&
                        itype.IsAssignableFrom(d) &&
                        d.IsClass && !d.IsAbstract);
                foreach (var implementation in implementations)
                {
                    var descriptor = new ServiceDescriptor(
                        itype,
                        implementation,
                        lifetime);

                    services.Add(descriptor);
                }
            }

            

            return services;
        }
        public static IServiceCollection AddServicesImplementations(
            this IServiceCollection services,
            ServiceLifetime lifetime = ServiceLifetime.Scoped)
        {
            var allInterfaces = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => t.IsInterface && t.Namespace.StartsWith("SMNETCORE.Services") && t.Namespace.EndsWith("Interface"));

            foreach (var itype in allInterfaces)
            {
                var implementations = AppDomain.CurrentDomain
                    .GetAssemblies().SelectMany(a => a.GetTypes())
                    .Where(d => d.FullName.StartsWith("SMNETCORE.Services") && !d.IsInterface && !d.FullName.EndsWith("Interface") &&
                        itype.IsAssignableFrom(d) &&
                        d.IsClass && !d.IsAbstract);
                foreach (var implementation in implementations)
                {
                    var descriptor = new ServiceDescriptor(
                        itype,
                        implementation,
                        lifetime);

                    services.Add(descriptor);
                }
            }



            return services;
        }

        public static void LoadAllSettings(WebApplicationBuilder builder)
        {
            builder.Logging.ClearProviders();
            builder.Logging.AddLog4Net("log4net.config");

            var files = Directory.GetFiles(
            AppContext.BaseDirectory,
            "*settings.json",
            SearchOption.AllDirectories);
            foreach(var config in files)
            {
                builder.Configuration.AddJsonFile(config, optional: false, reloadOnChange: true);
            }

        }
    }
}
