using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyInjectionContainerLib
{
    public enum TimeToLive
    {
        InstancePerDependency,
        Singleton
    }
    public class InterfaceImplementation
    {
        public Type ImplementationClass { get; }
        public TimeToLive TimeToLive { get; }
        public int NameOfDepend { get; }
        public InterfaceImplementation(Type ImplementationClass, TimeToLive TimeToLive, int nameOfDepend)
        {
            this.ImplementationClass = ImplementationClass;
            this.NameOfDepend = nameOfDepend;
            this.TimeToLive = TimeToLive;
        }
    }
    public class DependencyKey : Attribute
    {
        public int NameOfDepend { get; }
        public DependencyKey(object nameOfDepend)
        {
            this.NameOfDepend = Convert.ToInt32(nameOfDepend);
        }
    }
    public class DependenciesConfiguration
    {
        public Dictionary<Type, List<InterfaceImplementation>> InterfaceDependencies = new Dictionary<Type, List<InterfaceImplementation>>();

        public void Register<U, V>(TimeToLive TimeToLive, Enum name=null) where V : U 
            where U : class
        {
            Register(typeof(U), typeof(V), TimeToLive, Convert.ToInt32(name));
        }
        public void Register(Type Dependency, Type Implementation, TimeToLive timeToLive, int name=0)
        {
            InterfaceImplementation implementationBuf = new InterfaceImplementation(Implementation, timeToLive, name);
            if (InterfaceDependencies.ContainsKey(Dependency))
            {
                InterfaceDependencies[Dependency].Add(implementationBuf);
            }
            else
            {
                List<InterfaceImplementation> implementations = new List<InterfaceImplementation>();
                implementations.Add(implementationBuf);
                InterfaceDependencies.Add(Dependency, implementations);
            }
        }
    }
}
