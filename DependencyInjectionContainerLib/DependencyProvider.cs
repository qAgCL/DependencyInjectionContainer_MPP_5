using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System.Text;
using System.Collections.Concurrent;
using System.Reflection;

namespace DependencyInjectionContainerLib
{
    public class DependencyProvider
    {
        private DependenciesConfiguration dependenciesConfiguration;
        private ConcurrentDictionary<Type, object> singleton = new ConcurrentDictionary<Type, object>();
        public DependencyProvider(DependenciesConfiguration dependenciesConfiguration)
        {
            this.dependenciesConfiguration = dependenciesConfiguration;
        }

        public T Resolve<T>(Enum name = null)
        {
            return (T)Resolve(typeof(T),Convert.ToInt32(name));
        }

        private object Resolve(Type Dependency,int name=0)
        {
            InterfaceImplementation implementation = null;
            if (typeof(IEnumerable).IsAssignableFrom(Dependency))
            {
                Type type = Dependency.GetGenericArguments()[0];
                var implementations = Array.CreateInstance(type, dependenciesConfiguration.InterfaceDependencies[type].Count);
                for (int i = 0; i < implementations.Length; i++)
                {
                    var obj = CreateImplementation(dependenciesConfiguration.InterfaceDependencies[type][i]);
                    implementations.SetValue(obj, i);
                }
                return implementations;
            }
            if (Dependency.GenericTypeArguments.Length != 0)
            {
                if (!dependenciesConfiguration.InterfaceDependencies.ContainsKey(Dependency) && dependenciesConfiguration.InterfaceDependencies.ContainsKey(Dependency.GetGenericTypeDefinition())) 
                {
                    Type type = dependenciesConfiguration.InterfaceDependencies[Dependency.GetGenericTypeDefinition()][name].ImplementationClass.MakeGenericType(Dependency.GetGenericArguments()[0]);
                    implementation = new InterfaceImplementation(type, dependenciesConfiguration.InterfaceDependencies[Dependency.GetGenericTypeDefinition()][name].TimeToLive, 0);
                }
            }
            if (implementation == null)
            {
                implementation = dependenciesConfiguration.InterfaceDependencies[Dependency][name];
            }

            return CreateImplementation(implementation);
        }
        private object CreateImplementation(InterfaceImplementation implementation)
        {
            if (singleton.ContainsKey(implementation.ImplementationClass))
            {
                return singleton[implementation.ImplementationClass];
            }
            ConstructorInfo[] constructors = implementation.ImplementationClass.GetConstructors();
            ConstructorInfo constructor = constructors[0];
            ParameterInfo[] parameters = constructor.GetParameters();
            object[] parametersObj = new object[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                DependencyKey atr = parameters[i].GetCustomAttribute<DependencyKey>();
                if (atr == null)
                {
                    parametersObj.SetValue(Resolve(parameters[i].ParameterType, implementation.NameOfDepend), i);
                }
                else
                {
                    parametersObj.SetValue(Resolve(parameters[i].ParameterType, atr.NameOfDepend), i);
                }
            }
            var obj = Activator.CreateInstance(implementation.ImplementationClass, parametersObj);
            if (implementation.TimeToLive == TimeToLive.Singleton)
            {
                singleton.TryAdd(implementation.ImplementationClass, obj);
            }
            return obj;
        }
    }
}
