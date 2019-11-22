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

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class IfCollapsible : IfCollapsibleBase
    {
        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var ifStatement = (IfStatementSyntax) c.Node;

                    if (ifStatement.Else != null)
                    {
                        return;
                    }

                    var parentIfStatment = GetParentIfStatement(ifStatement);

                    if (parentIfStatment != null &&
                        parentIfStatment.Else == null)
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(
                            rule,
                            ifStatement.IfKeyword.GetLocation(),
                            additionalLocations: new[] { parentIfStatment.IfKeyword.GetLocation() }));
                    }
                },
                SyntaxKind.IfStatement);
        }

        private static IfStatementSyntax GetParentIfStatement(IfStatementSyntax ifStatement)
        {
            var parent = ifStatement.Parent;

            while (parent.IsKind(SyntaxKind.Block))
            {
                var block = (BlockSyntax) parent;

                if (block.Statements.Count != 1)
                {
                    return null;
                }

                parent = parent.Parent;
            }

            return parent as IfStatementSyntax;
        }
    }
}
