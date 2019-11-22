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
    public sealed class MethodsShouldUseBaseTypes : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3242";
        private const string MessageFormat = "Consider using more general type '{0}' instead of '{1}'.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c => FindViolations((MethodDeclarationSyntax)c.Node, c.SemanticModel)
                        .ForEach(d => c.ReportDiagnosticWhenActive(d)),
                SyntaxKind.MethodDeclaration);
        }

        private List<Diagnostic> FindViolations(MethodDeclarationSyntax methodDeclaration, SemanticModel semanticModel)
        {
            if (!(semanticModel.GetDeclaredSymbol(methodDeclaration) is IMethodSymbol methodSymbol) ||
                methodSymbol.Parameters.Length == 0 ||
                methodSymbol.IsOverride ||
                methodSymbol.IsVirtual ||
                methodSymbol.GetInterfaceMember() != null ||
                methodSymbol.IsEventHandler())
            {
                return Enumerable.Empty<Diagnostic>().ToList();
            }

            var methodAccessibility = methodSymbol.GetEffectiveAccessibility();
            // The GroupBy is useless in most of the cases but safe-guard in case of 2+ parameters with same name (invalid code).
            // In this case we analyze only the first parameter (a new analysis will be triggered after fixing the names).
            var parametersToCheck = methodSymbol.Parameters
                .Where(IsTrackedParameter)
                .GroupBy(p => p.Name)
                .ToDictionary(p => p.Key, p => new ParameterData(p.First(), methodAccessibility));

            var parameterUsesInMethod = methodDeclaration
                .DescendantNodes()
                .OfType<IdentifierNameSyntax>()
                .Where(id => parametersToCheck.Values.Any(p => p.MatchesIdentifier(id, semanticModel)));

            foreach (var identifierReference in parameterUsesInMethod)
            {
                var key = identifierReference.Identifier.ValueText ?? "";

                if (!parametersToCheck.TryGetValue(key, out var paramData) ||
                    !paramData.ShouldReportOn)
                {
                    continue;
                }

                if (identifierReference.Parent is EqualsValueClauseSyntax ||
                    identifierReference.Parent is AssignmentExpressionSyntax)
                {
                    paramData.ShouldReportOn = false;
                    continue;
                }

                var symbolUsedAs = FindParameterUseAsType(identifierReference, semanticModel);
                if (symbolUsedAs != null)
                {
                    paramData.AddUsage(symbolUsedAs);
                }
            }

            return parametersToCheck.Values
                .Select(p => p.GetRuleViolation())
                .WhereNotNull()
                .ToList();
        }

        private static bool IsTrackedParameter(IParameterSymbol parameterSymbol)
        {
            var type = parameterSymbol.Type;

            return !type.DerivesFrom(KnownType.System_Array) &&
                   !type.IsValueType &&
                   !type.Is(KnownType.System_String);
        }

        private static SyntaxNode GetFirstNonParenthesizedParent(SyntaxNode node)
        {
            if (node is ExpressionSyntax expression)
            {
                return expression.GetFirstNonParenthesizedParent();
            }

            return node;
        }

        private static ITypeSymbol FindParameterUseAsType(IdentifierNameSyntax identifier, SemanticModel semanticModel)
        {
            var callSite = semanticModel.GetEnclosingSymbol(identifier.SpanStart)?.ContainingAssembly;

            var identifierParent = GetFirstNonParenthesizedParent(identifier);

            if (identifierParent is ConditionalAccessExpressionSyntax conditionalAccess)
            {
                if (conditionalAccess.WhenNotNull is MemberBindingExpressionSyntax binding)
                {
                    return binding.Name != null
                        ? HandlePropertyOrField(identifier, semanticModel.GetSymbolInfo(binding.Name).Symbol, callSite)
                        : null;
                }

                if (conditionalAccess.WhenNotNull is InvocationExpressionSyntax invocationExpression &&
                    invocationExpression.Expression is MemberBindingExpressionSyntax memberBinding)
                {
                    return HandleInvocation(identifier, semanticModel.GetSymbolInfo(memberBinding).Symbol, semanticModel, callSite);
                }
            }
            else if (identifierParent is MemberAccessExpressionSyntax)
            {
                if (GetFirstNonParenthesizedParent(identifierParent) is InvocationExpressionSyntax invocationExpression)
                {
                    return HandleInvocation(identifier, semanticModel.GetSymbolInfo(invocationExpression).Symbol, semanticModel, callSite);
                }

                return HandlePropertyOrField(identifier, semanticModel.GetSymbolInfo(identifierParent).Symbol, callSite);
            }
            else if (identifierParent is ArgumentSyntax)
            {
                return semanticModel.GetTypeInfo(identifier).ConvertedType;
            }
            else if (identifierParent is ElementAccessExpressionSyntax)
            {
                return HandlePropertyOrField(identifier, semanticModel.GetSymbolInfo(identifierParent).Symbol, callSite);
            }
            else
            {
                // nothing to do
            }

            return null;
        }

        private static ITypeSymbol HandlePropertyOrField(IdentifierNameSyntax identifier, ISymbol symbol, IAssemblySymbol callSite)
        {
            if (!(symbol is IPropertySymbol propertySymbol))
            {
                return FindOriginatingSymbol(symbol, callSite);
            }

            var parent = GetFirstNonParenthesizedParent(identifier);
            var grandParent = GetFirstNonParenthesizedParent(parent);

            var propertyAccessor = grandParent is AssignmentExpressionSyntax
                    ? propertySymbol.SetMethod
                    : propertySymbol.GetMethod;

            return FindOriginatingSymbol(propertyAccessor, callSite);
        }

        private static ITypeSymbol HandleInvocation(IdentifierNameSyntax invokedOn, ISymbol invocationSymbol,
            SemanticModel semanticModel, IAssemblySymbol callSite)
        {
            if (!(invocationSymbol is IMethodSymbol methodSymbol))
            {
                return null;
            }

            return methodSymbol.IsExtensionMethod
                ? semanticModel.GetTypeInfo(invokedOn).ConvertedType
                : FindOriginatingSymbol(invocationSymbol, callSite);
        }

        private static INamedTypeSymbol FindOriginatingSymbol(ISymbol accessedMember, IAssemblySymbol usageSite)
        {
            if (accessedMember == null)
            {
                return null;
            }

            var originatingInterface = accessedMember.GetInterfaceMember()?.ContainingType;
            if (originatingInterface != null &&
                IsNotInternalOrSameAssembly(originatingInterface))
            {
                return originatingInterface;
            }

            var originatingType = SymbolHelper.GetOverriddenMember(accessedMember)?.ContainingType;
            if (originatingType != null &&
                IsNotInternalOrSameAssembly(originatingType))
            {
                return originatingType;
            }

            return accessedMember.ContainingType;

            // Do not suggest internal types that are declared in an assembly different than
            // the one that's declaring the parameter. Such types should not be suggested at
            // all if there is no InternalsVisibleTo attribute present in the compilation.
            // Since the check for the attribute must be done in CompilationEnd thus making
            // the rule unusable in Visual Studio, we will not suggest such classes and will
            // generate some False Negatives.
            bool IsNotInternalOrSameAssembly(INamedTypeSymbol namedTypeSymbol) =>
                namedTypeSymbol.ContainingAssembly.Equals(usageSite) ||
                namedTypeSymbol.GetEffectiveAccessibility() != Accessibility.Internal;
        }

        private class ParameterData
        {
            public bool ShouldReportOn { get; set; } = true;

            private readonly IParameterSymbol parameterSymbol;
            private readonly Accessibility methodAccessibility;
            private readonly Dictionary<ITypeSymbol, int> usedAs = new Dictionary<ITypeSymbol, int>();

            public ParameterData(IParameterSymbol parameterSymbol, Accessibility methodAccessibility)
            {
                this.parameterSymbol = parameterSymbol;
                this.methodAccessibility = methodAccessibility;
            }

            public void AddUsage(ITypeSymbol symbolUsedAs)
            {
                if (this.usedAs.ContainsKey(symbolUsedAs))
                {
                    this.usedAs[symbolUsedAs]++;
                }
                else
                {
                    this.usedAs[symbolUsedAs] = 1;
                }
            }

            public bool MatchesIdentifier(IdentifierNameSyntax id, SemanticModel semanticModel)
            {
                var symbol = semanticModel.GetSymbolInfo(id).Symbol;
                return Equals(this.parameterSymbol, symbol);
            }

            public Diagnostic GetRuleViolation()
            {
                if (!ShouldReportOn)
                {
                    return null;
                }

                var mostGeneralType = FindMostGeneralType();

                if (!Equals(mostGeneralType, this.parameterSymbol.Type) &&
                    CanSuggestBaseType(mostGeneralType.GetSymbolType()))
                {
                    return Diagnostic.Create(rule,
                        this.parameterSymbol.Locations.First(),
                        mostGeneralType.ToDisplayString(), this.parameterSymbol.Type.ToDisplayString());
                }

                return null;
            }

            private static bool CanSuggestBaseType(ITypeSymbol typeSymbol)
            {
                return
                    !typeSymbol.Is(KnownType.System_Object) &&
                    !typeSymbol.Is(KnownType.System_ValueType) &&
                    !typeSymbol.Name.StartsWith("_", StringComparison.Ordinal) &&
                    !typeSymbol.Is(KnownType.System_Enum) &&
                    !IsCollectionKvp(typeSymbol);
            }

            private static bool IsCollectionKvp(ITypeSymbol typeSymbol)
            {
                var namedType = typeSymbol as INamedTypeSymbol;
                var firstGenericType = namedType?.TypeArguments.FirstOrDefault() as INamedTypeSymbol;

                return namedType != null &&
                    firstGenericType != null &&
                    namedType.ConstructedFrom.Is(KnownType.System_Collections_Generic_ICollection_T) &&
                    firstGenericType.ConstructedFrom.Is(KnownType.System_Collections_Generic_KeyValuePair_TKey_TValue);
            }

            private ISymbol FindMostGeneralType()
            {
                var mostGeneralType = this.parameterSymbol.Type;

                var multipleEnumerableCalls = this.usedAs.Where(HasMultipleUseOfIEnumerable).ToList();
                foreach (var v in multipleEnumerableCalls)
                {
                    this.usedAs.Remove(v.Key);
                }

                if (this.usedAs.Count == 0)
                {
                    return mostGeneralType;
                }

                mostGeneralType = FindMostGeneralAccessibleClassOrSelf(mostGeneralType);
                mostGeneralType = FindMostGeneralAccessibleInterfaceOrSelf(mostGeneralType);
                return mostGeneralType;

                bool HasMultipleUseOfIEnumerable(KeyValuePair<ITypeSymbol, int> kvp)
                {
                    return kvp.Value > 1 &&
                        (kvp.Key.OriginalDefinition.Is(KnownType.System_Collections_Generic_IEnumerable_T) ||
                         kvp.Key.Is(KnownType.System_Collections_IEnumerable));
                }

                ITypeSymbol FindMostGeneralAccessibleClassOrSelf(ITypeSymbol typeSymbol)
                {
                    var currentSymbol = typeSymbol.BaseType;

                    while (currentSymbol != null)
                    {
                        if (DerivesOrImplementsAll(currentSymbol))
                        {
                            typeSymbol = currentSymbol;
                        }

                        currentSymbol = currentSymbol?.BaseType;
                    }

                    return typeSymbol;
                }

                ITypeSymbol FindMostGeneralAccessibleInterfaceOrSelf(ITypeSymbol typeSymbol)
                {
                    foreach (var @interface in typeSymbol.Interfaces)
                    {
                        if (DerivesOrImplementsAll(@interface))
                        {
                            return FindMostGeneralAccessibleInterfaceOrSelf(@interface);
                        }
                    }

                    return typeSymbol;
                }
            }

            private bool DerivesOrImplementsAll(ITypeSymbol type)
            {
                return type != null &&
                    this.usedAs.Keys.All(type.DerivesOrImplements) &&
                    IsConsistentAccessibility(type.GetEffectiveAccessibility());

                bool IsConsistentAccessibility(Accessibility baseTypeAccessibility)
                {
                    switch (this.methodAccessibility)
                    {
                        case Accessibility.NotApplicable:
                            return false;

                        case Accessibility.Private:
                            return true;

                        case Accessibility.Protected:
                        case Accessibility.Internal:
                            return baseTypeAccessibility == Accessibility.Public ||
                                baseTypeAccessibility == this.methodAccessibility;

                        case Accessibility.ProtectedAndInternal:
                        case Accessibility.Public:
                            return baseTypeAccessibility == Accessibility.Public;

                        default:
                            return false;
                    }
                }
            }
        }
    }
}
