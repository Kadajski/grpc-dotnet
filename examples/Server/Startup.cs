#region Copyright notice and license

// Copyright 2019 The gRPC Authors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#endregion

using System;
using System.Threading.Tasks;
using Grpc.AspNetCore;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GRPCServer
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddGrpc();
            services.AddSingleton<IncrementingCounter>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app
                .UseRouting(routes =>
                {
                    routes.MapGrpcService<ChatterService>();
                    routes.MapGrpcService<CounterService>(call => call.Intercept(new TestInterceptor()).Intercept(new TestInterceptor2()));
                    routes.MapGrpcService<GreeterService>();
                });
        }
    }

    public class TestInterceptor : Interceptor
    {
        public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request, ServerCallContext context, UnaryServerMethod<TRequest, TResponse> continuation)
        {
            Console.WriteLine($"Begin: {request}. {nameof(TestInterceptor)}");
            var response = await base.UnaryServerHandler(request, context, continuation);
            Console.WriteLine($"End: {response}. {nameof(TestInterceptor)}");
            return response;
        }
    }

    public class TestInterceptor2 : Interceptor
    {
        public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request, ServerCallContext context, UnaryServerMethod<TRequest, TResponse> continuation)
        {
            Console.WriteLine($"Begin: {request}. {nameof(TestInterceptor2)}");
            var response = await base.UnaryServerHandler(request, context, continuation);
            Console.WriteLine($"End: {response}. {nameof(TestInterceptor2)}");
            return response;
        }
    }
}
