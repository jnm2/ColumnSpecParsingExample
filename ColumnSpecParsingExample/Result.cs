using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace ColumnSpecParsingExample
{
    public readonly struct Result : IEquatable<Result>
    {
        private readonly string? errorMessage;

        private Result(string? errorMessage)
        {
            this.errorMessage = errorMessage;
        }

        public bool IsSuccess => errorMessage is null;

        public bool IsError([NotNullWhen(true)] out string? message)
        {
            message = errorMessage;
            return message != null;
        }

        public static Result Success() => default;

        public static ErrorResult Error(string message)
        {
            return new ErrorResult(message);
        }

        public static Result<T> Success<T>(T value) => Result<T>.Success(value);

        public override bool Equals(object? obj)
        {
            return obj is Result result && Equals(result);
        }

        public bool Equals(Result other)
        {
            return errorMessage == other.errorMessage;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(errorMessage);
        }

        public static bool operator ==(Result left, Result right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Result left, Result right)
        {
            return !(left == right);
        }

        public override string ToString()
        {
            return errorMessage is null
                ? "Success"
                : $"Error({errorMessage})";
        }

        public static implicit operator Result(ErrorResult r) => new Result(r.ErrorMessage);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public readonly struct ErrorResult
        {
            public string ErrorMessage { get; }

            public ErrorResult(string errorMessage)
            {
                if (string.IsNullOrEmpty(errorMessage))
                    throw new ArgumentException("An error message must be specified.", nameof(errorMessage));

                ErrorMessage = errorMessage;
            }
        }
    }
}
