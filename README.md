Template#
=========

Template# is a lightweight library for string templates.

The following scenario illustrates the kind of use this library was designed for:

>   We have a big library of audio files with bad filenames. We want to fix this
>   by naming the files like this: '[Track Number] - [Artist] - [Album] - [Title]'.

Template# would take a template like the one for song filenames above and compile
it. The _compilation_ is used to achieve faster string creation and makes sense
only when a large number of strings will be created.

## Example 1: simplicity

The simplest way to use Template# is with plain classes:

    var template = Templates.Compile<Song>("[C?<{0:00} of {1:00} - >Track Number,Tracks Count][Artist] - [C?<{0} - >Album][Title].[FileExtension]");
    List<Song> songs;
    ...
    foreach (Song song in songs) {
        Console.WriteLine(template.CreateString(song));
    }

Where the class `Song` could look something like this:

    public class Song
    {
        // This property maps to the parameter `Title`
        public string Title { get; }
        
        // With annotations one can modify name binding:
        [TemplateParameter(name = "Track Number")]
        public int? TrackNumber { get; }
    
        // The class `Artist` must have a `get` property `Name`.
        [TemplateParameter(property = "Name")]
        public Person Artist { get; }
        
        // The `Album` class must have the properties `Title`, `Year`, and `TracksCount`.
        [TemplateParameter(property = "Title")] // This will map the `Album` parameter to `Album.Title`
        [TemplateParameter(name = "Album Year", property = "Year")]
        [TemplateParameter(name = "Tracks Count", property = "TracksCount")]
        public Album Album { get; }
    }

The above will produce something like this:

    > ...
    > 01 of 12 - Queen - The Works - Radio Ga Ga.mp3
    > Ray Charles - I Can't Stop Loving You.ogg
    > ...

Note that placeholders like `[C?<format>parameter]` placeholer will be omitted if the `parameter` is `null` (i.e.: `C` stands for complex format and `?` stands for _conditional_).


## Example 2: speed

The most efficient way of using Template#:

    // Data lookup maps are functions which, when a given parameter, returns
    // another function called `lookup`. The lookup is used by the compiled
    // template to quickly access data for a given data source object.
    LookupMap<Song> dataLookup = parameter => {
        switch (parameter) {
        case "Artist":
            return song => song.Artist.Name;
        case "Album":
            return song => song.Album.Title;
        case "title":
            return song => song.Title;
        case "track number":
            return song => song.TrackNumber > 0 ? (object)song.TrackNumber : null;
        default:
            return null;
        }
    };
    // Compilation breaks the template into segments and also stores the
    // particular `lookup` functions for each parameter that appears in the
    // template. This makes the string creation a faster and suitable for
    // frequent calls.
    var template = Templates.Compile("...template...", dataLookup);
    StringBuilder sb = new StringBuilder();
    foreach (Song song in songs) {
        sb.Clear();
        template.CreateString(song, sb);
        Console.WriteLine(sb.ToString());
    }
