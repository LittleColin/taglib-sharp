//
// File.cs: Provides tagging and properties support for Xiph's Flac audio files.
//
// Author:
//   Brian Nickel (brian.nickel@gmail.com)
//
// Original Source:
//   flacfile.cpp from TagLib
//
// Copyright (C) 2006-2007 Brian Nickel
// Copyright (C) 2003-2004 Allan Sandfeld Jensen (Original Implementation)
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
using System.Collections.Generic;
using System.IO;
using TagLib.Ogg;

namespace TagLib.Flac
{
    /// <summary>
    ///    This class extends <see cref="TagLib.NonContainer.File" /> to
    ///    provide tagging and properties support for Xiph's Flac audio
    ///    files.
    /// </summary>
    /// <remarks>
    ///    A <see cref="TagLib.Ogg.XiphComment" /> will be added
    ///    automatically to any file that doesn't contain one. This change
    ///    does not effect the physical file until <see cref="Save" /> is
    ///    called and can be reversed using the following method:
    ///    <code>file.RemoveTags (file.TagTypes &amp; ~file.TagTypesOnDisk);</code>
    /// </remarks>
    [SupportedMimeType("taglib/flac", "flac")]
    [SupportedMimeType("audio/x-flac")]
    [SupportedMimeType("application/x-flac")]
    [SupportedMimeType("audio/flac")]
    public class File : NonContainer.File
    {
        /// <summary>
        ///    Contains the Flac metadata tag.
        /// </summary>
        private Metadata _metadata = null;

        /// <summary>
        ///    Contains the combination of all file tags.
        /// </summary>
        private CombinedTag _tag = null;

        /// <summary>
        ///    Contains the Flac header block.
        /// </summary>
        private ByteVector _headerBlock = null;

