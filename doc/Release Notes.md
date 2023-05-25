# Release Notes

## main

  - gen: Add non-throwing typed command invokers (see issue #8)
  - Add CrdtpPipeJsonConnection (.NET 5 or greater) (see issue #16)
  - Add simple `CrdtpUtf16MessageWriter` to assist logging
  - gen: Fix generation of XML documentation tags (closes issue #17)
  - Don't include `IClientApi` and `ISessionApi` internal interfaces into
    release builds, as they used only for documenting/modeling purposes
  - Fix possible deadlock in `Dispose` (see issue #7)
  - .NET 6 SDK is now required to build, also targeted to .NET 6, which is now
    primary supported target. .NET 5 is still intact, and .NET 4.6.1 and
    .NET Standard 2.0 also supported, with some limitations

## 0.3

  This release is not full featureful, but it has everything what is needed, and
  battle-tested and can be considered stable.

  - Add `CrdtpSessionHandler::OnBeforeSend` which allows to validate command by name 
    before sending (by using some ambient state, which is not known to CrdtpClient)

## 0.2.2

  - Treat null as NaN double value during JSON deserialization (issue #1)
  - Workaround: Ignore string decoding errors during JSON deserialization (issue #2)
  - Unhandled exception by default will abort client, even if no client handler provided
  - Support web socket connections on windows 7 (see issue #4)
  - compat: Fix Core/NullableAttributes to prevent accidental attributes leak

## 0.2.0

  - gen: Alternative naming for generated anonymous types. Option to emit partial types
  - Project reorganization
  - Removed cancellation token from `CrdtpConnection::SendAsync` methods (this method is not cancellable by design.)
  - Add CrdtpLogger (and temporary CrdtpConsoleLogger)
  - WebSockets: async send with synchronization for frameworks where WebSocket implementation have not internal synchronization
  - Improved register for cancellation
  - Refactor CrdtpConnection
  - Refactor a bit CrdtpClient and CrdtpSession
  - Add CrdtpClientHandler and CrdtpSessionHandler for listening some events

## 0.1.1

  - Initial release: includes client connectivity and code generation tool.
