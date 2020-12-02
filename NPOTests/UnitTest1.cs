using NUnit.Framework;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Threading;
using Moq;
using DependencyInjectionContainerLib;
namespace Tests
{
    public abstract class GetAbsNum
    {
        public abstract int GetNumAbs();
    }
    public interface IGetNum
    {
        int GetNum();
    }
    public class GetFive : GetAbsNum, IGetNum
    {
        public int GetNum()
        {
            return 5;
        }
        public override int GetNumAbs()
        {
            return 5;
        }
    }
    public class GetSeven : IGetNum
    {
        public int GetNum()
        {
            return 7;
        }
    }
    public class GetTen : IGetNum
    {
        public int GetNum()
        {
            return 10;
        }
    }
    public interface IGetSubstaction
    {
        int Substarction();
    }
    public class Subctract : IGetSubstaction
    {
        IGetNum getNum;
        public Subctract(IGetNum getNum)
        {
            this.getNum = getNum;
        }
        public int Substarction()
        {
            return 30 - getNum.GetNum();
        }
    }
    public enum Numbers
    {
        Five,
        Seven,
        Ten,
    }
    public class NameConst
    {
        IGetNum getNum;
        public NameConst([DependencyKey(Numbers.Five)]IGetNum getNum)
        {
            this.getNum = getNum;
        }
        public int GetNum()
        {
            return getNum.GetNum();
        }
    }
    interface IService<TGetNum> where TGetNum : IGetNum
    {
        int GetNum();
    }
    interface ISingleton
    {

    }
    public class Singleton : ISingleton
    {
        public int rand;
        public Singleton()
        {
            rand = new Random().Next();
        }
    }
    class GetGenricNum<TGetNum> : IService<TGetNum>
        where TGetNum : IGetNum
    {
        IGetNum getNum;
        public GetGenricNum(TGetNum getNum)
        {
            this.getNum = getNum;
        }
        public int GetNum()
        {
            return getNum.GetNum();
        }
    }
    public class Tests
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        [SetUp]
        public void Setup()
        {
            var config = new NLog.Config.LoggingConfiguration();

            var logfile = new NLog.Targets.FileTarget("logfile") { FileName = "logs.txt" };

            config.AddRule(LogLevel.Debug, LogLevel.Fatal, logfile);
            
            LogManager.Configuration = config;
        }

        [Test]
        public void TestEnumerableNum()
        {
            DependenciesConfiguration configuration = new DependenciesConfiguration();
            configuration.Register<IGetNum, GetSeven>(TimeToLive.InstancePerDependency);
            configuration.Register<IGetNum, GetFive>(TimeToLive.InstancePerDependency);
            configuration.Register<IGetNum, GetTen>(TimeToLive.InstancePerDependency);
            DependencyProvider provider = new DependencyProvider(configuration);
            var test = provider.Resolve<IGetNum>();
            Assert.AreEqual(test.GetNum(), 7);
            int actual=0;
            var listNum = provider.Resolve<IEnumerable<IGetNum>>();
            IEnumerator i = listNum.GetEnumerator();
            try
            {
                int[] expected = {7, 5, 10};
                int j = 0;
                while (i.MoveNext())
                {
                    Assert.IsTrue(i.Current is IGetNum);
                    actual = ((IGetNum)i.Current).GetNum();
                    Assert.AreEqual(expected[j], actual);
                    j++;
                }
                logger.Info(nameof(TestEnumerableNum) + " - passed");
            }
            catch (Exception e)
            {
                logger.Error(e, nameof(TestEnumerableNum) + " - failed");
            }
        }

        DependencyProvider providerSingle;
        int testInt;
        [Test]
        public void TestSingleton()
        {
            DependenciesConfiguration configuration = new DependenciesConfiguration();
            configuration.Register<ISingleton, Singleton>(TimeToLive.Singleton);
            providerSingle = new DependencyProvider(configuration);
            var test = providerSingle.Resolve<ISingleton>();
            Thread secondThread = new Thread(new ThreadStart(SecondThread));
            secondThread.Start();
            Thread.Sleep(1000);
            try
            {
                Assert.AreEqual((test as Singleton).rand, testInt);
                logger.Info(nameof(TestSingleton) + " - passed");
            }
            catch (Exception e)
            {
                logger.Error(e, nameof(TestSingleton) + " - failed");
            }
        }
        public void SecondThread()
        {
           var test = providerSingle.Resolve<ISingleton>();
           testInt = (test as Singleton).rand;
        }



