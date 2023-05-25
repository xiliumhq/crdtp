using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Xilium.Crdtp.Client.Dispatching;

namespace Xilium.Crdtp.Client;

partial class CrdtpClient
{
    public int GetNextCallId() => Interlocked.Increment(ref _callIdGen);

    // TODO: Move request management here.
    // 1. CrdtpSession should always be bound to client on creation.
    // 2. CrdtpSession - rename StateAndRequestMapLock to StateLock

    internal void AddRequest(int callId, CrdtpRequest request)
    {
        lock (RequestMapLock)
        {
            if (!_requests.TryAdd(callId, request))
            {
                throw Error.InvalidOperation("Request with same id already registered.");
            }

            request.Session._numberOfPendingRequests++;
        }
    }

    private CrdtpRequest GetAndRemoveRequest(int callId)
    {
        lock (RequestMapLock)
        {
#if NET5_0_OR_GREATER
            if (!_requests.Remove(callId, out var request))
                throw Error.InvalidOperation("Request with given id not found."); // TODO: Emit event
            request.Session._numberOfPendingRequests--;
            return request;
#else
            if (!_requests.TryGetValue(callId, out var request))
                throw Error.InvalidOperation("Request with given id not found."); // TODO: Emit event
            _ = _requests.Remove(callId);
            return request;
#endif
        }
    }

    internal bool UnregisterRequest(int callId, CrdtpRequest request)
    {
        lock (RequestMapLock)
        {
            // TODO: This would be nice to have TryRemove method, rather than this two calls.
            // Even if we remove unrelated request, we may always add it back.
            if (_requests.TryGetValue(callId, out var actualRequest))
            {
                if (actualRequest == (object)request)
                {
                    var result = _requests.Remove(callId);
                    if (result)
                        request.Session._numberOfPendingRequests--;
                    return result;
                }
            }
            return false;
        }
    }

    /// <summary>
    /// Cancels all pending requests.
    /// </summary>
    public void CancelPendingRequests()
    {
        lock (RequestMapLock)
        {
            if (_requests.Count == 0)
                return;

            // TODO: This is not optimal
            foreach (var request in _requests.Values.ToList())
            {
                // TODO: Does it safe to execute cancel? (request can be cancelled concurrently via cancellation token => and it should attempt to unreginster request from this collection and take lock)?
                request.Cancel(removeFromRequestMap: true);
            }
        }
    }

    public void CancelPendingRequests(CrdtpSession session)
    {
        lock (RequestMapLock)
        {
            if (session._numberOfPendingRequests == 0)
                return;

            // TODO: This is not optimal
            foreach (var request in _requests.Values
                // Take request until session._numberOfPendingRequests > 0
                .Where(x => x.Session == session)
                .ToList())
            {
                // TODO: Does it safe to execute cancel? (request can be cancelled concurrently via cancellation token => and it should attempt to unreginster request from this collection and take lock)?
                request.Cancel(removeFromRequestMap: true);
            }
        }
    }

    internal void Dispatch(Dispatchable dispatchable)
    {
        if (dispatchable.CallId.HasValue) // Response
        {
            var request = GetAndRemoveRequest(dispatchable.CallId.Value);
            if (request == null)
            {
                // TODO: Handle if request not found
                throw Error.InvalidOperation($"Protocol violation. Request with given id not found.");
            }

            // Lookup session & validate if session id is setup correctly,
            // however for error response skip session check.
            var session = request.Session;

            var sessionId = dispatchable.SessionId;

            // TODO: Optionally we can always trust to server's callId and
            // always skip this checks.
            var skipSessionIdCheck = string.IsNullOrEmpty(sessionId) &&
                dispatchable.DataType == Dispatchable.PayloadType.Error;
            if (!skipSessionIdCheck)
            {
                if (sessionId != session.SessionId)
                {
                    // TODO: Because we already removed request, then it should be aborted instead.
                    throw Error.InvalidOperation($"Protocol violation. Response's session did not match to request session.");
                }
            }

            var context = new CrdtpDispatchContext(session);
            request.Dispatch(context, dispatchable);
        }
        else if (!string.IsNullOrEmpty(dispatchable.Method)) // Event
        {
            if (!TryGetSession(dispatchable.SessionId, out var session))
                throw Error.InvalidOperation("Session not found.");
            session.DispatchEventInternal(dispatchable);
        }
        else
        {
            throw Error.InvalidOperation($"Protocol violation. Given message is not method result nor event notification.");
        }
    }


    // TODO: Move this method to metrics.
    public int GetNumberOfPendingRequests() => _requests?.Count ?? 0;
}
