namespace JsonECore;

/// <summary>
/// Exception thrown during JSON-E template evaluation.
/// </summary>
public class JsonEException : Exception
{
    public string ErrorCode { get; }
    public object?[] Args { get; }

    public JsonEException(string errorCode, string message, params object?[] args)
        : base(message)
    {
        ErrorCode = errorCode;
        Args = args;
    }

    public JsonEException(string errorCode, string message, Exception innerException, params object?[] args)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
        Args = args;
    }
}

/// <summary>
/// Error codes for JSON-E exceptions.
/// </summary>
public static class JsonEErrorCodes
{
    public const string InvalidOperator = "JSONE001";
    public const string SyntaxError = "JSONE002";
    public const string UndefinedVariable = "JSONE003";
    public const string TypeMismatch = "JSONE004";
    public const string DivisionByZero = "JSONE005";
    public const string IndexOutOfBounds = "JSONE006";
    public const string InvalidFunctionCall = "JSONE007";
    public const string InvalidDateTime = "JSONE008";
    public const string InvalidTemplate = "JSONE009";
    public const string InvalidArgument = "JSONE010";
}
