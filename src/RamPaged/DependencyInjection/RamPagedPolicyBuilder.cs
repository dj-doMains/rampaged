using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public class RamPagedPolicyBuilder : IRamPagedPolicyBuilder
    {
        public IServiceCollection Services { get; }

        public RamPagedPolicyBuilder(IServiceCollection services)
        {
            Services = services ?? throw new ArgumentNullException(nameof(services));
        }
    }
}