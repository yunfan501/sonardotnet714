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
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SonarAnalyzer.ControlFlowGraph;
using SonarAnalyzer.ControlFlowGraph.CSharp;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.LiveVariableAnalysis.CSharp
{
    public sealed class CSharpLiveVariableAnalysis : AbstractLiveVariableAnalysis
    {
        private readonly ISymbol declaration;
        private readonly SemanticModel semanticModel;

        private CSharpLiveVariableAnalysis(IControlFlowGraph controlFlowGraph, ISymbol declaration,
            SemanticModel semanticModel)
            : base(controlFlowGraph)
        {
            this.declaration = declaration;
            this.semanticModel = semanticModel;
        }

        public static AbstractLiveVariableAnalysis Analyze(IControlFlowGraph controlFlowGraph, ISymbol declaration,
            SemanticModel semanticModel)
        {
            var lva = new CSharpLiveVariableAnalysis(controlFlowGraph, declaration, semanticModel);
            lva.PerformAnalysis();
            return lva;
        }

        protected override void ProcessBlock(Block block, out HashSet<ISymbol> assignedInBlock,
            out HashSet<ISymbol> usedInBlock)
        {
            assignedInBlock = new HashSet<ISymbol>(); // Kill (The set of variables that are assigned a value.)
            usedInBlock = new HashSet<ISymbol>(); // Gen (The set of variables that are used before any assignment.)

            var assignmentLhs = new HashSet<SyntaxNode>();

            foreach (var instruction in block.Instructions.Reverse())
            {
                switch (instruction.Kind())
                {
                    case SyntaxKind.IdentifierName:
                        ProcessIdentifier(instruction, assignedInBlock, usedInBlock, assignmentLhs);
                        break;

                    case SyntaxKind.SimpleAssignmentExpression:
                        ProcessSimpleAssignment(instruction, assignedInBlock, usedInBlock, assignmentLhs);
                        break;

                    case SyntaxKind.VariableDeclarator:
                        ProcessVariableDeclarator((VariableDeclaratorSyntax)instruction, assignedInBlock,
                            usedInBlock);
                        break;

                    case SyntaxKind.AnonymousMethodExpression:
                    case SyntaxKind.ParenthesizedLambdaExpression:
                    case SyntaxKind.SimpleLambdaExpression:
                    case SyntaxKind.QueryExpression:
                        CollectAllCapturedLocal(instruction);
                        break;
                }
            }

            if (block.Instructions.Any())
            {
                return;
            }

            // Variable declaration in a foreach statement is not a VariableDeclarator, so handling it separately:
            if (block is BinaryBranchBlock foreachBlock &&
                foreachBlock.BranchingNode.IsKind(SyntaxKind.ForEachStatement))
            {
                var foreachNode = (ForEachStatementSyntax)foreachBlock.BranchingNode;
                ProcessVariableInForeach(foreachNode, assignedInBlock, usedInBlock);
            }

            // Keep alive the variables declared and used in the using statement until the UsingFinalizerBlock
            if (block is UsingEndBlock usingFinalizerBlock)
            {
                var disposableSymbols = usingFinalizerBlock.Identifiers
                    .Select(i => this.semanticModel.GetDeclaredSymbol(i.Parent)
                                ?? this.semanticModel.GetSymbolInfo(i.Parent).Symbol)
                    .WhereNotNull();
                foreach (var disposableSymbol in disposableSymbols)
                {
                    usedInBlock.Add(disposableSymbol);
                }
            }
        }

        private void ProcessVariableInForeach(ForEachStatementSyntax foreachNode, HashSet<ISymbol> assignedInBlock,
            HashSet<ISymbol> usedBeforeAssigned)
        {
            var symbol = this.semanticModel.GetDeclaredSymbol(foreachNode);
            if (symbol == null)
            {
                return;
            }

            assignedInBlock.Add(symbol);
            usedBeforeAssigned.Remove(symbol);
        }

        private void ProcessVariableDeclarator(VariableDeclaratorSyntax instruction, HashSet<ISymbol> assignedInBlock,
            HashSet<ISymbol> usedBeforeAssigned)
        {
            var symbol = this.semanticModel.GetDeclaredSymbol(instruction);
            if (symbol == null)
            {
                return;
            }

            assignedInBlock.Add(symbol);
            usedBeforeAssigned.Remove(symbol);
        }

        private void ProcessSimpleAssignment(SyntaxNode instruction, HashSet<ISymbol> assignedInBlock,
            HashSet<ISymbol> usedBeforeAssigned, HashSet<SyntaxNode> assignmentLhs)
        {
            var assignment = (AssignmentExpressionSyntax)instruction;
            var left = assignment.Left.RemoveParentheses();
            if (left.IsKind(SyntaxKind.IdentifierName))
            {
                var symbol = this.semanticModel.GetSymbolInfo(left).Symbol;
                if (symbol == null)
                {
                    return;
                }

                if (IsLocalScoped(symbol))
                {
                    assignmentLhs.Add(left);
                    assignedInBlock.Add(symbol);
                    usedBeforeAssigned.Remove(symbol);
                }
            }
        }

        private void ProcessIdentifier(SyntaxNode instruction, HashSet<ISymbol> assignedInBlock,
            HashSet<ISymbol> usedBeforeAssigned, HashSet<SyntaxNode> assignmentLhs)
        {
            var identifier = (IdentifierNameSyntax)instruction;
            var symbol = this.semanticModel.GetSymbolInfo(identifier).Symbol;
            if (symbol == null)
            {
                return;
            }

            if (!identifier.GetSelfOrTopParenthesizedExpression().IsInNameofCall(this.semanticModel) &&
                IsLocalScoped(symbol))
            {
                if (IsOutArgument(identifier))
                {
                    assignedInBlock.Add(symbol);
                    usedBeforeAssigned.Remove(symbol);
                }
                else
                {
                    if (!assignmentLhs.Contains(instruction))
                    {
                        usedBeforeAssigned.Add(symbol);
                    }
                }
            }
        }

        private void CollectAllCapturedLocal(SyntaxNode instruction)
        {
            var allCapturedSymbols = instruction.DescendantNodes()
                .OfType<IdentifierNameSyntax>()
                .Select(i => this.semanticModel.GetSymbolInfo(i).Symbol)
                .Where(s => s != null && IsLocalScoped(s));

            // Collect captured locals
            // Read and write both affects liveness
            this.capturedVariables.UnionWith(allCapturedSymbols);
        }

        internal static bool IsOutArgument(IdentifierNameSyntax identifier)
        {

            return identifier.GetFirstNonParenthesizedParent() is ArgumentSyntax argument &&
                argument.RefOrOutKeyword.IsKind(SyntaxKind.OutKeyword);
        }

        private bool IsLocalScoped(ISymbol symbol)
        {
            return IsLocalScoped(symbol, this.declaration);
        }

        internal static bool IsLocalScoped(ISymbol symbol, ISymbol declaration)
        {
            if (!(symbol is ILocalSymbol local) &&
                (!(symbol is IParameterSymbol parameter) || parameter.RefKind != RefKind.None))
            {
                return false;
            }

            return symbol.ContainingSymbol != null &&
                symbol.ContainingSymbol.Equals(declaration);
        }
    }
}
