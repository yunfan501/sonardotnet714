﻿using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    class Base
    {
        public void M()
        {
            GC.SuppressFinalize(this);
        }
        ~Base()
        { }
    }

    class Derived1 : Base
    {
        public void M()
        {
            GC.SuppressFinalize(this);
        }

        ~Derived1()
        { }
    }
    sealed class Derived2 : Base
    {
        public void M()
        {
            GC.SuppressFinalize(this);
        }
    }

    sealed class C1
    {
        public void M()
        {
        }
    }
    class C2
    {
        public void M()
        {
            GC.SuppressFinalize(this); // Compliant, not sealed
        }
    }

    class B1 { }
    sealed class C3 : B1
    {
        public void M()
        {
        }
    }

    sealed class Dummy
    {
        public void SuppressFinalize(object o)
        { }
        public void M()
        {
            SuppressFinalize(this);
        }
    }

    sealed class Compliant
    {
        ~Compliant()
        { }
        public void M()
        {
            GC.SuppressFinalize(this);
        }
    }
}
