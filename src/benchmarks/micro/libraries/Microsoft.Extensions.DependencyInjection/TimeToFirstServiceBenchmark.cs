﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using MicroBenchmarks;

namespace Microsoft.Extensions.DependencyInjection
{
    [BenchmarkCategory(Categories.Libraries)]
    public class TimeToFirstService
    {
        private ServiceCollection _transientServices;
        private ServiceCollection _scopedServices;
        private ServiceCollection _singletonServices;
#if INTERNAL_DI
        private ServiceProviderMode _mode;
#endif

        [Params("Expressions", "Dynamic", "Runtime", "ILEmit")]
        public string Mode {
            set {
#if INTERNAL_DI
                _mode = (ServiceProviderMode)Enum.Parse(typeof(ServiceProviderMode), value);
#endif
            }
        }

        [GlobalSetup(Targets = new[] { nameof(BuildProvider), nameof(Transient) })]
        public void SetupTransient()
        {
            _transientServices = new ServiceCollection();
            _transientServices.AddTransient<A>();
            _transientServices.AddTransient<B>();
            _transientServices.AddTransient<C>();
        }

        [Benchmark]
        public void BuildProvider()
        {
            using ServiceProvider transientSp = _transientServices.BuildServiceProvider(new ServiceProviderOptions()
            {
#if INTERNAL_DI
                Mode = _mode
#endif
            });
        }

        [Benchmark]
        public void Transient()
        {
            using ServiceProvider transientSp = _transientServices.BuildServiceProvider(new ServiceProviderOptions()
            {
#if INTERNAL_DI
                Mode = _mode
#endif
            });
            var temp = transientSp.GetService<A>();
            temp.Foo();
        }

        [GlobalSetup(Target = nameof(Scoped))]
        public void SetupScoped()
        {
            _scopedServices = new ServiceCollection();
            _scopedServices.AddScoped<A>();
            _scopedServices.AddScoped<B>();
            _scopedServices.AddScoped<C>();
        }

        [Benchmark]
        public void Scoped()
        {
            using ServiceProvider provider = _scopedServices.BuildServiceProvider(new ServiceProviderOptions()
            {
#if INTERNAL_DI
                Mode = _mode
#endif
            });
            IServiceScope scopedSp = provider.CreateScope();
            var temp = scopedSp.ServiceProvider.GetService<A>();
            temp.Foo();
        }

        [GlobalSetup(Target = nameof(Singleton))]
        public void SetupSingleton()
        {
            _singletonServices = new ServiceCollection();
            _singletonServices.AddSingleton<A>();
            _singletonServices.AddSingleton<B>();
            _singletonServices.AddSingleton<C>();
        }

        [Benchmark]
        public void Singleton()
        {
            using ServiceProvider singletonSp = _singletonServices.BuildServiceProvider(new ServiceProviderOptions()
            {
#if INTERNAL_DI
                Mode = _mode
#endif
            });
            var temp = singletonSp.GetService<A>();
            temp.Foo();
        }

        private class A
        {
            public A(B b) { }

            [MethodImpl(MethodImplOptions.NoInlining)]
            public void Foo() { }
        }

        private class B
        {
            public B(C c) { }
        }

        private class C { }
    }
}
