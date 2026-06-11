using LvlUp.Application.Abstractions.Messaging;
using LvlUp.SharedKernel;
using Microsoft.Extensions.Logging;

namespace LvlUp.Application.Abstractions.Behaviors;

internal static class LoggingDecorator
{
    internal sealed class CommandHandler<TCommand, TResponse>(
        ICommandHandler<TCommand, TResponse> innerHandler,
        ILogger<CommandHandler<TCommand, TResponse>> logger)
        : ICommandHandler<TCommand, TResponse>
        where TCommand : ICommand<TResponse>
    {
        public async Task<Result<TResponse>> HandleAsync(TCommand command, CancellationToken cancellationToken)
        {
            string commandName = typeof(TCommand).Name;

            logger.LogInformation("Processing command {CommandName}", commandName);

            Result<TResponse> result = await innerHandler.HandleAsync(command, cancellationToken);

            LogResult(logger, result, commandName);

            return result;
        }
    }

    internal sealed class CommandBaseHandler<TCommand>(
        ICommandHandler<TCommand> innerHandler,
        ILogger<CommandBaseHandler<TCommand>> logger)
        : ICommandHandler<TCommand>
        where TCommand : ICommand
    {
        public async Task<Result> HandleAsync(TCommand command, CancellationToken cancellationToken)
        {
            string commandName = typeof(TCommand).Name;

            logger.LogInformation("Processing command {CommandName}", commandName);

            Result result = await innerHandler.HandleAsync(command, cancellationToken);

            LogResult(logger, result, commandName);

            return result;
        }
    }

    internal sealed class QueryHandler<TQuery, TResponse>(
        IQueryHandler<TQuery, TResponse> innerHandler,
        ILogger<QueryHandler<TQuery, TResponse>> logger)
        : IQueryHandler<TQuery, TResponse>
        where TQuery : IQuery<TResponse>
    {
        public async Task<Result<TResponse>> HandleAsync(TQuery query, CancellationToken cancellationToken)
        {
            string queryName = typeof(TQuery).Name;

            logger.LogInformation("Processing query {QueryName}", queryName);

            Result<TResponse> result = await innerHandler.HandleAsync(query, cancellationToken);

            LogResult(logger, result, queryName);

            return result;
        }
    }

    private static void LogResult(ILogger logger, Result result, string requestName)
    {
        if (result.IsSuccess)
        {
            logger.LogInformation("Completed {RequestName}", requestName);
        }
        else
        {
            logger.LogError("Completed {RequestName} with error {ErrorCode}: {ErrorDescription}",
                requestName, result.Error.Code, result.Error.Description);
        }
    }
}
