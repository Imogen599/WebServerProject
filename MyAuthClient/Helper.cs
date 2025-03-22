using System.Reflection;
using System.Runtime.CompilerServices;

namespace MyAuthClient
{
	public static class Helper
	{
		internal static List<TContent> LoadContentFromAssembly<TContent>(Assembly assembly) where TContent : class
		{
			var loadableTypes = assembly.GetTypes()
				.Where(t => !t.IsAbstract && !t.ContainsGenericParameters)
				.Where(t => t.IsAssignableTo(typeof(TContent)))
				.Where(t =>
				{
					// Has default constructor check.
					bool derivedHasConstructor = t.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, Type.EmptyTypes) != null;
					bool baseHasHasConstructor = t.BaseType.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, Type.EmptyTypes) != null;
					return derivedHasConstructor || baseHasHasConstructor;
				})
				.OrderBy(type => type.FullName, StringComparer.InvariantCulture);

			List<TContent> result = [];

			foreach (var type in loadableTypes)
				result.Add(RuntimeHelpers.GetUninitializedObject(type) as TContent);

			return result;
		}
	}
}
