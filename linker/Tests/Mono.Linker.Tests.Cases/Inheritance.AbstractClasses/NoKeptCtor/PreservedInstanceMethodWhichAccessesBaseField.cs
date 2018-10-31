using System.Runtime.CompilerServices;
using Mono.Linker.Tests.Cases.Expectations.Assertions;

namespace Mono.Linker.Tests.Cases.Inheritance.AbstractClasses.NoKeptCtor {
	public class PreservedInstanceMethodWhichAccessesBaseField {
		public static void Main ()
		{
			StaticMethodOnlyUsed.MethodToTriggerInstanceMethodMark ();
		}

		[Kept]
		abstract class Base {
			// TODO : We should be able to remove this field
			[Kept]
			public int Field;
		}

		[Kept]
		[KeptBaseType (typeof (Base))] // TODO : Should be able to remove this
		class StaticMethodOnlyUsed : Base {
			
			// TODO : Technically the body doesn't need to be processed.  We could skip processing it and stub the method body with a throw instead
			[Kept]
			public void Foo ()
			{
				base.Field = 1;
			}

			[Kept]
			[PreserveDependency ("Foo")]
			public static void MethodToTriggerInstanceMethodMark ()
			{
			}
		}
	}
}