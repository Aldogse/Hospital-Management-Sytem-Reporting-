
using Microsoft.EntityFrameworkCore;
using Report_and_Analytics_API.Data;

namespace Report_and_Analytics_API.Services
{
    public class MySqlSync : BackgroundService
    {
        private readonly ILogger<MySqlSync> _logger;
        private readonly IServiceProvider _serviceProvider;

        public MySqlSync(ILogger<MySqlSync>Logger,IServiceProvider serviceProvider)
        {
            _logger = Logger;
            _serviceProvider = serviceProvider;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            throw new NotImplementedException();
        }

        //Task for SQL syncing
        private async Task SqlSync<T>(DbSet<T> sourceDatabase,DbSet<T> targetDatabase, ReportDbContext reportDbContext,CancellationToken token)
            where T : class 
        {
            var sourceDataset = await sourceDatabase.AsNoTracking().ToListAsync();
        }
    }
}
