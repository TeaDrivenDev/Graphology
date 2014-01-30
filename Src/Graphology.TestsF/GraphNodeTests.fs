namespace TeaDriven.Graphology.Tests

open Xunit
open Xunit.Extensions
open Ploeh.AutoFixture
open Ploeh.AutoFixture.Idioms
open FsUnit.Xunit

module GraphNodeTests =
    open TeaDriven.Graphology
    open TeaDriven.Graphology.Tests.Extensions

    [<Theory; AutoFoqData>]
    let ``Constructors should have null guards`` (assertion : GuardClauseAssertion) =
        assertion.Verify (typeof<GraphNode>.GetConstructors())

    [<Theory; AutoFoqData>]
    let ``Properties should be writable`` (assertion : WritablePropertyAssertion, fixture : IFixture) =
        fixture.MakeNonRecursive()
        assertion.Verify typeof<GraphNode>

    [<Theory; AutoFoqData>]
    let ``Subgraph should be empty when not set`` (fixture : IFixture) =
        fixture.MakeNonRecursive()
        fixture.Customize(new NoAutoPropertiesCustomization(typeof<GraphNode>))

        let sut = fixture.Create<GraphNode>()

        Assert.Empty(sut.SubGraph)