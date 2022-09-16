using System.Threading;
using System.Threading.Tasks;

using BlogManager.EFCore;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BlogManager.WebApi.HostedServices
{
    public class MigratorService : IHostedService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public MigratorService(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceScopeFactory.CreateScope();

            var blogContext = scope.ServiceProvider.GetService<BlogDbContext>();

            await blogContext.Database.MigrateAsync();
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
