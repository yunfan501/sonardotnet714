﻿namespace Tests.Diagnostics
{
    public class ExpressionBodyTest
    {
        void ConstantPattern(object o)
        {
            if (o == null)
            {
                if (o is null) // Noncompliant, always true
                {
                    o.ToString();
                }
                else
                { // Secondary, not executed code
                    o.ToString();
                }
            }
        }

        void ConstantPattern_NotIsNull(object o)
        {
            if (o == null)
            {
                if (!(o is null)) // Noncompliant, always false
                { // Secondary, not executed code
                    o.ToString();
                }
                else
                {
                    o.ToString();
                }
            }
        }

        void VariableDesignationPattern_Variable(object o)
        {
            if (o is string s)
            {
                if (s == null) // Noncompliant, always false
                { // Secondary, not executed code
                    s.ToString();
                }
            }
        }

        void VariableDesignationPattern_Source(object o)
        {
            // We can set NotNull constraint only for one of the variables in the if condition
            // and we choose the declared variable because it is more likely to have usages of
            // it inside the statement body.
            if (o is string s)
            {
                if (o == null) // Compliant, False Negative
                {
                    o.ToString();
                }
            }
        }

        void Patterns_In_Loops(object o, object[] items)
        {
            while (o is string s)
            {
                if (s == null) // Noncompliant, always false
                { // Secondary, not executed code
                    s.ToString();
                }
            }

            do
            {
                // The condition is evaluated after the first execution, so we cannot test s
            }
            while (o is string s);

            for (int i = 0; i < items.Length && items[i] is string s; i++)
            {
                if (s != null) // Noncompliant, always true
                {
                    s.ToString();
                }
            }
        }

        void Switch_Pattern_Source(object o)
        {
            switch (o)
            {
                case string s:
                    // We don't set constraints on the switch expression
                    if (o == null) // Compliant, we don't know anything about o
                    {
                        o.ToString();
                    }
                    break;

                default:
                    break;
            }
        }

        void Switch_Pattern(object o)
        {
            switch (o)
            {
                case string s:
                    if (s == null) // Noncompliant, always false
                    { // Secondary, unreachable
                        s.ToString();
                    }
                    break;

                case int _: // The discard is redundant, but still allowed
                case null:
                    if (o == null) // Compliant, False Negative
                    {
                    }

                    break;
                default:
                    break;
            }
        }

        void Switch_Pattern_Constant_With_When(int i)
        {
            switch (i)
            {
                case 0 when i == 0: // the when is redundant, but needed to convert the case to pattern
                    break;
                default:
                    break;
            }
        }
    }
}
