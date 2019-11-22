﻿using System;
using System.Linq;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    class ExceptionsShouldBeUsed
    {
        void NotCompliant()
        {
            new Exception(); // Noncompliant {{Throw this exception or remove this useless statement.}}
        }

        Exception Compliant()
        {
            var a = new Exception();
            var b = a = new Exception();
            field = new Exception();
            Property = new Exception();
            Foo(new Exception());
            if (new Exception() != null) { }
            return new Exception();
            throw new Exception();
        }

        void Foo(Exception ex) { }

        Exception CompliantArrow() => new Exception();

        Exception CompliantPropertyArrow => new Exception();

        void CompliantOut(out Exception e)
        {
            e = new Exception();
            Func<Exception> f = () => new Exception();
            var array = new[] { new Exception() };
        }

        IEnumerable<Exception> CompliantYield()
        {
            yield return new Exception();
        }

        Exception field;
        Exception Property { get; set; }
        void Method(Exception e) { }
    }
}
