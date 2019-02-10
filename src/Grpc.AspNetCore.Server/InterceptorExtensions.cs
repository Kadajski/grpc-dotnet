using Grpc.AspNetCore.Server.Internal;
using Grpc.Core.Interceptors;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class InterceptorExtensions
    {
        public static Interceptor Intercept(this Interceptor interceptor, Interceptor next)
        {
            return new ChainedInterceptor(interceptor, next);
        }
    }
}
