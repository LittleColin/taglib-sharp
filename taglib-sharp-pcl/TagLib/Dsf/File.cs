//
// File.cs: Provides tagging and properties support for the DSD (Direct Stream Digital) DSF
// file Format.
//
// Author:
//   Helmut Wahrmann
//
// Copyright (C) 2014 Helmut Wahrmann
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

namespace TagLib.Dsf
{
    /// <summary>
    ///    This class extends <see cref="TagLib.File" /> to provide
    ///    support for reading and writing tags and properties for files
    ///    using the AIFF file format.
    /// </summary>
    [SupportedMimeType("taglib/dsf", "dsf")]
    [SupportedMimeType("audio/x-dsf")]
    [SupportedMimeType("audio/dsf")]
    [SupportedMimeType("sound/dsf")]
    [SupportedMimeType("application/x-dsf")]
    public class File : TagLib.File
    {
        /// <summary>
        ///    Contains the address of the DSF header block.
        /// </summary>
        private ByteVector _headerBlock = null;

        /// <summary>
        ///  Contains the Id3v2 tag.
        /// </summary>
        private Id3v2.Tag _tag = null;

        /// <summary>
        ///  Contains the media properties.
        /// </summary>
        private Properties _properties = null;

        /// <summary>
        /// Contains the size of the DSF File
        /// </summary>
        private readonly uint _dsfSize = 0;

        /// <summary>
        /// Contains the start position of the Tag
        /// </summary>
        private long _tagStart;

        /// <summary>
        /// Contains the end position of the Tag
        /// </summary>
        private long _tagEnd;

        /// <summary>
        ///    The identifier used to recognize a DSF file.
        /// </summary>
        /// <value>
        ///    "DSD "
        /// </value>
        public static readonly ReadOnlyByteVector FileIdentifier = "DSD ";

        /// <summary>
        ///    The identifier used to recognize a Format chunk.
        /// </summary>
        /// <value>
        ///    "fmt "
        /// </value>
        public static readonly ReadOnlyByteVector FormatIdentifier = "fmt ";

        /// <summary>
        ///    The identifier used to recognize a DSF ID3 chunk.
        /// </summary>
        /// <value>
        ///    "ID3 "
        /// </value>
        public static readonly ReadOnlyByteVector Id3Identifier = "ID3";

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
        public File(string path)
            : this(path, ReadStyle.Average)
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
                    ReadStyle propertiesStyle)
            : base(abstraction)
        {
            Mode = AccessMode.Read;
            try
            {
                Read(true, propertiesStyle, out _dsfSize,
                     out _tagStart, out _tagEnd);
            }
            finally
            {
                Mode = AccessMode.Closed;
            }

            TagTypesOnDisk = TagTypes;
            GetTag(TagTypes.Id3V2, true);
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
        {
        }

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
                long originalTagLength = _tagEnd - _tagStart;
                ByteVector data = new ByteVector();

                if (_tag == null)
                {
                    // The tag has been removed
                    RemoveBlock(_tagStart, originalTagLength);
                    Insert(ByteVector.FromULong(0,
                                                false), 20, 8);
                }
                else
                {
                    data = _tag.Render();

                    // If tagging info cannot be found, place it at
                    // the end of the file.
                    if (_tagStart == 0 || _tagEnd < _tagStart)
                    {
                        _tagStart = _tagEnd = Length;
                        // Update the New Tag start
                        Insert(ByteVector.FromULong((ulong)(_tagStart),
                                                    false), 20, 8);
                    }

                    // Insert the tagging data.
                    Insert(data, _tagStart, data.Count);
                }

                long length = _dsfSize + data.Count - originalTagLength;

                // If the data size changed update the dsf  size.
                if (data.Count - originalTagLength != 0 &&
                    _tagStart <= _dsfSize)
                {
                    Insert(ByteVector.FromULong((ulong)(length),
                                                false), 12, 8);
                }
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
            if (types == TagTypes.Id3V2 ||
                types == TagTypes.AllTags)
            {
                _tag = null;
            }
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
            Tag id32Tag = null;

            switch (type)
            {
                case TagTypes.Id3V2:
                    if (_tag == null && create)
                    {
                        _tag = new Id3v2.Tag();
                        _tag.Version = 2;
                    }

                    id32Tag = _tag;
                    break;
            }

            return id32Tag;
        }

        /// <summary>
        ///    Reads the contents of the current instance determining
        ///    the size of the dsf data, the area the tagging is in,
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
        /// <param name="dsfSize">
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
                            out uint dsfSize, out long tagStart,
                            out long tagEnd)
        {
            Seek(0);
            if (ReadBlock(4) != FileIdentifier)
                throw new CorruptFileException(
                    "File does not begin with DSF identifier");

            Seek(12);
            dsfSize = ReadBlock(8).ToUInt(false);

            tagStart = (long)ReadBlock(8).ToULong(false);
            tagEnd = -1;

            // Get the properties of the file
            if (_headerBlock == null &&
                style != ReadStyle.None)
            {
                long fmtChunkPos = Find(FormatIdentifier, 0);

                if (fmtChunkPos == -1)
                {
                    throw new CorruptFileException(
                        "No Format chunk available in DSF file.");
                }

                Seek(fmtChunkPos);
                _headerBlock = ReadBlock((int)StreamHeader.Size);

                StreamHeader header = new StreamHeader(_headerBlock, dsfSize);
                _properties = new Properties(TimeSpan.Zero, header);
            }

            // Now position to the ID3 chunk, which we read before
            if (tagStart > 0)
            {
                Seek(tagStart);
                if (ReadBlock(3) == Id3Identifier)
                {
                    if (readTags && _tag == null)
                    {
                        _tag = new Id3v2.Tag(this, tagStart);
                    }

                    // Get the length of the tag out of the ID3 chunk
                    Seek(tagStart + 6);
                    uint tagSize = SynchData.ToUInt(ReadBlock(4)) + 10;

                    InvariantStartPosition = tagStart;
                    tagEnd = InvariantEndPosition = tagStart + tagSize;
                }
            }
        }
    }
}