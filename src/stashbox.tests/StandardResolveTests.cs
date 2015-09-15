﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ronin.Common;
using Stashbox.Infrastructure;
using System.Threading.Tasks;

namespace Stashbox.Tests
{
    [TestClass]
    public class StandardResolveTests
    {
        [TestMethod]
        public void ResolveTest()
        {
            IStashboxContainer container = new StashboxContainer();
            container.RegisterType<ITest1, Test1>();
            container.RegisterType<ITest2, Test2>();
            container.RegisterType<ITest3, Test3>();

            var test3 = container.Resolve<ITest3>();
            var test2 = container.Resolve<ITest2>();
            var test1 = container.Resolve<ITest1>();

            Assert.IsNotNull(test3);
            Assert.IsNotNull(test2);
            Assert.IsNotNull(test1);

            Shield.EnsureTypeOf<Test1>(test1);
            Shield.EnsureTypeOf<Test2>(test2);
            Shield.EnsureTypeOf<Test3>(test3);
        }

        [TestMethod]
        public void ResolveTest_Parallel()
        {
            IStashboxContainer container = new StashboxContainer();
            container.RegisterType<ITest1, Test1>();
            container.RegisterType<ITest2, Test2>();
            container.RegisterType<ITest3, Test3>();

            Parallel.For(0, 50000, (i) =>
            {
                if (i % 100 == 0)
                {
                    container.RegisterType<ITest1, Test1>();
                    container.RegisterType<ITest3, Test3>();
                }

                var test3 = container.Resolve<ITest3>();
                var test2 = container.Resolve<ITest2>();
                var test1 = container.Resolve<ITest1>();

                Assert.IsNotNull(test3);
                Assert.IsNotNull(test2);
                Assert.IsNotNull(test1);

                Shield.EnsureTypeOf<Test1>(test1);
                Shield.EnsureTypeOf<Test2>(test2);
                Shield.EnsureTypeOf<Test3>(test3);
            });
        }

        public interface ITest1 { string Name { get; set; } }

        public interface ITest2 { string Name { get; set; } }

        public interface ITest3 { string Name { get; set; } }

        public class Test1 : ITest1
        {
            public string Name { get; set; }
        }

        public class Test2 : ITest2
        {
            public string Name { get; set; }

            public Test2(ITest1 test1)
            {
                Shield.EnsureNotNull(test1);
                Shield.EnsureTypeOf<Test1>(test1);
            }
        }

        public class Test3 : ITest3
        {
            public string Name { get; set; }

            public Test3(ITest1 test1, ITest2 test2)
            {
                Shield.EnsureNotNull(test1);
                Shield.EnsureNotNull(test2);
                Shield.EnsureTypeOf<Test1>(test1);
                Shield.EnsureTypeOf<Test2>(test2);
            }
        }
    }
}