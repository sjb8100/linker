using Mono.Linker.Tests.Cases.Expectations.Assertions;

namespace Mono.Linker.Tests.Cases.Inheritance.AbstractClasses.NoKeptCtor.LinkXml {
	public class PreservesOverriddenMethodOverrideOfUsedVirtualStillRemoved2 {
		public static void Main ()
		{
			Base j = new Jar ();
			j.One ();
		}

		[Kept]
		[KeptMember (".ctor()")]
		abstract class Base {
			[Kept]
			public abstract void Foo ();

			[Kept]
			public abstract void One ();
		}

		[Kept]
		[KeptMember (".ctor()")]
		[KeptBaseType (typeof (Base))]
		abstract class Base2 : Base
		{
			[Kept]
			public override void One ()
			{
			}
		}

		[Kept]
		// We have to keep the base type in this case because the link xml requested that all methods be kept.
		// Removing the base type with an override kept would produce invalid IL. 
		[KeptBaseType (typeof (Base2))]
		class Bar : Base2 {
			[Kept]
			public override void Foo ()
			{
			}

			public override void One ()
			{
			}
		}

		[Kept]
		[KeptMember (".ctor()")]
		[KeptBaseType (typeof (Base2))]
		class Jar : Base2 {
			[Kept]
			public override void Foo ()
			{
			}

			[Kept]
			public override void One ()
			{
			}
		}
	}
}