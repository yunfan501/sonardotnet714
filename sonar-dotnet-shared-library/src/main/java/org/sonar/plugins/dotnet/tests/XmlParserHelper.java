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
package org.sonar.plugins.dotnet.tests;

import javax.annotation.Nullable;
import javax.xml.stream.XMLStreamConstants;
import javax.xml.stream.XMLStreamException;
import javax.xml.stream.XMLStreamReader;
import java.io.File;
import java.io.FileInputStream;
import java.io.FileNotFoundException;
import java.io.InputStreamReader;
import java.io.IOException;
import java.nio.charset.StandardCharsets;
import org.sonarsource.analyzer.commons.xml.SafetyFactory;

public class XmlParserHelper implements AutoCloseable {

  private final File file;
  private final InputStreamReader reader;
  private final XMLStreamReader stream;

  XmlParserHelper(File file) {
    try {
      this.file = file;
      this.reader = new InputStreamReader(new FileInputStream(file), StandardCharsets.UTF_8);
      this.stream = SafetyFactory.createXMLInputFactory().createXMLStreamReader(reader);

    } catch (FileNotFoundException | XMLStreamException e) {
      throw new IllegalStateException(e);
    }
  }

  void checkRootTag(String name) {
    String rootTag = nextStartTag();

    if (!name.equals(rootTag)) {
      throw parseError("Missing root element <" + name + ">");
    }
  }

  @Nullable
  String nextStartTag() {
    try {
      while (stream.hasNext()) {
        if (stream.next() == XMLStreamConstants.START_ELEMENT) {
          return stream.getLocalName();
        }
      }

      return null;
    } catch (XMLStreamException e) {
      throw new IllegalStateException("Error while parsing the XML file: " + file.getAbsolutePath(), e);
    }
  }

  @Nullable
  String nextStartOrEndTag() {
    try {
      while (stream.hasNext()) {
        int next = stream.next();
        if (next == XMLStreamConstants.START_ELEMENT) {
          return "<" + stream.getLocalName() + ">";
        } else if (next == XMLStreamConstants.END_ELEMENT) {
          return "</" + stream.getLocalName() + ">";
        }
      }

      return null;
    } catch (XMLStreamException e) {
      throw new IllegalStateException("Error while parsing the XML file: " + file.getAbsolutePath(), e);
    }
  }

  void checkRequiredAttribute(String name, int expectedValue) {
    int actualValue = getRequiredIntAttribute(name);
    if (expectedValue != actualValue) {
      throw parseError("Expected \"" + expectedValue + "\" instead of \"" + actualValue + "\" for the \"" + name + "\" attribute");
    }
  }

  int getRequiredIntAttribute(String name) {
    String value = getRequiredAttribute(name);
    return tagToIntValue(name, value);
  }

  int getIntAttributeOrZero(String name) {
    String value = getAttribute(name);
    return value == null ? 0 : tagToIntValue(name, value);
  }

  int tagToIntValue(String name, String value) {
    try {
      return Integer.parseInt(value);
    } catch (NumberFormatException e) {
      throw parseError("Expected an integer instead of \"" + value + "\" for the attribute \"" + name + "\"");
    }
  }

  @Nullable
  Double getDoubleAttribute(String name) {
    String value = getAttribute(name);
    if (value == null) {
      return null;
    }

    try {
      value = value.replace(',', '.');
      return Double.parseDouble(value);
    } catch (NumberFormatException e) {
      throw parseError("Expected an double instead of \"" + value + "\" for the attribute \"" + name + "\"");
    }
  }

  String getRequiredAttribute(String name) {
    String value = getAttribute(name);
    if (value == null) {
      throw parseError("Missing attribute \"" + name + "\" in element <" + stream.getLocalName() + ">");
    }

    return value;
  }

  @Nullable
  String getAttribute(String name) {
    for (int i = 0; i < stream.getAttributeCount(); i++) {
      if (name.equals(stream.getAttributeLocalName(i))) {
        return stream.getAttributeValue(i);
      }
    }

    return null;
  }

  ParseErrorException parseError(String message) {
    return new ParseErrorException(message + " in " + file.getAbsolutePath() + " at line " + stream.getLocation().getLineNumber());
  }

  @Override
  public void close() throws IOException {
    reader.close();

    if (stream != null) {
      try {
        stream.close();
      } catch (XMLStreamException e) {
        throw new IllegalStateException(e);
      }
    }
  }

  XMLStreamReader stream() {
    return stream;
  }

}
