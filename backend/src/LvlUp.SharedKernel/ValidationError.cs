namespace LvlUp.SharedKernel;

public sealed record ValidationError : Error
{
    public ValidationError(IReadOnlyCollection<Error> errors)
        : base("General.Validation", "One or more validation errors occurred.", ErrorType.Validation)
        => Errors = errors;

    public IReadOnlyCollection<Error> Errors { get; }

    public static ValidationError FromResults(IEnumerable<Result> results) =>
        new([.. results.Where(r => r.IsFailure).Select(r => r.Error)]);
}
