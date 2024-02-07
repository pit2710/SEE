﻿using NUnit.Framework;
using SEE.Utils;
using SEE.Utils.Paths;

namespace SEE.VCS
{
    /// <summary>
    /// Tests for <see cref="IVersionControl"/>.
    /// </summary>
    internal class TestVCS
    {
        /// <summary>
        /// The version control system under test.
        /// </summary>
        private IVersionControl vcs;

        /// <summary>
        /// To measure and print the performance. Comparisons between two
        /// revisions may be quite expensive for large repositories.
        /// </summary>
        Performance p;

        /// <summary>
        /// Sets up <see cref="vcs"/> before each test.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            p = Performance.Begin(nameof(TestVCS));
            // We are using our own git repository of SEE as a guinea pig.
            vcs = VersionControlFactory.GetVersionControl("git", DataPath.ProjectFolder());
            Assert.IsNotNull(vcs);
            Assert.IsTrue(vcs is GitVersionControl);
        }

        /// <summary>
        /// Resets <see cref="vcs"/> after each test.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            vcs = null;
            p.End(true);
        }


        /// <summary>
        /// A file existing revision 642531cb3cf12527135a81a8c466b30b3cb0e78f that was
        /// renamed to <see cref="gitVersionControl"/> in revision
        /// </summary>
        private const string versionControlSystems = "Assets/SEE/VCS/VersionControlSystems.cs";

        /// <summary>
        /// New filename of the successor of <see cref="versionControlSystems"/> that
        /// was renamed in revision 17357897093e352ab9d4e039af81a3cfd6eeb1e7.
        /// </summary>
        private const string gitVersionControl = "Assets/SEE/VCS/GitVersionControl.cs";

        /// <summary>
        /// Retrieve the file content of an existing version.
        /// </summary>
        [Test]
        public void TestShowGitVersionControl()
        {
            // The file was only renamed, but its content was not changed, hence its hash value
            // must be the same as before.
            AssertFile("Assets/SEE/VCS/GitVersionControl.cs", "17357897093e352ab9d4e039af81a3cfd6eeb1e7", -1400052609);
        }

        [Test]
        public void TestShowVersionControlSystems()
        {
            AssertFile("Assets/SEE/VCS/VersionControlSystems.cs", "642531cb3cf12527135a81a8c466b30b3cb0e78f", -1400052609);
        }

        /// <summary>
        /// Retrieves the content of <paramref name="fileName"/> in <paramref name="revision"/>
        /// and checks whether its hash value is equal to <paramref name="expectedHash"/>.
        /// </summary>
        /// <param name="fileName">path of file whose content is to be checked</param>
        /// <param name="revision">the revision in which to look up <paramref name="fileName"/></param>
        /// <param name="expectedHash">the expected has value of <paramref name="fileName"/>
        /// in <paramref name="revision"/></param>
        private void AssertFile(string fileName, string revision, int expectedHash)
        {
            string content = vcs.Show(fileName, revision);
            Assert.IsNotNull(content);
            Assert.AreNotEqual(0, content.Length);
            // Comparing the hash code is a convenient way to check whether we found the right file.
            Assert.AreEqual(expectedHash, content.GetHashCode());
        }

        /// <summary>
        /// Very old commit id in SEE dating back to 2019-08-12.
        /// </summary>
        private const string veryOldCommit = "acd278c48bdff127a737117035ec9fbce802dafb";

        /// <summary>
        /// Old commit id in SEE dating back to 2023-02-15.
        /// </summary>
        private const string oldCommit = "8bc77561d2a1f6f7142baa157a93027d752bfe82";

        /// <summary>
        /// Slightly older commit in SEE dating back to 2024-02-07.
        /// </summary>
        private const string slightlyOldCommit = "b8a992b11ed29d2551951c44b76d16b973f86388";

        /// <summary>
        /// New commit id in SEE dating back to 2024-02-07.
        /// This commit is newer than <see cref="veryOldCommit"/> and <see cref="slightlyOldCommit"/>.
        /// </summary>
        private const string newerCommit = "bfb9851d4469d3658bf43a7789601f81d29c18b8";

        [Test]
        public void TestRenamed()
        {
            Assert.AreEqual(Change.Renamed,
                            vcs.GetFileChange(gitVersionControl, slightlyOldCommit, newerCommit,
                                                            out string oldFilename));
            Assert.AreEqual(versionControlSystems, oldFilename);
        }

        [Test]
        public void TestAdded()
        {
            Assert.AreEqual(Change.Added,
                            vcs.GetFileChange(gitVersionControl, oldCommit, newerCommit,
                                                           out string oldFilename));
            Assert.IsNull(oldFilename);
        }

        [Test]
        public void TestDeleted()
        {
            const string filename = "Assets/Resources/Prefabs/Players/DesktopPlayer_BACKUP_565.prefab.meta";
            Assert.AreEqual(Change.Deleted,
                            vcs.GetFileChange(filename,
                                                            "e63d92e0506c5c38b213a7a3420b4fca0187cfc2",
                                                            "3de77399fcacf45a63094ebfb31ce708f03d1067",
                                                            out string oldFilename));
            Assert.AreEqual(filename, oldFilename);
        }

        [Test]
        public void TestUnknownNewCommitID()
        {
            Assert.Throws<UnknownCommitID>(() => vcs.GetFileChange(gitVersionControl, oldCommit, "DOESNOTEXIST", out string _));
        }

        [Test]
        public void TestUnknownOldCommitID()
        {
            Assert.Throws<UnknownCommitID>(() => vcs.GetFileChange(gitVersionControl, "DOESNOTEXIST", newerCommit, out string _));
        }

        [Test]
        public void TestUnknown()
        {
            Assert.AreEqual(Change.Unknown,
                            vcs.GetFileChange("THIS_FILE_DOES_NOT_EXIST", slightlyOldCommit, newerCommit,
                                                            out string _));
        }
    }
}
