using FluentValidation;

namespace ValueOf;

public abstract class ValueOf<T, TDerived>
    where T : notnull
    where TDerived : ValueOf<T, TDerived>
{
    public T Value { get; }

    protected ValueOf(T value)
    {
        ArgumentNullException.ThrowIfNull(value);
        Value = value;
    }

    protected ValueOf(T value, AbstractValidator<T> validator)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(validator);
        validator.ValidateAndThrow(value);
        Value = value;
    }

    public R Transform<R>(Func<T, R> transform) => transform(Value);

    public override bool Equals(object? obj) =>
        obj is ValueOf<T, TDerived> other && EqualityComparer<T>.Default.Equals(Value, other.Value);

    public override int GetHashCode() => Value.GetHashCode();

    public override string ToString() => Value.ToString() ?? string.Empty;

    public static implicit operator T(ValueOf<T, TDerived> valueOf) => valueOf.Value;

    public static explicit operator ValueOf<T, TDerived>(T value) =>
        (ValueOf<T, TDerived>)Activator.CreateInstance(typeof(TDerived), value)!;
}

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
