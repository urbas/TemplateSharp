Template#
=========

>   __Template# is a simple and lightweight library for mass string creation through templates.__

Here is an example template (a song filename template):

    [TrackNumber] - [Artist] - [Album] - [Title]

Template# is useful when a lot of strings have to be created using the same template.

## Scenario

The following scenario illustrates the kind of use-case this library was designed for:

>   _Say we have a big library of audio files with bad filenames. We want to fix this
>   by naming the files like this: '[TrackNumber] - [Artist] - [Album] - [Title]'._

Elements enclosed within `[` and `]` brackets are called _placeholders_. These
are replaced by extracting concrete values from all the processed songs.
Template# provides an easy way to achieve this. Say we have a class called `Song`
which contains all concrete values for the above placeholders:

    public class Song
    {
        public string Title { get; }
        public string Album { get; }
        public string Artist { get; }
        public int? TrackNumber;
    }

Just having this class is enough to start building templated strings with it.
Template# looks up data for all the placeholders by inspecting the properties,
fields and even parameterless functions of the `Song` class. If Template# finds
a corresponding class member it will use it to fill out the placeholder with it.

__Here is a concrete example__:

    List<Song> songs = ...;
    var template = Templates.Compile<Song>("[TrackNumber] - [Artist] - [Album] - [Title]");
    foreach (Song song in songs) {
        Console.WriteLine(template.CreateString(song));
    }

The parameter `song` in the function call `template.CreateString(song)` is
called __data source object__. It contains the concrete values for parameters
in placeholders.

## Why is Template# fast?

Template# compiles its templates first. Afterwards the compiled templates can be
used repeatedly to create a lot of strings as swiftly as possible.

__But what happens in the compilation stage?__

The most important part of the compilation stage is baking-in of the binding of
placeholders with their concrete values. The concrete values are stored in the
data source object and have to be looked up for each data source object when the
strings are being created. We call this process _dynamic binding_.

However, dynamic binding can be quite slow if it is performed through
reflection. To avoid this Template# binds the placeholders with the `Song`'s
properties only once and creates a baked function which directly accesses the
property, field or function. This is achieved with the help of _expression
trees_. In essence, expression trees are functions which are created and
compiled at runtime.

## Example 1: simplicity

The simplest way to use Template# is with plain classes:

    var template = Templates.Compile<Song>("[?<{0:00} of {1:00} - >Track Number,Tracks Count][Artist] - [?<{0} - >Album][Title].[FileExtension]");
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

Note that placeholders like `[?<format>parameter]` placeholer will be omitted if the `parameter` is `null` (i.e.: `C` stands for complex format and `?` stands for _conditional_).


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
