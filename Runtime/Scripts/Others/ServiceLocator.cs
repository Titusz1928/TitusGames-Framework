using System;
using System.Collections.Generic;
using UnityEngine;
using static Codice.CM.Common.CmCallContext;

namespace TitusGames.Framework
{
    public class ServiceLocator
    {
        private readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

        private static ServiceLocator _current;

        /// <summary>
        /// Global access point to the locator. Auto-initializes if accessed before explicit initialization.
        /// </summary>
        public static ServiceLocator Current => _current ??= new ServiceLocator();

        public static void Initialize()
        {
            _current = new ServiceLocator();
        }

        /// <summary>
        /// Registers a service instance against its type.
        /// </summary>
        public void Register<T>(T service, bool overwrite = false)
        {
            Type type = typeof(T);

            if (_services.ContainsKey(type))
            {
                if (!overwrite)
                {
                    Debug.LogWarning($"[ServiceLocator] Service of type {type.Name} is already registered.");
                    return;
                }

                _services[type] = service;
                Debug.Log($"[ServiceLocator] Service of type {type.Name} was overwritten.");
                return;
            }

            _services.Add(type, service);
        }

        /// <summary>
        /// Resolves and returns the requested service. Throws an exception if missing or destroyed.
        /// </summary>
        public T Get<T>()
        {
            Type type = typeof(T);

            if (!TryGet<T>(out T service))
            {
                throw new InvalidOperationException($"[ServiceLocator] Service of type {type.Name} is not registered or has been destroyed!");
            }

            return service;
        }

        /// <summary>
        /// Safely attempts to retrieve a service without throwing exceptions.
        /// Handles Unity MonoBehaviour lifetime checks (destroyed object safety).
        /// </summary>
        public bool TryGet<T>(out T service)
        {
            Type type = typeof(T);

            if (_services.TryGetValue(type, out var rawService))
            {
                // Handle Unity lifetime checks: MonoBehaviours might be destroyed while dictionary still holds reference
                if (rawService is UnityEngine.Object unityObj && unityObj == null)
                {
                    _services.Remove(type); // Clean up stale reference
                    service = default;
                    return false;
                }

                service = (T)rawService;
                return true;
            }

            service = default;
            return false;
        }

        /// <summary>
        /// Gets an existing service or uses a factory delegate to create and register it on demand.
        /// </summary>
        public T GetOrRegister<T>(Func<T> factory) where T : class
        {
            if (TryGet<T>(out var existingService))
            {
                return existingService;
            }

            T newService = factory();
            Register<T>(newService);
            return newService;
        }

        /// <summary>
        /// Unregisters a service by its type.
        /// </summary>
        public void Unregister<T>()
        {
            Type type = typeof(T);
            if (_services.ContainsKey(type))
            {
                _services.Remove(type);
            }
        }

        /// <summary>
        /// Clears all registered services. Useful when resetting game state or swapping scenes.
        /// </summary>
        public void ClearAll()
        {
            _services.Clear();
        }
    }
}