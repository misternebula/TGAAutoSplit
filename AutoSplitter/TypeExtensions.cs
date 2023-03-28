using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AutoSplitter
{
	public static class TypeExtensions
	{
		private const BindingFlags Flags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static;

		public static MemberInfo GetAnyMember(this Type type, string name) =>
			type.GetMember(name, Flags).FirstOrDefault() ??
			type.BaseType?.GetMember(name, Flags).FirstOrDefault() ??
			type.BaseType?.BaseType?.GetMember(name, Flags).FirstOrDefault();

		public static T GetValue<T>(this object obj, string name)
		{
			switch (obj.GetType().GetAnyMember(name))
			{
				case FieldInfo field:
					return (T)field.GetValue(obj);
				case PropertyInfo property:
					return (T)property.GetValue(obj, null);
			}
			return default;
		}

		public static void SetValue(this object obj, string name, object value)
		{
			switch (obj.GetType().GetAnyMember(name))
			{
				case FieldInfo field:
					field.SetValue(obj, value);
					break;
				case PropertyInfo property:
					property.SetValue(obj, value, null);
					break;
			}
		}
	}
}
