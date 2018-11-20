using System;
using Xunit;

namespace CoreCLR.UnitTest
{
    public class AllocationsTests
    {
        ///////////////////////////////////////////////////////////////////////
        // Listing 15-10
        [Fact]
        public void SampleTest()
        {
            // Arrange
            string input = "Hello world!";
            var startAllocations = GC.GetAllocatedBytesForCurrentThread();

            // Act
            ReadOnlySpan<char> span = input.AsSpan().Slice(0, 5);

            // Assert
            var endAllocations = GC.GetAllocatedBytesForCurrentThread();

            Assert.Equal(startAllocations, endAllocations);
            Assert.Equal("Hello", span.ToString());
        }
    }
}
