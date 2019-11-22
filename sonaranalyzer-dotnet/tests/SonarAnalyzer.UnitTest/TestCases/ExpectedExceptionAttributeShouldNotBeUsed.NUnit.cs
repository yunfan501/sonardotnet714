﻿using System;
using NUnit.Framework;

namespace Tests.Diagnostics
{
    [TestFixture]
    class Program
    {
        [Test]
        [NUnit.Framework.ExpectedException(typeof(ArgumentNullException))]  // Noncompliant
        public void TestFoo2()
        {
            var x = true;
            x.ToString();
        }

        [Test]
        [NUnit.Framework.ExpectedException(typeof(ArgumentNullException))]  // Compliant - one line
        public void TestFoo4()
        {
            new object().ToString();
        }

        [Test]
        [NUnit.Framework.ExpectedException(typeof(ArgumentNullException))]  // Compliant - one line
        public string TestFoo6() => new object().ToString();

        [Test]
        public void TestFoo8()
        {
            bool callFailed = false;
            try
            {
                //...
            }
            catch (ArgumentNullException)
            {
                callFailed = true;
            }
        }
    }
}
