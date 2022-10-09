using System;
using Xilium.Crdtp.Core;
using Xunit;

namespace Xilium.Crdtp.Client.Tests;

public class CrdtpResponseOfTTest
{
    [Fact]
    public void CtorWithError()
    {
        // Calling ctor with error, but with null - should lead to exception,
        // because, it internally used as value or error marker.
        Assert.Throws<ArgumentNullException>(
            () => new CrdtpResponse<Unit>(default(CrdtpErrorResponse)!));
    }

    [Fact]
    public void ResultOfUnit()
    {
        // Note, Unit type always represented with `null`.
        var expectedResult = default(Unit)!;
        VerifyResultOfReferenceType(expectedResult);
    }

    [Fact]
    public void ResultWithError()
    {
        var error = new CrdtpErrorResponse(1, "error message", "error data");
        VerifyResultOfReferenceTypeWithError<Unit>(error);
        VerifyResultOfReferenceTypeWithError<ClassResult>(error);
    }

    [Fact]
    public void ResultOfClass()
    {
        var expectedResult = new ClassResult();
        VerifyResultOfReferenceType(expectedResult);
    }

    [Fact]
    public void ResultOfValue()
    {
        var expected = new ValueResult("foo");
        var response = new CrdtpResponse<ValueResult>(expected);

        Assert.True(response.IsSuccess);
        Assert.True(!response.IsError);

        {
            Assert.True(response.TryGetResult(out var actualResult));
            Assert.Equal(expected, actualResult);
        }

        {
            Assert.False(response.TryGetError(out var error));
            Assert.Null(error);
        }

        Assert.Equal(expected, response.GetResult());
        Assert.Throws<InvalidOperationException>(() => response.GetError());
    }

    private void VerifyResultOfReferenceType<T>(T? expected)
        where T : class
    {
        var response = new CrdtpResponse<T>(expected!);
        Assert.True(response.IsSuccess);
        Assert.True(!response.IsError);

        {
            Assert.True(response.TryGetResult(out var actualResult));
            Assert.Same(expected, actualResult);
        }

        {
            Assert.False(response.TryGetError(out var error));
            Assert.Null(error);
        }

        Assert.Same(expected, response.GetResult());
        Assert.Throws<InvalidOperationException>(() => response.GetError());
    }

    private void VerifyResultOfReferenceTypeWithError<T>(CrdtpErrorResponse error)
        where T : class
    {
        var response = new CrdtpResponse<T>(error);
        Assert.True(!response.IsSuccess);
        Assert.True(response.IsError);

        {
            Assert.False(response.TryGetResult(out var actualResult));
            Assert.Null(actualResult);
        }

        {
            Assert.True(response.TryGetError(out var actualError));
            Assert.Same(error, actualError);
        }

        var cerex = Assert.Throws<CrdtpErrorResponseException>(() => response.GetResult());
        Assert.Same(error, cerex.ErrorResponse);

        Assert.Same(error, response.GetError());
    }

    public sealed class ClassResult { }

    public readonly struct ValueResult
    {
        private readonly string _value;

        public ValueResult(string value)
        {
            _value = value;
        }

        public string Value => _value;
    }

}
