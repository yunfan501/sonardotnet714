﻿/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
 * mailto: contact AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

using System.Collections.Generic;
using System.Linq;
using SonarAnalyzer.SymbolicExecution.Constraints;

namespace SonarAnalyzer.SymbolicExecution.SymbolicValues
{
    public class AndSymbolicValue : BinarySymbolicValue
    {
        public AndSymbolicValue(SymbolicValue leftOperand, SymbolicValue rightOperand)
            : base(leftOperand, rightOperand)
        {
        }

        public override IEnumerable<ProgramState> TrySetConstraint(SymbolicValueConstraint constraint, ProgramState programState)
        {
            if (!(constraint is BoolConstraint boolConstraint))
            {
                return new[] { programState };
            }

            if (boolConstraint == BoolConstraint.True)
            {
                return ThrowIfTooMany(
                    LeftOperand.TrySetConstraint(BoolConstraint.True, programState)
                        .SelectMany(ps => RightOperand.TrySetConstraint(BoolConstraint.True, ps)));
            }

            return ThrowIfTooMany(
                LeftOperand.TrySetConstraint(BoolConstraint.True, programState)
                    .SelectMany(ps => RightOperand.TrySetConstraint(BoolConstraint.False, ps))
                .Union(LeftOperand.TrySetConstraint(BoolConstraint.False, programState)
                    .SelectMany(ps => RightOperand.TrySetConstraint(BoolConstraint.True, ps)))
                .Union(LeftOperand.TrySetConstraint(BoolConstraint.False, programState)
                    .SelectMany(ps => RightOperand.TrySetConstraint(BoolConstraint.False, ps))));
        }

        public override string ToString()
        {
            return LeftOperand + " & " + RightOperand;
        }
    }
}
