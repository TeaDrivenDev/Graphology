namespace TeaDriven.Graphology.Tests

open Xunit
open Xunit.Extensions
open Ploeh.AutoFixture
open Ploeh.AutoFixture.Xunit
open Ploeh.AutoFixture.Idioms
open FsUnit.Xunit

module CompositeGetTypeNameStringTests =
    open System
    open System.Collections.Generic
    open TeaDriven.Graphology.Visualization
    open Foq

    [<Theory; AutoFoqData>]
    let ``Constructors should have null guards`` (assertion : GuardClauseAssertion) =
        assertion.Verify (typeof<CompositeGetTypeNameString>.GetConstructors())

    [<Theory; AutoFoqData>]
    let ``Class should implement IGetTypeNameString`` (sut : CompositeGetTypeNameString) =
        Assert.IsAssignableFrom<IGetTypeNameString> sut

    [<Theory; AutoFoqData>]
    let ``For() should have null guards`` (sut : CompositeGetTypeNameString) =
        Assert.Throws<ArgumentNullException>(fun () -> sut.For null |> ignore)

    [<Theory; AutoFoqData>]
    let ``For() with no inner instances returns false`` (fixture : IFixture) =
        fixture.Inject<IEnumerable<IGetTypeNameString>> (new List<IGetTypeNameString>())

        let sut = fixture.Create<CompositeGetTypeNameString>()

        let result, resultName = sut.For typeof<AutoFoqDataAttribute>

        result |> should be False

    [<Theory; AutoFoqData>]
    let ``For() returns false if no inner instance handles`` (fixture : IFixture) =
        let innerInstances =
            [for i in 1..3 ->
                Mock<IGetTypeNameString>().SetupByName("For").Returns((true, "lol")).Create()
//                { new IGetTypeNameString with
//                    member __.For(t, name) =
//                        name <- ""
//                        false
//                }
            ]

        fixture.Inject<IEnumerable<IGetTypeNameString>> innerInstances

        let sut = fixture.Create<CompositeGetTypeNameString>()

        let result, resultName = sut.For typeof<string>

        result |> should be True
        Assert.Equal<string>("lol", resultName)