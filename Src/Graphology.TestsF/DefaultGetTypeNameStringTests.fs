namespace TeaDriven.Graphology.Tests

open Xunit
open Xunit.Extensions
open Ploeh.AutoFixture
open Ploeh.AutoFixture.Idioms
open FsUnit.Xunit

module DefaultGetTypeNameStringTests =
    open System
    open TeaDriven.Graphology.Visualization

    [<Theory; AutoFoqData>]
    let ``Constructors should have null guards`` (assertion : GuardClauseAssertion) =
        assertion.Verify (typeof<DefaultGetTypeNameString>.GetConstructors())

    [<Theory; AutoFoqData>]
    let ``Class should implement IGetTypeNameString`` (sut : DefaultGetTypeNameString) =
        Assert.IsAssignableFrom<IGetTypeNameString> sut

    [<Theory; AutoFoqData>]
    let ``For() should have null guards`` (sut : DefaultGetTypeNameString) =
        Assert.Throws<ArgumentNullException>(fun () -> sut.For null |> ignore)

    [<Theory; AutoFoqData>]
    let ``For() returns true and correct name`` (sut : DefaultGetTypeNameString) =
        let inputType = typeof<FactAttribute>

        let expectedName = "FactAttribute"

        let result, resultName = sut.For inputType

        result |> should be True
        resultName |> should equal expectedName