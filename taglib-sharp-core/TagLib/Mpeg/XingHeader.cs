//
// XingHeader.cs: Provides information about a variable bitrate MPEG audio
// stream.
//
// Author:
//   Brian Nickel (brian.nickel@gmail.com)
//
// Original Source:
//   xingheader.cpp from TagLib
//
// Copyright (C) 2005-2007 Brian Nickel
// Copyright (C) 2003 by Ismael Orenstein (Original Implementation)
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

namespace TagLib.Mpeg
{
    /// <summary>
    ///    This structure provides information about a variable bitrate MPEG
    ///    audio stream.
    /// </summary>
    public struct XingHeader
    {
        /// <summary>
        ///    Contains the frame count.
        /// </summary>
        private readonly uint _frames;

        /// <summary>
        ///    Contains the stream size.
        /// </summary>
        private readonly uint _size;

        /// <summary>
        ///    Indicates that a physical Xing header is present.
        /// </summary>
        private readonly bool _present;

        /// <summary>
        ///    Contains te Xing identifier.
        /// </summary>
        /// <value>
        ///    "Xing"
        /// </value>
        public static readonly ReadOnlyByteVector FileIdentifier = "Xing";

        /// <summary>
        ///    An empty and unset Xing header.
        /// </summary>
        public static readonly XingHeader Unknown = new XingHeader(0, 0);

        /// <summary>
        ///    Constructs and initializes a new instance of <see
        ///    cref="XingHeader" /> with a specified frame count and
        ///    size.
        /// </summary>
        /// <param name="frame">
        ///    A <see cref="uint" /> value specifying the frame count of
        ///    the audio represented by the new instance.
        /// </param>
        /// <param name="size">
        ///    A <see cref="uint" /> value specifying the stream size of
        ///    the audio represented by the new instance.
        /// </param>
        private XingHeader(uint frame, uint size)
        {
            _frames = frame;
            _size = size;
            _present = false;
        }

        /// <summary>
        ///    Constructs and initializes a new instance of <see
        ///    cref="XingHeader" /> by reading its raw contents.
        /// </summary>
        /// <param name="data">
        ///    A <see cref="ByteVector" /> object containing the raw
        ///    Xing header.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///    <paramref name="data" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="CorruptFileException">
        ///    <paramref name="data" /> does not start with <see
        ///    cref="FileIdentifier" />.
        /// </exception>
        public XingHeader(ByteVector data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            // Check to see if a valid Xing header is available.
            if (!data.StartsWith(FileIdentifier))
                throw new CorruptFileException(
                    "Not a valid Xing header");

            int position = 8;

            if ((data[7] & 0x01) != 0)
            {
                _frames = data.Mid(position, 4).ToUInt();
                position += 4;
            }
            else
                _frames = 0;

            if ((data[7] & 0x02) != 0)
            {
                _size = data.Mid(position, 4).ToUInt();
                position += 4;
            }
            else
                _size = 0;

            _present = true;
        }

        /// <summary>
        ///    Gets the total number of frames in the file, as indicated
        ///    by the current instance.
        /// </summary>
        /// <value>
        ///    A <see cref="uint" /> value containing the number of
        ///    frames in the file, or <c>0</c> if not specified.
        /// </value>
        public uint TotalFrames => _frames;

        /// <summary>
        ///    Gets the total size of the file, as indicated by the
        ///    current instance.
        /// </summary>
        /// <value>
        ///    A <see cref="uint" /> value containing the total size of
        ///    the file, or <c>0</c> if not specified.
        /// </value>
        public uint TotalSize => _size;

        /// <summary>
        ///    Gets whether or not a physical Xing header is present in
        ///    the file.
        /// </summary>
        /// <value>
        ///    A <see cref="bool" /> value indicating whether or not the
        ///    current instance represents a physical Xing header.
        /// </value>
        public bool Present => _present;

        /// <summary>
        ///    Gets the offset at which a Xing header would appear in an
        ///    MPEG audio packet based on the version and channel mode.
        /// </summary>
        /// <param name="version">
        ///    A <see cref="Version" /> value specifying the version of
        ///    the MPEG audio packet.
        /// </param>
        /// <param name="channelMode">
        ///    A <see cref="ChannelMode" /> value specifying the channel
        ///    mode of the MPEG audio packet.
        /// </param>
        /// <returns>
        ///    A <see cref="int" /> value indicating the offset in an
        ///    MPEG audio packet at which the Xing header would appear.
        /// </returns>
        public static int XingHeaderOffset(Version version,
                                            ChannelMode channelMode)
        {
            bool singleChannel =
                channelMode == ChannelMode.SingleChannel;

            if (version == Version.Version1)
                return singleChannel ? 0x15 : 0x24;
            else
                return singleChannel ? 0x0D : 0x15;
        }
    }
}