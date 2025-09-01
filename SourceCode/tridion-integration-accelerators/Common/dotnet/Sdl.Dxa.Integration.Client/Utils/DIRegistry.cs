using System;
using System.Collections.Generic;
using System.Linq;

namespace Sdl.Dxa.Modules.Crm
{
    /// <summary>
    /// Simple non-framework specific dependency injection implementation
    /// </summary>
    public class DIRegisty
    {        
        private static IDictionary<Type, IList<object>> _implementations = new Dictionary<Type, IList<object>>();

        public static void Register<T>(object implementation)
        {
            IList<object> list;
            if (!_implementations.TryGetValue(typeof(T), out list))
            {
                list = new List<object>();
                _implementations.Add(typeof(T), list);
            }

            if (!list.Contains(implementation))
            {
                list.Add(implementation);
            }
        }

        /// <summary>
        /// Get implementation (singleton) of specified type. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static T Get<T>()
        {
            var list = GetList<T>();
            if (list.Count == 1)
            {
                return list[0];
            }
            else if (list.Count > 1)
            {
                throw new Exception("No qualifying implementation of type '" + typeof(T) + "'. Found " + list.Count + " implementations.");
            }

            return default(T);
        }

        /// <summary>
        /// Get all available implementations of specified type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IList<T> GetList<T>()
        {
            IList<object> implList;
            if (_implementations.TryGetValue(typeof(T), out implList))
            {
                return implList.Cast<T>().ToList();
            }

            return Enumerable.Empty<T>().ToList();
        }
    }
}