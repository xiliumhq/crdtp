using System;
using Xilium.Crdtp.Core;
using Xunit;

namespace Xilium.Crdtp.Client.Tests;

public class CrdtpResponseTest
{
    [Fact]
    public void CtorWithError()
    {
        // Calling ctor with error, but with null - should lead to exception,
        // because, it internally used as value or error marker.
        Assert.Throws<ArgumentNullException>(
            () => new CrdtpResponse(default(CrdtpErrorResponse)!));
    }

    [Fact]
    public void SuccessResult()
    {
        var response = new CrdtpResponse(default(Unit)!);
        Assert.True(response.IsSuccess);
        Assert.True(!response.IsError);

        {
            Assert.False(response.TryGetError(out var error));
            Assert.Null(error);
        }

        // void
        response.GetResult();
        Assert.Throws<InvalidOperationException>(() => response.GetError());
    }

    [Fact]
    public void ErrorResult()
    {
        var error = new CrdtpErrorResponse(1, "error message", "error data");

        var response = new CrdtpResponse(error);
        Assert.True(!response.IsSuccess);
        Assert.True(response.IsError);

        {
            Assert.True(response.TryGetError(out var actualError));
            Assert.Same(error, actualError);
        }

        var cerex = Assert.Throws<CrdtpErrorResponseException>(() => response.GetResult());
        Assert.Same(error, cerex.ErrorResponse);

        Assert.Same(error, response.GetError());
    }
}
