// -----------------------------------------------------------------------
// <copyright file = "SqliteProgressiveWinGroupConverter.Tests.cs" company = "IGT">
//     Copyright (c) 2021 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Link.Math.Sqlite.Models.Converters.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Converters;
    using Evaluation.Data;
    using NUnit.Framework;

    [TestFixture]
    public class ProgressiveWinGroupConverterTests
    {
        [TestFixture]
        public class WhenCallingConvertToProgressiveWinGroupsTests
        {
            private ProgressiveWinGroupConverter progressiveWinGroupConverter;

            [SetUp]
            public void Arrange()
            {
                progressiveWinGroupConverter = new ProgressiveWinGroupConverter();
            }

            [Test]
            public void WithNullProgressiveInfo()
            {
                Assert.That(() =>
                {
                    progressiveWinGroupConverter.Convert((IEnumerable<ProgressiveWinGroup>)null);
                }, Throws.InstanceOf<ArgumentNullException>());
            }

            [Test]
            public void WithEmptyProgressiveInfo()
            {
                IEnumerable<ProgressiveWinGroup> progressiveWinGroups = null;

                Assert.That(() =>
                {
                    progressiveWinGroups = progressiveWinGroupConverter.Convert(string.Empty);
                }, Throws.Nothing);

                Assert.That(progressiveWinGroups.Any(), Is.False);
            }

            [Test]
            public void WithValidProgressiveInfo()
            {
                List<ProgressiveWinGroup> progressiveWinGroups = 
                    progressiveWinGroupConverter.Convert("0:2,1:1,2:6,3:1,4:1,5:4,6:1").ToList();

                Assert.That(progressiveWinGroups.Count, Is.EqualTo(7));
                Assert.That(progressiveWinGroups[0].Level, Is.EqualTo(0));
                Assert.That(progressiveWinGroups[0].Count, Is.EqualTo(2));
                Assert.That(progressiveWinGroups[1].Level, Is.EqualTo(1));
                Assert.That(progressiveWinGroups[1].Count, Is.EqualTo(1));
                Assert.That(progressiveWinGroups[2].Level, Is.EqualTo(2));
                Assert.That(progressiveWinGroups[2].Count, Is.EqualTo(6));
                Assert.That(progressiveWinGroups[3].Level, Is.EqualTo(3));
                Assert.That(progressiveWinGroups[3].Count, Is.EqualTo(1));
                Assert.That(progressiveWinGroups[4].Level, Is.EqualTo(4));
                Assert.That(progressiveWinGroups[4].Count, Is.EqualTo(1));
                Assert.That(progressiveWinGroups[5].Level, Is.EqualTo(5));
                Assert.That(progressiveWinGroups[5].Count, Is.EqualTo(4));
                Assert.That(progressiveWinGroups[6].Level, Is.EqualTo(6));
                Assert.That(progressiveWinGroups[6].Count, Is.EqualTo(1));
            }
        }

        [TestFixture]
        public class WhenConvertFromProgressiveWinGroupsTests
        {
            private ProgressiveWinGroupConverter progressiveWinGroupConverter;
            private List<ProgressiveWinGroup> validProgressiveWinGroups;

            [SetUp]
            public void Arrange()
            {
                progressiveWinGroupConverter = new ProgressiveWinGroupConverter();
                validProgressiveWinGroups = new List<ProgressiveWinGroup>
                {
                    new ProgressiveWinGroup(0, 2),
                    new ProgressiveWinGroup(1, 1),
                    new ProgressiveWinGroup(2, 6),
                    new ProgressiveWinGroup(3, 1),
                    new ProgressiveWinGroup(4, 1),
                    new ProgressiveWinGroup(5, 4),
                    new ProgressiveWinGroup(6, 1),
                };
            }

            [Test]
            public void WithNullProgressiveWinGroups()
            {
                Assert.That(() =>
                {
                    progressiveWinGroupConverter.Convert((IEnumerable<ProgressiveWinGroup>)null);
                }, Throws.InstanceOf<ArgumentNullException>());
            }

            [Test]
            public void WithEmptyProgressiveWinGroups()
            {
                string progressiveInfo = null;

                Assert.That(() =>
                {
                    progressiveInfo = progressiveWinGroupConverter.Convert(
                        new List<ProgressiveWinGroup>());
                }, Throws.Nothing);

                Assert.That(progressiveInfo, Is.EqualTo(string.Empty));
            }

            [Test]
            public void WithValidProgressiveWinGroups()
            {
                string progressiveInfo =
                    progressiveWinGroupConverter.Convert(validProgressiveWinGroups);

                Assert.That(progressiveInfo, Is.EqualTo("0:2,1:1,2:6,3:1,4:1,5:4,6:1"));
            }
        }
    }
}
