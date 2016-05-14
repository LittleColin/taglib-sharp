//
// File.cs:
//
// Author:
//   Brian Nickel (brian.nickel@gmail.com)
//
// Original Source:
//   oggfile.cpp from TagLib
//
// Copyright (C) 2005-2007 Brian Nickel
// Copyright (C) 2003 Scott Wheeler (Original Implementation)
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

namespace TagLib.Ogg
{
    /// <summary>
    ///    This class extends <see cref="TagLib.File" /> to provide tagging
    ///    and properties support for Ogg files.
    /// </summary>
    [SupportedMimeType("taglib/ogg", "ogg")]
    [SupportedMimeType("taglib/oga", "oga")]
    [SupportedMimeType("taglib/ogv", "ogv")]
    [SupportedMimeType("taglib/opus", "opus")]
    [SupportedMimeType("application/ogg")]
    [SupportedMimeType("application/x-ogg")]
    [SupportedMimeType("audio/vorbis")]
    [SupportedMimeType("audio/x-vorbis")]
    [SupportedMimeType("audio/x-vorbis+ogg")]
    [SupportedMimeType("audio/ogg")]
    [SupportedMimeType("audio/x-ogg")]
    [SupportedMimeType("video/ogg")]
    [SupportedMimeType("video/x-ogm+ogg")]
    [SupportedMimeType("video/x-theora+ogg")]
    [SupportedMimeType("video/x-theora")]
    [SupportedMimeType("audio/opus")]
    [SupportedMimeType("audio/x-opus")]
    [SupportedMimeType("audio/x-opus+ogg")]
    public class File : TagLib.File
    {
        /// <summary>
        ///   Contains the tags for the file.
        /// </summary>
        private readonly GroupedComment _tag;

