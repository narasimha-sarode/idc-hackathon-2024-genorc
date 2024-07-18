using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenOrcAdvisor
{
    internal class GenOrcAdvisorBackgroundService : IHostedService
    {
        private readonly GPTWorker _worker;

        public GenOrcAdvisorBackgroundService(GPTWorker worker)
        {
            _worker = worker;
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _worker.DoWork(cancellationToken);

            await Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.FromCanceled(cancellationToken);
        }
    }
}
