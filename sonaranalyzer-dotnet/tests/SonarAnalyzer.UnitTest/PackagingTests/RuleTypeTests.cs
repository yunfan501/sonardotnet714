/*
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

extern alias csharp;
extern alias vbnet;
using System.Collections.Immutable;
using System.Linq;
using System.Resources;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SonarAnalyzer.UnitTest.PackagingTests
{
    [TestClass]
    public class RuleTypeTests
    {
        [TestMethod]
        public void DetectRuleTypeChanges_CS()
        {
            DetectTypeChanges(csharp::SonarAnalyzer.RspecStrings.ResourceManager,
                CsRuleTypeMapping.RuleTypesCs, nameof(CsRuleTypeMapping.RuleTypesCs));
        }

        [TestMethod]
        public void DetectRuleTypeChanges_VB()
        {
            DetectTypeChanges(vbnet::SonarAnalyzer.RspecStrings.ResourceManager,
                VbRuleTypeMapping.RuleTypesVb, nameof(VbRuleTypeMapping.RuleTypesVb));
        }

        private static void DetectTypeChanges(ResourceManager resourceManager, IImmutableDictionary<string, string> expectedTypes, string expectedTypesName)
        {
            var items = Enumerable
                .Range(1, 10000)
                .Select(i => new
                {
                    ExpectedType = expectedTypes.GetValueOrDefault(i.ToString()),
                    ActualType = resourceManager.GetString($"S{i}_Type"),
                    RuleId = i,
                })
                .Where(x => x.ActualType != x.ExpectedType)
                .ToList();

            // IMPORTANT: If this test fails, you should add the types of the new rules
            // in the dictionaries above. It is a manual task, sorry.
            var newRules = items.Where(x => x.ExpectedType == null);
            newRules.Should().BeEmpty($"you need to add the rules in {expectedTypesName}");

            // IMPORTANT: Rules should not be deleted without careful consideration and prior
            // deprecation. We need to notify the platform team as well.
            var deletedRules = items.Where(x => x.ActualType == null);
            deletedRules.Should().BeEmpty($"YOU SHOULD NEVER DELETE RULES!");

            // IMPORTANT: If this test fails, you should update the types of the changed rules
            // in the dictionaries above. Also add a GitHub issue specifying the change of type
            // and update peach and next.
            var changedRules = items.Where(x => x.ActualType != null && x.ExpectedType != null);
            changedRules.Should().BeEmpty($"you need to change the rules in {expectedTypesName}.");
        }
    }
}
