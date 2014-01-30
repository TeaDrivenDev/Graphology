namespace TeaDriven.Graphology.Tests

//module Conventions =

open System
open System.Linq
open Xunit
open Xunit.Extensions
open NSubstitute
open Ploeh.AutoFixture
open Ploeh.AutoFixture.Xunit
open Ploeh.AutoFixture.AutoFoq
open Ploeh.AutoFixture.AutoNSubstitute

type AutoFoqDataAttribute () =
    inherit AutoDataAttribute ((new Fixture() :> IFixture).Customize(new AutoNSubstituteCustomization()))

[<AttributeUsage(AttributeTargets.Method, AllowMultiple = true)>]
// this is necessary because F# 3.0 does not inherit the AttributeUsage attribute
// http://stackoverflow.com/questions/12621301/why-does-f-not-allow-multiple-attributes-where-c-sharp-does
type InlineAutoFoqDataAttribute ([<ParamArray>] parameters : Object[]) =
    inherit InlineAutoDataAttribute (new AutoFoqDataAttribute(), parameters)

module Extensions =
    type IFixture with
        member fixture.MakeNonRecursive () =
            fixture.Behaviors.Remove(fixture.Behaviors.First(fun b -> b :? ThrowingRecursionBehavior))
            fixture.Behaviors.Add(new OmitOnRecursionBehavior())