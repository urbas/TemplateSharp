// 
// TemplateEngineV1Test.cs
// 
// Author:
//   Matej Urbas <matej.urbas@gmail.com>
// 
// Copyright 2012 Matej Urbas
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

#if ENABLE_TESTS

using System;
using NUnit.Framework;
using System.Reflection;
using System.Collections.Generic;
using System.Text;

namespace Template.Text
{
    [TestFixture]
 	public class TemplateEngineV1Test
    {
        #region Test Templates
        private const string Template_Ok_1_General = @"[F?<00>Track Number][?< of {0:00}>Track Count][C?< - >Track Number][Artist] - [Album] [?<({0})>Album Year] - [Title] - Title uppercased: [TITLE], Title lowercased: [title], Title Length: [Title Length], TITLE length: [TITLE Length], Direct property access:[LowercasedTitle], Direct field access:[TrackNumber], Direct method access:[GetUppercasedTitle]";
        private const string Template_Ok_2_Simple = @"[F?<00>Track Number][?< of {0:00}>Track Count][C?< - >Track Number][Artist] - [Album] [?<({0})>Album Year] - [Title]";
        #endregion

        #region Test Data
        private List<Song> songs = new List<Song>();
        private List<Artist> artists = new List<Artist>();
        private List<Album> albums = new List<Album>();
        private List<string> expectedStrings = new List<string>();
        private List<string> expectedStrings_2_Simple = new List<string>();
        #endregion

        #region Test Lifecycle Methods
        public TemplateEngineV1Test()
        {
            albums.Add(new Album() { Title = "Rock Hard", TrackCount = 12, Year = 1996 });
            albums.Add(new Album() { Title = "Make Music, Not War", Year = 1803 });
            albums.Add(new Album() { Title = "Greatness of the Sung", TrackCount = 5 });

            artists.Add(new Artist() { Name = "Rockers" });
            artists.Add(new Artist() { Name = "Wolfgang Pretzl" });
            artists.Add(new Artist() { Name = "The Violin Maniacs" });

            songs.Add(new Song() { Title = "Soften me Gently", Artist = artists[0], Album = albums[0], TrackNumber = 3 });
            expectedStrings.Add(@"03 of 12 - Rockers - Rock Hard (1996) - Soften me Gently - Title uppercased: SOFTEN ME GENTLY, Title lowercased: soften me gently, Title Length: 16, TITLE length: 16, Direct property access:soften me gently, Direct field access:3, Direct method access:SOFTEN ME GENTLY");
            expectedStrings_2_Simple.Add(@"03 of 12 - Rockers - Rock Hard (1996) - Soften me Gently");

            songs.Add(new Song() { Title = "Die Wurst des Prinzips", Artist = artists[1], Album = albums[1] });
            expectedStrings.Add(@"Wolfgang Pretzl - Make Music, Not War (1803) - Die Wurst des Prinzips - Title uppercased: DIE WURST DES PRINZIPS, Title lowercased: die wurst des prinzips, Title Length: 22, TITLE length: 22, Direct property access:die wurst des prinzips, Direct field access:, Direct method access:DIE WURST DES PRINZIPS");
            expectedStrings_2_Simple.Add(@"Wolfgang Pretzl - Make Music, Not War (1803) - Die Wurst des Prinzips");

            songs.Add(new Song() { Title = "Look, I Have a Violin", Artist = artists[2], Album = albums[2], TrackNumber = 17 });
            expectedStrings.Add(@"17 of 05 - The Violin Maniacs - Greatness of the Sung  - Look, I Have a Violin - Title uppercased: LOOK, I HAVE A VIOLIN, Title lowercased: look, i have a violin, Title Length: 21, TITLE length: 21, Direct property access:look, i have a violin, Direct field access:17, Direct method access:LOOK, I HAVE A VIOLIN");
            expectedStrings_2_Simple.Add(@"17 of 05 - The Violin Maniacs - Greatness of the Sung  - Look, I Have a Violin");

            songs.Add(new Song() { Title = "I Saw a Platypus", Artist = artists[2] });
            expectedStrings.Add(@"The Violin Maniacs -   - I Saw a Platypus - Title uppercased: I SAW A PLATYPUS, Title lowercased: i saw a platypus, Title Length: 16, TITLE length: 16, Direct property access:i saw a platypus, Direct field access:, Direct method access:I SAW A PLATYPUS");
            expectedStrings_2_Simple.Add(@"The Violin Maniacs -   - I Saw a Platypus");

            songs.Add(new Song() { Title = "We Were Seated", Album = albums[0] });
            expectedStrings.Add(@" of 12 - Rock Hard (1996) - We Were Seated - Title uppercased: WE WERE SEATED, Title lowercased: we were seated, Title Length: 14, TITLE length: 14, Direct property access:we were seated, Direct field access:, Direct method access:WE WERE SEATED");
            expectedStrings_2_Simple.Add(@" of 12 - Rock Hard (1996) - We Were Seated");

            songs.Add(new Song() { Title = "On the Fringe of Steffy", Album = albums[1] });
            expectedStrings.Add(@" - Make Music, Not War (1803) - On the Fringe of Steffy - Title uppercased: ON THE FRINGE OF STEFFY, Title lowercased: on the fringe of steffy, Title Length: 23, TITLE length: 23, Direct property access:on the fringe of steffy, Direct field access:, Direct method access:ON THE FRINGE OF STEFFY");
            expectedStrings_2_Simple.Add(@" - Make Music, Not War (1803) - On the Fringe of Steffy");
        }
        #endregion

        #region Tests
        [Test]
        public void TestTemplates ()
        {
            TestTemplate(Template_Ok_1_General, (i, str) => { if (i < expectedStrings.Count) NUnit.Framework.Assert.AreEqual(expectedStrings[i], str, string.Format("At song number {0}", i)); });
            TestTemplate(Template_Ok_2_Simple, (i, str) => NUnit.Framework.Assert.AreEqual(expectedStrings_2_Simple[i], str, string.Format("At song number {0}", i)));
        }

        private void TestTemplate(string stringTempalte, Action<int, string> checkExpected)
        {
            var template = Templates.Compile<Song>(stringTempalte);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < songs.Count; i++) {
                sb.Clear();
                template.CreateString(sb, songs[i]);
                string str = sb.ToString();
                if (checkExpected != null)
                    checkExpected(i, str);
            }
        }
        #endregion
    }

    #region Data Source Classes
    public class Song
    {
        [TemplateParameter("title")]
        [TemplateParameter("Title Length", Property = "Length")]
        public string Title;

        [TemplateParameter(Property = "Name")]
        public Artist Artist { get; set; }

        [TemplateParameter("Track Number")]
        public int? TrackNumber;

        [TemplateParameter(Property = "Title")]
        [TemplateParameter("Album Year", Property = "Year")]
        [TemplateParameter("Track Count", Property = "TrackCount")]
        public Album Album { get; set; }

        [TemplateParameter("TITLE")]
        [TemplateParameter("TITLE Length", Property = "Length")]
        public string GetUppercasedTitle() {
            return (Title ?? "").ToUpper();
        }

        [TemplateParameter("title")]
        public string LowercasedTitle {
            get { return (Title ?? "").ToLower(); }
        }
    }

    public class Artist
    {
        public string Name { get; set; }
    }

    public class Album
    {
        public string Title { get; set; }
        public int? Year { get; set; }
        public int? TrackCount;
    }
    #endregion
}

#endif