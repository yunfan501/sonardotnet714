﻿using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    public class PropertyToAutoProperty
    {
        private int field;

        public int Property01 //Noncompliant
        {
            get => field;
            set => field = value;
        }
    }
}
