namespace Tests.Diagnostics
{
    public class FooNotCompliant
    {
        public static FooNotCompliant operator +(FooNotCompliant x, FooNotCompliant y) => null; // Noncompliant {{Implement alternative method 'Add' for the operator '+'.}}
        public static FooNotCompliant operator &(FooNotCompliant x, FooNotCompliant y) => null; // Noncompliant {{Implement alternative method 'BitwiseAnd' for the operator '&'.}}
        public static FooNotCompliant operator |(FooNotCompliant x, FooNotCompliant y) => null; // Noncompliant {{Implement alternative method 'BitwiseOr' for the operator '|'.}}
        public static FooNotCompliant operator /(FooNotCompliant x, FooNotCompliant y) => null; // Noncompliant {{Implement alternative method 'Divide' for the operator '/'.}}
        public static FooNotCompliant operator ^(FooNotCompliant x, FooNotCompliant y) => null; // Noncompliant {{Implement alternative method 'Xor' for the operator '^'.}}
        public static bool operator ==(FooNotCompliant x, FooNotCompliant y) => true; // Noncompliant {{Implement alternative method 'Equals' for the operator '=='.}}
        public static bool operator !=(FooNotCompliant x, FooNotCompliant y) => true; // Noncompliant {{Implement alternative method 'Equals' for the operator '!='.}}
        public static FooNotCompliant operator >(FooNotCompliant x, FooNotCompliant y) => null; // Noncompliant {{Implement alternative method 'Compare' for the operator '>'.}}
        public static FooNotCompliant operator <(FooNotCompliant x, FooNotCompliant y) => null; // Noncompliant {{Implement alternative method 'Compare' for the operator '<'.}}
        public static FooNotCompliant operator >=(FooNotCompliant x, FooNotCompliant y) => null; // Noncompliant {{Implement alternative method 'Compare' for the operator '>='.}}
        public static FooNotCompliant operator <=(FooNotCompliant x, FooNotCompliant y) => null; // Noncompliant {{Implement alternative method 'Compare' for the operator '<='.}}
        public static FooNotCompliant operator --(FooNotCompliant x) => null; // Noncompliant {{Implement alternative method 'Decrement' for the operator '--'.}}
        public static FooNotCompliant operator ++(FooNotCompliant x) => null; // Noncompliant {{Implement alternative method 'Increment' for the operator '++'.}}
        public static FooNotCompliant operator <<(FooNotCompliant x, int y) => null; // Noncompliant {{Implement alternative method 'LeftShift' for the operator '<<'.}}
        public static FooNotCompliant operator >>(FooNotCompliant x, int y) => null; // Noncompliant {{Implement alternative method 'RightShift' for the operator '>>'.}}
        public static FooNotCompliant operator !(FooNotCompliant x) => null; // Noncompliant {{Implement alternative method 'LogicalNot' for the operator '!'.}}
        public static FooNotCompliant operator %(FooNotCompliant x, FooNotCompliant y) => null; // Noncompliant {{Implement alternative method 'Mod' for the operator '%'.}}
        public static FooNotCompliant operator *(FooNotCompliant x, FooNotCompliant y) => null; // Noncompliant {{Implement alternative method 'Multiply' for the operator '*'.}}
        public static FooNotCompliant operator ~(FooNotCompliant x) => null; // Noncompliant {{Implement alternative method 'OnesComplement' for the operator '~'.}}
        public static FooNotCompliant operator -(FooNotCompliant x, FooNotCompliant y) => null; // Noncompliant {{Implement alternative method 'Subtract' for the operator '-'.}}
        public static bool operator true(FooNotCompliant x) => true; // Compliant, we don't check for true
        public static bool operator false(FooNotCompliant x) => false; // Compliant, we don't check for false
        public static FooNotCompliant operator -(FooNotCompliant x) => null; // Noncompliant {{Implement alternative method 'Negate' for the operator '-'.}}
        public static FooNotCompliant operator +(FooNotCompliant x) => null; // Noncompliant {{Implement alternative method 'Plus' for the operator '+'.}}
    }

