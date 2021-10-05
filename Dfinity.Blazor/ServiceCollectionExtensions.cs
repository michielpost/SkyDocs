using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dfinity.Blazor
{
    public static class ServiceCollectionExtensions
    {
        public static void AddDfinityBlazor(this IServiceCollection services)
        {
            services.AddScoped<DfinityService>();
        }
    }
}
