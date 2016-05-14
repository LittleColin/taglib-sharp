//
// File.cs:
//
// Author:
//   Brian Nickel (brian.nickel@gmail.com)
//
// Copyright (C) 2007 Brian Nickel
//
// This library is free software; you can redistribute it and/or modify
// it  under the terms of the GNU Lesser General Public License version
// 2.1 as published by the Free Software Foundation.
//
// This library is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307
// USA
//

using System;
using TagLib.Id3v2;

namespace TagLib.Riff
{
    /// <summary>
    ///    This class extends <see cref="TagLib.File" /> to provide
    ///    support for reading and writing tags and properties for files
    ///    using the RIFF file format such as AVI and Wave files.
    /// </summary>
    [SupportedMimeType("taglib/avi", "avi")]
    [SupportedMimeType("taglib/wav", "wav")]
    [SupportedMimeType("taglib/divx", "divx")]
    [SupportedMimeType("video/avi")]
    [SupportedMimeType("video/msvideo")]
    [SupportedMimeType("video/x-msvideo")]
    [SupportedMimeType("image/avi")]
    [SupportedMimeType("application/x-troff-msvideo")]
    [SupportedMimeType("audio/avi")]
    [SupportedMimeType("audio/wav")]
    [SupportedMimeType("audio/wave")]
    [SupportedMimeType("audio/x-wav")]
    public class File : TagLib.File
    {
        /// <summary>
        ///  Contains all the tags of the file.
        /// </summary>
        private readonly CombinedTag _tag = new CombinedTag();

        /// <summary>
        ///  Contains the INFO tag.
        /// </summary>
        private InfoTag _infoTag = null;

        /// <summary>
        ///  Contains the MovieID tag.
        /// </summary>
        private MovieIdTag _midTag = null;

        /// <summary>
        ///  Contains the DivX tag.
        /// </summary>
        private DivXTag _divxTag = null;

        /// <summary>
        ///  Contains the Id3v2 tag.
        /// </summary>
        private Id3v2.Tag _id32Tag = null;

        /// <summary>
        ///  Contains the media properties.
        /// </summary>
        private Properties _properties = null;

        /// <summary>
        ///    The identifier used to recognize a RIFF files.
        /// </summary>
        /// <value>
        ///    "RIFF"
        /// </value>
        public static readonly ReadOnlyByteVector FileIdentifier = "RIFF";

        /// <summary>
        ///    Constructs and initializes a new instance of <see
        ///    cref="File" /> for a specified path in the local file
        ///    system and specified read style.
        /// </summary>
        /// <param name="path">
        ///    A <see cref="string" /> object containing the path of the
        ///    file to use in the new instance.
        /// </param>
        /// <param name="propertiesStyle">
        ///    A <see cref="ReadStyle" /> value specifying at what level
        ///    of accuracy to read the media properties, or <see
        ///    cref="ReadStyle.None" /> to ignore the properties.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///    <paramref name="path" /> is <see langword="null" />.
        /// </exception>
        public File(string path, ReadStyle propertiesStyle)
            : this(new LocalFileAbstraction(path),
                    propertiesStyle)
        {
        }

        /// <summary>
        ///    Constructs and initializes a new instance of <see
        ///    cref="File" /> for a specified path in the local file
        ///    system with an average read style.
        /// </summary>
        /// <param name="path">
        ///    A <see cref="string" /> object containing the path of the
        ///    file to use in the new instance.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///    <paramref name="path" /> is <see langword="null" />.
        /// </exception>
        public File(string path) : this(path, ReadStyle.Average)
        {
        }

        /// <summary>
        ///    Constructs and initializes a new instance of <see
        ///    cref="File" /> for a specified file abstraction and
        ///    specified read style.
        /// </summary>
        /// <param name="abstraction">
        ///    A <see cref="IFileAbstraction" /> object to use when
        ///    reading from and writing to the file.
        /// </param>
        /// <param name="propertiesStyle">
        ///    A <see cref="ReadStyle" /> value specifying at what level
        ///    of accuracy to read the media properties, or <see
        ///    cref="ReadStyle.None" /> to ignore the properties.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///    <paramref name="abstraction" /> is <see langword="null"
        ///    />.
        /// </exception>
        public File(IFileAbstraction abstraction,
                     ReadStyle propertiesStyle) : base(abstraction)
        {
            uint riffSize;
            long tagStart, tagEnd;

            Mode = AccessMode.Read;
            try
            {
                Read(true, propertiesStyle, out riffSize,
                    out tagStart, out tagEnd);
            }
            finally
            {
                Mode = AccessMode.Closed;
            }

            TagTypesOnDisk = TagTypes;

            GetTag(TagTypes.Id3V2, true);
            GetTag(TagTypes.RiffInfo, true);
            GetTag(TagTypes.MovieId, true);
            GetTag(TagTypes.DivX, true);
        }

