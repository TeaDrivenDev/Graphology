using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
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

            var graphologist = new Graphologist();

            // Act
            var graph = graphologist.Graph(grah);

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