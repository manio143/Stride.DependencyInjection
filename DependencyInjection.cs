using Microsoft.Extensions.DependencyInjection;
using Stride.Core;
using Stride.Engine;
using System;
using System.Reflection;

namespace Stride.DependencyInjection
{
    /// <summary>
    /// Marks IoC injectable dependencies.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class InjectAttribute : Attribute
    {
        /// <summary>
        /// If true and the dependency cannot be resolved an exception will be thrown.
        /// </summary>
        public bool Required { get; set; } = false;
    }

    /// <summary>
    /// Marks classes that get their dependecies injected when placed as a member of a class that calls InjectDependencies from <see cref="DependencyExtensions"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class DependencyInjectableAttribute : Attribute
    { }

    public static class DependencyExtensions
    {
        /// <summary>
        /// Uses <paramref name="dependencyService"/> to inject dependencies into fields
        /// and properties marked with <see cref="InjectAttribute"/>.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="dependencyService"></param>
        public static void InjectDependencies(this object obj, DependencyService dependencyService)
        {
            InjectDependencies(dependencyService, obj);
        }

        /// <summary>
        /// Inject dependencies into fields and properties marked with <see cref="InjectAttribute"/>.
        /// </summary>
        /// <param name="script"></param>
        public static void InjectDependencies(this ScriptComponent script)
        {
            var dependencyService = script.Services.GetSafeServiceAs<DependencyService>();
            InjectDependencies(dependencyService, script);
        }

        private const BindingFlags bindingFlags =
            BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
        private static void InjectDependencies(DependencyService dependencyService, object obj)
        {
            var fields = obj.GetType().GetFields(bindingFlags);
            var properties = obj.GetType().GetProperties(bindingFlags);

            foreach (var field in fields)
            {
                var componentType = field.FieldType;
                InjectAttribute inj;

                if ((inj = field.GetCustomAttribute<InjectAttribute>()) != null)
                {
                    var injectedComponent = Resolve(dependencyService, componentType, inj.Required);

                    field.SetValue(obj, injectedComponent);
                }
                else if (componentType.GetCustomAttribute<DependencyInjectableAttribute>() != null
                    && field.GetValue(obj) != null)
                {
                    InjectDependencies(dependencyService, field.GetValue(obj));
                }
            }
            foreach (var prop in properties)
            {
                var componentType = prop.PropertyType;
                InjectAttribute inj;

                if ((inj = prop.GetCustomAttribute<InjectAttribute>()) != null)
                {
                    var injectedComponent = Resolve(dependencyService, componentType, inj.Required);

                    prop.SetValue(obj, injectedComponent);
                }
                else if (componentType.GetCustomAttribute<DependencyInjectableAttribute>() != null
                   && prop.GetValue(obj) != null)
                {
                    InjectDependencies(dependencyService, prop.GetValue(obj));
                }
            }
        }

        private static object Resolve(DependencyService dependencyService, Type componentType, bool required)
        {
            if (required)
            {
                return typeof(ServiceProviderServiceExtensions)
                    .GetMethod(nameof(ServiceProviderServiceExtensions.GetRequiredService))
                    .Invoke(null, new object[] { dependencyService.ServiceProvider, componentType });
            }
            else
            {
                var provider = dependencyService.ServiceProvider;
                return provider.GetType()
                    .GetMethod(nameof(provider.GetService))
                    .Invoke(provider, new object[] { componentType });
            }
        }
    }
}
