using Stride.Engine;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Stride.DependencyInjection
{
    /// <summary>
    /// May be placed on a field or property of type derving from <see cref="EntityComponent"/> or IEnumerable&lt;<see cref="EntityComponent"/>&gt; and used with <see cref="InjectEntityComponentsExtension"/> to retrieve the dependent components.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class InjectComponentAttribute : Attribute
    { }

    /// <summary>
    /// Exception thrown when the Entity doesn't have a requested component (see also <seealso cref="InjectComponentAttribute"/>).
    /// </summary>
    [Serializable]
    public class NullEntityComponentException : Exception
    {
        public NullEntityComponentException() { }
        public NullEntityComponentException(string componentType)
            : base($"This component requires the entity to also have {componentType}") { }
    }

    public static class InjectEntityComponentsExtension
    {
        private const BindingFlags bindingFlags =
            BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;

        /// <summary>
        /// Search through all fields and properties of the component and inject dependent components based on <see cref="InjectComponentAttribute"/>.
        /// </summary>
        /// <param name="component"></param>
        public static void InjectComponents(this EntityComponent component)
        {
            var fields = component.GetType().GetFields(bindingFlags);
            var properties = component.GetType().GetProperties(bindingFlags);

            var getComponentMethod = component.Entity.GetType().GetMethod("Get", new Type[] { });
            var getAllComponentsMethod = component.Entity.GetType().GetMethod("GetAll", new Type[] { });

            foreach (var field in fields)
            {
                if (field.GetCustomAttribute<InjectComponentAttribute>() != null)
                {
                    object injectedComponent;

                    var componentType = field.FieldType;
                    if (typeof(IEnumerable<EntityComponent>).IsAssignableFrom(componentType))
                    {
                        injectedComponent = getAllComponentsMethod.MakeGenericMethod(componentType.GetGenericArguments()[0])
                            .Invoke(component.Entity, new object[] { });
                    }
                    else
                    {
                        injectedComponent = getComponentMethod.MakeGenericMethod(componentType)
                            .Invoke(component.Entity, new object[] { });
                    }

                    if (injectedComponent == null)
                        throw new NullEntityComponentException(componentType.Name);

                    field.SetValue(component, injectedComponent);
                }
            }
            foreach (var prop in properties)
            {
                if (prop.GetCustomAttribute<InjectComponentAttribute>() != null)
                {
                    object injectedComponent;

                    var componentType = prop.PropertyType;
                    if (typeof(IEnumerable<EntityComponent>).IsAssignableFrom(componentType))
                    {
                        injectedComponent = getAllComponentsMethod.MakeGenericMethod(componentType.GetGenericArguments()[0])
                            .Invoke(component.Entity, new object[] { });
                    }
                    else
                    {
                        injectedComponent = getComponentMethod.MakeGenericMethod(componentType)
                            .Invoke(component.Entity, new object[] { });
                    }

                    if (injectedComponent == null)
                        throw new NullEntityComponentException(componentType.Name);

                    prop.SetValue(component, injectedComponent);
                }
            }
        }
    }
}
