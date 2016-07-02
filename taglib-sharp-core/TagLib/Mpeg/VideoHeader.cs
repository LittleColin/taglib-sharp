//
// VideoHeader.cs: Provides information about an MPEG video stream.
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

namespace TagLib.Mpeg
{
    /// <summary>
    ///    This structure implements <see cref="IVideoCodec" /> and provides
    ///    information about an MPEG video stream.
    /// </summary>
    public struct VideoHeader : IVideoCodec
    {
        /// <summary>
        ///    Contains frame rate values.
        /// </summary>
        private static readonly double[] FrameRates = {
            0, 24000d/1001d, 24, 25, 30000d/1001d, 30, 50,
            60000d/1001d, 60
        };

        /// <summary>
        ///    Contains the video width.
        /// </summary>
        private readonly int _width;

        /// <summary>
        ///    Contains the video height.
        /// </summary>
        private readonly int _height;

        /// <summary>
        ///    Contains the index in <see cref="FrameRates" /> of the
        ///    video frame rate.
        /// </summary>
        private readonly int _frameRateIndex;

        /// <summary>
        ///    Contains the video bitrate.
        /// </summary>
        private readonly int _bitrate;

        /// <summary>
        ///    Constructs and initializes a new instance of <see
        ///    cref="VideoHeader" /> by reading it from a specified
        ///    location in a specified file.
        /// </summary>
        /// <param name="file">
        ///    A <see cref="TagLib.File" /> object to read from.
        /// </param>
        /// <param name="position">
        ///    A <see cref="long" /> value indicating the position in
        ///    <paramref name="file" /> at which the header begins.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///    <paramref name="file" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="CorruptFileException">
        ///    Insufficient data could be read for the header.
        /// </exception>
        public VideoHeader(TagLib.File file, long position)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));

            file.Seek(position);
            ByteVector data = file.ReadBlock(7);

            if (data.Count < 7)
                throw new CorruptFileException(
                    "Insufficient data in header.");

            _width = data.Mid(0, 2).ToUShort() >> 4;
            _height = data.Mid(1, 2).ToUShort() & 0x0FFF;
            _frameRateIndex = data[3] & 0x0F;
            _bitrate = (int)((data.Mid(4, 3).ToUInt() >> 6) &
                0x3FFFF);
        }

        /// <summary>
        ///    Gets the duration of the media represented by the current
        ///    instance.
        /// </summary>
        /// <value>
        ///    Always <see cref="TimeSpan.Zero" />.
        /// </value>
        public TimeSpan Duration => TimeSpan.Zero;

        /// <summary>
        ///    Gets the types of media represented by the current
        ///    instance.
        /// </summary>
        /// <value>
        ///    Always <see cref="MediaTypes.Video" />.
        /// </value>
        public MediaTypes MediaTypes => MediaTypes.Video;

        /// <summary>
        ///    Gets a text description of the media represented by the
        ///    current instance.
        /// </summary>
        /// <value>
        ///    A <see cref="string" /> object containing a description
        ///    of the media represented by the current instance.
        /// </value>
        public string Description => "MPEG Video";

        /// <summary>
        ///    Gets the width of the video represented by the current
        ///    instance.
        /// </summary>
        /// <value>
        ///    A <see cref="int" /> value containing the width of the
        ///    video represented by the current instance.
        /// </value>
        public int VideoWidth => _width;

        /// <summary>
        ///    Gets the height of the video represented by the current
        ///    instance.
        /// </summary>
        /// <value>
        ///    A <see cref="int" /> value containing the height of the
        ///    video represented by the current instance.
        /// </value>
        public int VideoHeight => _height;

        /// <summary>
        ///    Gets the frame rate of the video represented by the
        ///    current instance.
        /// </summary>
        /// <value>
        ///    A <see cref="double" /> value containing the frame rate
        ///    of the video represented by the current instance.
        /// </value>
        public double VideoFrameRate => _frameRateIndex < 9 ?
            FrameRates[_frameRateIndex] : 0;

        /// <summary>
        ///    Gets the bitrate of the video represented by the current
        ///    instance.
        /// </summary>
        /// <value>
        ///    A <see cref="int" /> value containing a bitrate of the
        ///    video represented by the current instance.
        /// </value>
        public int VideoBitrate => _bitrate;
    }
}