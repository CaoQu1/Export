using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Export.Common
{
    /// <summary>
    /// 服务注册类
    /// </summary>
    public class ServiceRegistry
    {
        /// <summary>
        /// 服务类型
        /// </summary>
        public Type ServiceType { get; }

        /// <summary>
        /// 服务的生命周期
        /// </summary>
        public LifecCycle LifecCycle { get; }

        /// <summary>
        /// 生成服务实例的委托
        /// </summary>
        public Func<ObjectDI, Type[], object> Factory { get; }

        /// <summary>
        /// 相同服务注册的服务信息链表
        /// </summary>
        internal ServiceRegistry Next { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="serviceType"></param>
        /// <param name="lifetime"></param>
        /// <param name="factory"></param>
        public ServiceRegistry(
            Type serviceType,
            LifecCycle lifetime,
            Func<ObjectDI, Type[], object> factory)
        {
            ServiceType = serviceType;
            LifecCycle = lifetime;
            Factory = factory;
        }

        /// <summary>
        /// 返回链表中所有的服务注册信息
        /// </summary>
        /// <returns></returns>
        internal IEnumerable<ServiceRegistry> AsEnumerable()
        {
            var list = new List<ServiceRegistry>();
            for (var self = this; self != null; self = self.Next)
            {
                list.Add(self);
            }
            return list;
        }
    }

    /// <summary>
    /// 服务提供接口
    /// </summary>
    public interface IServiceProvider
    {
        /// <summary>
        /// 返回服务实例
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        object GetService(Type serviceType);
    }

    /// <summary>
    /// DI类
    /// </summary>
    public class ObjectDI : IServiceProvider, IDisposable
    {
        internal ObjectDI _root;
        internal ConcurrentDictionary<Type, ServiceRegistry> _registries;
        private ConcurrentDictionary<ServiceRegistry, object> _services;
        private ConcurrentBag<IDisposable> _disposables;
        private volatile bool _disposed;

        public ObjectDI()
        {
            _registries = new ConcurrentDictionary<Type, ServiceRegistry>();
            _root = this;
            _services = new ConcurrentDictionary<ServiceRegistry, object>();
            _disposables = new ConcurrentBag<IDisposable>();
        }

        internal ObjectDI(ObjectDI parent)
        {
            _root = parent._root;
            _registries = _root._registries;
            _services = new ConcurrentDictionary<ServiceRegistry, object>();
            _disposables = new ConcurrentBag<IDisposable>();
        }

        private void EnsureNotDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("ObjectDI");
            }
        }

        public ObjectDI Register(ServiceRegistry registry)
        {
            EnsureNotDisposed();
            if (_registries.TryGetValue(registry.ServiceType,
                out var existing))
            {
                _registries[registry.ServiceType] = registry;
                registry.Next = existing;
            }
            else
            {
                _registries[registry.ServiceType] = registry;
            }
            return this;
        }

        private object GetServiceCore(
       ServiceRegistry registry,
       Type[] genericArguments)
        {
            var serviceType = registry.ServiceType;
            object GetOrCreate(
                ConcurrentDictionary<ServiceRegistry, object> services,
                ConcurrentBag<IDisposable> disposables)
            {
                if (services.TryGetValue(registry, out var service))
                {
                    return service;
                }
                service = registry.Factory(this, genericArguments);
                services[registry] = service;
                if (service != null)
                {
                    var disposable = service as IDisposable;
                    if (null != disposable)
                    {
                        disposables.Add(disposable);
                    }
                }
                return service;
            }

            switch (registry.LifecCycle)
            {
                case LifecCycle.Single:
                    return GetOrCreate(_root._services, _root._disposables);
                case LifecCycle.Scoped:
                    return GetOrCreate(_services, _disposables);
                default:
                    {
                        var service = registry.Factory(this, genericArguments);
                        var disposable = service as IDisposable;
                        if (null != disposable)
                        {
                            _disposables.Add(disposable);
                        }
                        return service;
                    }
            }
        }

        public object GetService(Type serviceType)
        {
            EnsureNotDisposed();
            if (serviceType == typeof(ObjectDI) ||
                serviceType == typeof(IServiceProvider))
            {
                return this;
            }

            ServiceRegistry registry;
            if (serviceType.IsGenericType &&
                serviceType.GetGenericTypeDefinition()
                == typeof(IEnumerable<>))
            {
                var elementType = serviceType
                    .GetGenericArguments()[0];
                if (!_registries.TryGetValue(elementType,
                    out registry))
                {
                    return Array.CreateInstance(elementType, 0);
                }

                var registries = registry.AsEnumerable();
                var services = registries.Select(it =>
                    GetServiceCore(it, new Type[0])).ToArray();
                Array array = Array.CreateInstance(elementType, services.Length);
                services.CopyTo(array, 0);
                return array;
            }

            if (serviceType.IsGenericType &&
                !_registries.ContainsKey(serviceType))
            {
                var definition = serviceType
                    .GetGenericTypeDefinition();
                return _registries.TryGetValue(definition, out registry)
                    ? GetServiceCore(registry, serviceType.GetGenericArguments())
                    : null;
            }
            return _registries.TryGetValue(serviceType, out registry)
                    ? GetServiceCore(registry, new Type[0])
                    : null;
        }

        public void Dispose()
        {
            _disposed = true;
            foreach (var disposable in _disposables)
            {
                disposable.Dispose();
            }
            while (!_disposables.IsEmpty)
            {
                _disposables.TryTake(out _);
            }
            _services.Clear();
        }
    }

    /// <summary>
    /// DI扩展
    /// </summary>
    public static class ObjectDIExtensions
    {
        public static ObjectDI Register(
            this ObjectDI cat,
            Type from,
            Type to,
            LifecCycle lifetime)
        {
            Func<ObjectDI, Type[], object> factory =
                (_, arguments) => Create(_, to, arguments);
            cat.Register(new ServiceRegistry(from, lifetime, factory));
            return cat;
        }

        public static ObjectDI Register<TFrom, TTo>(
            this ObjectDI cat,
            LifecCycle lifetime) where TTo : TFrom
            => cat.Register(typeof(TFrom), typeof(TTo), lifetime);

        public static ObjectDI Register<TServiceType>(
            this ObjectDI cat,
            TServiceType instance)
        {
            Func<ObjectDI, Type[], object> factory =
                (_, arguments) => instance;
            cat.Register(new ServiceRegistry(typeof(TServiceType),
                LifecCycle.Single, factory));
            return cat;
        }

        public static ObjectDI Register<TServiceType>(
           this ObjectDI cat,
           Func<ObjectDI, TServiceType> factory,
           LifecCycle lifetime)
        {
            cat.Register(new ServiceRegistry(
             typeof(TServiceType), lifetime, (_, arguments) => factory(_)));
            return cat;
        }

        public static bool HasRegistry<T>(this ObjectDI cat)
            => cat.HasRegistry(typeof(T));
        public static bool HasRegistry(this ObjectDI cat, Type serviceType)
            => cat._root._registries.ContainsKey(serviceType);

        private static object Create(
            ObjectDI cat,
            Type type,
            Type[] genericArguments)
        {
            if (genericArguments.Length > 0)
            {
                type = type.MakeGenericType(genericArguments);
            }
            var constructors = type
                .GetConstructors();
            if (constructors.Length == 0)
            {
                throw new InvalidOperationException($"Cannot create the instance of {type} which does not have an public constructor.");
            }
            var constructor = constructors.FirstOrDefault(
                it => it.GetCustomAttributes(false)
                .OfType<InjectionAttribute>().Any());
            constructor = constructor ?? constructors.First();
            var parameters = constructor.GetParameters();
            if (parameters.Length == 0)
            {
                return Activator.CreateInstance(type);
            }
            var arguments = new object[parameters.Length];
            for (int index = 0; index < arguments.Length; index++)
            {
                var parameter = parameters[index];
                var parameterType = parameter.ParameterType;
                if (cat.HasRegistry(parameterType))
                {
                    arguments[index] = cat.GetService(parameterType);
                }
                else if (parameter.HasDefaultValue)
                {
                    arguments[index] = parameter.DefaultValue;
                }
                else
                {
                    throw new InvalidOperationException($"Cannot create the instance of {type} whose constructor has non-registered parameter type(s)");
                }
            }
            return Activator.CreateInstance(type, arguments);
        }

        public static IEnumerable<T> GetServices<T>(this ObjectDI cat)
       => cat.GetService<IEnumerable<T>>();
        public static T GetService<T>(this ObjectDI cat)
            => (T)cat.GetService(typeof(T));
    }

    /// <summary>
    /// 注入特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Constructor)]
    public class InjectionAttribute : Attribute
    {

    }

    /// <summary>
    /// 生命周期
    /// </summary>
    public enum LifecCycle
    {
        Single,
        Transient,
        Scoped
    }
}
