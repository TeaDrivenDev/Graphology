﻿using System.Collections.Generic;
using Xunit;

namespace TeaDriven.Graphology.Tests
{
    public class _InsightTests
    {
        [Fact]
        public void X()
        {
            // Arrange
            var grah = new Grah(new[] { new Grah2(), new Grah2() });

            var createGraphologist = new CreateGraphologist();

            var graphologist = createGraphologist.Now();

            // Act
            var graph = graphologist.Graph(grah);

            // Assert
        }

        [Fact]
        public void WriteGraphologistGraph()
        {
            // Arrange
            var projectPath = @"..\Graphology";
            var graphName = "Graphologist";

            var graphologist = new CreateGraphologist()
                               {
                                   TypeExclusions = new MinimalTypeExclusions()
                                                    {
                                                        Exclude = new ExactNamespaceTypeExclusion("System")
                                                    }
                               }.Now();

            // Act
            graphologist.WriteGraph(graphologist, projectPath, graphName);

            // Assert
        }
    }

    public class Grah
    {
        private readonly IEnumerable<Grah2> _grah2S;

        public Grah(IEnumerable<Grah2> grah2s)
        {
            _grah2S = grah2s;
        }
    }

    public class Grah2 { }
}