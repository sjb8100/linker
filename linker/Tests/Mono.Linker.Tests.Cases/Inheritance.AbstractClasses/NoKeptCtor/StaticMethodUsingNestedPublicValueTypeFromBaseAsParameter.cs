using Mono.Linker.Tests.Cases.Expectations.Assertions;

namespace Mono.Linker.Tests.Cases.Inheritance.AbstractClasses.NoKeptCtor {
	public class StaticMethodUsingNestedPublicValueTypeFromBaseAsParameter {
		public static void Main ()
		{
			StaticMethodOnlyUsed.StaticMethod ();
		}

		[Kept]
		abstract class Base {
			[Kept]
			public struct NestedType {
				public void Foo ()
				{
				}
			}
		}

		[Kept]
		class StaticMethodOnlyUsed : Base {
			[Kept]
			public static void StaticMethod ()
			{
				Helper (default (NestedType));
			}

			[Kept]
			private static void Helper (NestedType arg)
			{
			}
		}
	}
}