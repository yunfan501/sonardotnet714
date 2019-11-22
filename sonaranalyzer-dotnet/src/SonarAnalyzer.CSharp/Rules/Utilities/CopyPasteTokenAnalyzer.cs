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

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class CopyPasteTokenAnalyzer : CopyPasteTokenAnalyzerBase
    {
        protected override GeneratedCodeRecognizer GeneratedCodeRecognizer => Helpers.CSharp.CSharpGeneratedCodeRecognizer.Instance;

        protected override bool IsUsingDirective(SyntaxNode node) => node is UsingDirectiveSyntax;

        protected override string GetCpdValue(SyntaxToken token)
        {
            if (token.IsKind(SyntaxKind.NumericLiteralToken))
            {
                return "$num";
            }

            if (token.IsKind(SyntaxKind.StringLiteralToken))
            {
                return "$str";
            }

            if (token.IsKind(SyntaxKind.CharacterLiteralToken))
            {
                return "$char";
            }

            return token.Text;
        }
    }
}
