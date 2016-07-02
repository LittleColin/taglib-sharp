//
// FileParser.cs: Provides methods for reading important information from an
// MPEG-4 file.
//
// Author:
//   Brian Nickel (brian.nickel@gmail.com)
//
// Copyright (C) 2006-2007 Brian Nickel
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

namespace TagLib.Mpeg4
{
    /// <summary>
    ///    This class provides methods for reading important information
    ///    from an MPEG-4 file.
    /// </summary>
    public class FileParser
    {
        /// <summary>
        ///    Contains the file to read from.
        /// </summary>
        private readonly TagLib.File _file;

        /// <summary>
        ///    Contains the first header found in the file.
        /// </summary>
        private BoxHeader _firstHeader;

        /// <summary>
        ///    Contains the ISO movie header box.
        /// </summary>
        private IsoMovieHeaderBox _mvhdBox;

        /// <summary>
        ///    Contains the ISO user data boxes.
        /// </summary>
        private readonly List<IsoUserDataBox> _udtaBoxes = new List<IsoUserDataBox>();

        /// <summary>
        ///    Contains the box headers from the top of the file to the
        ///    "moov" box.
        /// </summary>
        private BoxHeader[] _moovTree;

        /// <summary>
        ///    Contains the box headers from the top of the file to the
        ///    "udta" box.
        /// </summary>
        private BoxHeader[] _udtaTree;

        /// <summary>
        ///    Contains the "stco" boxes found in the file.
        /// </summary>
        private readonly List<Box> _stcoBoxes = new List<Box>();

        /// <summary>
        ///    Contains the "stsd" boxes found in the file.
        /// </summary>
        private readonly List<Box> _stsdBoxes = new List<Box>();

        /// <summary>
        ///    Contains the position at which the "mdat" box starts.
        /// </summary>
        private long _mdatStart = -1;

        /// <summary>
        ///    Contains the position at which the "mdat" box ends.
        /// </summary>
        private long _mdatEnd = -1;