        /// <summary>
        ///    Contains the media properties.
        /// </summary>
        private Properties _properties;

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
            Mode = AccessMode.Read;
            try
            {
                _tag = new GroupedComment();
                Read(propertiesStyle);
                TagTypesOnDisk = TagTypes;
            }
            finally
            {
                Mode = AccessMode.Closed;
            }
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
        ///    Saves the changes made in the current instance to the
        ///    file it represents.
        /// </summary>
        public override void Save()
        {
            Mode = AccessMode.Write;
            try
            {
                long end;
                List<Page> pages = new List<Page>();
                Dictionary<uint, Bitstream> streams =
                    ReadStreams(pages, out end);
                Dictionary<uint, Paginator> paginators =
                    new Dictionary<uint, Paginator>();
                List<List<Page>> newPages =
                    new List<List<Page>>();
                Dictionary<uint, int> shifts =
                    new Dictionary<uint, int>();

                foreach (Page page in pages)
                {
                    uint id = page.Header.StreamSerialNumber;
                    if (!paginators.ContainsKey(id))
                        paginators.Add(id,
                            new Paginator(
                                streams[id].Codec));

                    paginators[id].AddPage(page);
                }

                foreach (uint id in paginators.Keys)
                {
                    paginators[id].SetComment(
                        _tag.GetComment(id));
                    int shift;
                    newPages.Add(new List<Page>(
                        paginators[id]
                            .Paginate(out shift)));
                    shifts.Add(id, shift);
                }

                ByteVector output = new ByteVector();
                bool empty;
                do
                {
                    empty = true;
                    foreach (List<Page> streamPages in newPages)
                    {
                        if (streamPages.Count == 0)
                            continue;

                        output.Add(streamPages[0].Render());
                        streamPages.RemoveAt(0);

                        if (streamPages.Count != 0)
                            empty = false;
                    }
                } while (!empty);

                Insert(output, 0, end);
                InvariantStartPosition = output.Count;
                InvariantEndPosition = Length;

                TagTypesOnDisk = TagTypes;

                Page.OverwriteSequenceNumbers(this,
                    output.Count, shifts);
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
            if ((types & TagTypes.Xiph)
                != TagTypes.None)
                _tag.Clear();
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
        public override Tag GetTag(TagTypes type,
                                           bool create)
        {
            if (type == TagTypes.Xiph)
                foreach (XiphComment comment in _tag.Comments)
                    return comment;

            return null;
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
        ///    Reads the file with a specified read style.
        /// </summary>
        /// <param name="propertiesStyle">
        ///    A <see cref="ReadStyle" /> value specifying at what level
        ///    of accuracy to read the media properties, or <see
        ///    cref="ReadStyle.None" /> to ignore the properties.
        /// </param>
        private void Read(ReadStyle propertiesStyle)
        {
            long end;
            Dictionary<uint, Bitstream> streams = ReadStreams(null,
                out end);
            List<ICodec> codecs = new List<ICodec>();
            InvariantStartPosition = end;
            InvariantEndPosition = Length;

            foreach (uint id in streams.Keys)
            {
                _tag.AddComment(id,
                    streams[id].Codec.CommentData);
                codecs.Add(streams[id].Codec);
            }

            if (propertiesStyle == ReadStyle.None)
                return;

            PageHeader lastHeader = LastPageHeader;

            TimeSpan duration = streams[lastHeader
                .StreamSerialNumber].GetDuration(
                    lastHeader.AbsoluteGranularPosition);
            _properties = new Properties(duration, codecs);
        }

        /// <summary>
        ///    Reads the file until all streams have finished their
        ///    property and tagging data.
        /// </summary>
        /// <param name="pages">
        ///    A <see cref="T:System.Collections.Generic.List`1"/>
        ///    object to be filled with <see cref="Page" /> objects as
        ///    they are read, or <see langword="null"/> if the pages
        ///    are not to be stored.
        /// </param>
        /// <param name="end">
        ///    A <see cref="long" /> value reference to be updated to
        ///    the postion of the first page not read by the current
        ///    instance.
        /// </param>
        /// <returns>
        ///    A <see cref="T:System.Collections.Generic.Dictionary`2"
        ///    /> object containing stream serial numbers as the keys
        ///    <see cref="Bitstream" /> objects as the values.
        /// </returns>
        private Dictionary<uint, Bitstream> ReadStreams(List<Page> pages,
                                                         out long end)
        {
            Dictionary<uint, Bitstream> streams =
                new Dictionary<uint, Bitstream>();
            List<Bitstream> activeStreams = new List<Bitstream>();

            long position = 0;

            do
            {
                Bitstream stream = null;
                Page page = new Page(this, position);

                if ((page.Header.Flags &
                    PageFlags.FirstPageOfStream) != 0)
                {
                    stream = new Bitstream(page);
                    streams.Add(page.Header
                        .StreamSerialNumber, stream);
                    activeStreams.Add(stream);
                }

                if (stream == null)
                    stream = streams[
                        page.Header.StreamSerialNumber];

                if (activeStreams.Contains(stream)
                    && stream.ReadPage(page))
                    activeStreams.Remove(stream);

                if (pages != null)
                    pages.Add(page);

                position += page.Size;
            } while (activeStreams.Count > 0);

            end = position;

            return streams;
        }

        /// <summary>
        ///    Gets the last page header in the file.
        /// </summary>
        /// <value>
        ///    A <see cref="PageHeader" /> object containing the last
        ///    page header in the file.
        /// </value>
        /// <remarks>
        ///    The last page header is used to determine the last
        ///    absolute granular position of a stream so the duration
        ///    can be calculated.
        /// </remarks>
        private PageHeader LastPageHeader
        {
            get
            {
                long lastPageHeaderOffset = RFind("OggS");

                if (lastPageHeaderOffset < 0)
                    throw new CorruptFileException(
                        "Could not find last header.");

                return new PageHeader(this,
                    lastPageHeaderOffset);
            }
        }
    }
}