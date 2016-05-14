//
// AviStream.cs:
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

namespace TagLib.Riff
{
    /// <summary>
    ///    This abstract class provides basic support for parsing a raw AVI
    ///    stream list.
    /// </summary>
    public abstract class AviStream
    {
        /// <summary>
        ///    Contains the stream header.
        /// </summary>
        private readonly AviStreamHeader _header;

        /// <summary>
        ///    Contains the stream codec information.
        /// </summary>
        private ICodec _codec;

        /// <summary>
        ///    Constructs and intializes a new instance of <see
        ///    cref="AviStream" /> with a specified stream header.
        /// </summary>
        /// <param name="header">
        ///   A <see cref="AviStreamHeader"/> object containing the
        ///   stream's header.
        /// </param>
        protected AviStream(AviStreamHeader header)
        {
            _header = header;
        }

        /// <summary>
        ///    Parses a stream list item.
        /// </summary>
        /// <param name="id">
        ///    A <see cref="ByteVector" /> object containing the item's
        ///    ID.
        /// </param>
        /// <param name="data">
        ///    A <see cref="ByteVector" /> object containing the item's
        ///    data.
        /// </param>
        /// <param name="start">
        ///    A <see cref="uint" /> value specifying the index in
        ///    <paramref name="data" /> at which the item data begins.
        /// </param>
        /// <param name="length">
        ///    A <see cref="uint" /> value specifying the length of the
        ///    item.
        /// </param>
        public virtual void ParseItem(ByteVector id, ByteVector data,
                                       int start, int length)
        {
        }

        /// <summary>
        ///    Gets the stream header.
        /// </summary>
        /// <value>
        ///    A <see cref="AviStreamHeader" /> object containing the
        ///    header information for the stream.
        /// </value>
        public AviStreamHeader Header => _header;

        /// <summary>
        ///    Gets the codec information.
        /// </summary>
        /// <value>
        ///    A <see cref="ICodec" /> object containing the codec
        ///    information for the stream.
        /// </value>
        public ICodec Codec
        {
            get { return _codec; }
            protected set { _codec = value; }
        }

        /// <summary>
        ///    Parses a raw AVI stream list and returns the stream
        ///    information.
        /// </summary>
        /// <param name="data">
        ///    A <see cref="ByteVector" /> object containing raw stream
        ///    list.
        /// </param>
        /// <returns>
        ///    A <see cref="AviStream" /> object containing stream
        ///    information.
        /// </returns>
        public static AviStream ParseStreamList(ByteVector data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            if (!data.StartsWith("strl"))
                return null;

            AviStream stream = null;
            int pos = 4;

            while (pos + 8 < data.Count)
            {
                ByteVector id = data.Mid(pos, 4);
                int blockLength = (int)data.Mid(pos + 4, 4)
                    .ToUInt(false);

                if (id == "strh" && stream == null)
                {
                    AviStreamHeader streamHeader =
                        new AviStreamHeader(data, pos + 8);
                    if (streamHeader.Type == "vids")
                        stream = new AviVideoStream(
                            streamHeader);
                    else if (streamHeader.Type == "auds")
                        stream = new AviAudioStream(
                            streamHeader);
                }
                else if (stream != null)
                {
                    stream.ParseItem(id, data, pos + 8, blockLength);
                }

                pos += blockLength + 8;
            }

            return stream;
        }
    }

    /// <summary>
    ///    This class extends <see cref="AviStream" /> to provide support
    ///    for reading audio stream data.
    /// </summary>
    public class AviAudioStream : AviStream
    {
        /// <summary>
        ///    Constructs and intializes a new instance of <see
        ///    cref="AviAudioStream" /> with a specified stream header.
        /// </summary>
        /// <param name="header">
        ///   A <see cref="AviStreamHeader"/> object containing the
        ///   stream's header.
        /// </param>
        public AviAudioStream(AviStreamHeader header)
            : base(header)
        {
        }

        /// <summary>
        ///    Parses a stream list item.
        /// </summary>
        /// <param name="id">
        ///    A <see cref="ByteVector" /> object containing the item's
        ///    ID.
        /// </param>
        /// <param name="data">
        ///    A <see cref="ByteVector" /> object containing the item's
        ///    data.
        /// </param>
        /// <param name="start">
        ///    A <see cref="uint" /> value specifying the index in
        ///    <paramref name="data" /> at which the item data begins.
        /// </param>
        /// <param name="length">
        ///    A <see cref="uint" /> value specifying the length of the
        ///    item.
        /// </param>
        public override void ParseItem(ByteVector id, ByteVector data,
                                        int start, int length)
        {
            if (id == "strf")
                Codec = new WaveFormatEx(data, start);
        }
    }

    /// <summary>
    ///    This class extends <see cref="AviStream" /> to provide support
    ///    for reading video stream data.
    /// </summary>
    public class AviVideoStream : AviStream
    {
        /// <summary>
        ///    Constructs and intializes a new instance of <see
        ///    cref="AviVideoStream" /> with a specified stream header.
        /// </summary>
        /// <param name="header">
        ///   A <see cref="AviStreamHeader"/> object containing the
        ///   stream's header.
        /// </param>
        public AviVideoStream(AviStreamHeader header)
            : base(header)
        {
        }

