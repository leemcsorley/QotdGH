using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Qotd.Data
{
    public static class Serializer
    {
        private static Dictionary<Type, Tuple<PropertyInfo, PropertyInfo>[]> CACHE = new Dictionary<Type, Tuple<PropertyInfo, PropertyInfo>[]>();

        public static void SerializeComplexProps<T>(T obj)
        {
            lock (CACHE)
            {
                if (!CACHE.ContainsKey(typeof(T)))
                {
                    List<Tuple<PropertyInfo, PropertyInfo>> list = new List<Tuple<PropertyInfo, PropertyInfo>>();
                    var allprops = typeof(T).GetProperties().ToArray();
                    var props = allprops.Where(p => p.Name.EndsWith("_Data")).ToArray();

                    if (props != null && props.Length > 0)
                    {
                        foreach (var p in props)
                        {
                            var p1 = allprops.Where(p2 => p2.Name == p.Name.Replace("_Data", "")).Single();

                            list.Add(new Tuple<PropertyInfo, PropertyInfo>(p, p1));
                        }
                    }
                    CACHE[typeof(T)] = list.ToArray();
                }
                var ps = CACHE[typeof(T)];

                foreach (var p in ps)
                {
                    throw new System.NotImplementedException();
                }
            }
        }
    }
}