        /// <summary>
        ///    Constructs and initializes a new instance of <see
        ///    cref="File" /> for a specified file abstraction with an
        ///    average read style.
        /// </summary>
        /// <param name="abstraction">
        ///    A <see cref="IFileAbstraction" /> object to use when
        ///    reading from and writing to the file.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///    <paramref name="abstraction" /> is <see langword="null"
        ///    />.
        /// </exception>
        public File(IFileAbstraction abstraction)
            : this(abstraction, ReadStyle.Average)
        { }

        /// <summary>
        ///    Gets a abstract representation of all tags stored in the
        ///    current instance.
        /// </summary>
        /// <value>
        ///    A <see cref="TagLib.Tag" /> object representing all tags
        ///    stored in the current instance.
        /// </value>
        public override Tag Tag => _tag;

        /// <summary>
        ///    Gets the media properties of the file represented by the
        ///    current instance.
        /// </summary>
        /// <value>
        ///    A <see cref="TagLib.Properties" /> object containing the
        ///    media properties of the file represented by the current
        ///    instance.
        /// </value>
        public override Properties Properties => _properties;

        /// <summary>
        ///    Saves the changes made in the current instance to the
        ///    file it represents.
        /// </summary>
        public override void Save()
        {
            Mode = AccessMode.Write;
            try
            {
                ByteVector data = new ByteVector();

                // Enclose the Id3v2 tag in an "ID32" item and
                // embed it as the first tag.
                if (_id32Tag != null)
                {
                    ByteVector tagData = _id32Tag.Render();
                    if (tagData.Count > 10)
                    {
                        if (tagData.Count % 2 == 1)
                            tagData.Add(0);
                        data.Add("ID32");
                        data.Add(ByteVector.FromUInt(
                            (uint)tagData.Count,
                            false));
                        data.Add(tagData);
                    }
                }

                // Embed "INFO" as the second tag.
                if (_infoTag != null)
                    data.Add(_infoTag.RenderEnclosed());

                // Embed "MID " as the third tag.
                if (_midTag != null)
                    data.Add(_midTag.RenderEnclosed());

                // Embed the DivX tag in "IDVX and embed it as
                // the fourth tag.
                if (_divxTag != null && !_divxTag.IsEmpty)
                {
                    ByteVector tagData = _divxTag.Render();
                    data.Add("IDVX");
                    data.Add(ByteVector.FromUInt(
                        (uint)tagData.Count, false));
                    data.Add(tagData);
                }

                // Read the file to determine the current RIFF
                // size and the area tagging does in.
                uint riffSize;
                long tagStart, tagEnd;
                Read(false, ReadStyle.None, out riffSize,
                    out tagStart, out tagEnd);

                // If tagging info cannot be found, place it at
                // the end of the file.
                if (tagStart < 12 || tagEnd < tagStart)
                    tagStart = tagEnd = Length;

                int length = (int)(tagEnd - tagStart);

                // If the tag isn't at the end of the file,
                // try appending using padding to improve
                // write time now or for subsequent writes.
                if (tagEnd != Length)
                {
                    int paddingSize = length - data.Count - 8;
                    if (paddingSize < 0)
                        paddingSize = 1024;

                    data.Add("JUNK");
                    data.Add(ByteVector.FromUInt(
                        (uint)paddingSize, false));
                    data.Add(new ByteVector(paddingSize));
                }

                // Insert the tagging data.
                Insert(data, tagStart, length);

                // If the data size changed, and the tagging
                // data is within the RIFF portion of the file,
                // update the riff size.
                if (data.Count - length != 0 &&
                    tagStart <= riffSize)
                    Insert(ByteVector.FromUInt((uint)
                        (riffSize + data.Count - length),
                        false), 4, 4);

                // Update the tag types.
                TagTypesOnDisk = TagTypes;
            }
            finally
            {
                Mode = AccessMode.Closed;
            }
        }

