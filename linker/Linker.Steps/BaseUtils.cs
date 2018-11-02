using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;

namespace Mono.Linker.Steps {
	// TODO by Mike : Clean up and find a better home
	static class BaseUtils {
		public static bool NeedToCheckTypeHierarchy (TypeDefinition visibilityScope, Action<TypeReference> handleUnresolved, out TypeDefinition[] basesOfScope)
		{
			basesOfScope = null;
			
			// We do not currently change the base type of value types
			if (visibilityScope.IsValueType)
				return false;

			basesOfScope = CollectBases (visibilityScope, handleUnresolved)?.ToArray ();

			// No need to do this for types derived from object.  It already has the lowest base class
			if (basesOfScope == null || basesOfScope.Length == 0)
				return false;

			return true;
		}
		
		public static IEnumerable<TypeReference> AllGenericTypesFor (TypeReference type)
		{
			if (type.FullName.Contains("Mono.Linker"))
				Console.WriteLine();
			
			if (type is IGenericInstance genericInstanceType) {
				foreach (var generic in AllGenericTypesFor (genericInstanceType)) {
					yield return generic;
				}
			}
		}
		
		public static IEnumerable<TypeReference> AllGenericTypesFor (MethodReference method)
		{
			// TODO by Mike : Do I need this?  ProcessMethod is too late a point to use this.  When would I call this?
			if (method is IGenericInstance genericInstanceMethod)
			{
				if (method.FullName.Contains("Mono.Linker"))
					Console.WriteLine();
				
				foreach (var generic in AllGenericTypesFor (genericInstanceMethod))
					yield return generic;
			}

			foreach (var generic in AllGenericTypesFor (method.DeclaringType))
				yield return generic;
		}
		
		private static IEnumerable<TypeReference> AllGenericTypesFor (IGenericInstance instance)
		{
			// TODO : Nested?  Distinct?
			foreach (var generic in instance.GenericArguments)
			{
				if (generic is GenericParameter)
					continue;

				yield return generic;
			}
		}

		public static IEnumerable<TypeReference> ConstraintsFor (TypeReference type)
		{
			if (type is IGenericInstance genericInstanceType) {
//				foreach (var generic in ConstraintsFor (genericInstanceType)) {
//					yield return generic;
//				}
				
				throw new NotImplementedException();
			}
			
			foreach (var genericParameter in type.GenericParameters)
			{
				foreach (var constraint in genericParameter.Constraints)
					yield return constraint;
			}
		}

		public static IEnumerable<TypeReference> ConstraintsFor (MethodReference method)
		{
			foreach (var genericParameter in method.GenericParameters)
			{
				foreach (var constraint in genericParameter.Constraints)
					yield return constraint;
			}
			
			if (method is IGenericInstance genericInstanceMethod) {
				throw new NotImplementedException();
			}
			
			foreach (var generic in ConstraintsFor (method.DeclaringType))
				yield return generic;
		}

		private static IEnumerable<TypeReference> ConstraintsFor (IGenericInstance instance)
		{
			foreach (var generic in instance.GenericArguments) {
				if (generic is GenericParameter parameter) {
					foreach (var constraint in parameter.Constraints)
						yield return constraint;
				}
			}
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
		
		public static bool IsTypeHierarchyRequiredFor (FieldReference field, TypeDefinition [] basesOfScope, TypeDefinition visibilityScope, Action<FieldReference> handleUnresolved)
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
		
		public static bool IsTypeHierarchyRequiredFor (MethodReference method,  TypeDefinition [] basesOfScope, TypeDefinition visibilityScope, Action<MethodReference> handleUnresolved)
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
		
		public static bool IsTypeHierarchyRequiredFor (TypeReference type, TypeDefinition [] basesOfScope, TypeDefinition visibilityScope, Action<TypeReference> handleUnresolved)
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
		
		public static bool IsTypeHierarchyRequiredForType (TypeDefinition memberType, TypeDefinition [] basesOfBodyType, TypeDefinition visibilityScope)
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