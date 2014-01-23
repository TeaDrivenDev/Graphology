Graphology
==========

SOLID application code consists of many rather small classes mutually hidden behind abstractions (usually interfaces in .NET). This makes them easily maintainable, extendable and testable, but bears the disadvantage that it is often hard to tell which specific implementation of an interface the current code will collaborate with in the final application.  

**Graphology** is a small library that can traverse a SOLID object graph and create a textual representation of it to visualize its structure and concrete implementations used.


Usage is currently manual: In the unit test project for your production code, create a test method like this (runnable [here](https://github.com/TeaDrivenDev/Graphology/blob/master/Src/Graphology.Tests/_InsightTests.cs)):

	[Fact]
	public void WriteGraphologistGraph()
	{
	    // Arrange
	    var projectPath = @"..\Graphology";
	    var graphName = "DefaultGraphVisualization";
	
	    var graphologist = new Graphologist(new MinimalTypeExclusions()
	                                        {
	                                            Exclude = new ExactNamespaceTypeExclusion("System")
	                                        });
	
		// This would be the evaluation of your composition root, like an IoC container resolution.
	    var target = new DefaultGraphVisualization();
	
	    // Act
	    graphologist.WriteGraph(target, projectPath, graphName);
	
	    // Assert
	}


The `target` variable is assigned the object that you want to graph, and the result is written to the folder given in `projectPath` (relative to the test project folder) with the name set in `graphName` suffixed with "_Graph.txt". If you add this file to the production project (which currently also is a one-time manual step), it will always be available there and updated whenever you re-run the test method (**ReSharper** works for this, and I suppose the **xUnit test runner** will do it as well; continuous test runners like **NCrunch** run the tests from their own temporary folders, so the relative path will point into Nirvana and the test will fail - exclude such tests in NCrunch). 


The result of the above test looks like this (note that there is a recursion at `LazyGetTypeNameString`; **Graphology** will recognize this and stop further traversal of that branch):

	 > DefaultGraphVisualization : IGraphVisualization
	    > DefaultGetNodeString : IGetNodeString
	       > DefaultGetDepthString : IGetDepthString
	       > DefaultGetMemberTypesString : IGetMemberTypesString
	          > LazyGetTypeNameString : IGetTypeNameString
	             > CompositeGetTypeNameString : IGetTypeNameString
	                > IGetTypeNameString[] : IEnumerable<IGetTypeNameString>
	                   > RecursiveGenericTypeGetTypeNameString : IGetTypeNameString
	                      > LazyGetTypeNameString : IGetTypeNameString
	                   > DefaultGetTypeNameString : IGetTypeNameString
	
	
Despite the work involved in setting it up, it is already useful, especially if you work with larger SOLID object graphs that change a lot or make use of patterns like Composite and Decorator.


The `Graphologist` class serves as a facade for easier usage, but the code is completely written using the SOLID principles and can be composed in any way necessary.  

The `TypeExclusions` class and its derivatives are used to completely exclude certain types completely or keep their instances from being traversed into. This can be necessary for example because certain types (like the primitive types in the `System` namespace) cannot be further traversed into using Reflection, or because it is not helpful to see the internal object graphs of third party library classes.


**Graphology**'s API was massively influenced by [AutoFixture](https://github.com/AutoFixture/AutoFixture); the build process was taken from that project as well.  

All **Graphology** code currently resides in a single file to allow for easily including it in any project without having to reference a DLL. It is not quite certain that makes a lot of sense given the additionally necessary reference to **Fasterflect**. 


Current status:
**Graphology** is usable for its originally intended purpose, although still rather 'bare bones'. It is not a good example of TDD as many parts were spiked, so test coverage is currently rather low. There is also no XML documentation on the classes.

Ideally, this will at some point become something like a Visual Studio or (more likely) ReSharper plugin updating live from the composition root and enabling navigation to individual classes right from the graph tree.

<br />
<br />
This document was created with MarkdownPad.