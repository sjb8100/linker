using System;
using Mono.Linker.Tests.Cases.Expectations.Assertions;

namespace Mono.Linker.Tests.Cases.Inheritance.AbstractClasses.NoKeptCtor {
	public class UseStaticMethodWithCustomAttributeFromBaseType {
		public static void Main ()
		{
			StaticMethodOnlyUsed.StaticMethod ();
		}

		[Kept]
		abstract class Base {
			[Kept]
			[KeptMember (".ctor()")]
			[KeptBaseType (typeof (Attribute))]
			protected class BaseDefinedAttribute : Attribute
			{
			}
		}

		[Kept]
		class StaticMethodOnlyUsed : Base {
			[Kept]
			[KeptAttributeAttribute (typeof (BaseDefinedAttribute))]
			[BaseDefined]
			public static void StaticMethod ()
			{
			}
		}
	}
}