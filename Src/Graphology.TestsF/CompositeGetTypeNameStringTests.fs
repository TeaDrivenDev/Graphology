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

    [<Fact>]
    let Test () =
        let fixture = new Fixture()
        fixture.Customize(new AutoFoq.AutoFoqCustomization())

        let sut = fixture.Create<IGetNodeString>()
        ()

    [<Fact>]
    let Test2 () =
        let mock = Mock<IGetNodeString>().Create()
        let expectNull = mock.For(null, 1)

        ()

//    [<Theory; AutoFoqData>]
//    let ``For() returns false if no inner instance handles`` (fixture : IFixture) =
//        let mutable name
//        let innerInstance = Mock<IGetTypeNameString>().SetupByName("For").Returns("sdjf")..Create()
//
//        let result, resultName = innerInstance.For typeof<IGetNodeString>
//
//        result |> should be False