﻿using System;

namespace Tests.Diagnostics
{
    public class EmptyStatement
    {
        public int MyField;

        public EmptyStatement()
        {

            Console.WriteLine();
            while (true)
                ; // Fixed
        }
    }
}
