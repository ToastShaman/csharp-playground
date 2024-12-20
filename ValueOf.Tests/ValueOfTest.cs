﻿using FluentAssertions;
using FluentValidation;

namespace ValueOf.Tests;

public class ValueOfTest
{
    class MySecret(string value) : SecretString<MySecret>(value) { }

    class Firstname(string value) : NonBlankString<Firstname>(value) { }

    class Email(string value) : ValueOf<string, Email>(value, new Validator())
    {
        public class Validator : AbstractValidator<string>
        {
            public Validator()
            {
                Include(new NonBlankStringValidator());
                RuleFor(x => x).EmailAddress().WithMessage("Email must be a valid email address");
            }
        }
    }

    [Fact(DisplayName = "Equals and GetHashCode")]
    public void EqualsAndHashCode()
    {
        Firstname name1 = new("John");
        Firstname name2 = new("John");
        Firstname name3 = new("Jane");

        name1.Equals(name2).Should().BeTrue();
        name1.Equals(name3).Should().BeFalse();
        name1.GetHashCode().Should().Be(name2.GetHashCode());
    }

    [Theory(DisplayName = "Firstname should not be empty or blank")]
    [InlineData("")]
    [InlineData("   ")]
    public void ValidatesFirstname(string input)
    {
        Action act = () => new Firstname(input);
        act.Should().Throw<ValidationException>();
    }

    [Theory(DisplayName = "Email should be validated")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("@")]
    [InlineData("foo@bar.com@")]
    public void ValidatesEmail(string input)
    {
        Action act = () => new Email(input);
        act.Should().Throw<ValidationException>();
    }

    [Fact(DisplayName = "Implicit conversion")]
    public void ImplicitConversion()
    {
        Firstname name = (Firstname)"John";

        string raw = (string)name;

        raw.Should().Be("John");
    }

    [Fact(DisplayName = "Transforms")]
    public void Transforms()
    {
        Firstname name = new("John");

        Firstname transformed = name.Map(static x => x.ToUpper());

        transformed.Unwrap().Should().Be("JOHN");
    }

    [Fact(DisplayName = "Secret string should be masked")]
    public void SecretStringToString()
    {
        new MySecret("password").ToString().Should().Be("********");
    }
}