        /// <summary>
        ///    Constructs and initializes a new instance of <see
        ///    cref="FileParser" /> for a specified file.
        /// </summary>
        /// <param name="file">
        ///    A <see cref="TagLib.File" /> object to perform operations
        ///    on.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///    <paramref name="file" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="CorruptFileException">
        ///    <paramref name="file" /> does not start with a
        ///    "<c>ftyp</c>" box.
        /// </exception>
        public FileParser(TagLib.File file)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));

            _file = file;
            _firstHeader = new BoxHeader(file, 0);

            if (_firstHeader.BoxType != "ftyp")
                throw new CorruptFileException(
                    "File does not start with 'ftyp' box.");
        }

        /// <summary>
        ///    Gets the movie header box read by the current instance.
        /// </summary>
        /// <value>
        ///    A <see cref="IsoMovieHeaderBox" /> object read by the
        ///    current instance, or <see langword="null" /> if not found.
        /// </value>
        /// <remarks>
        ///    This value will only be set by calling <see
        ///    cref="ParseTagAndProperties()" />.
        /// </remarks>
        public IsoMovieHeaderBox MovieHeaderBox => _mvhdBox;

        /// <summary>
        ///    Gets all user data boxes read by the current instance.
        /// </summary>
        /// <value>
        ///    A <see cref="IsoUserDataBox" /> array read by the
        ///    current instance.
        /// </value>
        /// <remarks>
        ///    This value will only be set by calling <see
        ///    cref="ParseTag()" /> and <see
        ///    cref="ParseTagAndProperties()" />.
        /// </remarks>
        public IsoUserDataBox[] UserDataBoxes => _udtaBoxes.ToArray();

        public IsoUserDataBox UserDataBox => UserDataBoxes.Length == 0 ? null : UserDataBoxes[0];

        /// <summary>
        ///    Gets the audio sample entry read by the current instance.
        /// </summary>
        /// <value>
        ///    A <see cref="IsoAudioSampleEntry" /> object read by the
        ///    current instance, or <see langword="null" /> if not found.
        /// </value>
        /// <remarks>
        ///    This value will only be set by calling <see
        ///    cref="ParseTagAndProperties()" />.
        /// </remarks>
        public IsoAudioSampleEntry AudioSampleEntry
        {
            get
            {
                foreach (IsoSampleDescriptionBox box in _stsdBoxes)
                    foreach (Box sub in box.Children)
                    {
                        IsoAudioSampleEntry entry = sub
                            as IsoAudioSampleEntry;

                        if (entry != null)
                            return entry;
                    }
                return null;
            }
        }

        /// <summary>
        ///    Gets the visual sample entry read by the current
        ///    instance.
        /// </summary>
        /// <value>
        ///    A <see cref="IsoVisualSampleEntry" /> object read by the
        ///    current instance, or <see langword="null" /> if not found.
        /// </value>
        /// <remarks>
        ///    This value will only be set by calling <see
        ///    cref="ParseTagAndProperties()" />.
        /// </remarks>
        public IsoVisualSampleEntry VisualSampleEntry
        {
            get
            {
                foreach (IsoSampleDescriptionBox box in _stsdBoxes)
                    foreach (Box sub in box.Children)
                    {
                        IsoVisualSampleEntry entry = sub
                            as IsoVisualSampleEntry;

                        if (entry != null)
                            return entry;
                    }
                return null;
            }
        }

        /// <summary>
        ///    Gets the box headers for the first "<c>moov</c>" box and
        ///    all parent boxes up to the top of the file as read by the
        ///    current instance.
        /// </summary>
        /// <value>
        ///    A <see cref="BoxHeader[]" /> containing the headers for
        ///    the first "<c>moov</c>" box and its parent boxes up to
        ///    the top of the file, in the order they appear, or <see
        ///    langword="null" /> if none is present.
        /// </value>
        /// <remarks>
        ///    This value is useful for overwriting box headers, and is
        ///    only be set by calling <see cref="ParseBoxHeaders()" />.
        /// </remarks>
        public BoxHeader[] MoovTree => _moovTree;

        /// <summary>
        ///    Gets the box headers for the first "<c>udta</c>" box and
        ///    all parent boxes up to the top of the file as read by the
        ///    current instance.
        /// </summary>
        /// <value>
        ///    A <see cref="BoxHeader[]" /> containing the headers for
        ///    the first "<c>udta</c>" box and its parent boxes up to
        ///    the top of the file, in the order they appear, or <see
        ///    langword="null" /> if none is present.
        /// </value>
        /// <remarks>
        ///    This value is useful for overwriting box headers, and is
        ///    only be set by calling <see cref="ParseBoxHeaders()" />.
        /// </remarks>
        public BoxHeader[] UdtaTree => _udtaTree;

        /// <summary>
        ///    Gets all chunk offset boxes read by the current instance.
        /// </summary>
        /// <value>
        ///    A <see cref="Box[]" /> containing all chunk offset boxes
        ///    read by the current instance.
        /// </value>
        /// <remarks>
        ///    These boxes contain offset information for media data in
        ///    the current instance and can be devalidated by size
        ///    change operations, in which case they need to be
        ///    corrected. This value will only be set by calling <see
        ///    cref="ParseChunkOffsets()" />.
        /// </remarks>
        public Box[] ChunkOffsetBoxes => _stcoBoxes.ToArray();

        /// <summary>
        ///    Gets the position at which the "<c>mdat</c>" box starts.
        /// </summary>
        /// <value>
        ///    A <see cref="long" /> value containing the seek position
        ///    at which the "<c>mdat</c>" box starts.
        /// </value>
        /// <remarks>
        ///    The "<c>mdat</c>" box contains the media data for the
        ///    file and is used for estimating the invariant data
        ///    portion of the file.
        /// </remarks>
        public long MdatStartPosition => _mdatStart;

        /// <summary>
        ///    Gets the position at which the "<c>mdat</c>" box ends.
        /// </summary>
        /// <value>
        ///    A <see cref="long" /> value containing the seek position
        ///    at which the "<c>mdat</c>" box ends.
        /// </value>
        /// <remarks>
        ///    The "<c>mdat</c>" box contains the media data for the
        ///    file and is used for estimating the invariant data
        ///    portion of the file.
        /// </remarks>
        public long MdatEndPosition => _mdatEnd;

        /// <summary>
        ///    Parses the file referenced by the current instance,
        ///    searching for box headers that will be useful in saving
        ///    the file.
        /// </summary>
        public void ParseBoxHeaders()
        {
            try
            {
                ResetFields();
                ParseBoxHeaders(_firstHeader.TotalBoxSize,
                                 _file.Length, null);
            }
            catch (CorruptFileException e)
            {
                _file.MarkAsCorrupt(e.Message);
            }
        }

        /// <summary>
        ///    Parses the file referenced by the current instance,
        ///    searching for tags.
        /// </summary>
        public void ParseTag()
        {
            try
            {
                ResetFields();
                ParseTag(_firstHeader.TotalBoxSize, _file.Length, null);
            }
            catch (CorruptFileException e)
            {
                _file.MarkAsCorrupt(e.Message);
            }
        }

        /// <summary>
        ///    Parses the file referenced by the current instance,
        ///    searching for tags and properties.
        /// </summary>
        public void ParseTagAndProperties()
        {
            try
            {
                ResetFields();
                ParseTagAndProperties(_firstHeader.TotalBoxSize,
                                       _file.Length, null, null);
            }
            catch (CorruptFileException e)
            {
                _file.MarkAsCorrupt(e.Message);
            }
        }

        /// <summary>
        ///    Parses the file referenced by the current instance,
        ///    searching for chunk offset boxes.
        /// </summary>
        public void ParseChunkOffsets()
        {
            try
            {
                ResetFields();
                ParseChunkOffsets(_firstHeader.TotalBoxSize,
                                   _file.Length);
            }
            catch (CorruptFileException e)
            {
                _file.MarkAsCorrupt(e.Message);
            }
        }

        /// <summary>
        ///    Parses boxes for a specified range, looking for headers.
        /// </summary>
        /// <param name="start">
        ///    A <see cref="long" /> value specifying the seek position
        ///    at which to start reading.
        /// </param>
        /// <param name="end">
        ///    A <see cref="long" /> value specifying the seek position
        ///    at which to stop reading.
        /// </param>
        /// <param name="parents">
        ///    A <see cref="T:System.Collections.Generic.List`1" /> object containing all the parent
        ///    handlers that apply to the range.
        /// </param>
        private void ParseBoxHeaders(long start, long end,
                                      List<BoxHeader> parents)
        {
            BoxHeader header;

            for (long position = start; position < end;
                position += header.TotalBoxSize)
            {
                header = new BoxHeader(_file, position);

                if (_moovTree == null &&
                    header.BoxType == BoxType.Moov)
                {
                    List<BoxHeader> newParents = AddParent(
                        parents, header);
                    _moovTree = newParents.ToArray();
                    ParseBoxHeaders(
                        header.HeaderSize + position,
                        header.TotalBoxSize + position,
                        newParents);
                }
                else if (header.BoxType == BoxType.Mdia ||
                  header.BoxType == BoxType.Minf ||
                  header.BoxType == BoxType.Stbl ||
                  header.BoxType == BoxType.Trak)
                {
                    ParseBoxHeaders(
                        header.HeaderSize + position,
                        header.TotalBoxSize + position,
                        AddParent(parents, header));
                }
                else if (_udtaTree == null &&
                  header.BoxType == BoxType.Udta)
                {
                    // For compatibility, we still store the tree to the first udta
                    // block. The proper way to get this info is from the individual
                    // IsoUserDataBox.ParentTree member.
                    _udtaTree = AddParent(parents,
                        header).ToArray();
                }
                else if (header.BoxType == BoxType.Mdat)
                {
                    _mdatStart = position;
                    _mdatEnd = position + header.TotalBoxSize;
                }

                if (header.TotalBoxSize == 0)
                    break;
            }
        }

        /// <summary>
        ///    Parses boxes for a specified range, looking for tags.
        /// </summary>
        /// <param name="start">
        ///    A <see cref="long" /> value specifying the seek position
        ///    at which to start reading.
        /// </param>
        /// <param name="end">
        ///    A <see cref="long" /> value specifying the seek position
        ///    at which to stop reading.
        /// </param>
        private void ParseTag(long start, long end,
                                      List<BoxHeader> parents)
        {
            BoxHeader header;

            for (long position = start; position < end;
                position += header.TotalBoxSize)
            {
                header = new BoxHeader(_file, position);

                if (header.BoxType == BoxType.Moov)
                {
                    ParseTag(header.HeaderSize + position,
                        header.TotalBoxSize + position,
                        AddParent(parents, header));
                }
                else if (header.BoxType == BoxType.Mdia ||
                  header.BoxType == BoxType.Minf ||
                  header.BoxType == BoxType.Stbl ||
                  header.BoxType == BoxType.Trak)
                {
                    ParseTag(header.HeaderSize + position,
                        header.TotalBoxSize + position,
                        AddParent(parents, header));
                }
                else if (header.BoxType == BoxType.Udta)
                {
                    IsoUserDataBox udtaBox = BoxFactory.CreateBox(_file,
                    header) as IsoUserDataBox;

                    // Since we can have multiple udta boxes, save the parent for each one
                    List<BoxHeader> newParents = AddParent(
                        parents, header);
                    udtaBox.ParentTree = newParents.ToArray();

                    _udtaBoxes.Add(udtaBox);
                }
                else if (header.BoxType == BoxType.Mdat)
                {
                    _mdatStart = position;
                    _mdatEnd = position + header.TotalBoxSize;
                }

                if (header.TotalBoxSize == 0)
                    break;
            }
        }

        /// <summary>
        ///    Parses boxes for a specified range, looking for tags and
        ///    properties.
        /// </summary>
        /// <param name="start">
        ///    A <see cref="long" /> value specifying the seek position
        ///    at which to start reading.
        /// </param>
        /// <param name="end">
        ///    A <see cref="long" /> value specifying the seek position
        ///    at which to stop reading.
        /// </param>
        /// <param name="handler">
        ///    A <see cref="IsoHandlerBox" /> object that applied to the
        ///    range being searched.
        /// </param>
        private void ParseTagAndProperties(long start, long end,
                                            IsoHandlerBox handler, List<BoxHeader> parents)
        {
            BoxHeader header;

            for (long position = start; position < end;
                position += header.TotalBoxSize)
            {
                header = new BoxHeader(_file, position);
                ByteVector type = header.BoxType;

                if (type == BoxType.Moov)
                {
                    ParseTagAndProperties(header.HeaderSize + position,
                        header.TotalBoxSize + position,
                        handler,
                        AddParent(parents, header));
                }
                else if (type == BoxType.Mdia ||
                  type == BoxType.Minf ||
                  type == BoxType.Stbl ||
                  type == BoxType.Trak)
                {
                    ParseTagAndProperties(
                        header.HeaderSize + position,
                        header.TotalBoxSize + position,
                        handler,
                        AddParent(parents, header));
                }
                else if (type == BoxType.Stsd)
                {
                    _stsdBoxes.Add(BoxFactory.CreateBox(
                        _file, header, handler));
                }
                else if (type == BoxType.Hdlr)
                {
                    handler = BoxFactory.CreateBox(_file,
                        header, handler) as
                            IsoHandlerBox;
                }
                else if (_mvhdBox == null &&
                  type == BoxType.Mvhd)
                {
                    _mvhdBox = BoxFactory.CreateBox(_file,
                        header, handler) as
                            IsoMovieHeaderBox;
                }
                else if (type == BoxType.Udta)
                {
                    IsoUserDataBox udtaBox = BoxFactory.CreateBox(_file,
                        header, handler) as
                            IsoUserDataBox;

                    // Since we can have multiple udta boxes, save the parent for each one
                    List<BoxHeader> newParents = AddParent(
                        parents, header);
                    udtaBox.ParentTree = newParents.ToArray();

                    _udtaBoxes.Add(udtaBox);
                }
                else if (type == BoxType.Mdat)
                {
                    _mdatStart = position;
                    _mdatEnd = position + header.TotalBoxSize;
                }

                if (header.TotalBoxSize == 0)
                    break;
            }
        }

        /// <summary>
        ///    Parses boxes for a specified range, looking for chunk
        ///    offset boxes.
        /// </summary>
        /// <param name="start">
        ///    A <see cref="long" /> value specifying the seek position
        ///    at which to start reading.
        /// </param>
        /// <param name="end">
        ///    A <see cref="long" /> value specifying the seek position
        ///    at which to stop reading.
        /// </param>
        private void ParseChunkOffsets(long start, long end)
        {
            BoxHeader header;

            for (long position = start; position < end;
                position += header.TotalBoxSize)
            {
                header = new BoxHeader(_file, position);

                if (header.BoxType == BoxType.Moov)
                {
                    ParseChunkOffsets(
                        header.HeaderSize + position,
                        header.TotalBoxSize + position);
                }
                else if (header.BoxType == BoxType.Moov ||
                  header.BoxType == BoxType.Mdia ||
                  header.BoxType == BoxType.Minf ||
                  header.BoxType == BoxType.Stbl ||
                  header.BoxType == BoxType.Trak)
                {
                    ParseChunkOffsets(
                        header.HeaderSize + position,
                        header.TotalBoxSize + position);
                }
                else if (header.BoxType == BoxType.Stco ||
                  header.BoxType == BoxType.Co64)
                {
                    _stcoBoxes.Add(BoxFactory.CreateBox(
                        _file, header));
                }
                else if (header.BoxType == BoxType.Mdat)
                {
                    _mdatStart = position;
                    _mdatEnd = position + header.TotalBoxSize;
                }

                if (header.TotalBoxSize == 0)
                    break;
            }
        }

        /// <summary>
        ///    Resets all internal fields.
        /// </summary>
        private void ResetFields()
        {
            _mvhdBox = null;
            _udtaBoxes.Clear();
            _moovTree = null;
            _udtaTree = null;
            _stcoBoxes.Clear();
            _stsdBoxes.Clear();
            _mdatStart = -1;
            _mdatEnd = -1;
        }

        /// <summary>
        ///    Adds a parent to the end of an existing list of parents.
        /// </summary>
        /// <param name="parents">
        ///    A <see cref="T:System.Collections.Generic.List`1" /> object containing an existing
        ///    list of parents.
        /// </param>
        /// <param name="current">
        ///    A <see cref="BoxHeader" /> object to add to the list.
        /// </param>
        /// <returns>
        ///    A new <see cref="T:System.Collections.Generic.List`1" /> object containing the list
        ///    of parents, including the added header.
        /// </returns>
        private static List<BoxHeader> AddParent(List<BoxHeader> parents,
                                                  BoxHeader current)
        {
            List<BoxHeader> boxes = new List<BoxHeader>();
            if (parents != null)
                boxes.AddRange(parents);
            boxes.Add(current);
            return boxes;
        }
    }
}