        /// <summary>
        ///    Parses a stream list item.
        /// </summary>
        /// <param name="id">
        ///    A <see cref="ByteVector" /> object containing the item's
        ///    ID.
        /// </param>
        /// <param name="data">
        ///    A <see cref="ByteVector" /> object containing the item's
        ///    data.
        /// </param>
        /// <param name="start">
        ///    A <see cref="uint" /> value specifying the index in
        ///    <paramref name="data" /> at which the item data begins.
        /// </param>
        /// <param name="length">
        ///    A <see cref="uint" /> value specifying the length of the
        ///    item.
        /// </param>
        public override void ParseItem(ByteVector id, ByteVector data,
                                        int start, int length)
        {
            if (id == "strf")
                Codec = new BitmapInfoHeader(data, start);
        }
    }

    /// <summary>
    ///    This structure provides a representation of a Microsoft
    ///    AviStreamHeader structure, minus the first 8 bytes.
    /// </summary>
    public struct AviStreamHeader
    {
        /// <summary>
        ///    Contains the stream type.
        /// </summary>
        private readonly ByteVector _type;

        /// <summary>
        ///    Contains the stream handler.
        /// </summary>
        private readonly ByteVector _handler;

        /// <summary>
        ///    Contains the flags.
        /// </summary>
        private readonly uint _flags;

        /// <summary>
        ///    Contains the priority.
        /// </summary>
        private readonly uint _priority;

        /// <summary>
        ///    Contains the initial frame count.
        /// </summary>
        private readonly uint _initialFrames;

        /// <summary>
        ///    Contains the scale.
        /// </summary>
        private readonly uint _scale;

        /// <summary>
        ///    Contains the rate.
        /// </summary>
        private readonly uint _rate;

        /// <summary>
        ///    Contains the start delay.
        /// </summary>
        private readonly uint _start;

        /// <summary>
        ///    Contains the stream length.
        /// </summary>
        private readonly uint _length;

        /// <summary>
        ///    Contains the suggested buffer size.
        /// </summary>
        private readonly uint _suggestedBufferSize;

        /// <summary>
        ///    Contains the quality (between 0 and 10,000).
        /// </summary>
        private readonly uint _quality;

        /// <summary>
        ///    Contains the sample size.
        /// </summary>
        private readonly uint _sampleSize;

        /// <summary>
        ///    Contains the position for the left side of the video.
        /// </summary>
        private readonly ushort _left;

        /// <summary>
        ///    Contains the position for the top side of the video.
        /// </summary>
        private readonly ushort _top;

        /// <summary>
        ///    Contains the position for the right side of the video.
        /// </summary>
        private readonly ushort _right;

        /// <summary>
        ///    Contains the position for the bottom side of the video.
        /// </summary>
        private readonly ushort _bottom;

        /// <summary>
        ///    Constructs and initializes a new instance of <see
        ///    cref="AviStreamHeader" /> by reading the raw structure
        ///    from the beginning of a <see cref="ByteVector" /> object.
        /// </summary>
        /// <param name="data">
        ///    A <see cref="ByteVector" /> object containing the raw
        ///    data structure.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///    <paramref name="data" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="CorruptFileException">
        ///    <paramref name="data" /> contains less than 56 bytes.
        /// </exception>
        [Obsolete("Use WaveFormatEx(ByteVector,int)")]
        public AviStreamHeader(ByteVector data) : this(data, 0)
        {
        }

        /// <summary>
        ///    Constructs and initializes a new instance of <see
        ///    cref="AviStreamHeader" /> by reading the raw structure
        ///    from a specified position in a <see cref="ByteVector" />
        ///    object.
        /// </summary>
        /// <param name="data">
        ///    A <see cref="ByteVector" /> object containing the raw
        ///    data structure.
        /// </param>
        /// <param name="offset">
        ///    A <see cref="int" /> value specifying the index in
        ///    <paramref name="data"/> at which the structure begins.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///    <paramref name="data" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///    <paramref name="offset" /> is less than zero.
        /// </exception>
        /// <exception cref="CorruptFileException">
        ///    <paramref name="data" /> contains less than 56 bytes at
        ///    <paramref name="offset" />.
        /// </exception>
        public AviStreamHeader(ByteVector data, int offset)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            if (offset < 0)
                throw new ArgumentOutOfRangeException(
                    nameof(offset));

            if (offset + 56 > data.Count)
                throw new CorruptFileException(
                    "Expected 56 bytes.");

