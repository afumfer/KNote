using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Model
{
    public static class ReflectionExtensions
    {
        public static List<FieldInfo> GetAllFields(Type type, BindingFlags flags)
        {
            if (type == typeof(object))
                return new List<FieldInfo>();

            // Get all fields recursively           
            List<FieldInfo> myList = GetAllFields(type.BaseType, flags);
            myList.AddRange(type.GetFields(flags));
            return myList;
        }

        public static Type GetElementTypeEnumerable<T>(object o)
        {
            var enumerable = o as IEnumerable<T>;
            if (enumerable == null)
                return null;

            Type[] interfaces = enumerable.GetType().GetInterfaces();

            return (from i in interfaces
                    where i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>)
                    select i.GetGenericArguments()[0]).FirstOrDefault();
        }

        public static bool IsEnumerable<T>(object o)
        {
            var enumerable = o as IEnumerable<T>;
            if (enumerable == null)
                return false;
            else
                return true;
        }

    }
}
