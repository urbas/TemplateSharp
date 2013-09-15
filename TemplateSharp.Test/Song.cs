using System;

namespace Template.Text
{
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
}

