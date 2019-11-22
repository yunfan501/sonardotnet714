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

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SonarAnalyzer.Helpers
{
    public class ParameterLoadingAnalysisContext
    {
        private readonly SonarAnalysisContext context;

        private readonly ICollection<Action<CompilationStartAnalysisContext>> compilationStartActions =
            new List<Action<CompilationStartAnalysisContext>>();

        internal IEnumerable<Action<CompilationStartAnalysisContext>> CompilationStartActions => this.compilationStartActions;

        internal ParameterLoadingAnalysisContext(SonarAnalysisContext context)
        {
            this.context = context;
        }

        internal void RegisterCompilationAction(Action<CompilationAnalysisContext> action)
        {
            this.context.RegisterCompilationAction(action);
        }

        internal void RegisterCompilationStartAction(Action<CompilationStartAnalysisContext> action)
        {
            // only collect compilation start actions and call them later
            this.compilationStartActions.Add(action);
        }

        internal void RegisterSyntaxNodeAction<TLanguageKindEnum>(Action<SyntaxNodeAnalysisContext> action, ImmutableArray<TLanguageKindEnum> syntaxKinds) where TLanguageKindEnum : struct
        {
            this.context.RegisterSyntaxNodeAction(action, syntaxKinds);
        }

        internal void RegisterCodeBlockStartAction<TLanguageKindEnum>(Action<CodeBlockStartAnalysisContext<TLanguageKindEnum>> action) where TLanguageKindEnum : struct
        {
            this.context.RegisterCodeBlockStartAction(action);
        }
    }
}
