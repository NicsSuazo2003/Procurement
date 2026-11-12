using Drmc.Procurement.Application.Common.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace Drmc.Procurement.Infrastructure.Messaging;

/// <summary>Resolves each request's typed handler from the current DI scope.</summary>
public sealed class ServiceProviderDispatcher(IServiceProvider services) : IDispatcher
{
    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(request.GetType(), typeof(TResponse));
        var handler = services.GetRequiredService(handlerType);
        var handleMethod = handlerType.GetMethod(nameof(IRequestHandler<IRequest<TResponse>, TResponse>.Handle))!;

        return (Task<TResponse>)handleMethod.Invoke(handler, [request, cancellationToken])!;
    }
}
