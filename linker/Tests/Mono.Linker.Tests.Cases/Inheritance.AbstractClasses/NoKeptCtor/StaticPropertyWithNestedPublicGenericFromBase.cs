using Mono.Linker.Tests.Cases.Expectations.Assertions;

namespace Mono.Linker.Tests.Cases.Inheritance.AbstractClasses.NoKeptCtor {
	public class StaticPropertyWithNestedPublicGenericFromBase {
		public static void Main ()
		{
			StaticMethodOnlyUsed.StaticMethod ();
		}

		[Kept]
		abstract class Base {
			[Kept]
			public class NestedType {
				public void Foo ()
				{
				}
			}
		}

		[Kept]
		class StaticMethodOnlyUsed : Base {
			[Kept]
			private static Container<NestedType> Property { get; [Kept] set; }
			
			[Kept]
			public static void StaticMethod ()
			{
				Property = null;
			}

			[Kept]
			class Container<T> {
			}
		}
	}
}