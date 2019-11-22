/*
 * SonarVB
 * Copyright (C) 2012-2019 SonarSource SA
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
package org.sonar.plugins.vbnet;

import org.sonar.api.Plugin;
import org.sonarsource.dotnet.shared.plugins.AbstractPropertyDefinitions;
import org.sonarsource.dotnet.shared.plugins.CodeCoverageProvider;
import org.sonarsource.dotnet.shared.plugins.DotNetPluginMetadata;
import org.sonarsource.dotnet.shared.plugins.DotNetSensor;
import org.sonarsource.dotnet.shared.plugins.EncodingPerFile;
import org.sonarsource.dotnet.shared.plugins.GeneratedFileFilter;
import org.sonarsource.dotnet.shared.plugins.PropertiesSensor;
import org.sonarsource.dotnet.shared.plugins.ProtobufDataImporter;
import org.sonarsource.dotnet.shared.plugins.ReportPathCollector;
import org.sonarsource.dotnet.shared.plugins.RoslynDataImporter;
import org.sonarsource.dotnet.shared.plugins.RoslynProfileExporter;
import org.sonarsource.dotnet.shared.plugins.SonarLintProfileExporter;
import org.sonarsource.dotnet.shared.plugins.UnitTestResultsProvider;
import org.sonarsource.dotnet.shared.plugins.WrongEncodingFileFilter;

public class VbNetPlugin implements Plugin {

  static final String LANGUAGE_KEY = "vbnet";
  static final String LANGUAGE_NAME = "VB.NET";

  static final String REPOSITORY_KEY = "vbnet";
  static final String REPOSITORY_NAME = "SonarAnalyzer";
  static final String PLUGIN_KEY = "vbnet";
  static final String SONARANALYZER_NAME = "SonarAnalyzer.VisualBasic";

  static final String FILE_SUFFIXES_KEY = AbstractPropertyDefinitions.getFileSuffixProperty(LANGUAGE_KEY);
  static final String FILE_SUFFIXES_DEFVALUE = ".vb";

  static final DotNetPluginMetadata METADATA = new VbNetPluginMetadata();

  @Override
  public void define(Context context) {
    context.addExtensions(
      METADATA,
      VbNet.class,
      ReportPathCollector.class,
      VbNetSonarRulesDefinition.class,
      DotNetSensor.class,
      VbNetConfiguration.class,
      VbNetGlobalProtobufFileProcessor.class,
      WrongEncodingFileFilter.class,
      EncodingPerFile.class,
      GeneratedFileFilter.class,
      PropertiesSensor.class,
      SonarLintProfileExporter.class,
      SonarLintFakeProfileImporter.class,
      ProtobufDataImporter.class,
      RoslynDataImporter.class,
      RoslynProfileExporter.class);

    context.addExtensions(new VbNetPropertyDefinitions(context.getRuntime()).create());
    context.addExtension(new VbNetSonarWayProfile(context.getRuntime()));
    context.addExtensions(new CodeCoverageProvider(METADATA).extensions());
    context.addExtensions(new UnitTestResultsProvider(METADATA).extensions());
    context.addExtensions(RoslynProfileExporter.sonarLintRepositoryProperties(METADATA));
  }

  private static class VbNetPluginMetadata implements DotNetPluginMetadata {

    @Override
    public String languageKey() {
      return LANGUAGE_KEY;
    }

    @Override
    public String pluginKey() {
      return PLUGIN_KEY;
    }

    @Override
    public String languageName() {
      return LANGUAGE_NAME;
    }

    @Override
    public String shortLanguageName() {
      return LANGUAGE_NAME;
    }

    @Override
    public String sonarAnalyzerName() {
      return SONARANALYZER_NAME;
    }

    @Override
    public String repositoryKey() {
      return REPOSITORY_KEY;
    }
  }
}
