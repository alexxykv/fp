﻿using System;

namespace ResultOfTask
{
    public class None
    {
        private None()
        {
        }
    }
    public struct Result<T>
    {
        public Result(string error, T value = default(T))
        {
            Error = error;
            Value = value;
        }
        public string Error { get; }
        internal T Value { get; }
        public T GetValueOrThrow()
        {
            if (IsSuccess) return Value;
            throw new InvalidOperationException($"No value. Only Error {Error}");
        }
        public bool IsSuccess => Error == null;
    }

    public static class Result
    {
        public static Result<T> AsResult<T>(this T value)
        {
            return Ok(value);
        }

        public static Result<T> Ok<T>(T value)
        {
            return new Result<T>(null, value);
        }

        public static Result<T> Fail<T>(string e)
        {
            return new Result<T>(e);
        }

        public static Result<T> Of<T>(Func<T> f, string error = null)
        {
            try
            {
                return Ok(f());
            }
            catch (Exception e)
            {
                return Fail<T>(error ?? e.Message);
            }
        }

        public static Result<TOutput> Then<TInput, TOutput>(
            this Result<TInput> input,
            Func<TInput, TOutput> continuation)
        {
            return Then(input, value => Of(() => continuation(input.Value)));
        }

        public static Result<TOutput> Then<TInput, TOutput>(
            this Result<TInput> input,
            Func<TInput, Result<TOutput>> continuation)
        {
            if (input.IsSuccess)
                return continuation(input.Value);
            return Fail<TOutput>(input.Error);
        }

        public static Result<TInput> OnFail<TInput>(
            this Result<TInput> input,
            Action<string> handleError)
        {
            if (input.IsSuccess)
                return input;
            handleError(input.Error);
            return Fail<TInput>(input.Error);
        }

        public static Result<TInput> ReplaceError<TInput>(
            this Result<TInput> input, Func<string, string> repalced)
        {
            if (input.IsSuccess)
            {
                return input;
            }
            return new Result<TInput>(repalced(input.Error), input.Value);
        }

        public static Result<TInput> RefineError<TInput>(
            this Result<TInput> input, string refine)
        {
            return new Result<TInput>(
                string.Join(". ", new[] { refine, input.Error }),
                input.Value);
        }
    }
}