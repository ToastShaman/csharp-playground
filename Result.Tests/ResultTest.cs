﻿using FluentAssertions;

namespace Result.Tests;

public class ResultTest
{
    [Fact(DisplayName = "Can map a success result")]
    public void CanMapASuccessResult()
    {
        var result = Result.Success<int, string>(42);
        var mapped = result.Map(value => value * 2);
        mapped.Should().BeOfType<Success<int, string>>();
        mapped.GetOrDefault().Should().Be(84);
    }

    [Fact(DisplayName = "Can map a failure result")]
    public void CanMapAFailureResult()
    {
        var result = Result.Failure<int, string>("error");
        var mapped = result.Map(value => value * 2);
        mapped.Should().BeOfType<Failure<int, string>>();
        mapped.GetOrDefault().Should().Be(0);
    }

    [Fact(DisplayName = "Can flat map a success result")]
    public void CanFlatMapASuccessResult()
    {
        var result = Result.Success<int, string>(42);
        var mapped = result.FlatMap(value => IResult<int, string>.Success(value * 2));
        mapped.Should().BeOfType<Success<int, string>>();
        mapped.GetOrDefault().Should().Be(84);
    }

    [Fact(DisplayName = "Can flat map a failure result")]
    public void CanFlatMapAFailureResult()
    {
        var result = Result.Failure<int, string>("error");
        var mapped = result.FlatMap(value => IResult<int, string>.Success(value * 2));
        mapped.Should().BeOfType<Failure<int, string>>();
        mapped.GetOrDefault().Should().Be(0);
    }

    [Fact(DisplayName = "Can fold a success result")]
    public void CanFoldASuccessResult()
    {
        var result = Result.Success<int, string>(42);
        var folded = result.Fold(value => value * 2, error => 0);
        folded.Should().Be(84);
    }

    [Fact(DisplayName = "Can fold a failure result")]
    public void CanFoldAFailureResult()
    {
        var result = Result.Failure<int, string>("error");
        var folded = result.Fold(value => value * 2, error => 0);
        folded.Should().Be(0);
    }

    [Fact(DisplayName = "Can swap a success result")]
    public void CanSwapASuccessResult()
    {
        var result = Result.Success<int, string>(42);
        var swapped = result.Swap();
        swapped.Should().BeOfType<Failure<string, int>>().Which.Unwrap().Should().Be(42);
    }

    [Fact(DisplayName = "Can swap a failure result")]
    public void CanSwapAFailureResult()
    {
        var result = Result.Failure<int, string>("error");
        var swapped = result.Swap();
        swapped.Should().BeOfType<Success<string, int>>().Which.Unwrap().Should().Be("error");
    }

    [Fact(DisplayName = "Can map a failure result on a failure")]
    public void CanMapAFailureResultOnFailure()
    {
        var result = Result.Failure<int, string>("error");
        var mapped = result.MapFailure(error => error.Length);
        mapped.Should().BeOfType<Failure<int, int>>().Which.Unwrap().Should().Be(5);
    }

    [Fact(DisplayName = "Can map a failure result on a failure")]
    public void CanFlatMapAFailureResultOnFailure()
    {
        var result = Result.Failure<int, string>("error");
        var mapped = result.FlatMapFailure(error => IResult<int, string>.Success(error.Length));
        mapped.Should().BeOfType<Success<int, string>>();
        mapped.GetOrDefault().Should().Be(5);
    }

    [Fact(DisplayName = "Can run on success action")]
    public void CanRunOnSuccessAction()
    {
        var actionExecuted = false;
        var result = Result.Success<int, string>(42);
        var actioned = result.OnSuccess(value => actionExecuted = true);
        actionExecuted.Should().BeTrue();
    }

    [Fact(DisplayName = "Can run on failure action")]
    public void CanRunOnFailureAction()
    {
        var actionExecuted = false;
        var result = Result.Failure<int, string>("error");
        var actioned = result.OnFailure(error => actionExecuted = true);
        actionExecuted.Should().BeTrue();
    }

    [Fact(DisplayName = "Can zip two success results")]
    public void CanZipTwoSuccessResults()
    {
        var result1 = Result.Success<int, string>(42);
        var result2 = Result.Success<int, string>(42);
        var zipped = Result.Zip(result1, result2, static (a, b) => a + b);
        zipped.Should().BeOfType<Success<int, string>>();
        zipped.GetOrDefault().Should().Be(84);
    }
}
