//
// GeneralEncapsulatedObjectFrame.cs:
//
// Author:
//   Brian Nickel (brian.nickel@gmail.com)
//
// Original Source:
//   generalencapsulatedobjectframe.cpp from TagLib
//
// Copyright (C) 2007 Brian Nickel
// Copyright (C) 2007 Scott Wheeler (Original Implementation)
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

using System.Globalization;
using System.Text;

namespace TagLib.Id3v2
{
    /// <summary>
    ///    This class extends <see cref="Frame" />, implementing support for
    ///    ID3v2 General Encapsulated Object (GEOB) Frames.
    /// </summary>
    /// <remarks>
    ///    <para>A <see cref="GeneralEncapsulatedObjectFrame" /> should be
    ///    used for storing files and other objects relevant to the file but
    ///    not supported by other frames.</para>
    /// </remarks>
    public class GeneralEncapsulatedObjectFrame : Frame
    {
        /// <summary>
        ///    Contains the text encoding to use when rendering the
        ///    current instance.
        /// </summary>
        private StringType _encoding = Tag.DefaultEncoding;

        /// <summary>
        ///    Contains the mime type of <see cref="_data" />.
        /// </summary>
        private string _mimeType = null;

        /// <summary>
        ///    Contains the original file name.
        /// </summary>
        private string _fileName = null;

        /// <summary>
        ///    Contains the description.
        /// </summary>
        private string _description = null;

        /// <summary>
        ///    Contains the data.
        /// </summary>
        private ByteVector _data = null;

        /// <summary>
        ///    Constructs and initializes a new instance of <see
        ///    cref="GeneralEncapsulatedObjectFrame" /> with no
        ///    contents.
        /// </summary>
        public GeneralEncapsulatedObjectFrame()
            : base(FrameType.Geob, 4)
        {
        }

        /// <summary>
        ///    Constructs and initializes a new instance of <see
        ///    cref="GeneralEncapsulatedObjectFrame" /> by reading its
        ///    raw data in a specified ID3v2 version.
        /// </summary>
        /// <param name="data">
        ///    A <see cref="ByteVector" /> object starting with the raw
        ///    representation of the new frame.
        /// </param>
        /// <param name="version">
        ///    A <see cref="byte" /> indicating the ID3v2 version the
        ///    raw frame is encoded in.
        /// </param>
        public GeneralEncapsulatedObjectFrame(ByteVector data,
                                               byte version)
            : base(data, version)
        {
            SetData(data, 0, version, true);
        }

        /// <summary>
        ///    Constructs and initializes a new instance of <see
        ///    cref="GeneralEncapsulatedObjectFrame" /> by reading its
        ///    raw data in a specified ID3v2 version.
        /// </summary>
        /// <param name="data">
        ///    A <see cref="ByteVector" /> object containing the raw
        ///    representation of the new frame.
        /// </param>
        /// <param name="offset">
        ///    A <see cref="int" /> indicating at what offset in
        ///    <paramref name="data" /> the frame actually begins.
        /// </param>
        /// <param name="header">
        ///    A <see cref="FrameHeader" /> containing the header of the
        ///    frame found at <paramref name="offset" /> in the data.
        /// </param>
        /// <param name="version">
        ///    A <see cref="byte" /> indicating the ID3v2 version the
        ///    raw frame is encoded in.
        /// </param>
        protected internal GeneralEncapsulatedObjectFrame(ByteVector data,
                                                           int offset,
                                                           FrameHeader header,
                                                           byte version)
            : base(header)
        {
            SetData(data, offset, version, false);
        }

        /// <summary>
        ///    Gets and sets the text encoding to use when storing the
        ///    current instance.
        /// </summary>
        /// <value>
        ///    A <see cref="string" /> containing the text encoding to
        ///    use when storing the current instance.
        /// </value>
        /// <remarks>
        ///    This encoding is overridden when rendering if <see
        ///    cref="Tag.ForceDefaultEncoding" /> is <see
        ///    langword="true" /> or the render version does not support
        ///    it.
        /// </remarks>
        public StringType TextEncoding
        {
            get { return _encoding; }
            set { _encoding = value; }
        }

        /// <summary>
        ///    Gets and sets the mime-type of the object stored in the
        ///    current instance.
        /// </summary>
        /// <value>
        ///    A <see cref="string" /> containing the mime-type of the
        ///    object stored in the current instance.
        /// </value>
        public string MimeType
        {
            get
            {
                if (_mimeType != null)
                    return _mimeType;

                return string.Empty;
            }
            set { _mimeType = value; }
        }

        /// <summary>
        ///    Gets and sets the file name of the object stored in the
        ///    current instance.
        /// </summary>
        /// <value>
        ///    A <see cref="string" /> containing the file name of the
        ///    object stored in the current instance.
        /// </value>
        public string FileName
        {
            get
            {
                if (_fileName != null)
                    return _fileName;

                return string.Empty;
            }
            set { _fileName = value; }
        }

        /// <summary>
        ///    Gets and sets the description stored in the current
        ///    instance.
        /// </summary>
        /// <value>
        ///    A <see cref="string" /> containing the description
        ///    stored in the current instance.
        /// </value>
        /// <remarks>
        ///    There should only be one frame with a matching
        ///    description and type per tag.
        /// </remarks>
        public string Description
        {
            get
            {
                if (_description != null)
                    return _description;

                return string.Empty;
            }
            set { _description = value; }
        }