    public class FooCompliant
    {
        public static FooCompliant operator +(FooCompliant x, FooCompliant y) => null;
        public static FooCompliant operator &(FooCompliant x, FooCompliant y) => null;
        public static FooCompliant operator |(FooCompliant x, FooCompliant y) => null;
        public static FooCompliant operator /(FooCompliant x, FooCompliant y) => null;
        public static FooCompliant operator ^(FooCompliant x, FooCompliant y) => null;
        public static bool operator ==(FooCompliant x, FooCompliant y) => true;
        public static bool operator !=(FooCompliant x, FooCompliant y) => true;
        public static FooCompliant operator >(FooCompliant x, FooCompliant y) => null;
        public static FooCompliant operator <(FooCompliant x, FooCompliant y) => null;
        public static FooCompliant operator >=(FooCompliant x, FooCompliant y) => null;
        public static FooCompliant operator <=(FooCompliant x, FooCompliant y) => null;
        public static FooCompliant operator --(FooCompliant x) => null;
        public static FooCompliant operator ++(FooCompliant x) => null;
        public static FooCompliant operator <<(FooCompliant x, int y) => null;
        public static FooCompliant operator >>(FooCompliant x, int y) => null;
        public static FooCompliant operator !(FooCompliant x) => null;
        public static FooCompliant operator %(FooCompliant x, FooCompliant y) => null;
        public static FooCompliant operator *(FooCompliant x, FooCompliant y) => null;
        public static FooCompliant operator ~(FooCompliant x) => null;
        public static FooCompliant operator -(FooCompliant x, FooCompliant y) => null;
        public static FooCompliant operator -(FooCompliant x) => null;
        public static FooCompliant operator +(FooCompliant x) => null;

        public FooCompliant Add(FooCompliant y) => null;
        public FooCompliant BitwiseAnd(FooCompliant x, FooCompliant y) => null;
        public FooCompliant BitwiseOr(FooCompliant x, FooCompliant y) => null;
        public FooCompliant Divide(FooCompliant x, FooCompliant y) => null;
        public FooCompliant Xor(FooCompliant x, FooCompliant y) => null;
        public bool Equals(FooCompliant x, FooCompliant y) => true;
        public FooCompliant Compare(FooCompliant x, FooCompliant y) => null;
        public FooCompliant Decrement(FooCompliant x) => null;
        public FooCompliant Increment(FooCompliant x) => null;
        public FooCompliant LeftShift(FooCompliant x, int y) => null;
        public FooCompliant RightShift(FooCompliant x, int y) => null;
        public FooCompliant LogicalNot(FooCompliant x) => null;
        public FooCompliant Mod(FooCompliant x, FooCompliant y) => null;
        public FooCompliant Multiply(FooCompliant x, FooCompliant y) => null;
        public FooCompliant OnesComplement(FooCompliant x) => null;
        public FooCompliant Subtract(FooCompliant x, FooCompliant y) => null;
        public FooCompliant Negate(FooCompliant x) => null;
        public FooCompliant Plus(FooCompliant x) => null;
    }

    // For comparison operators we allow "CompareTo" in addition to "Compare"
    public class FooCompliant_OtherNames
    {
        public static FooCompliant operator >(FooCompliant_OtherNames x, FooCompliant_OtherNames y) => null;
        public static FooCompliant operator <(FooCompliant_OtherNames x, FooCompliant_OtherNames y) => null;
        public static FooCompliant operator >=(FooCompliant_OtherNames x, FooCompliant_OtherNames y) => null;
        public static FooCompliant operator <=(FooCompliant_OtherNames x, FooCompliant_OtherNames y) => null;

        public FooCompliant CompareTo(FooCompliant x, FooCompliant y) => null;
    }

    public class FooCompliant_Static
    {
        public static FooCompliant_Static operator +(FooCompliant_Static x, FooCompliant_Static y) => null;

        public static FooCompliant_Static Add(FooCompliant_Static y) => null;
    }

    public class FooCompliant_OtherArguments
    {
        public static FooCompliant_OtherArguments operator +(FooCompliant_OtherArguments x, FooCompliant_OtherArguments y) => null;

        // We don't care about return type, argument count and types, just as FxCop
        public FooCompliant_OtherArguments Add() => null;
    }
}
