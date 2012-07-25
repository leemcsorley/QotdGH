using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Qotd.Data
{
    public static class Denormaliser
    {
        private static Dictionary<Type, Tuple<PropertyInfo, PropertyInfo[]>[]> CACHE = new Dictionary<Type, Tuple<PropertyInfo, PropertyInfo[]>[]>();

        public static void Denormalise<T>(T obj)
        {
            lock (CACHE)
            {
                if (!CACHE.ContainsKey(typeof(T)))
                {
                    PropertyInfo[] props = typeof(T).GetProperties().Where(p => p.Name.StartsWith("denorm")).ToArray();

                    List<Tuple<PropertyInfo, PropertyInfo[]>> plist = new List<Tuple<PropertyInfo, PropertyInfo[]>>();
                    foreach (var p in props)
                    {
                        List<PropertyInfo> list = new List<PropertyInfo>();
                        string[] split = p.Name.Split('_');
                        Type type = typeof(T);
                        for (int i = 1; i < split.Length; i++)
                        {
                            var p1 = type.GetProperty(split[i]);
                            list.Add(p1);
                            type = p1.PropertyType;
                        }
                        plist.Add(
                            new Tuple<PropertyInfo, PropertyInfo[]>(p, list.ToArray()));
                    }
                    CACHE.Add(typeof(T), plist.ToArray());
                }
                
                var ps = CACHE[typeof(T)];

                foreach (var p in ps)
                {
                    object val = obj;
                    foreach (var p1 in p.Item2)
                    {
                        if (val == null) break;
                        val = p1.GetValue(val, null);
                    }
                    if (val != null)
                        p.Item1.SetValue(obj, val, null);
                }
            }
        }
    }
}
