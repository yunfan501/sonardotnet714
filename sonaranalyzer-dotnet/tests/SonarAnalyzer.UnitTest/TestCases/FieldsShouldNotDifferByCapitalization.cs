using System;

namespace Tests.TestCases
{
    public class Fruit
    {
        protected int flesh;
        private int flesh_color;
    }

    public class Raspberry : Fruit
    {
        private int fLeSh; // Noncompliant {{Rename this field; it may be confused with 'flesh' in 'Fruit'.}}
        private static int FLESH; // Noncompliant {{Rename this field; it may be confused with 'flesh' in 'Fruit'.}}
        private static int FLESH_COLOR; // Compliant, base class field is private
    }
}
