using Mono.Linker.Tests.Cases.Expectations.Assertions;

namespace Mono.Linker.Tests.Cases.Inheritance.AbstractClasses.NoKeptCtor {
	public class StaticMethodUsingNestedTypeFromBase {
		public static void Main ()
		{
			StaticMethodOnlyUsed.StaticMethod ();
		}

		[Kept]
		abstract class Base {
			[Kept]
			[KeptMember (".ctor()")]
			protected class NestedType {
				[Kept]
				public void Foo ()
				{
				}
			}
		}

		[Kept]
		[KeptBaseType (typeof (Base))]
		class StaticMethodOnlyUsed : Base {
			[Kept]
			public static void StaticMethod ()
			{
				new NestedType ().Foo ();
			}
		}
	}
}