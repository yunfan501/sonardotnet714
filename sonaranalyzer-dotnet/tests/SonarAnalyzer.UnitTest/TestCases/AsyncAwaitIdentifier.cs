﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tests.Diagnostics
{
    public class AsyncAwaitIdentifier
    {
        public AsyncAwaitIdentifier()
        {
            int async = 42; // Noncompliant {{Rename 'async' to not use a contextual keyword as an identifier.}}
//              ^^^^^
            int await = 42; // Noncompliant {{Rename 'await' to not use a contextual keyword as an identifier.}}

            await = 42*2;
        }

        public void Foo(int async) // Noncompliant
        {
            var x = from await in new List<int>() {5,6,7 } // Noncompliant
//                       ^^^^^
            select await;
        }

        public static async Task<string> Foo(string a)
        {
            return await Foo(a);
        }
    }
}
