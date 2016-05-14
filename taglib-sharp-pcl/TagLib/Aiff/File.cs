//
// File.cs: Provides tagging and properties support for Apple's AIFF
// files.
//
// Author:
//   Helmut Wahrmann
//
// Copyright (C) 2009 Helmut Wahrmann
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

namespace TagLib.Aiff
{
    /// <summary>
    ///    This class extends <see cref="TagLib.File" /> to provide
    ///    support for reading and writing tags and properties for files
    ///    using the AIFF file format.
    /// </summary>
    [SupportedMimeType("taglib/aif", "aif")]
    [SupportedMimeType("audio/x-aiff")]
    [SupportedMimeType("audio/aiff")]
    [SupportedMimeType("sound/aiff")]
    [SupportedMimeType("application/x-aiff")]
    public class File : TagLib.File
    {
        /// <summary>
        ///    Contains the address of the AIFF header block.
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
        ///    The identifier used to recognize a AIFF files.
        /// </summary>
        /// <value>
        ///    "FORM"
        /// </value>
        public static readonly ReadOnlyByteVector FileIdentifier = "FORM";

        /// <summary>
        ///    The identifier used to recognize a AIFF Common chunk.
        /// </summary>
        /// <value>
        ///    "COMM"
        /// </value>
        public static readonly ReadOnlyByteVector CommIdentifier = "COMM";

        /// <summary>
        ///    The identifier used to recognize a AIFF Sound Data Chunk.
        /// </summary>
        /// <value>
        ///    "SSND"
        /// </value>
        public static readonly ReadOnlyByteVector SoundIdentifier = "SSND";

        /// <summary>
        ///    The identifier used to recognize a AIFF ID3 chunk.
        /// </summary>
        /// <value>
        ///    "ID3 "
        /// </value>
        public static readonly ReadOnlyByteVector Id3Identifier = "ID3 ";

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
                uint aiffSize;
                long tagStart, tagEnd;
                Read(true, propertiesStyle, out aiffSize,
                     out tagStart, out tagEnd);
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
                ByteVector data = new ByteVector();

                // Add the ID3 chunk and ID32 tag to the vector
                if (_tag != null)
                {
                    ByteVector tagData = _tag.Render();
                    if (tagData.Count > 10)
                    {
                        if (tagData.Count % 2 == 1)
                            tagData.Add(0);

                        data.Add("ID3 ");
                        data.Add(ByteVector.FromUInt(
                                     (uint)tagData.Count,
                                     true));
                        data.Add(tagData);
                    }
                }

                // Read the file to determine the current AIFF
                // size and the area tagging is in.
                uint aiffSize;
                long tagStart, tagEnd;
                Read(false, ReadStyle.None, out aiffSize,
                     out tagStart, out tagEnd);

                // If tagging info cannot be found, place it at
                // the end of the file.
                if (tagStart < 12 || tagEnd < tagStart)
                    tagStart = tagEnd = Length;

                int length = (int)(tagEnd - tagStart + 8);

                // Insert the tagging data.
                Insert(data, tagStart, length);

                // If the data size changed update the aiff size.
                if (data.Count - length != 0 &&
                    tagStart <= aiffSize)
                {
                    // Depending, if a Tag has been added or removed,
                    // the length needs to be adjusted
                    if (_tag == null)
                    {
                        length -= 16;
                    }
                    else
                    {
                        length -= 8;
                    }

                    Insert(ByteVector.FromUInt((uint)
                                               (aiffSize + data.Count - length),
                                               true), 4, 4);
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
        /// <param name="aiffSize">
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
                          out uint aiffSize, out long tagStart,
                          out long tagEnd)
        {
            Seek(0);
            if (ReadBlock(4) != FileIdentifier)
                throw new CorruptFileException(
                    "File does not begin with AIFF identifier");

            aiffSize = ReadBlock(4).ToUInt(true);
            tagStart = -1;
            tagEnd = -1;

            // Get the properties of the file
            if (_headerBlock == null &&
                style != ReadStyle.None)
            {
                long commonChunkPos = Find(CommIdentifier, 0);

                if (commonChunkPos == -1)
                {
                    throw new CorruptFileException(
                        "No Common chunk available in AIFF file.");
                }

                Seek(commonChunkPos);
                _headerBlock = ReadBlock((int)StreamHeader.Size);

                StreamHeader header = new StreamHeader(_headerBlock, aiffSize);
                _properties = new Properties(TimeSpan.Zero, header);
            }

            // Now we search for the ID3 chunk.
            // Normally it appears after the Sound data chunk. But as the order of
            // chunks is free, it might be the case that the ID3 chunk appears before
            // the sound data chunk.
            // So we search first for the Sound data chunk and see, if an ID3 chunk appears before
            long id3ChunkPos = -1;
            long soundChunkPos = Find(SoundIdentifier, 0, Id3Identifier);
            if (soundChunkPos == -1)
            {
                // The ID3 chunk appears before the Sound chunk
                id3ChunkPos = Find(Id3Identifier, 0);
            }

            // Now let's look for the Sound chunk again
            // Since a previous return value of -1 does mean, that the ID3 chunk was found first
            soundChunkPos = Find(SoundIdentifier, 0);
            if (soundChunkPos == -1)
            {
                throw new CorruptFileException(
                    "No Sound chunk available in AIFF file.");
            }

            // Get the length of the Sound chunk and use this as a start value to look for the ID3 chunk
            Seek(soundChunkPos + 4);
            ulong soundChunkLength = ReadBlock(4).ToULong(true);
            long startSearchPos = (long)soundChunkLength + soundChunkPos + 4;

            if (id3ChunkPos == -1)
            {
                id3ChunkPos = Find(Id3Identifier, startSearchPos);
            }

            if (id3ChunkPos > -1)
            {
                if (readTags && _tag == null)
                {
                    _tag = new Id3v2.Tag(this,
                                        id3ChunkPos + 8);
                }

                // Get the length of the tag out of the ID3 chunk
                Seek(id3ChunkPos + 4);
                uint tagSize = ReadBlock(4).ToUInt(true) + 8;

                tagStart = InvariantStartPosition = id3ChunkPos;
                tagEnd = InvariantEndPosition = tagStart + tagSize;
            }
        }
    }
}