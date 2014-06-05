namespace TeaDriven.Graphology.Tests

open Xunit
open Xunit.Extensions
open Ploeh.AutoFixture.Idioms
open FsUnit.Xunit

module DefaultGetDepthStringTests =
    open TeaDriven.Graphology.Visualization

    [<Theory; AutoFoqData>]
    let ``Constructors should have null guards`` (assertion : GuardClauseAssertion) =
        assertion.Verify (typeof<DefaultGetDepthString>.GetConstructors())

    [<Theory; AutoFoqData>]
    let ``Class should implement IGetDepthString`` (sut : DefaultGetDepthString) =
        Assert.IsAssignableFrom<IGetDepthString> sut

    [<Theory>]
    [<InlineAutoFoqData(-1, "")>]
    [<InlineAutoFoqData(0, "")>]
    [<InlineAutoFoqData(1, "> ")>]
    [<InlineAutoFoqData(2, "    > ")>]
    [<InlineAutoFoqData(3, "        > ")>]
    let ``For() returns true and correct name`` (depth : int, expectedResult : string, sut : DefaultGetDepthString) =
        let result = sut.For depth

        result |> should equal expectedResult