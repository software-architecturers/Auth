using System.Threading;
using System.Threading.Tasks;
using MediatR.Pipeline;
using Serilog;
using Serilog.Core;

namespace Auth.Application.Behaviors
{
    public class RequestLogger<TRequest> : IRequestPreProcessor<TRequest>
    {
        private static readonly ILogger Log = Serilog.Log.ForContext<TRequest>();

        public Task Process(TRequest request, CancellationToken cancellationToken)
        {
            Log.Information("MediatR request: {@Request}", request);
            return Task.CompletedTask;
        }
    }
}