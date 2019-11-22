﻿using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    class XAttribute : Attribute { }
    public class ThreadStaticNonStaticField
    {
        [ThreadStatic]  // Noncompliant
//       ^^^^^^^^^^^^
        private int count1 = 0, count11 = 0;

        [ThreadStatic, X]  // Noncompliant {{Remove the 'ThreadStatic' attribute from this definition.}}
        private int count2 = 0;

        [System.ThreadStatic]
        private static int count3 = 0;

        private int count4 = 0;
    }
}
