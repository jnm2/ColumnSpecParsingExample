using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace ColumnSpecParsingExample
{
    [DebuggerDisplay("{ToString(),nq}")]
    public readonly struct Result<T> : IEquatable<Result<T>>
    {
        [AllowNull, MaybeNull]
        private readonly T value;
        private readonly string? errorMessage;

        private Result([AllowNull] T value, string? errorMessage)
        {
            this.value = value;
            this.errorMessage = errorMessage;
        }

        public static Result<T> Success(T value)
        {
            return new Result<T>(value, errorMessage: null);
        }

        public static Result<T> Error(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("Error message must be specified.", nameof(message));

            return new Result<T>(value: default, message);
        }

        public bool IsSuccess(out T value)
        {
            value = this.value;
            return errorMessage is null;
        }

        public bool IsError([NotNullWhen(true)] out string? message)
        {
            message = errorMessage;
            return message != null;
        }

        public override bool Equals(object? obj)
        {
            return obj is Result<T> result && Equals(result);
        }

        public bool Equals(Result<T> other)
        {
            return EqualityComparer<T>.Default.Equals(value, other.value) &&
                   errorMessage == other.errorMessage;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(value, errorMessage);
        }

        public T Value
        {
            get
            {
                if (errorMessage != null) throw new InvalidOperationException("The result is not success.");
                return value;
            }
        }

        public static implicit operator Result<T>(Result.ErrorResult error) => Error(error.ErrorMessage);

        public override string ToString()
        {
            return errorMessage is null
                ? $"Success({value})"
                : $"Error({errorMessage})";
        }
    }
}
