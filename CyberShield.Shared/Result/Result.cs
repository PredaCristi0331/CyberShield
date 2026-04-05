namespace CyberShield.Shared.Result;

public readonly record struct Result(bool IsSuccess, string Error)
{
    public static Result Ok() => new(true, "");
    public static Result Fail(string error) => new(false, error);
}