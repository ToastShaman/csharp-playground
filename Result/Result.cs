namespace Result;

public interface IResult<S, F>
{
    public static IResult<S, F> Success(S value) => new Success<S, F>(value);

    public static IResult<S, F> Failure(F error) => new Failure<S, F>(error);

    public bool IsSuccess { get; }

    public bool IsFailure { get; }

    public IResult<S, F> OnSuccess(Action<S> action);

    public IResult<S, F> OnFailure(Action<F> action);

    public IResult<R, F> Map<R>(Func<S, R> transform);

    public IResult<R, F> FlatMap<R>(Func<S, IResult<R, F>> transform);

    public IResult<S, R> MapFailure<R>(Func<F, R> transform);

    public IResult<S, R> FlatMapFailure<R>(Func<F, IResult<S, R>> transform);

    public R Fold<R>(Func<S, R> ifSuccess, Func<F, R> ifFailure);

    public IResult<F, S> Swap();

    public S? GetOrDefault();

    public S GetOrElse(S defaultValue);

    public S GetOrElseGet(Func<S> action);
}

public class Success<S, F> : IResult<S, F>
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

    public IResult<R, F> Map<R>(Func<S, R> transform) => new Success<R, F>(transform(_value));

    public IResult<R, F> FlatMap<R>(Func<S, IResult<R, F>> transform) => transform(_value);

    public IResult<S, R> MapFailure<R>(Func<F, R> transform) => (IResult<S, R>)this;

    public IResult<S, R> FlatMapFailure<R>(Func<F, IResult<S, R>> transform) => (IResult<S, R>)this;

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

    public IResult<R, F> Map<R>(Func<S, R> transform) => (IResult<R, F>)this;

    public IResult<R, F> FlatMap<R>(Func<S, IResult<R, F>> transform) => (IResult<R, F>)this;

    public IResult<S, R> MapFailure<R>(Func<F, R> transform) =>
        new Failure<S, R>(transform(_value));

    public IResult<S, R> FlatMapFailure<R>(Func<F, IResult<S, R>> transform) => transform(_value);

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
