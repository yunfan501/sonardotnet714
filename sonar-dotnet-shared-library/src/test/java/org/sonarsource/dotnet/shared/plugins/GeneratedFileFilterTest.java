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

import java.io.IOException;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.util.Arrays;
import java.util.HashSet;
import org.junit.Rule;
import org.junit.Test;
import org.sonar.api.batch.fs.InputFile;
import org.sonar.api.utils.log.LogTester;
import org.sonar.api.utils.log.LoggerLevel;

import static org.assertj.core.api.Assertions.assertThat;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.when;

public class GeneratedFileFilterTest {

  @Rule
  public LogTester logs = new LogTester();

  @Test
  public void accept_returns_false_for_autogenerated_files() throws IOException {
    // Arrange
    GeneratedFileFilter filter = createFilter(Paths.get("autogenerated"));

    // Act
    Boolean result = filter.accept(newInputFile("autogenerated"));

    // Assert
    assertThat(result).isFalse();
    assertThat(logs.logs(LoggerLevel.DEBUG)).contains("Skipping auto generated file: autogenerated");
  }

  @Test
  public void accept_returns_true_for_nonautogenerated_files() throws IOException {
    // Arrange
    GeneratedFileFilter filter = createFilter(Paths.get("c:\\autogenerated"));

    // Act
    Boolean result = filter.accept(newInputFile("File1"));

    // Assert
    assertThat(result).isTrue();
    assertThat(logs.logs(LoggerLevel.DEBUG)).isEmpty();
  }

  private InputFile newInputFile(String path) {
    InputFile file = mock(InputFile.class);
    when(file.path()).thenReturn(Paths.get(path));
    when(file.toString()).thenReturn(path);
    return file;
  }

  private GeneratedFileFilter createFilter(Path... generated) throws IOException {
    AbstractGlobalProtobufFileProcessor processor = mock(AbstractGlobalProtobufFileProcessor.class);
    when(processor.getGeneratedFilePaths()).thenReturn(new HashSet<>(Arrays.asList(generated)));

    return new GeneratedFileFilter(processor);
  }
}