using System.Linq.Expressions;
using FluentValidation;

namespace ValueOf;

public abstract class ValueOf<T, TDerived>
    where T : notnull
    where TDerived : ValueOf<T, TDerived>
{
    private static readonly Func<T, TDerived> _factory;

    private readonly T _value;

    public ValueOf(T value)
        : this(value, new NoOpValidator<T>()) { }

    public ValueOf(T value, AbstractValidator<T> validator)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(validator);
        validator.ValidateAndThrow(value);
        _value = value;
    }

    static ValueOf()
    {
        var constructor =
            typeof(TDerived)
                .GetConstructors()
                .FirstOrDefault(static ctor =>
                {
                    var parameters = ctor.GetParameters();
                    var has1Param = parameters.Length == 1;
                    var hasParamT = parameters[0].ParameterType == typeof(T);
                    return has1Param && hasParamT;
                }) ?? throw new FailedToCreateValueTypeException(typeof(TDerived), typeof(T));

        var param = Expression.Parameter(typeof(T), "value");
        var newExpression = Expression.New(constructor, param);
        var lambda = Expression.Lambda<Func<T, TDerived>>(newExpression, param);
        _factory = lambda.Compile();
    }

    public TDerived Map(Func<T, T> transform) => _factory(transform(_value));

    public R Unwrap<R>(Func<T, R> transform) => transform(_value);

    public T Unwrap() => _value;

    public override bool Equals(object? obj) =>
        obj is ValueOf<T, TDerived> other
        && EqualityComparer<T>.Default.Equals(_value, other._value);

    public override int GetHashCode() => _value.GetHashCode();

    public override string ToString() => _value.ToString() ?? string.Empty;

    public static implicit operator T(ValueOf<T, TDerived> valueOf) => valueOf._value;

    public static explicit operator ValueOf<T, TDerived>(T value) => _factory(value);
}

public class FailedToCreateValueTypeException(Type clazz, Type parameter)
    : Exception($"Type {clazz} must have a constructor with a single {parameter} parameter.") { }

public class NonBlankString<TDerived>(string value, AbstractValidator<string> validator)
    : ValueOf<string, TDerived>(value, validator)
    where TDerived : ValueOf<string, TDerived>
{
    public NonBlankString(string value)
        : this(value, new NonBlankStringValidator()) { }
}

public class NonBlankStringValidator : AbstractValidator<string>
{
    public NonBlankStringValidator()
    {
        RuleFor(x => x)
            .NotNull()
            .WithMessage("String must not be null")
            .NotEmpty()
            .WithMessage("String must not be empty")
            .Must(x => !string.IsNullOrWhiteSpace(x))
            .WithMessage("String must not be whitespace");
    }
}

public class SecretString<TDerived>(string value, AbstractValidator<string> validator)
    : ValueOf<string, TDerived>(value, validator)
    where TDerived : ValueOf<string, TDerived>
{
    public SecretString(string value)
        : this(value, new NonBlankStringValidator()) { }

    public override string ToString() => "********";
}

public class NoOpValidator<T> : AbstractValidator<T>
{
    public NoOpValidator()
    {
        // No rules are added, so this validator does nothing
    }
}