        /// <summary>
        ///    Removes a set of tag types from the current instance.
        /// </summary>
        /// <param name="types">
        ///    A bitwise combined <see cref="TagLib.TagTypes" /> value
        ///    containing tag types to be removed from the file.
        /// </param>
        /// <remarks>
        ///    In order to remove all tags from a file, pass <see
        ///    cref="TagTypes.AllTags" /> as <paramref name="types" />.
        /// </remarks>
        public override void RemoveTags(TagTypes types)
        {
            if ((types & TagTypes.Id3V2) != TagTypes.None)
                _id32Tag = null;
            if ((types & TagTypes.RiffInfo) != TagTypes.None)
                _infoTag = null;
            if ((types & TagTypes.MovieId) != TagTypes.None)
                _midTag = null;
            if ((types & TagTypes.DivX) != TagTypes.None)
                _divxTag = null;

            _tag.SetTags(_id32Tag, _infoTag, _midTag, _divxTag);
        }

        /// <summary>
        ///    Gets a tag of a specified type from the current instance,
        ///    optionally creating a new tag if possible.
        /// </summary>
        /// <param name="type">
        ///    A <see cref="TagLib.TagTypes" /> value indicating the
        ///    type of tag to read.
        /// </param>
        /// <param name="create">
        ///    A <see cref="bool" /> value specifying whether or not to
        ///    try and create the tag if one is not found.
        /// </param>
        /// <returns>
        ///    A <see cref="Tag" /> object containing the tag that was
        ///    found in or added to the current instance. If no
        ///    matching tag was found and none was created, <see
        ///    langword="null" /> is returned.
        /// </returns>
        public override Tag GetTag(TagTypes type, bool create)
        {
            Tag tag = null;

            switch (type)
            {
                case TagTypes.Id3V2:
                    if (_id32Tag == null && create)
                    {
                        _id32Tag = new Id3v2.Tag();
                        _id32Tag.Version = 4;
                        _id32Tag.Flags |= HeaderFlags
                            .FooterPresent;
                        _tag.CopyTo(_id32Tag, true);
                    }

                    tag = _id32Tag;
                    break;

                case TagTypes.RiffInfo:
                    if (_infoTag == null && create)
                    {
                        _infoTag = new InfoTag();
                        _tag.CopyTo(_infoTag, true);
                    }

                    tag = _infoTag;
                    break;

                case TagTypes.MovieId:
                    if (_midTag == null && create)
                    {
                        _midTag = new MovieIdTag();
                        _tag.CopyTo(_midTag, true);
                    }

                    tag = _midTag;
                    break;

                case TagTypes.DivX:
                    if (_divxTag == null && create)
                    {
                        _divxTag = new DivXTag();
                        _tag.CopyTo(_divxTag, true);
                    }

                    tag = _divxTag;
                    break;
            }

            _tag.SetTags(_id32Tag, _infoTag, _midTag, _divxTag);
            return tag;
        }