            _type = data.Mid(offset, 4);
            _handler = data.Mid(offset + 4, 4);
            _flags = data.Mid(offset + 8, 4).ToUInt(false);
            _priority = data.Mid(offset + 12, 4).ToUInt(false);
            _initialFrames = data.Mid(offset + 16, 4).ToUInt(false);
            _scale = data.Mid(offset + 20, 4).ToUInt(false);
            _rate = data.Mid(offset + 24, 4).ToUInt(false);
            _start = data.Mid(offset + 28, 4).ToUInt(false);
            _length = data.Mid(offset + 32, 4).ToUInt(false);
            _suggestedBufferSize = data.Mid(offset + 36, 4).ToUInt(false);
            _quality = data.Mid(offset + 40, 4).ToUInt(false);
            _sampleSize = data.Mid(offset + 44, 4).ToUInt(false);
            _left = data.Mid(offset + 48, 2).ToUShort(false);
            _top = data.Mid(offset + 50, 2).ToUShort(false);
            _right = data.Mid(offset + 52, 2).ToUShort(false);
            _bottom = data.Mid(offset + 54, 2).ToUShort(false);
        }

        /// <summary>
        ///    Gets the stream type.
        /// </summary>
        /// <value>
        ///    A four-byte <see cref="ByteVector" /> object specifying
        ///    stream type.
        /// </value>
        public ByteVector Type => _type;

        /// <summary>
        ///    Gets the stream handler (codec) ID.
        /// </summary>
        /// <value>
        ///    A four-byte <see cref="ByteVector" /> object specifying
        ///    stream handler ID.
        /// </value>
        public ByteVector Handler => _handler;

        /// <summary>
        ///    Gets the stream flags.
        /// </summary>
        /// <value>
        ///    A <see cref="uint" /> value specifying stream flags.
        /// </value>
        public uint Flags => _flags;

        /// <summary>
        ///    Gets the stream priority.
        /// </summary>
        /// <value>
        ///    A <see cref="uint" /> value specifying stream priority.
        /// </value>
        public uint Priority => _priority;

        /// <summary>
        ///    Gets how far ahead audio is from video.
        /// </summary>
        /// <value>
        ///    A <see cref="uint" /> value specifying how far ahead
        ///    audio is from video.
        /// </value>
        public uint InitialFrames => _initialFrames;

        /// <summary>
        ///    Gets the scale of the stream.
        /// </summary>
        /// <value>
        ///    A <see cref="uint" /> value specifying the scale of the
        ///    stream.
        /// </value>
        /// <remarks>
        ///    Dividing <see cref="Rate"/> by <see cref="Scale" /> gives
        ///    the number of samples per second.
        /// </remarks>
        public uint Scale => _scale;

        /// <summary>
        ///    Gets the rate of the stream.
        /// </summary>
        /// <value>
        ///    A <see cref="uint" /> value specifying the rate of the
        ///    stream.
        /// </value>
        /// <remarks>
        ///    Dividing <see cref="Rate"/> by <see cref="Scale" /> gives
        ///    the number of samples per second.
        /// </remarks>
        public uint Rate => _rate;

        /// <summary>
        ///    Gets the start delay of the stream.
        /// </summary>
        /// <value>
        ///    A <see cref="uint" /> value specifying the start delay of
        ///    the stream.
        /// </value>
        public uint Start => _start;

        /// <summary>
        ///    Gets the length of the stream.
        /// </summary>
        /// <value>
        ///    A <see cref="uint" /> value specifying the length of the
        ///    stream.
        /// </value>
        public uint Length => _length;

        /// <summary>
        ///    Gets the suggested buffer size for the stream.
        /// </summary>
        /// <value>
        ///    A <see cref="uint" /> value specifying the buffer size.
        /// </value>
        public uint SuggestedBufferSize => _suggestedBufferSize;

        /// <summary>
        ///    Gets the quality of the stream data.
        /// </summary>
        /// <value>
        ///    A <see cref="uint" /> value specifying the quality of the
        ///    stream data between 0 and 10,000.
        /// </value>
        public uint Quality => _quality;

        /// <summary>
        ///    Gets the sample size of the stream data.
        /// </summary>
        /// <value>
        ///    A <see cref="uint" /> value specifying the sample size.
        /// </value>
        public uint SampleSize => _sampleSize;

        /// <summary>
        ///    Gets the position at which the left of the video is to
        ///    be displayed in the rectangle whose width is given in the
        ///    the file's <see cref="AviHeader"/>.
        /// </summary>
        /// <value>
        ///    A <see cref="ushort" /> value specifying the left
        ///    position.
        /// </value>
        public ushort Left => _left;

        /// <summary>
        ///    Gets the position at which the top of the video is to be
        ///    displayed in the rectangle whose height is given in the
        ///    the file's <see cref="AviHeader"/>.
        /// </summary>
        /// <value>
        ///    A <see cref="ushort" /> value specifying the top
        ///    position.
        /// </value>
        public ushort Top => _top;

        /// <summary>
        ///    Gets the position at which the right of the video is to
        ///    be displayed in the rectangle whose width is given in the
        ///    the file's <see cref="AviHeader"/>.
        /// </summary>
        /// <value>
        ///    A <see cref="ushort" /> value specifying the right
        ///    position.
        /// </value>
        public ushort Right => _right;

        /// <summary>
        ///    Gets the position at which the bottom of the video is
        ///    to be displayed in the rectangle whose height is given in
        ///    the file's <see cref="AviHeader"/>.
        /// </summary>
        /// <value>
        ///    A <see cref="ushort" /> value specifying the bottom
        ///    position.
        /// </value>
        public ushort Bottom => _bottom;
    }
}