﻿#define DEBUG
using System;

namespace Rules2015
{
    class S2228_ConsoleWriteLineInDebugBlock
    {

#if !DEBUG
        public void Method1()
        {
            Console.WriteLine("Hello World"); // won't be processed - nodes aren't active
        }
#else
        public void DoStuff()
        {
            Console.WriteLine("Hello World"); // Noncompliant: false-positive (we don't handle logical operators in debug blocks)
        }
#endif
    }
}
