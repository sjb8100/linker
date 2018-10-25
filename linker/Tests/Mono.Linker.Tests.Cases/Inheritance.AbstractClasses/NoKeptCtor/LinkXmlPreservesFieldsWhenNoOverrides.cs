using Mono.Linker.Tests.Cases.Expectations.Assertions;

namespace Mono.Linker.Tests.Cases.Inheritance.AbstractClasses.NoKeptCtor {
	public class LinkXmlPreservesFieldsWhenNoOverrides {
		public static void Main ()
		{
		}

		abstract class Base {
		}

		[Kept]
		class Bar : Base {
			public void OtherMethod ()
			{
			}
		}
	}
}