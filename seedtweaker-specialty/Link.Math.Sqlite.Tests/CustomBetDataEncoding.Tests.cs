// -----------------------------------------------------------------------
// <copyright file = "CustomBetDataEncoding.Tests.cs" company = "IGT">
//     Copyright (c) 2021 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Link.Math.Sqlite.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Collection.Generic;
    using NUnit.Framework;

    [TestFixture]
    public class CustomBetDataEncodingTests
    {
        [TestFixture]
        public class EncodingTests
        {
            [SetUp]
            public void Arrange()
            {
                encoder = new CustomBetDataEncoding();
            }

            private ICustomBetDataEncoding encoder;

            [Test]
            public void EmptyData()
            {
                var dataSample = new Dictionary<string, long>();
                string result = null;
                Assert.DoesNotThrow(() => result = encoder.Encode(new ReadOnlyDictionary<string, long>(dataSample)));
                Assert.AreEqual("", result);
            }

            [Test]
            public void NullData()
            {
                string result = null;
                Assert.DoesNotThrow(() => result = encoder.Encode(null));
                Assert.AreEqual("", result);
            }

            [Test]
            public void OrderedTest()
            {
                var dataSample = new Dictionary<string, long>()
                {
                    { "2", 2 },
                    { "0", 0 },
                    { "1", 1 },
                };
                string result = null;
                Assert.DoesNotThrow(() => result = encoder.Encode(new ReadOnlyDictionary<string, long>(dataSample)));
                Assert.AreEqual("{0:0,1:1,2:2}", result);
            }

            [Test]
            public void ValidData()
            {
                var dataSample = new Dictionary<string, long>()
                {
                    { "0", 0 },
                    { "1", 1 },
                    { "2", 2 },
                };
                string result = null;
                Assert.DoesNotThrow(() => result = encoder.Encode(new ReadOnlyDictionary<string, long>(dataSample)));
                Assert.AreEqual("{0:0,1:1,2:2}", result);
            }


            [Test]
            public void KeyNameDelimiterValues([Values("first,second", "first:second")] string keyName)
            {
                var dataSample = new Dictionary<string, long>()
                {
                    { keyName, 0 },
                };
                string result = null;
                Assert.Throws<FormatException>(() => result = encoder.Encode(new ReadOnlyDictionary<string, long>(dataSample)));
            }
        }

        [TestFixture]
        public class DecodingTests
        {
            [SetUp]
            public void Arrange()
            {
                decoder = new CustomBetDataEncoding();
            }

            private ICustomBetDataEncoding decoder;

            [Test]
            public void DuplicatePairs()
            {
                Assert.Throws<ArgumentException>(() => decoder.Decode("{0:0,0:1}"));
            }

            [Test]
            public void InvalidFormat([Values(",", "{", "}", "{1:1", "1:1}")]string encoding)
            {
                Assert.Throws<FormatException>(() => decoder.Decode(encoding));
            }

            [Test]
            public void InvalidArguments([Values("{,}", "{1:1,}")]string encoding)
            {
                Assert.Throws<ArgumentException>(() => decoder.Decode(encoding));
            }

            [Test]
            public void InvalidValue()
            {
                Assert.Throws<FormatException>(() => decoder.Decode("{key:value}"));
            }

            [Test]
            public void NullEmptyString([Values("", null)] string value)
            {
                IReadOnlyDictionary<string, long> result = null;

                Assert.DoesNotThrow(() => result = decoder.Decode(value));
                Assert.IsNotNull(result);
                Assert.AreEqual(0, result.Count());
            }

            [Test]
            public void ValidDecode()
            {
                IReadOnlyDictionary<string, long> result = null;

                Assert.DoesNotThrow(() => result = decoder.Decode("{0:0,1:1,2:2}"));
                Assert.IsNotNull(result);
                Assert.AreEqual(3, result.Count());
                Assert.AreEqual(0, result["0"]);
                Assert.AreEqual(1, result["1"]);
                Assert.AreEqual(2, result["2"]);
            }
        }
    }
}