using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DependencyInjectionContainerLib;
using System.Collections;
using System.Collections.Generic;
namespace TestForLib
{
    public abstract class GetAbsNum{
        public abstract int GetNumAbs();
    }
    public interface IGetNum
    {
        int GetNum();
    }
    public class GetFive : GetAbsNum,IGetNum
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
    public class GetSeven: IGetNum
    {
        public int GetNum()
        {
            return 7;
        }
    }
    public class GetTen: IGetNum
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
    public class Subctract: IGetSubstaction
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
    [TestClass]
    public class LibTests
    {
        [TestInitialize]
        public void Initialize()
        {

        }
        [TestMethod]
        public void TestEnumerableNum()
        {
            DependenciesConfiguration configuration = new DependenciesConfiguration();
            configuration.Register<IGetNum, GetSeven>(TimeToLive.InstancePerDependency);
            configuration.Register<IGetNum, GetFive>(TimeToLive.InstancePerDependency);
            configuration.Register<IGetNum, GetTen>(TimeToLive.InstancePerDependency);
            DependencyProvider provider = new DependencyProvider(configuration);

            var test = provider.Resolve<IGetNum>();

            Assert.AreEqual(test.GetNum(), 7);

            var listNum = provider.Resolve<IEnumerable<IGetNum>>();


            IEnumerator i = listNum.GetEnumerator();
            i.MoveNext();
            Assert.AreEqual(((IGetNum)i.Current).GetNum(), 7);
            i.MoveNext();
            Assert.AreEqual(((IGetNum)i.Current).GetNum(), 5);
            i.MoveNext();
            Assert.AreEqual(((IGetNum)i.Current).GetNum(), 10);
        }

        [TestMethod]
        public void TestAbsractrFive()
        {
            DependenciesConfiguration configuration = new DependenciesConfiguration();
            configuration.Register<GetAbsNum, GetFive>(TimeToLive.InstancePerDependency);
            DependencyProvider provider = new DependencyProvider(configuration);

            var test = provider.Resolve<GetAbsNum>();

            Assert.AreEqual(test.GetNumAbs(), 5);
        }
        [TestMethod]
        public void TestInrefaceFive()
        {
            DependenciesConfiguration configuration = new DependenciesConfiguration();
            configuration.Register<IGetNum, GetFive>(TimeToLive.InstancePerDependency);
            DependencyProvider provider = new DependencyProvider(configuration);

            var test = provider.Resolve<IGetNum>();

            Assert.AreEqual(test.GetNum(), 5);
        }
        [TestMethod]
        public void TestAsSelfFive()
        {
            DependenciesConfiguration configuration = new DependenciesConfiguration();
            configuration.Register<GetFive, GetFive>(TimeToLive.InstancePerDependency);
            DependencyProvider provider = new DependencyProvider(configuration);

            var test = provider.Resolve<GetFive>();

            Assert.AreEqual(test.GetNum(), 5);
        }

        [TestMethod]
        public void TestDependenies()
        {
            DependenciesConfiguration configuration = new DependenciesConfiguration();
            configuration.Register<IGetNum, GetSeven>(TimeToLive.InstancePerDependency);
            configuration.Register<IGetSubstaction, Subctract>(TimeToLive.InstancePerDependency);
            DependencyProvider provider = new DependencyProvider(configuration);
            var test = provider.Resolve<IGetSubstaction>();
            Assert.AreEqual(test.Substarction(), 23);
        }

        public enum Numbers
        {
            Five,
            Seven,
            Ten,
        }

        [TestMethod]
        public void TestNameDependenies()
        {
            DependenciesConfiguration configuration = new DependenciesConfiguration();
            configuration.Register<IGetNum, GetFive>(TimeToLive.InstancePerDependency,Numbers.Five);
            configuration.Register<IGetNum, GetSeven>(TimeToLive.InstancePerDependency, Numbers.Seven);
            configuration.Register<IGetNum, GetTen>(TimeToLive.InstancePerDependency,Numbers.Ten);
            DependencyProvider provider = new DependencyProvider(configuration);

            var testSeven= provider.Resolve<IGetNum>(Numbers.Seven);
            var testFive = provider.Resolve<IGetNum>(Numbers.Five);
            var testTen = provider.Resolve<IGetNum>(Numbers.Ten);

            Assert.AreEqual(testSeven.GetNum(), 7);
            Assert.AreEqual(testFive.GetNum(), 5);
            Assert.AreEqual(testTen.GetNum(), 10);
        }
   
        [TestMethod]
        public void TestRecursion()
        {
            DependenciesConfiguration configuration = new DependenciesConfiguration();
            configuration.Register<IGetNum, GetSeven>(TimeToLive.InstancePerDependency);
            configuration.Register<IGetSubstaction, Subctract>(TimeToLive.InstancePerDependency);
            DependencyProvider provider = new DependencyProvider(configuration);
            var test = provider.Resolve<IGetSubstaction>();
            Assert.AreEqual(test.Substarction(), 23);
        }
    }
}