        /// <summary>
        ///    Gets and sets the object data stored in the current
        ///    instance.
        /// </summary>
        /// <value>
        ///    A <see cref="ByteVector" /> containing the object data
        ///    stored in the current instance.
        /// </value>
        public ByteVector Object
        {
            get { return _data != null ? _data : new ByteVector(); }
            set { _data = value; }
        }

        /// <summary>
        ///    Creates a text description of the current instance.
        /// </summary>
        /// <returns>
        ///    A <see cref="string" /> object containing a description
        ///    of the current instance.
        /// </returns>
        public override string ToString()
        {
            StringBuilder builder
                = new StringBuilder();

            if (Description.Length == 0)
            {
                builder.Append(Description);
                builder.Append(" ");
            }

            builder.AppendFormat(
                CultureInfo.InvariantCulture,
                "[{0}] {1} bytes", MimeType, Object.Count);

            return builder.ToString();
        }

        /// <summary>
        ///    Gets a specified encapsulated object frame from the
        ///    specified tag, optionally creating it if it does not
        ///    exist.
        /// </summary>
        /// <param name="tag">
        ///    A <see cref="Tag" /> object to search in.
        /// </param>
        /// <param name="description">
        ///    A <see cref="string" /> specifying the description to
        ///    match.
        /// </param>
        /// <param name="create">
        ///    A <see cref="bool" /> specifying whether or not to create
        ///    and add a new frame to the tag if a match is not found.
        /// </param>
        /// <returns>
        ///    A <see cref="GeneralEncapsulatedObjectFrame" /> object
        ///    containing the matching frame, or <see langword="null" />
        ///    if a match wasn't found and <paramref name="create" /> is
        ///    <see langword="false" />.
        /// </returns>
        public static GeneralEncapsulatedObjectFrame Get(Tag tag,
                                                          string description,
                                                          bool create)
        {
            GeneralEncapsulatedObjectFrame geob;
            foreach (Frame frame in tag.GetFrames(FrameType.Geob))
            {
                geob = frame as GeneralEncapsulatedObjectFrame;

                if (geob == null)
                    continue;

                if (geob.Description != description)
                    continue;

                return geob;
            }

            if (!create)
                return null;

            geob = new GeneralEncapsulatedObjectFrame();
            geob.Description = description;
            tag.AddFrame(geob);
            return geob;
        }

        /// <summary>
        ///    Populates the values in the current instance by parsing
        ///    its field data in a specified version.
        /// </summary>
        /// <param name="data">
        ///    A <see cref="ByteVector" /> object containing the
        ///    extracted field data.
        /// </param>
        /// <param name="version">
        ///    A <see cref="byte" /> indicating the ID3v2 version the
        ///    field data is encoded in.
        /// </param>
        /// <exception cref="CorruptFileException">
        ///    <paramref name="data" /> contains less than 5 bytes.
        /// </exception>
        protected override void ParseFields(ByteVector data,
                                             byte version)
        {
            if (data.Count < 4)
                throw new CorruptFileException(
                    "An object frame must contain at least 4 bytes.");

            int start = 0;

            _encoding = (StringType)data[start++];

            int end = data.Find(
                ByteVector.TextDelimiter(StringType.Latin1),
                start);

            if (end < start)
                return;

            _mimeType = data.ToString(StringType.Latin1, start,
                end - start);

            ByteVector delim = ByteVector.TextDelimiter(
                _encoding);
            start = end + 1;
            end = data.Find(delim, start, delim.Count);

            if (end < start)
                return;

            _fileName = data.ToString(_encoding, start,
                end - start);
            start = end + delim.Count;
            end = data.Find(delim, start, delim.Count);

            if (end < start)
                return;

            _description = data.ToString(_encoding, start,
                end - start);
            start = end + delim.Count;

            data.RemoveRange(0, start);
            _data = data;
        }

        /// <summary>
        ///    Renders the values in the current instance into field
        ///    data for a specified version.
        /// </summary>
        /// <param name="version">
        ///    A <see cref="byte" /> indicating the ID3v2 version the
        ///    field data is to be encoded in.
        /// </param>
        /// <returns>
        ///    A <see cref="ByteVector" /> object containing the
        ///    rendered field data.
        /// </returns>
        protected override ByteVector RenderFields(byte version)
        {
            StringType encoding = CorrectEncoding(_encoding,
                version);
            ByteVector v = new ByteVector();

            v.Add((byte)encoding);

            if (MimeType != null)
                v.Add(ByteVector.FromString(MimeType,
                    StringType.Latin1));
            v.Add(ByteVector.TextDelimiter(StringType.Latin1));

            if (FileName != null)
                v.Add(ByteVector.FromString(FileName,
                    encoding));
            v.Add(ByteVector.TextDelimiter(encoding));

            if (Description != null)
                v.Add(ByteVector.FromString(Description,
                    encoding));
            v.Add(ByteVector.TextDelimiter(encoding));

            v.Add(_data);
            return v;
        }

        /// <summary>
        ///    Creates a deep copy of the current instance.
        /// </summary>
        /// <returns>
        ///    A new <see cref="Frame" /> object identical to the
        ///    current instance.
        /// </returns>
        public override Frame Clone()
        {
            GeneralEncapsulatedObjectFrame frame =
                new GeneralEncapsulatedObjectFrame();
            frame._encoding = _encoding;
            frame._mimeType = _mimeType;
            frame._fileName = _fileName;
            frame._description = _description;
            if (_data != null)
                frame._data = new ByteVector(_data);
            return frame;
        }
    }
}