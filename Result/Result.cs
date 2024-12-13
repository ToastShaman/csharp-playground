namespace Result;

public static class Result
{
    public static IResult<S, F> Success<S, F>(S value)
        where S : notnull
        where F : notnull => new Success<S, F>(value);

    public static IResult<S, F> Failure<S, F>(F error)
        where S : notnull
        where F : notnull => new Failure<S, F>(error);

    public static IResult<S, Exception> TryCall<S>(Func<S> action)
        where S : notnull
    {
        try
        {
            return Success<S, Exception>(action());
        }
        catch (Exception e)
        {
            return Failure<S, Exception>(e);
        }
    }

    public static IResult<Z, F> Zip<S1, S2, F, Z>(
        IResult<S1, F> r1,
        IResult<S2, F> r2,
        Func<S1, S2, Z> f
    )
        where S1 : notnull
        where S2 : notnull
        where F : notnull
        where Z : notnull => r1.FlatMap(value1 => r2.Map(value2 => f(value1, value2)));

    public static IResult<Z, F> Zip<S1, S2, S3, F, Z>(
        IResult<S1, F> r1,
        IResult<S2, F> r2,
        IResult<S3, F> r3,
        Func<S1, S2, S3, Z> f
    )
        where S1 : notnull
        where S2 : notnull
        where S3 : notnull
        where F : notnull
        where Z : notnull =>
        r1.FlatMap(value1 => r2.FlatMap(value2 => r3.Map(value3 => f(value1, value2, value3))));

    public static IResult<Z, F> Zip<S1, S2, S3, S4, F, Z>(
        IResult<S1, F> r1,
        IResult<S2, F> r2,
        IResult<S3, F> r3,
        IResult<S4, F> r4,
        Func<S1, S2, S3, S4, Z> f
    )
        where S1 : notnull
        where S2 : notnull
        where S3 : notnull
        where S4 : notnull
        where F : notnull
        where Z : notnull =>
        r1.FlatMap(value1 =>
            r2.FlatMap(value2 =>
                r3.FlatMap(value3 => r4.Map(value4 => f(value1, value2, value3, value4)))
            )
        );
}

public interface IResult<S, F>
    where S : notnull
    where F : notnull
{
    public static IResult<S, F> Success(S value) => new Success<S, F>(value);

    public static IResult<S, F> Failure(F error) => new Failure<S, F>(error);

    public bool IsSuccess { get; }

    public bool IsFailure { get; }

    public IResult<S, F> OnSuccess(Action<S> action);

    public IResult<S, F> OnFailure(Action<F> action);

    public IResult<R, F> Map<R>(Func<S, R> transform)
        where R : notnull;

    public IResult<R, F> FlatMap<R>(Func<S, IResult<R, F>> transform)
        where R : notnull;

    public IResult<S, R> MapFailure<R>(Func<F, R> transform)
        where R : notnull;

    public IResult<S, R> FlatMapFailure<R>(Func<F, IResult<S, R>> transform)
        where R : notnull;

    public R Fold<R>(Func<S, R> ifSuccess, Func<F, R> ifFailure);

    public IResult<F, S> Swap();

    public S? GetOrDefault();

    public S GetOrElse(S defaultValue);

    public S GetOrElseGet(Func<S> action);
}

public class Success<S, F> : IResult<S, F>
    where S : notnull
    where F : notnull
{
    private readonly S _value;

    public Success(S value)
    {
        ArgumentNullException.ThrowIfNull(value);
        _value = value;
    }

    public bool IsSuccess => true;

    public bool IsFailure => false;

    public S? GetOrDefault() => _value;

    public IResult<R, F> Map<R>(Func<S, R> transform)
        where R : notnull => new Success<R, F>(transform(_value));

    public IResult<R, F> FlatMap<R>(Func<S, IResult<R, F>> transform)
        where R : notnull => transform(_value);

    public IResult<S, R> MapFailure<R>(Func<F, R> transform)
        where R : notnull => (IResult<S, R>)this;

    public IResult<S, R> FlatMapFailure<R>(Func<F, IResult<S, R>> transform)
        where R : notnull => (IResult<S, R>)this;

    public R Fold<R>(Func<S, R> ifSuccess, Func<F, R> ifFailure) => ifSuccess(_value);

    public IResult<F, S> Swap() => new Failure<F, S>(_value);

    public S Unwrap() => _value;

    public IResult<S, F> OnSuccess(Action<S> action)
    {
        action(_value);
        return this;
    }

    public IResult<S, F> OnFailure(Action<F> action) => this;

    public S GetOrElse(S defaultValue) => _value;

    public S GetOrElseGet(Func<S> action) => _value;
}

public class Failure<S, F> : IResult<S, F>
    where S : notnull
    where F : notnull
{
    private readonly F _value;

    public Failure(F error)
    {
        ArgumentNullException.ThrowIfNull(error);
        _value = error;
    }

    public bool IsSuccess => false;

    public bool IsFailure => true;

    public S? GetOrDefault() => default;

    public IResult<R, F> Map<R>(Func<S, R> transform)
        where R : notnull => (IResult<R, F>)this;

    public IResult<R, F> FlatMap<R>(Func<S, IResult<R, F>> transform)
        where R : notnull => (IResult<R, F>)this;

    public IResult<S, R> MapFailure<R>(Func<F, R> transform)
        where R : notnull => new Failure<S, R>(transform(_value));

    public IResult<S, R> FlatMapFailure<R>(Func<F, IResult<S, R>> transform)
        where R : notnull => transform(_value);

    public R Fold<R>(Func<S, R> ifSuccess, Func<F, R> ifFailure) => ifFailure(_value);

    public IResult<F, S> Swap() => new Success<F, S>(_value);

    public F Unwrap() => _value;

    public IResult<S, F> OnSuccess(Action<S> action) => this;

    public IResult<S, F> OnFailure(Action<F> action)
    {
        action(_value);
        return this;
    }

    public S GetOrElse(S defaultValue) => defaultValue;

    public S GetOrElseGet(Func<S> func) => func();
}
