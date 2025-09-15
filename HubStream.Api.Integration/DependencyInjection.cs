using HubStream.Api.Integration.ApiClient;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HubStream.Api.Integration
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddHubStreamApiClient(this IServiceCollection services, Action<HttpClient> configureClient)
        {
            services.AddHttpClient<IHubStreamApiClient, HubStreamApiClient>(configureClient);
            return services;
        }
    }
}
