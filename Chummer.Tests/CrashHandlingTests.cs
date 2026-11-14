/*  This file is part of Chummer5a.
 *
 *  Chummer5a is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  Chummer5a is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with Chummer5a.  If not, see <http://www.gnu.org/licenses/>.
 *
 *  You can obtain the full source code for Chummer5a at
 *  https://github.com/chummer5a/chummer5a
 */

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Chummer.Tests
{
    [TestClass]
    public class CrashHandlingTests
    {
        private static readonly string[] s_astrKnownErrorLogKeys =
        {
            "visible-crash-id",
            "visible-build-type",
            "commandline",
            "visible-version",
            "chummer-version",
            "os-type",
            "human-readable-os-version",
            "visible-error-friendly",
            "visible-stacktrace",
            "installation-id",
            "option-upload-logs-set",
            "chummer-ui-language",
            "chummer-cultureinfo",
            "system-cultureinfo"
        };

        private const string TestExceptionMessage = "This is a test exception that the crash handler is supposed to be able to process.";

        [TestMethod]
        public void GenerateJsonCrashReportFromException()
        {
            try
            {
                // We need to throw our exception to generate the members we'll need to check in our JSON, just passing it is not enough
                throw new ArgumentException(TestExceptionMessage);
            }
#pragma warning disable MSTEST0058 // Do not use asserts in catch blocks
            catch (ArgumentException exTest)
            {
                string strJsonPath = Backend.CrashHandler.GenerateCrashDumpJson(exTest, DateTime.UtcNow);
                Assert.IsTrue(File.Exists(strJsonPath));

                JsonSerializerSettings objSerializerSettings = new JsonSerializerSettings
                {
                    Converters = new JsonConverter[] { new JsonDictionaryConverter() }
                };
                JsonSerializer objSerializer = JsonSerializer.Create(objSerializerSettings);
                IDictionary<string, object> dicParts;

                using (FileStream objFileStream = new FileStream(strJsonPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (StreamReader objStreamReader = new StreamReader(objFileStream))
                using (JsonReader objJsonReader = new JsonTextReader(objStreamReader))
                {
                    dicParts = objSerializer.Deserialize<IDictionary<string, object>>(objJsonReader);
                }

                Assert.IsTrue(dicParts.TryGetValue("_dicAttributes", out object objAttributes));
                if (objAttributes is IDictionary<string, object> dicAttributes)
                {
                    foreach (string strKnownKey in s_astrKnownErrorLogKeys)
                        Assert.IsTrue(dicAttributes.ContainsKey(strKnownKey));

                    foreach (KeyValuePair<string, object> kvpEntry in dicAttributes)
                    {
                        string strKey = kvpEntry.Key;
                        string strValue = kvpEntry.Value?.ToString() ?? string.Empty;
                        switch (strKey)
                        {
                            case "visible-error-friendly":
                                Assert.AreEqual(strValue, TestExceptionMessage.AnonymizePath());
                                break;
                            case "visible-stacktrace":
                                Assert.AreEqual(strValue, exTest.StackTrace.AnonymizePath());
                                break;
                            default:
                                Assert.IsFalse(string.IsNullOrEmpty(strValue));
                                break;
                        }
                    }
                }
                else
                    Assert.Fail("Attributes dictionary is not the expected type of IDictionary<string, object>.");
            }
#pragma warning restore MSTEST0058 // Do not use asserts in catch blocks
        }

        // Taken from https://stackoverflow.com/a/6417753 with some modifications
        private sealed class JsonDictionaryConverter : CustomCreationConverter<IDictionary<string, object>>
        {
            public override IDictionary<string, object> Create(Type objectType)
            {
                return new Dictionary<string, object>();
            }

            public override bool CanConvert(Type objectType)
            {
                // in addition to handling IDictionary<string, object>
                // we want to handle the deserialization of dict value
                // which is of type object
                return objectType == typeof(object) || base.CanConvert(objectType);
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                switch (reader.TokenType)
                {
                    case JsonToken.StartObject:
                    case JsonToken.Null:
                        return base.ReadJson(reader, objectType, existingValue, serializer);
                    case JsonToken.StartArray:
                        return serializer.Deserialize<List<Dictionary<string, object>>>(reader);
                    default:
                        return serializer.Deserialize(reader);
                }
            }
        }
    }
}
