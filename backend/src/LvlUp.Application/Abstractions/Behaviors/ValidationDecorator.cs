using FluentValidation;
using FluentValidation.Results;
using LvlUp.Application.Abstractions.Messaging;
using LvlUp.SharedKernel;

namespace LvlUp.Application.Abstractions.Behaviors;

internal static class ValidationDecorator
{
    internal sealed class CommandHandler<TCommand, TResponse>(
        ICommandHandler<TCommand, TResponse> innerHandler,
        IEnumerable<IValidator<TCommand>> validators)
        : ICommandHandler<TCommand, TResponse>
        where TCommand : ICommand<TResponse>
    {
        public async Task<Result<TResponse>> HandleAsync(TCommand command, CancellationToken cancellationToken)
        {
            ValidationFailure[] validationFailures = await ValidateAsync(command, validators, cancellationToken);

            if (validationFailures.Length == 0)
            {
                return await innerHandler.HandleAsync(command, cancellationToken);
            }

            return Result.Failure<TResponse>(CreateValidationError(validationFailures));
        }
    }

    internal sealed class CommandBaseHandler<TCommand>(
        ICommandHandler<TCommand> innerHandler,
        IEnumerable<IValidator<TCommand>> validators)
        : ICommandHandler<TCommand>
        where TCommand : ICommand
    {
        public async Task<Result> HandleAsync(TCommand command, CancellationToken cancellationToken)
        {
            ValidationFailure[] validationFailures = await ValidateAsync(command, validators, cancellationToken);

            if (validationFailures.Length == 0)
            {
                return await innerHandler.HandleAsync(command, cancellationToken);
            }

            return Result.Failure(CreateValidationError(validationFailures));
        }
    }

    private static async Task<ValidationFailure[]> ValidateAsync<TCommand>(
        TCommand command,
        IEnumerable<IValidator<TCommand>> validators,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
        {
            return [];
        }

        var context = new ValidationContext<TCommand>(command);

        ValidationResult[] validationResults = await Task.WhenAll(
            validators.Select(validator => validator.ValidateAsync(context, cancellationToken)));

        return [.. validationResults.Where(result => !result.IsValid).SelectMany(result => result.Errors)];
    }

    private static ValidationError CreateValidationError(IEnumerable<ValidationFailure> validationFailures) =>
        new([.. validationFailures.Select(failure => Error.Problem(failure.ErrorCode, failure.ErrorMessage))]);
}