        [Test]
        public void TestAbsractrFive()
        {
            DependenciesConfiguration configuration = new DependenciesConfiguration();
            configuration.Register<GetAbsNum, GetFive>(TimeToLive.InstancePerDependency);
            DependencyProvider provider = new DependencyProvider(configuration);
            var actual = provider.Resolve<GetAbsNum>();
            var expected = new GetFive(); 
            try
            {
                Assert.IsTrue(actual is GetFive);
                Assert.AreEqual(expected.GetNumAbs(), actual.GetNumAbs());
                logger.Info(nameof(TestAbsractrFive)+" - passed");
            }
            catch(Exception e)
            {
                logger.Error(e,nameof(TestAbsractrFive)+" - failed");
            }
        }

        [Test]
        public void TestInrefaceFive()
        {
            DependenciesConfiguration configuration = new DependenciesConfiguration();
            configuration.Register<IGetNum, GetFive>(TimeToLive.InstancePerDependency);
            DependencyProvider provider = new DependencyProvider(configuration);

            Mock<IGetNum> mockActual = new Mock<IGetNum>();
            mockActual.Setup(mock => mock.GetNum()).Returns(5);

            var expected = provider.Resolve<IGetNum>();
            var actual = new GetFive();

            try
            {
                Assert.AreEqual(expected.GetNum(), mockActual.Object.GetNum());
                Assert.IsTrue(expected is GetFive);
                Assert.AreEqual(expected.GetNum(), actual.GetNum());
                logger.Info(nameof(TestInrefaceFive) + " - passed");
            }
            catch (Exception e)
            {
                logger.Error(e, nameof(TestInrefaceFive) + " - failed");
            }
        }

        [Test]
        public void TestAsSelfFive()
        {
            DependenciesConfiguration configuration = new DependenciesConfiguration();
            configuration.Register<GetFive, GetFive>(TimeToLive.InstancePerDependency);
            DependencyProvider provider = new DependencyProvider(configuration);

            var actual = provider.Resolve<GetFive>();

            var expected = new GetFive(); 
            try
            {
                Assert.IsTrue(actual is GetFive);
                Assert.AreEqual(expected.GetNum(), actual.GetNum());
                logger.Info(nameof(TestAsSelfFive) + " - passed");
            }
            catch (Exception e)
            {
                logger.Error(e, nameof(TestAsSelfFive) + " - failed");
            }
        }

        [Test]
        public void TestDependenies()
        {
            DependenciesConfiguration configuration = new DependenciesConfiguration();
            configuration.Register<IGetNum, GetSeven>(TimeToLive.InstancePerDependency);
            configuration.Register<IGetSubstaction, Subctract>(TimeToLive.InstancePerDependency);
            DependencyProvider provider = new DependencyProvider(configuration);
            var actual = provider.Resolve<IGetSubstaction>();
            var expected = new Subctract(new GetSeven()); 
            try
            {
                Assert.IsTrue(actual is Subctract);
                Assert.AreEqual(expected.Substarction(), actual.Substarction());
                logger.Info(nameof(TestDependenies) + " - passed");
            }
            catch (Exception e)
            {
                logger.Error(e, nameof(TestDependenies) + " - failed");
            }
        }



