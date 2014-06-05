namespace TeaDriven.Graphology.Tests

open Xunit
open Xunit.Extensions
open Ploeh.AutoFixture.Idioms
open FsUnit.Xunit

module DefaultGetNextLetterCodeTests =
    open TeaDriven.Graphology.Visualization

    [<Theory; AutoFoqData>]
    let ``Constructors should have null guards`` (assertion : GuardClauseAssertion) =
        assertion.Verify (typeof<DefaultGetNextLetterCode>.GetConstructors())

    [<Theory; AutoFoqData>]
    let ``Class should implement IGetNextLetterCode`` (sut : DefaultGetNextLetterCode) =
        Assert.IsAssignableFrom<IGetNextLetterCode> sut

    [<Theory; AutoFoqData>]
    let ``From() should have null guards`` (assertion: GuardClauseAssertion) =
        assertion.Verify (typeof<DefaultGetNextLetterCode>.GetMethod "From")

    [<Theory>]
    [<InlineAutoFoqData("A", "B")>]
    [<InlineAutoFoqData("M", "N")>]
    [<InlineAutoFoqData("Z", "AA")>]
    [<InlineAutoFoqData("AA", "AB")>]
    [<InlineAutoFoqData("AZ", "BA")>]
    [<InlineAutoFoqData("ZZ", "AAA")>]
    [<InlineAutoFoqData("AAAZ", "AABA")>]
    let ``For() returns correct next letter code`` (oldCode : string, expectedResult : string, sut : DefaultGetNextLetterCode) =
        let result = sut.From oldCode

        result |> should equal expectedResult