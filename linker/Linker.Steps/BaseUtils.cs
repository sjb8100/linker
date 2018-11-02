using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;

namespace Mono.Linker.Steps
{
	// TODO by Mike : Clean up and find a better home
	static class BaseUtils
	{
		public static IEnumerable<TypeReference> AllGenericTypesFor (TypeReference type)
		{
			if (type is GenericInstanceType genericInstanceType)
			{
				// TODO : Nested?  Distinct?
				foreach (var generic in genericInstanceType.GenericArguments)
					yield return generic;
			}

//			yield break;
		}
		
		public static IEnumerable<TypeReference> AllGenericTypesFor (MethodReference method)
		{
			// TODO by Mike : Do I need this?  ProcessMethod is too late a point to use this.  When would I call this?
			if (method is GenericInstanceMethod genericInstanceMethod)
				throw new NotImplementedException();

			yield break;
		}
		
		public static List<TypeDefinition> CollectBases (TypeDefinition type, Action<TypeReference> handleUnresolved)
		{
			var bases = new List<TypeDefinition> ();
			var current = type.BaseType;

			while (current != null)
			{
				var resolved = current.Resolve ();
				if (resolved == null)
				{
					handleUnresolved (current);
					return null;
				}

				// Exclude Object.  We don't care about that
				if (resolved.BaseType == null)
					break;
				
				bases.Add (resolved);
				current = resolved.BaseType;
			}

			return bases;
		}
		
		public static bool IsTypeHierarchyRequiredFor (FieldReference field,  List<TypeDefinition> basesOfScope, TypeDefinition visibilityScope, Action<FieldReference> handleUnresolved)
		{
			var resolved = field.Resolve ();
			if (resolved == null) {
				handleUnresolved (field);
				return true;
			}
			
			var fromBase = basesOfScope.FirstOrDefault (b => resolved.DeclaringType == b);
			if (fromBase != null)
			{
				if (!resolved.IsStatic)
					return true;

				if (resolved.IsPublic)
					return false;

				// protected
				if (resolved.IsFamily)
					return true;

				// It must be internal.  Trust that if the compiler allowed it we can continue to access
				if (!resolved.IsPrivate)
					return false;
				
				return false;
			}
			
			if (IsTypeHierarchyRequiredForType (resolved.DeclaringType, basesOfScope, visibilityScope))
				return true;

			return false;
		}
		
		public static bool IsTypeHierarchyRequiredFor (MethodReference method,  List<TypeDefinition> basesOfScope, TypeDefinition visibilityScope, Action<MethodReference> handleUnresolved)
		{
			var resolved = method.Resolve ();
			if (resolved == null) {
				handleUnresolved (method);
				return true;
			}
			
			var fromBase = basesOfScope.FirstOrDefault (b => resolved.DeclaringType == b);
			if (fromBase != null)
			{
				if (!resolved.IsStatic)
					return true;

				if (resolved.IsPublic)
					return false;

				// protected
				if (resolved.IsFamily)
					return true;

				// It must be internal.  Trust that if the compiler allowed it we can continue to access
				if (!resolved.IsPrivate)
					return false;
				
				return false;
			}
			
			// If the method wasn't declared on a base type of the current body, then we need to check if any of the methods types parents are base types
			// of the body
			if (IsTypeHierarchyRequiredForType (resolved.DeclaringType, basesOfScope, visibilityScope))
				return true;

			return false;
		}
		
		public static bool IsTypeHierarchyRequiredFor (TypeReference type, List<TypeDefinition> basesOfScope, TypeDefinition visibilityScope, Action<TypeReference> handleUnresolved)
		{
			if(type.FullName.Contains("Mono.Linker"))
				Console.WriteLine();
			
			// TODO : Is there a way for a generic parameter to cause the base type to be needed?
			if (type is GenericParameter)
				throw new NotImplementedException();
			
			var resolved = type.Resolve ();
			if (resolved == null) {
				handleUnresolved (type);
			}

			if (IsTypeHierarchyRequiredForType (resolved, basesOfScope, visibilityScope))
				return true;

			return false;
		}
		
		public static bool IsTypeHierarchyRequiredForType (TypeDefinition memberType, List<TypeDefinition> basesOfBodyType, TypeDefinition visibilityScope)
		{
			var current = memberType;
			var parentsOfMemberType = new List<TypeDefinition> ();
			TypeDefinition foundBase = null;
			while (current != null) {
				foundBase = basesOfBodyType.FirstOrDefault (b => current == b);
				parentsOfMemberType.Add (current);
				if (foundBase != null) {
					break;
				}

				current = current.DeclaringType;
			}

			if (foundBase == null)
				return false;

			if (memberType.IsPublic)
				return false;

			if (memberType.IsNestedPublic) {
				return parentsOfMemberType.Any (p => p != foundBase && !p.IsNestedPublic);
			}

			return true;
		}
	}
}