        /// <summary>
        ///    Contains the stream start position.
        /// </summary>
        private long _streamStart = 0;

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
            : base(path, propertiesStyle)
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
            : base(path)
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
            : base(abstraction, propertiesStyle)
        {
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
            : base(abstraction)
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
        ///    Saves the changes made in the current instance to the
        ///    file it represents.
        /// </summary>
        public override void Save()
        {
            Mode = AccessMode.Write;
            try
            {
                // Update the tags at the beginning of the file.
                long metadataStart = StartTag.Write();
                long metadataEnd;

                // Get all the blocks, but don't read the data for ones
                // we're filling with stored data.
                IList<Block> oldBlocks = ReadBlocks(ref metadataStart,
                    out metadataEnd, BlockMode.Blacklist,
                    BlockType.XiphComment, BlockType.Picture);

                // Create new vorbis comments is they don't exist.
                GetTag(TagTypes.Xiph, true);

                // Create new blocks and add the basics.
                List<Block> newBlocks = new List<Block>();
                newBlocks.Add(oldBlocks[0]);

                // Add blocks we don't deal with from the file.
                foreach (Block block in oldBlocks)
                    if (block.Type != BlockType.StreamInfo &&
                        block.Type != BlockType.XiphComment &&
                        block.Type != BlockType.Picture &&
                        block.Type != BlockType.Padding)
                        newBlocks.Add(block);

                newBlocks.Add(new Block(BlockType.XiphComment,
                    (GetTag(TagTypes.Xiph, true) as
                        XiphComment).Render(false)));

                foreach (IPicture picture in _metadata.Pictures)
                {
                    if (picture == null)
                        continue;

                    newBlocks.Add(new Block(BlockType.Picture,
                        new Picture(picture).Render()));
                }

                // Get the length of the blocks.
                long length = 0;
                foreach (Block block in newBlocks)
                    length += block.TotalSize;

                // Find the padding size to avoid trouble. If that fails
                // make some.
                long paddingSize = metadataEnd - metadataStart -
                    BlockHeader.Size - length;
                if (paddingSize < 0)
                    paddingSize = 1024 * 4;

                // Add a padding block.
                if (paddingSize != 0)
                    newBlocks.Add(new Block(BlockType.Padding,
                        new ByteVector((int)paddingSize)));

                // Render the blocks.
                ByteVector blockData = new ByteVector();
                for (int i = 0; i < newBlocks.Count; i++)
                    blockData.Add(newBlocks[i].Render(
                        i == newBlocks.Count - 1));

                // Update the blocks.
                Insert(blockData, metadataStart, metadataEnd -
                    metadataStart);

                // Update the tags at the end of the file.
                EndTag.Write();

                TagTypesOnDisk = TagTypes;
            }
            finally
            {
                Mode = AccessMode.Closed;
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
            switch (type)
            {
                case TagTypes.Xiph:
                    return _metadata.GetComment(create, _tag);

                case TagTypes.FlacMetadata:
                    return _metadata;
            }

            Tag t = (base.Tag as NonContainer.Tag).GetTag(type);

            if (t != null || !create)
                return t;

            switch (type)
            {
                case TagTypes.Id3V1:
                    return EndTag.AddTag(type, Tag);

                case TagTypes.Id3V2:
                    return StartTag.AddTag(type, Tag);

                case TagTypes.Ape:
                    return EndTag.AddTag(type, Tag);

                default:
                    return null;
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
            if ((types & TagTypes.Xiph) != 0)
                _metadata.RemoveComment();

            if ((types & TagTypes.FlacMetadata) != 0)
                _metadata.Clear();

            base.RemoveTags(types);
        }

        /// <summary>
        ///    Reads format specific information at the start of the
        ///    file.
        /// </summary>
        /// <param name="start">
        ///    A <see cref="long" /> value containing the seek position
        ///    at which the tags end and the media data begins.
        /// </param>
        /// <param name="propertiesStyle">
        ///    A <see cref="ReadStyle" /> value specifying at what level
        ///    of accuracy to read the media properties, or <see
        ///    cref="ReadStyle.None" /> to ignore the properties.
        /// </param>
        protected override void ReadStart(long start,
                                           ReadStyle propertiesStyle)
        {
            long end;
            IList<Block> blocks = ReadBlocks(ref start, out end,
                BlockMode.Whitelist, BlockType.StreamInfo,
                BlockType.XiphComment, BlockType.Picture);
            _metadata = new Metadata(blocks);

            TagTypesOnDisk |= _metadata.TagTypes;

            if (propertiesStyle != ReadStyle.None)
            {
                // Check that the first block is a
                // METADATA_BLOCK_STREAMINFO.
                if (blocks.Count == 0 ||
                    blocks[0].Type != BlockType.StreamInfo)
                    throw new CorruptFileException(
                        "FLAC stream does not begin with StreamInfo.");

                // The stream exists from the end of the last
                // block to the end of the file.
                _streamStart = end;
                _headerBlock = blocks[0].Data;
            }
        }

        /// <summary>
        ///    Reads format specific information at the end of the
        ///    file.
        /// </summary>
        /// <param name="end">
        ///    A <see cref="long" /> value containing the seek position
        ///    at which the media data ends and the tags begin.
        /// </param>
        /// <param name="propertiesStyle">
        ///    A <see cref="ReadStyle" /> value specifying at what level
        ///    of accuracy to read the media properties, or <see
        ///    cref="ReadStyle.None" /> to ignore the properties.
        /// </param>
        protected override void ReadEnd(long end,
                                         ReadStyle propertiesStyle)
        {
            _tag = new CombinedTag(_metadata, base.Tag);
            GetTag(TagTypes.Xiph, true);
        }

        /// <summary>
        ///    Reads the audio properties from the file represented by
        ///    the current instance.
        /// </summary>
        /// <param name="start">
        ///    A <see cref="long" /> value containing the seek position
        ///    at which the tags end and the media data begins.
        /// </param>
        /// <param name="end">
        ///    A <see cref="long" /> value containing the seek position
        ///    at which the media data ends and the tags begin.
        /// </param>
        /// <param name="propertiesStyle">
        ///    A <see cref="ReadStyle" /> value specifying at what level
        ///    of accuracy to read the media properties, or <see
        ///    cref="ReadStyle.None" /> to ignore the properties.
        /// </param>
        /// <returns>
        ///    A <see cref="TagLib.Properties" /> object describing the
        ///    media properties of the file represented by the current
        ///    instance.
        /// </returns>
        protected override Properties ReadProperties(long start,
                                                      long end,
                                                      ReadStyle propertiesStyle)
        {
            StreamHeader header = new StreamHeader(_headerBlock,
                end - _streamStart);
            return new Properties(TimeSpan.Zero, header);
        }

        /// <summary>
        ///    Indicates whether or not the block types passed into
        ///    <see cref="ReadBlocks" /> are to be white-listed or
        ///    black-listed.
        /// </summary>
        private enum BlockMode
        {
            /// <summary>
            ///    All block types except those provided are to be
            ///    returned.
            /// </summary>
            Blacklist,

            /// <summary>
            ///    Only those block types provides should be
            ///    returned.
            /// </summary>
            Whitelist
        }

        /// <summary>
        ///    Reads all metadata blocks starting from the current
        ///    instance, starting at a specified position.
        /// </summary>
        /// <param name="start">
        ///    A <see cref="long" /> value reference specifying the
        ///    position at which to start searching for the blocks. This
        ///    will be updated to the position of the first block.
        /// </param>
        /// <param name="end">
        ///    A <see cref="long" /> value reference updated to the
        ///    position at which the last block ends.
        /// </param>
        /// <param name="mode">
        ///    A <see cref="BlockMode" /> value indicating whether to
        ///    white-list or black-list the contents of <paramref
        ///    name="types" />.
        /// </param>
        /// <param name="types">
        ///    A <see cref="BlockType[]" /> containing the types to look
        ///    for or not look for as specified by <paramref name="mode"
        ///    />.
        /// </param>
        /// <returns>
        ///    A <see cref="T:System.Collections.Generic.IList`1" /> object containing the blocks
        ///    read from the current instance.
        /// </returns>
        /// <exception cref="CorruptFileException">
        ///    "<c>fLaC</c>" could not be found.
        /// </exception>
        private IList<Block> ReadBlocks(ref long start, out long end,
                                         BlockMode mode,
                                         params BlockType[] types)
        {
            List<Block> blocks = new List<Block>();

            long startPosition = Find("fLaC", start);

            if (startPosition < 0)
                throw new CorruptFileException(
                    "FLAC stream not found at starting position.");

            end = start = startPosition + 4;

            Seek(start);

            BlockHeader header;

            do
            {
                header = new BlockHeader(ReadBlock((int)
                    BlockHeader.Size));

                bool found = false;
                foreach (BlockType type in types)
                    if (header.BlockType == type)
                    {
                        found = true;
                        break;
                    }

                if ((mode == BlockMode.Whitelist && found) ||
                    (mode == BlockMode.Blacklist && !found))
                    blocks.Add(new Block(header,
                        ReadBlock((int)
                            header.BlockSize)));
                else
                    Seek(header.BlockSize,
                        SeekOrigin.Current);

                end += header.BlockSize + BlockHeader.Size;
            } while (!header.IsLastBlock);

            return blocks;
        }
    }

    /// <summary>
    ///    This class extends <see cref="CombinedTag" /> to provide support
    ///    for reading and writing FLAC metadata boxes.
    /// </summary>
    /// <remarks>
    ///    At this point, only Xiph Comments and pictures are supported.
    /// </remarks>
    public class Metadata : CombinedTag
    {
        /// <summary>
        ///    Contains the pictures.
        /// </summary>
        private readonly List<IPicture> _pictures = new List<IPicture>();

        /// <summary>
        ///    Constructs and initializes a new instance of <see
        ///    cref="Metadata" /> using a collection of blocks.
        /// </summary>
        /// <param name="blocks">
        ///    A <see cref="T:System.Collections.Generic.List`1" /> object containing <see
        ///    cref="Block" /> objects to use in the new instance.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///    <paramref name="blocks" /> is <see langword="null" />.
        /// </exception>
        [Obsolete("Use Metadata(IEnumerable<Block>)")]
        public Metadata(List<Block> blocks)
            : this(blocks as IEnumerable<Block>)
        {
        }

        /// <summary>
        ///    Constructs and initializes a new instance of <see
        ///    cref="Metadata" /> using a collection of blocks.
        /// </summary>
        /// <param name="blocks">
        ///    A <see cref="T:System.Collections.Generic.IEnumerable`1" /> object enumerating <see
        ///    cref="Block" /> objects to use in the new instance.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///    <paramref name="blocks" /> is <see langword="null" />.
        /// </exception>
        public Metadata(IEnumerable<Block> blocks)
        {
            if (blocks == null)
                throw new ArgumentNullException(nameof(blocks));

            foreach (Block block in blocks)
            {
                if (block.Data.Count == 0)
                    continue;

                if (block.Type == BlockType.XiphComment)
                    AddTag(new XiphComment(block.Data));
                else if (block.Type == BlockType.Picture)
                    _pictures.Add(new Picture(block.Data));
            }
        }

        /// <summary>
        ///    Gets the first Xiph comment stored in the current
        ///    instance, optionally creating one if necessary.
        /// </summary>
        /// <param name="create">
        ///    A <see cref="bool" /> value indicating whether or not a
        ///    comment should be added if one cannot be found.
        /// </param>
        /// <param name="copy">
        ///    A <see cref="Tag" /> object containing the source tag to
        ///    copy the values from, or <see langword="null" /> to not
        ///    copy values.
        /// </param>
        /// <returns>
        ///    A <see cref="Ogg.XiphComment" /> object containing the
        ///    tag that was found in or added to the current instance.
        ///    If no matching tag was found and none was created, <see
        ///    langword="null" /> is returned.
        /// </returns>
        public XiphComment GetComment(bool create, Tag copy)
        {
            foreach (Tag t in Tags)
                if (t is XiphComment)
                    return t as XiphComment;

            if (!create)
                return null;

            XiphComment c = new XiphComment();

            if (copy != null)
                copy.CopyTo(c, true);

            AddTag(c);

            return c;
        }

        /// <summary>
        ///    Removes all child Xiph Comments from the current
        ///    instance.
        /// </summary>
        public void RemoveComment()
        {
            XiphComment c;

            while ((c = GetComment(false, null)) != null)
                RemoveTag(c);
        }

        /// <summary>
        ///    Gets the tag types contained in the current instance.
        /// </summary>
        /// <value>
        ///    A bitwise combined <see cref="TagLib.TagTypes" /> value
        ///    containing the tag types stored in the current instance.
        /// </value>
        public override TagTypes TagTypes => TagTypes.FlacMetadata | base.TagTypes;

        /// <summary>
        ///    Gets and sets a collection of pictures associated with
        ///    the media represented by the current instance.
        /// </summary>
        /// <value>
        ///    A <see cref="IPicture[]" /> containing a collection of
        ///    pictures associated with the media represented by the
        ///    current instance or an empty array if none are present.
        /// </value>
        public override IPicture[] Pictures
        {
            get { return _pictures.ToArray(); }
            set
            {
                _pictures.Clear();
                if (value != null)
                    _pictures.AddRange(value);
            }
        }

        /// <summary>
        ///    Clears the values stored in the current instance.
        /// </summary>
        public override void Clear()
        {
            _pictures.Clear();
        }
    }
}