        /// <summary>
        ///    Reads the contents of the current instance determining
        ///    the size of the riff data, the area the tagging is in,
        ///    and optionally reading in the tags and media properties.
        /// </summary>
        /// <param name="readTags">
        ///    If <see langword="true" />, any tags found will be read
        ///    into the current instance.
        /// </param>
        /// <param name="style">
        ///    A <see cref="ReadStyle"/> value specifying how the media
        ///    data is to be read into the current instance.
        /// </param>
        /// <param name="riffSize">
        ///    A <see cref="uint"/> value reference to be filled with
        ///    the size of the RIFF data as read from the file.
        /// </param>
        /// <param name="tagStart">
        ///    A <see cref="long" /> value reference to be filled with
        ///    the absolute seek position at which the tagging data
        ///    starts.
        /// </param>
        /// <param name="tagEnd">
        ///    A <see cref="long" /> value reference to be filled with
        ///    the absolute seek position at which the tagging data
        ///    ends.
        /// </param>
        /// <exception cref="CorruptFileException">
        ///    The file does not begin with <see cref="FileIdentifier"
        ///    />.
        /// </exception>
        private void Read(bool readTags, ReadStyle style,
                           out uint riffSize, out long tagStart,
                           out long tagEnd)
        {
            Seek(0);
            if (ReadBlock(4) != FileIdentifier)
                throw new CorruptFileException(
                    "File does not begin with RIFF identifier");

            riffSize = ReadBlock(4).ToUInt(false);
            ByteVector streamFormat = ReadBlock(4);
            tagStart = -1;
            tagEnd = -1;

            long position = 12;
            long length = Length;
            uint size = 0;
            TimeSpan duration = TimeSpan.Zero;
            ICodec[] codecs = new ICodec[0];

            // Read until there are less than 8 bytes to read.
            do
            {
                bool tagFound = false;

                Seek(position);
                string fourcc = ReadBlock(4).ToString(StringType.Utf8);
                size = ReadBlock(4).ToUInt(false);

                switch (fourcc)
                {
                    // "fmt " is used by Wave files to hold the
                    // WaveFormatEx structure.
                    case "fmt ":
                        if (style == ReadStyle.None ||
                            streamFormat != "WAVE")
                            break;

                        Seek(position + 8);
                        codecs = new ICodec[] {
                        new WaveFormatEx (ReadBlock (18), 0)
                    };
                        break;

                    // "data" contains the audio data for wave
                    // files. It's contents represent the invariant
                    // portion of the file and is used to determine
                    // the duration of a file. It should always
                    // appear after "fmt ".
                    case "data":
                        if (streamFormat != "WAVE")
                            break;

                        InvariantStartPosition = position;
                        InvariantEndPosition = position + size;

                        if (style == ReadStyle.None ||
                            codecs.Length != 1 ||
                            !(codecs[0] is WaveFormatEx))
                            break;

                        duration += TimeSpan.FromSeconds(
                            size / (double)
                            ((WaveFormatEx)codecs[0])
                                .AverageBytesPerSecond);

                        break;

                    // Lists are used to store a variety of data
                    // collections. Read the type and act on it.
                    case "LIST":
                        {
                            switch (ReadBlock(4).ToString(StringType.Utf8))
                            {
                                // "hdlr" is used by AVI files to hold
                                // a media header and BitmapInfoHeader
                                // and WaveFormatEx structures.
                                case "hdrl":
                                    if (style == ReadStyle.None ||
                                        streamFormat != "AVI ")
                                        continue;

                                    AviHeaderList headerList =
                                        new AviHeaderList(this,
                                            position + 12,
                                            (int)(size - 4));
                                    duration = headerList.Header.Duration;
                                    codecs = headerList.Codecs;
                                    break;

                                // "INFO" is a tagging format handled by
                                // the InfoTag class.
                                case "INFO":
                                    if (readTags && _infoTag == null)
                                        _infoTag = new InfoTag(
                                            this,
                                            position + 12,
                                            (int)(size - 4));

                                    tagFound = true;
                                    break;

                                // "MID " is a tagging format handled by
                                // the MovieIdTag class.
                                case "MID ":
                                    if (readTags && _midTag == null)
                                        _midTag = new MovieIdTag(
                                            this,
                                            position + 12,
                                            (int)(size - 4));

                                    tagFound = true;
                                    break;

                                // "movi" contains the media data for
                                // and AVI and its contents represent
                                // the invariant portion of the file.
                                case "movi":
                                    if (streamFormat != "AVI ")
                                        break;

                                    InvariantStartPosition = position;
                                    InvariantEndPosition = position + size;
                                    break;
                            }
                            break;
                        }

                    // "ID32" is a custom box for this format that
                    // contains an ID3v2 tag.
                    case "ID32":
                        if (readTags && _id32Tag == null)
                            _id32Tag = new Id3v2.Tag(this,
                                position + 8);

                        tagFound = true;
                        break;

                    // "IDVX" is used by DivX and holds an ID3v1-
                    // style tag.
                    case "IDVX":
                        if (readTags && _divxTag == null)
                            _divxTag = new DivXTag(this,
                                position + 8);

                        tagFound = true;
                        break;

                    // "JUNK" is a padding element that could be
                    // associated with tag data.
                    case "JUNK":
                        if (tagEnd == position)
                            tagEnd = position + 8 + size;
                        break;
                }

                // Determine the region of the file that
                // contains tags.
                if (tagFound)
                {
                    if (tagStart == -1)
                    {
                        tagStart = position;
                        tagEnd = position + 8 + size;
                    }
                    else if (tagEnd == position)
                    {
                        tagEnd = position + 8 + size;
                    }
                }

                // Move to the next item.
            } while ((position += 8 + size) + 8 < length);

            // If we're reading properties, and one were found,
            // throw an exception. Otherwise, create the Properties
            // object.
            if (style != ReadStyle.None)
            {
                if (codecs.Length == 0)
                    throw new UnsupportedFormatException(
                        "Unsupported RIFF type.");

                _properties = new Properties(duration, codecs);
            }

            // If we're reading tags, update the combined tag.
            if (readTags)
                _tag.SetTags(_id32Tag, _infoTag, _midTag, _divxTag);
        }
    }
}