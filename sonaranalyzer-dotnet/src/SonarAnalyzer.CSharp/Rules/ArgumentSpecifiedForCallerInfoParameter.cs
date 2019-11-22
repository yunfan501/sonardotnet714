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
using System.Linq;
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
    public sealed class ArgumentSpecifiedForCallerInfoParameter : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3236";
        private const string MessageFormat = "Remove this argument from the method call; it hides the caller information.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        private static readonly ImmutableArray<KnownType> CallerInfoAttributesToReportOn =
            ImmutableArray.Create(
                KnownType.System_Runtime_CompilerServices_CallerFilePathAttribute,
                KnownType.System_Runtime_CompilerServices_CallerLineNumberAttribute
            );

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var methodCall = (InvocationExpressionSyntax)c.Node;
                    var methodParameterLookup = new CSharpMethodParameterLookup(methodCall, c.SemanticModel);

                    var methodSymbol = methodParameterLookup.MethodSymbol;
                    if (methodSymbol == null)
                    {
                        return;
                    }

                    var argumentMappings = methodParameterLookup.GetAllArgumentParameterMappings();
                    foreach (var argumentMapping in argumentMappings)
                    {
                        var parameter = argumentMapping.Symbol;
                        var argument = argumentMapping.SyntaxNode;

                        var callerInfoAttributeDataOnCall = GetCallerInfoAttribute(parameter);
                        if (callerInfoAttributeDataOnCall == null)
                        {
                            continue;
                        }

                        if (c.SemanticModel.GetSymbolInfo(argument.Expression).Symbol is IParameterSymbol symbolForArgument &&
                            Equals(callerInfoAttributeDataOnCall.AttributeClass, GetCallerInfoAttribute(symbolForArgument)?.AttributeClass))
                        {
                            continue;
                        }

                        c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, argument.GetLocation()));
                    }
                },
                SyntaxKind.InvocationExpression);
        }

        private static AttributeData GetCallerInfoAttribute(IParameterSymbol parameter) =>
            parameter.GetAttributes(CallerInfoAttributesToReportOn).FirstOrDefault();
    }
}
