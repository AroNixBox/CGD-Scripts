using System;
using System.Collections.Generic;

namespace Core.Runtime.Service {
    public static class ServiceLocator {

    static readonly Dictionary<Type, object> Services = new();
        public static void Register<T>(T service) where T : class {
            var type = typeof(T);
            if (Services.ContainsKey(type)) {
                UnityEngine.Debug.LogWarning($"Service {type.Name} is already registered. Overwriting.");
            }
            Services[type] = service;
        }

        /// <summary>
        /// Try to get a service without throwing an exception
        /// </summary>
        public static bool TryGet<T>(out T service) where T : class {
            var type = typeof(T);
            if (Services.TryGetValue(type, out var obj)) {
                service = obj as T;
                return true;
            }
            service = null;
            return false;
        }

        /// <summary>
        /// Unregister a service
        /// </summary>
        public static void Unregister<T>() where T : class {
            Services.Remove(typeof(T));
        }
    }
}
