using Mono.Linker.Tests.Cases.Expectations.Assertions;
using Mono.Linker.Tests.Cases.Expectations.Metadata;
using Mono.Linker.Tests.Cases.References.Dependencies;

namespace Mono.Linker.Tests.Cases.References {
	[SetupLinkerAction ("copy", "copied")]
	[SetupCompileBefore ("library.dll", new [] {"Dependencies/AssemblyOnlyUsedByUsing_Lib.cs"})]
	
	// When csc is used, `copied.dll` will have a reference to `library.dll`
	[SetupCompileBefore ("copied.dll", new [] {"Dependencies/AssemblyOnlyUsedByUsing_Copied.cs"}, new [] {"library.dll"}, compilerToUse: "csc")]

	// Here to assert that the test is setup correctly to copy the copied assembly.  This is an important aspect of the bug
	[KeptMemberInAssembly ("copied.dll", typeof (AssemblyOnlyUsedByUsing_Copied), "Unused()")]
	
	// We need to keep library.dll because `copied.dll` references
	[KeptAssembly ("library.dll")]
	public class AssemblyOnlyUsedByUsingWithCsc {
		public static void Main ()
		{
			// Use something to keep the reference at compile time
			AssemblyOnlyUsedByUsing_Copied.UsedToKeepReference ();
		}
	}
}