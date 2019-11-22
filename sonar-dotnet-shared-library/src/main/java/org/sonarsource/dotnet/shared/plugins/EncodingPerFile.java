/*
 * SonarSource :: .NET :: Shared library
 * Copyright (C) 2014-2019 SonarSource SA
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
package org.sonarsource.dotnet.shared.plugins;

import java.nio.charset.Charset;
import java.nio.charset.StandardCharsets;
import java.nio.file.Path;
import org.sonar.api.batch.InstantiationStrategy;
import org.sonar.api.batch.ScannerSide;
import org.sonar.api.batch.fs.InputFile;
import org.sonar.api.utils.log.Logger;
import org.sonar.api.utils.log.Loggers;

@ScannerSide
@InstantiationStrategy(InstantiationStrategy.PER_BATCH)
public class EncodingPerFile {
  private static final Logger LOG = Loggers.get(EncodingPerFile.class);
  private final AbstractGlobalProtobufFileProcessor globalReportProcessor;

  public EncodingPerFile(AbstractGlobalProtobufFileProcessor globalReportProcessor) {
    this.globalReportProcessor = globalReportProcessor;
  }

  boolean encodingMatch(InputFile inputFile) {
    Path inputFilePath = inputFile.path().toAbsolutePath();

    if (!globalReportProcessor.getRoslynEncodingPerPath().containsKey(inputFilePath)) {
      // When there is no entry for a file, it means it was not processed by Roslyn. So we consider encoding to be ok.
      return true;
    }

    Charset roslynEncoding = globalReportProcessor.getRoslynEncodingPerPath().get(inputFilePath);
    if (roslynEncoding == null) {
      LOG.warn("File '{}' does not have encoding information. Skip it.", inputFilePath);
      return false;
    }

    Charset sqEncoding = inputFile.charset();

    boolean sameEncoding = sqEncoding.equals(roslynEncoding);
    if (!sameEncoding) {
      if (sqEncoding.equals(StandardCharsets.UTF_16LE) && roslynEncoding.equals(StandardCharsets.UTF_16)) {
        sameEncoding = true;
      } else {
        LOG.warn("Encoding detected by Roslyn and encoding used by SonarQube do not match for file {}. "
          + "SonarQube encoding is '{}', Roslyn encoding is '{}'. File will be skipped.",
          inputFilePath, sqEncoding, roslynEncoding);
      }
    }
    return sameEncoding;
  }
}
