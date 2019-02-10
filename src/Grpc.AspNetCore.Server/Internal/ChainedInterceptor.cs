using Grpc.Core;
using Grpc.Core.Interceptors;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Grpc.AspNetCore.Server.Internal
{
    internal class ChainedInterceptor : Interceptor
    {
        private Interceptor _next;
        private Interceptor _current;

        public ChainedInterceptor(Interceptor current, Interceptor next)
        {
            _current = current;
            _next = next;
        }

        public override Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request, ServerCallContext context, UnaryServerMethod<TRequest, TResponse> continuation)
        {
            return _next.UnaryServerHandler(request, context, (req, sc) => _current.UnaryServerHandler(req, sc, continuation));
        }

        public override Task<TResponse> ClientStreamingServerHandler<TRequest, TResponse>(IAsyncStreamReader<TRequest> requestStream, ServerCallContext context, ClientStreamingServerMethod<TRequest, TResponse> continuation)
        {
            return _next.ClientStreamingServerHandler(requestStream, context, (req, sc) => _current.ClientStreamingServerHandler(req, sc, continuation));
        }

        public override AsyncServerStreamingCall<TResponse> AsyncServerStreamingCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context, AsyncServerStreamingCallContinuation<TRequest, TResponse> continuation)
        {
            return _next.AsyncServerStreamingCall(request, context, (req, sc) => _current.AsyncServerStreamingCall(req, sc, continuation));
        }

        public override Task DuplexStreamingServerHandler<TRequest, TResponse>(IAsyncStreamReader<TRequest> requestStream, IServerStreamWriter<TResponse> responseStream, ServerCallContext context, DuplexStreamingServerMethod<TRequest, TResponse> continuation)
        {
            return _next.DuplexStreamingServerHandler(requestStream, responseStream, context, (reqStream, resStream, sc) => _current.DuplexStreamingServerHandler(reqStream, resStream, sc, continuation));
        }

        public override Task ServerStreamingServerHandler<TRequest, TResponse>(TRequest request, IServerStreamWriter<TResponse> responseStream, ServerCallContext context, ServerStreamingServerMethod<TRequest, TResponse> continuation)
        {
            return _next.ServerStreamingServerHandler(request, responseStream, context, (req, resStream, sc) => _current.ServerStreamingServerHandler(req, resStream, sc, continuation));
        }
    }
}
