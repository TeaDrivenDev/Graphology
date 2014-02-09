namespace TeaDriven.Graphology.Tests

open Xunit
open Xunit.Extensions
open Ploeh.AutoFixture
open Ploeh.AutoFixture.Xunit
open Ploeh.AutoFixture.Idioms
open FsUnit.Xunit

module RootNamespaceTypeExclusionTests =
    open System
    open System.Collections.Generic
    open TeaDriven.Graphology.Traversal
    open Foq

    [<Theory; AutoFoqData>]
    let ``Constructors should have null guards`` (assertion : GuardClauseAssertion) =
        assertion.Verify (typeof<RootNamespaceTypeExclusion>.GetConstructors())

    [<Theory; AutoFoqData>]
    let ``Class should implement ITypeExclusion`` (sut : RootNamespaceTypeExclusion) =
        Assert.IsAssignableFrom<ITypeExclusion> sut

    [<Theory; AutoFoqData>]
    let ``Namespace should be constructor initialized`` (assertion : ConstructorInitializedMemberAssertion) =
        assertion.Verify (typeof<RootNamespaceTypeExclusion>.GetProperty("Namespace"))

    [<Theory; AutoFoqData>]
    let ``AppliesTo() should have null guards`` (assertion : GuardClauseAssertion) =
        assertion.Verify(typeof<RootNamespaceTypeExclusion>.GetMethod("AppliesTo"))

    [<Theory>]
    [<InlineAutoFoqData("System.IO", typeof<System.IO.FileInfo>, true)>]
    [<InlineAutoFoqData("System.IO", typeof<System.IO.Compression.DeflateStream>, true)>]
    [<InlineAutoFoqData("System", typeof<String>, true)>]
    [<InlineAutoFoqData("System", typeof<String[]>, true)>]
    [<InlineAutoFoqData("System.IO", typeof<TeaDriven.Graphology.IGraphologist>, false)>]
    let ``AppliesTo() returns true for types in the given namespace and below, false for others``
        (``namespace``, ``type``, expectedResult) =

        let sut = RootNamespaceTypeExclusion ``namespace``

        let result = sut.AppliesTo ``type``

        Assert.Equal(expectedResult, result)

module TypeExclusionsDingsTests =
    open System
    open System.Collections.Generic
    open TeaDriven.Graphology.Traversal
    open Foq

    [<Theory; AutoFoqData>]
    let Dings() =
        let sut = TestTypeExclusions(Exclude=CompositeTypeExclusion(ExactNamespaceTypeExclusion "System"))

        let result = sut.Exclude.AppliesTo typeof<String>

        result |> should be True