        [Test]
        public void TestNameDependenies()
        {
            DependenciesConfiguration configuration = new DependenciesConfiguration();
            configuration.Register<IGetNum, GetFive>(TimeToLive.InstancePerDependency, Numbers.Five);
            configuration.Register<IGetNum, GetSeven>(TimeToLive.InstancePerDependency, Numbers.Seven);
            configuration.Register<IGetNum, GetTen>(TimeToLive.InstancePerDependency, Numbers.Ten);
            DependencyProvider provider = new DependencyProvider(configuration);

            var actualSeven = provider.Resolve<IGetNum>(Numbers.Seven);
            var actualFive = provider.Resolve<IGetNum>(Numbers.Five);
            var actualTen = provider.Resolve<IGetNum>(Numbers.Ten);
          
            var expectedSeven = new GetSeven();
            var expectedFive = new GetFive();
            var expectedTen = new GetTen();
            try
            {
                Assert.IsTrue(actualSeven is GetSeven);
                Assert.IsTrue(actualFive is GetFive);
                Assert.IsTrue(actualTen is GetTen);
                Assert.AreEqual(expectedSeven.GetNum(), actualSeven.GetNum());
                Assert.AreEqual(expectedFive.GetNum(), actualFive.GetNum());
                Assert.AreEqual(expectedTen.GetNum(), actualTen.GetNum());
                logger.Info(nameof(TestNameDependenies) + " - passed");
            }
            catch (Exception e)
            {
                logger.Error(e, nameof(TestNameDependenies) + " - failed");
            }
        }


        [Test]
        public void TestGeneric()
        {
            DependenciesConfiguration configuration = new DependenciesConfiguration();
            configuration.Register<IGetNum, GetTen>(TimeToLive.InstancePerDependency);
            configuration.Register<IService<IGetNum>, GetGenricNum<IGetNum>>(TimeToLive.InstancePerDependency);
            DependencyProvider provider = new DependencyProvider(configuration);

            var actual = provider.Resolve<IService<IGetNum>>(); 
            var expected = new GetGenricNum<IGetNum>(new GetTen());
            try
            {
                Assert.IsTrue(actual is GetGenricNum<IGetNum>);
                Assert.AreEqual(expected.GetNum(), actual.GetNum());
                logger.Info(nameof(TestGeneric) + " - passed");
            }
            catch (Exception e)
            {
                logger.Error(e, nameof(TestGeneric) + " - failed");
            }
        }

        [Test]
        public void TestOpenGeneric()
        {
            DependenciesConfiguration configuration = new DependenciesConfiguration();
            configuration.Register<IGetNum, GetTen>(TimeToLive.InstancePerDependency);
            configuration.Register(typeof(IService<>), typeof(GetGenricNum<>), TimeToLive.InstancePerDependency);
            DependencyProvider provider = new DependencyProvider(configuration);

            var expected = new GetGenricNum<IGetNum>(new GetTen());
            var actual = provider.Resolve<IService<IGetNum>>();

            try
            {
                Assert.IsTrue(actual is GetGenricNum<IGetNum>);
                Assert.AreEqual(expected.GetNum(), actual.GetNum());
                logger.Info(nameof(TestOpenGeneric) + " - passed");
            }
            catch (Exception e)
            {
                logger.Error(e, nameof(TestOpenGeneric) + " - failed");
            }
        }

      //  [Test]
        public void TestNameDependeniesFromConstructor()
        {
            DependenciesConfiguration configuration = new DependenciesConfiguration();
            configuration.Register<IGetNum, GetFive>(TimeToLive.InstancePerDependency, Numbers.Five);
            configuration.Register<IGetNum, GetSeven>(TimeToLive.InstancePerDependency, Numbers.Seven);
            configuration.Register<IGetNum, GetTen>(TimeToLive.InstancePerDependency, Numbers.Ten);
            configuration.Register<NameConst, NameConst>(TimeToLive.InstancePerDependency);
            DependencyProvider provider = new DependencyProvider(configuration);

            var test = provider.Resolve<NameConst>();

            Assert.AreEqual(test.GetNum(), 5);
        }

        [Test]
        public void TestExpetion()
        {
            DependenciesConfiguration configuration = new DependenciesConfiguration();
            configuration.Register<IGetNum, GetFive>(TimeToLive.InstancePerDependency);
            configuration.Register<IGetNum, GetSeven>(TimeToLive.InstancePerDependency);
            DependencyProvider provider = new DependencyProvider(configuration);
            try
            {
                var test = provider.Resolve<GetTen>();
                logger.Error(nameof(TestOpenGeneric) + " - failed");
                Assert.Fail();
            }
            catch (Exception e)
            {
                logger.Info(e,nameof(TestExpetion) + " - passed");
                Assert.Pass();
            }
        }
    }
}