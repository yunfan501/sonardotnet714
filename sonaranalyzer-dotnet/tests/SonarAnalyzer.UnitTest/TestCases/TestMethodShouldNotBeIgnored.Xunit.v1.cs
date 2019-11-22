﻿using MSTest = Microsoft.VisualStudio.TestTools.UnitTesting;
using Xunit;
using Xunit.Extensions;

// Legacy xUnit (v1.9.1)

// Regression test for #1705: https://github.com/SonarSource/sonar-csharp/issues/1705
// The rule should not be applied to xUnit tests. Previously, the rule was raising if
// an MSTest [Ignore] attribute was applied to a xUnit test.

namespace Tests.Diagnostics
{
    [MSTest.Ignore()]
    public class XUnitClass
    {
        [Fact]
        [MSTest.Ignore()]
        public void Foo1()
        {
        }

        [Theory]
        [InlineData("")]
        [MSTest.Ignore]
        public void Foo2(string s)
        {
        }
    }
}
