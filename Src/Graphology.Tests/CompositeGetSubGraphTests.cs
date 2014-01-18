//using NSubstitute;
//using Ploeh.AutoFixture;
//using Ploeh.AutoFixture.Idioms;
//using Ploeh.AutoFixture.Xunit;
//using System;
//using System.Collections.Generic;
//using TeaDriven.Graphology.Tests.Conventions;
//using Xunit;
//using Xunit.Extensions;

//namespace TeaDriven.Graphology.Tests
//{
//    public class CompositeGetSubGraphTests
//    {
//        [Theory, AutoNSubstituteData]
//        public void Constructor_HasNullGuards(GuardClauseAssertion assertion)
//        {
//            assertion.Verify(typeof(CompositeGetSubGraph).GetConstructors());
//        }

//        [Theory, AutoNSubstituteData]
//        public void Constructor_ReturnsIGetSubGraph(CompositeGetSubGraph sut)
//        {
//            Assert.IsAssignableFrom<IGetSubGraph>(sut);
//        }

//        [Theory(Skip = "Will always fail because of the out parameter"), AutoNSubstituteData]
//        public void For_HasNullGuards(GuardClauseAssertion assertion)
//        {
//            assertion.Verify(typeof(CompositeGetSubGraph).GetMethod("For"));
//        }

//        [Theory, AutoNSubstituteData]
//        public void For_EmptyInnerList_ReturnsFalseAndEmptyString(IFixture fixture, object dings, Type dingsType, int depth, IEnumerable<object> graphPath)
//        {
//            // Arrange
//            fixture.Inject<IEnumerable<IGetSubGraph>>(new List<IGetSubGraph>());

//            var sut = fixture.Create<CompositeGetSubGraph>();

//            var expectedGraph = "";
//            var expectedResult = false;

//            // Act
//            string resultGraph;
//            var result = sut.For(dings, dingsType, depth, graphPath, out resultGraph);

//            // Assert
//            Assert.Equal(expectedGraph, resultGraph);
//            Assert.Equal(expectedResult, result);
//        }

//        [Theory, AutoNSubstituteData]
//        public void For_NoInnerInstanceHandles_ReturnsFalse(IFixture fixture,
//                                                            object dings,
//                                                            Type dingsType,
//                                                            int depth,
//                                                            IEnumerable<object> graphPath,
//                                                            [Frozen] IEnumerable<IGetSubGraph> innerInstances,
//                                                            [Frozen] CompositeGetSubGraph sut)
//        {
//            // Arrange
//            foreach (var innerInstance in innerInstances)
//            {
//                var graph = "";
//                innerInstance.For(dings, dingsType, depth, graphPath, out graph).Returns(x =>
//                                                                                         {
//                                                                                             x[4] = fixture.Create<string>();
//                                                                                             return false;
//                                                                                         });
//            }

//            var expectedResult = false;

//            // Act
//            string resultGraph;
//            var result = sut.For(dings, dingsType, depth, graphPath, out resultGraph);

//            // Assert
//            Assert.Equal(expectedResult, result);
//        }

//        [Theory, AutoNSubstituteData]
//        public void For_FirstInnerInstanceHandles_ReturnsTrueAndGraph(IFixture fixture,
//                                                                      object dings,
//                                                                      Type dingsType,
//                                                                      int depth,
//                                                                      IEnumerable<object> graphPath,
//                                                                      string expectedGraph,
//                                                                      IGetSubGraph firstInnerInstance,
//                                                                      [Frozen(As = typeof(IEnumerable<IGetSubGraph>))]  List<IGetSubGraph>
//                                                                          innerInstances,
//                                                                      [Frozen] CompositeGetSubGraph sut)
//        {
//            // Arrange
//            var outIn = Arg.Any<string>();

//            firstInnerInstance.For(dings, dingsType, depth, graphPath, out outIn).Returns(x =>
//                                                                                          {
//                                                                                              x[4] = expectedGraph;
//                                                                                              return true;
//                                                                                          });

//            foreach (var innerInstance in innerInstances)
//            {
//                var graph = Arg.Any<string>();
//                innerInstance.For(dings, dingsType, depth, graphPath, out graph).ReturnsForAnyArgs(x => { throw new Exception(); });
//            }

//            innerInstances.Insert(0, firstInnerInstance);

//            var expectedResult = true;

//            // Act
//            string resultGraph;
//            var result = sut.For(dings, dingsType, depth, graphPath, out resultGraph);

//            // Assert
//            Assert.Equal(expectedGraph, resultGraph);
//            Assert.Equal(expectedResult, result);
//        }

//        [Theory, AutoNSubstituteData]
//        public void For_LastInnerInstanceHandles_ReturnsTrueAndGraph(IFixture fixture,
//                                                                     object dings,
//                                                                     Type dingsType,
//                                                                     int depth,
//                                                                     IEnumerable<object> graphPath,
//                                                                     string expectedGraph,
//                                                                     IGetSubGraph lastInnerInstance,
//                                                                     [Frozen(As = typeof(IEnumerable<IGetSubGraph>))]  List<IGetSubGraph>
//                                                                         innerInstances,
//                                                                     [Frozen] CompositeGetSubGraph sut)
//        {
//            // Arrange
//            var outIn = Arg.Any<string>();

//            lastInnerInstance.For(dings, dingsType, depth, graphPath, out outIn).Returns(x =>
//                                                                                         {
//                                                                                             x[4] = expectedGraph;
//                                                                                             return true;
//                                                                                         });

//            foreach (var innerInstance in innerInstances)
//            {
//                var graph = "c";

//                var resGraph = fixture.Create<string>();

//                innerInstance.For(dings, dingsType, depth, graphPath, out graph).Returns(x =>
//                                                                                         {
//                                                                                             x[4] = resGraph;
//                                                                                             return false;
//                                                                                         });
//            }

//            innerInstances.Add(lastInnerInstance);

//            var expectedResult = true;

//            // Act
//            string resultGraph;
//            var result = sut.For(dings, dingsType, depth, graphPath, out resultGraph);

//            // Assert
//            Assert.Equal(expectedGraph, resultGraph);
//            Assert.Equal(expectedResult, result);
//        }
//    }
//}