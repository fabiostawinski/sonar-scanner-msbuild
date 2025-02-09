﻿/*
 * SonarScanner for MSBuild
 * Copyright (C) 2016-2021 SonarSource SA
 * mailto:info AT sonarsource DOT com
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
using System.Reflection;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace SonarScanner.MSBuild.Tasks
{
    /// <summary>
    /// MSBuild task that determines whether the project should be treated as a test project for analysis purposes based on its references.
    /// </summary>
    public sealed class IsTestByReference : Task
    {
        // This list is duplicated in sonar-dotnet and sonar-security and should be manually synchronized after each change.
        internal /* for testing */ static readonly ISet<string> TestAssemblyNames = new HashSet<string>
        {
            "DOTMEMORY.UNIT",
            "MICROSOFT.VISUALSTUDIO.TESTPLATFORM.TESTFRAMEWORK",
            "MICROSOFT.VISUALSTUDIO.QUALITYTOOLS.UNITTESTFRAMEWORK",
            "MACHINE.SPECIFICATIONS",
            "NUNIT.FRAMEWORK",
            "NUNITLITE",
            "TECHTALK.SPECFLOW",
            "XUNIT", // Legacy Xunit (v1.x)
            "XUNIT.CORE",
            // Assertion
            "FLUENTASSERTIONS",
            "SHOULDLY",
            // Mock
            "FAKEITEASY",
            "MOQ",
            "NSUBSTITUTE",
            "RHINO.MOCKS",
            "TELERIK.JUSTMOCK"
        };

        /// <summary>
        /// List of resolved reference names for current project.
        /// </summary>
        public string[] References { get; set; }

        /// <summary>
        /// Returns name of the test reference for a Test project.
        /// Return null for a Product project.
        /// </summary>
        [Output]
        public string TestReference { get; private set; }

        public override bool Execute()
        {
            if (References == null || References.Length == 0)
            {
                Log.LogMessage(MessageImportance.Low, Resources.IsTest_NoReferences, TestReference);
            }
            else
            {
                TestReference = References.Select(ParseName).FirstOrDefault(x => x != null && TestAssemblyNames.Contains(x.ToUpperInvariant()));
                Log.LogMessage(MessageImportance.Low, TestReference == null ? Resources.IsTest_NoTestReference : Resources.IsTest_ResolvedReference, TestReference);
            }
            return true;
        }

        private string ParseName(string fullName)
        {
            try
            {
                return new AssemblyName(fullName).Name;
            }
            catch
            {
                Log.LogMessage(MessageImportance.Normal, Resources.IsTest_UnableToParseAssemblyName, fullName);
                return null;
            }
        }
    }
}
