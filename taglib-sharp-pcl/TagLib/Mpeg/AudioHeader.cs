//
// AudioHeader.cs: Provides information about an MPEG audio stream.
//
// Author:
//   Brian Nickel (brian.nickel@gmail.com)
//
// Original Source:
//   mpegheader.cpp from TagLib
//
// Copyright (C) 2005-2007 Brian Nickel
// Copyright (C) 2003 by Scott Wheeler (Original Implementation)
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
using System.Text;

namespace TagLib.Mpeg
{
    /// <summary>
    ///    Indicates the MPEG version of a file or stream.
    /// </summary>
    public enum Version
    {
        /// <summary>
        ///    Unknown version.
        /// </summary>
        Unknown = -1,

        /// <summary>
        ///    MPEG-1
        /// </summary>
        Version1 = 0,

        /// <summary>
        ///    MPEG-2
        /// </summary>
        Version2 = 1,

        /// <summary>
        ///    MPEG-2.5
        /// </summary>
        Version25 = 2
    }

    /// <summary>
    ///    Indicates the MPEG audio channel mode of a file or stream.
    /// </summary>
    public enum ChannelMode
    {
        /// <summary>
        ///    Stereo
        /// </summary>
        Stereo = 0,

        /// <summary>
        ///    Joint Stereo
        /// </summary>
        JointStereo = 1,

        /// <summary>
        ///    Dual Channel Mono
        /// </summary>
        DualChannel = 2,

        /// <summary>
        ///    Single Channel Mono
        /// </summary>
        SingleChannel = 3
    }

    /// <summary>
    ///    This structure implements <see cref="IAudioCodec" /> and provides
    ///    information about an MPEG audio stream.
    /// </summary>
    public struct AudioHeader : IAudioCodec
    {
        /// <summary>
        ///    Contains a sample rate table for MPEG audio.
        /// </summary>
        private static readonly int[,] SampleRates = {
            {44100, 48000, 32000, 0}, // Version 1
			{22050, 24000, 16000, 0}, // Version 2
			{11025, 12000,  8000, 0}  // Version 2.5
		};

        /// <summary>
        ///    Contains a block size table for MPEG audio.
        /// </summary>
        private static readonly int[,] BlockSize = {
            {0, 384, 1152, 1152}, // Version 1
			{0, 384, 1152,  576}, // Version 2
			{0, 384, 1152,  576}  // Version 2.5
		};

        /// <summary>
        ///    Contains a bitrate table for MPEG audio.
        /// </summary>
        private static readonly int[,,] Bitrates = {
            { // Version 1
				{0, 32, 64, 96, 128, 160, 192, 224, 256, 288,
                    320, 352, 384, 416, 448, -1}, // layer 1
				{0, 32, 48, 56,  64,  80,  96, 112, 128, 160,
                    192, 224, 256, 320, 384, -1}, // layer 2
				{0, 32, 40, 48,  56,  64,  80,  96, 112, 128,
                    160, 192, 224, 256, 320, -1}  // layer 3
			},
            { // Version 2 or 2.5
				{0, 32, 48, 56, 64, 80, 96, 112, 128, 144, 160,
                    176, 192, 224, 256, -1}, // layer 1
				{0,  8, 16, 24, 32, 40, 48,  56,  64,  80,  96,
                    112, 128, 144, 160, -1}, // layer 2
				{0,  8, 16, 24, 32, 40, 48,  56,  64,  80,  96,
                    112, 128, 144, 160, -1}  // layer 3
			}
        };

        /// <summary>
        ///    Contains the header flags.
        /// </summary>
        private readonly uint _flags;

        /// <summary>
        ///    Contains the audio stream length.
        /// </summary>
        private long _streamLength;

        /// <summary>
        ///    Contains the associated Xing header.
        /// </summary>
        private XingHeader _xingHeader;

        /// <summary>
        ///    Contains the associated VBRI header.
        /// </summary>
        private VbriHeader _vbriHeader;

        /// <summary>
        ///    Contains the audio stream duration.
        /// </summary>
        private TimeSpan _duration;

        /// <summary>
        ///    An empty and unset header.
        /// </summary>
        public static readonly AudioHeader Unknown = new AudioHeader(0, 0, XingHeader.Unknown, VbriHeader.Unknown);

        /// <summary>
        ///    Constructs and initializes a new instance of <see
        ///    cref="AudioHeader" /> by populating it with specified
        ///    values.
        /// </summary>
        /// <param name="flags">
        ///    A <see cref="uint" /> value specifying flags for the new
        ///    instance.
        /// </param>
        /// <param name="streamLength">
        ///    A <see cref="long" /> value specifying the stream length
        ///    of the new instance.
        /// </param>
        /// <param name="xingHeader">
        ///    A <see cref="XingHeader" /> object representing the Xing
        ///    header associated with the new instance.
        /// </param>
        /// <param name="vbriHeader">
        ///    A <see cref="VbriHeader" /> object representing the VBRI
        ///    header associated with the new instance.
        /// </param>
        private AudioHeader(uint flags, long streamLength,
                             XingHeader xingHeader,
                             VbriHeader vbriHeader)
        {
            _flags = flags;
            _streamLength = streamLength;
            _xingHeader = xingHeader;
            _vbriHeader = vbriHeader;
            _duration = TimeSpan.Zero;
        }

        /// <summary>
        ///    Constructs and initializes a new instance of <see
        ///    cref="AudioHeader" /> by reading its contents from a
        ///    <see cref="ByteVector" /> object and its Xing Header from
        ///    the appropriate location in the specified file.
        /// </summary>
        /// <param name="data">
        ///    A <see cref="ByteVector" /> object containing the header
        ///    to read.
        /// </param>
        /// <param name="file">
        ///    A <see cref="TagLib.File" /> object to read the Xing
        ///    header from.
        /// </param>
        /// <param name="position">
        ///    A <see cref="long" /> value indicating the position in
        ///    <paramref name="file" /> at which the header begins.
        /// </param>
        /// <exception cref="CorruptFileException">
        ///    <paramref name="data" /> is less than 4 bytes long,
        ///    does not begin with a MPEG audio synch, has a negative
        ///    bitrate, or has a sample rate of zero.
        /// </exception>
        private AudioHeader(ByteVector data, TagLib.File file,
                             long position)
        {
            _duration = TimeSpan.Zero;
            _streamLength = 0;

            string error = GetHeaderError(data);
            if (error != null)
            {
                throw new CorruptFileException(error);
            }

            _flags = data.ToUInt();

            _xingHeader = XingHeader.Unknown;

            _vbriHeader = VbriHeader.Unknown;

            // Check for a Xing header that will help us in
            // gathering information about a VBR stream.
            file.Seek(position + XingHeader.XingHeaderOffset(
                Version, ChannelMode));

            ByteVector xingData = file.ReadBlock(16);
            if (xingData.Count == 16 && xingData.StartsWith(
                XingHeader.FileIdentifier))
                _xingHeader = new XingHeader(xingData);

            if (_xingHeader.Present)
                return;

            // A Xing header could not be found, next chec for a
            // Fraunhofer VBRI header.
            file.Seek(position + VbriHeader.VbriHeaderOffset());

            // Only get the first 24 bytes of the Header.
            // We're not interested in the TOC entries.
            ByteVector vbriData = file.ReadBlock(24);
            if (vbriData.Count == 24 &&
                vbriData.StartsWith(VbriHeader.FileIdentifier))
                _vbriHeader = new VbriHeader(vbriData);
        }

        /// <summary>
        ///    Gets the MPEG version used to encode the audio
        ///    represented by the current instance.
        /// </summary>
        /// <value>
        ///    A <see cref="Version" /> value indicating the MPEG
        ///    version used to encode the audio represented by the
        ///    current instance.
        /// </value>
        public Version Version
        {
            get
            {
                switch ((_flags >> 19) & 0x03)
                {
                    case 0:
                        return Version.Version25;

                    case 2:
                        return Version.Version2;

                    default:
                        return Version.Version1;
                }
            }
        }

        /// <summary>
        ///    Gets the MPEG audio layer used to encode the audio
        ///    represented by the current instance.
        /// </summary>
        /// <value>
        ///    A <see cref="int" /> value indicating the MPEG audio
        ///    layer used to encode the audio represented by the current
        ///    instance.
        /// </value>
        public int AudioLayer
        {
            get
            {
                switch ((_flags >> 17) & 0x03)
                {
                    case 1:
                        return 3;

                    case 2:
                        return 2;

                    default:
                        return 1;
                }
            }
        }

        /// <summary>
        ///    Gets the bitrate of the audio represented by the current
        ///    instance.
        /// </summary>
        /// <value>
        ///    A <see cref="int" /> value containing a bitrate of the
        ///    audio represented by the current instance.
        /// </value>
        public int AudioBitrate
        {
            get
            {
                if (_xingHeader.TotalSize > 0 &&
                    _duration > TimeSpan.Zero)
                    return (int)Math.Round(((
                        (XingHeader.TotalSize * 8L) /
                        _duration.TotalSeconds) / 1000.0));

                if (_vbriHeader.TotalSize > 0 &&
                    _duration > TimeSpan.Zero)
                    return (int)Math.Round(((
                        (VbriHeader.TotalSize * 8L) /
                        _duration.TotalSeconds) / 1000.0));

                return Bitrates[
                    Version == Version.Version1 ? 0 : 1,
                    AudioLayer > 0 ? AudioLayer - 1 : 0,
                    (int)(_flags >> 12) & 0x0F];
            }
        }

        /// <summary>
        ///    Gets the sample rate of the audio represented by the
        ///    current instance.
        /// </summary>
        /// <value>
        ///    A <see cref="int" /> value containing the sample rate of
        ///    the audio represented by the current instance.
        /// </value>
        public int AudioSampleRate => SampleRates[(int)Version,
            (int)(_flags >> 10) & 0x03];

        /// <summary>
        ///    Gets the number of channels in the audio represented by
        ///    the current instance.
        /// </summary>
        /// <value>
        ///    A <see cref="int" /> value containing the number of
        ///    channels in the audio represented by the current
        ///    instance.
        /// </value>
        public int AudioChannels => ChannelMode == ChannelMode.SingleChannel ? 1 : 2;

        /// <summary>
        ///    Gets the length of the frames in the audio represented by
        ///    the current instance.
        /// </summary>
        /// <value>
        ///    A <see cref="int" /> value containing the length of the
        ///    frames in the audio represented by the current instance.
        /// </value>
        public int AudioFrameLength
        {
            get
            {
                switch (AudioLayer)
                {
                    case 1:
                        return 48000 * AudioBitrate /
                            AudioSampleRate +
                            (IsPadded ? 4 : 0);

                    case 2:
                        return 144000 * AudioBitrate /
                            AudioSampleRate +
                            (IsPadded ? 1 : 0);

                    case 3:
                        if (Version == Version.Version1)
                            goto case 2;

                        return 72000 * AudioBitrate /
                            AudioSampleRate +
                            (IsPadded ? 1 : 0);

                    default: return 0;
                }
            }
        }

        /// <summary>
        ///    Gets the duration of the media represented by the current
        ///    instance.
        /// </summary>
        /// <value>
        ///    A <see cref="TimeSpan" /> containing the duration of the
        ///    media represented by the current instance.
        /// </value>
        /// <remarks>
        ///    If <see cref="XingHeader" /> is equal to <see
        ///    cref="XingHeader.Unknown" /> and <see
        ///    cref="SetStreamLength" /> has not been called, this value
        ///    will not be correct.
        ///    If <see cref="VbriHeader" /> is equal to <see
        ///    cref="VbriHeader.Unknown" /> and <see
        ///    cref="SetStreamLength" /> has not been called, this value
        ///    will not be correct.
        /// </remarks>
        public TimeSpan Duration
        {
            get
            {
                if (_duration > TimeSpan.Zero)
                    return _duration;

                if (_xingHeader.TotalFrames > 0)
                {
                    // Read the length and the bitrate from
                    // the Xing header.

                    double timePerFrame = BlockSize[(int)Version,
                        AudioLayer] / (double)
                        AudioSampleRate;

                    _duration = TimeSpan.FromSeconds(
                        timePerFrame *
                        XingHeader.TotalFrames);
                }
                else if (_vbriHeader.TotalFrames > 0)
                {
                    // Read the length and the bitrate from
                    // the VBRI header.

                    double timePerFrame =
                        BlockSize[
                            (int)Version, AudioLayer]
                        / (double)AudioSampleRate;

                    _duration = TimeSpan.FromSeconds(
                        Math.Round(timePerFrame *
                            VbriHeader.TotalFrames));
                }
                else if (AudioFrameLength > 0 &&
                  AudioBitrate > 0)
                {
                    // Since there was no valid Xing or VBRI
                    // header found, we hope that we're in a
                    // constant bitrate file.

                    int frames = (int)(_streamLength
                         / AudioFrameLength + 1);

                    _duration = TimeSpan.FromSeconds(
                        AudioFrameLength *
                        frames / (double)
                        (AudioBitrate * 125) + 0.5);
                }

                return _duration;
            }
        }

        /// <summary>
        ///    Gets a text description of the media represented by the
        ///    current instance.
        /// </summary>
        /// <value>
        ///    A <see cref="string" /> object containing a description
        ///    of the media represented by the current instance.
        /// </value>
        public string Description
        {
            get
            {
                StringBuilder builder = new StringBuilder();

                builder.Append("MPEG Version ");

                switch (Version)
                {
                    case Version.Version1:
                        builder.Append("1");
                        break;

                    case Version.Version2:
                        builder.Append("2");
                        break;

                    case Version.Version25:
                        builder.Append("2.5");
                        break;
                }

                builder.Append(" Audio, Layer ");
                builder.Append(AudioLayer);

                if (_xingHeader.Present || _vbriHeader.Present)
                    builder.Append(" VBR");

                return builder.ToString();
            }
        }

        /// <summary>
        ///    Gets the types of media represented by the current
        ///    instance.
        /// </summary>
        /// <value>
        ///    Always <see cref="MediaTypes.Audio" />.
        /// </value>
        public MediaTypes MediaTypes => MediaTypes.Audio;

        /// <summary>
        ///    Gets whether or not the audio represented by the current
        ///    instance is protected.
        /// </summary>
        /// <value>
        ///    A <see cref="bool" /> value indicating whether or not the
        ///    audio represented by the current instance is protected.
        /// </value>
        public bool IsProtected => ((_flags >> 16) & 1) == 0;

        /// <summary>
        ///    Gets whether or not the audio represented by the current
        ///    instance is padded.
        /// </summary>
        /// <value>
        ///    A <see cref="bool" /> value indicating whether or not the
        ///    audio represented by the current instance is padded.
        /// </value>
        public bool IsPadded => ((_flags >> 9) & 1) == 1;

        /// <summary>
        ///    Gets whether or not the audio represented by the current
        ///    instance is copyrighted.
        /// </summary>
        /// <value>
        ///    A <see cref="bool" /> value indicating whether or not the
        ///    audio represented by the current instance is copyrighted.
        /// </value>
        public bool IsCopyrighted => ((_flags >> 3) & 1) == 1;

        /// <summary>
        ///    Gets whether or not the audio represented by the current
        ///    instance is original.
        /// </summary>
        /// <value>
        ///    A <see cref="bool" /> value indicating whether or not the
        ///    audio represented by the current instance is original.
        /// </value>
        public bool IsOriginal => ((_flags >> 2) & 1) == 1;

        /// <summary>
        ///    Gets the MPEG audio channel mode of the audio represented
        ///    by the current instance.
        /// </summary>
        /// <value>
        ///    A <see cref="ChannelMode" /> value indicating the MPEG
        ///    audio channel mode of the audio represented by the
        ///    current instance.
        /// </value>
        public ChannelMode ChannelMode => (ChannelMode)((_flags >> 6) & 0x03);

        /// <summary>
        ///    Gets the Xing header found in the audio represented by
        ///    the current instance.
        /// </summary>
        /// <value>
        ///    A <see cref="XingHeader" /> object containing the Xing
        ///    header found in the audio represented by the current
        ///    instance, or <see cref="XingHeader.Unknown" /> if no
        ///    header was found.
        /// </value>
        public XingHeader XingHeader => _xingHeader;

        /// <summary>
        ///    Gets the VBRI header found in the audio represented by
        ///    the current instance.
        /// </summary>
        /// <value>
        ///    A <see cref="VbriHeader" /> object containing the VBRI
        ///    header found in the audio represented by the current
        ///    instance, or <see cref="VbriHeader.Unknown" /> if no
        ///    header was found.
        /// </value>
        public VbriHeader VbriHeader => _vbriHeader;

        /// <summary>
        ///    Sets the length of the audio stream represented by the
        ///    current instance.
        /// </summary>
        /// <param name="streamLength">
        ///    A <see cref="long" /> value specifying the length in
        ///    bytes of the audio stream represented by the current
        ///    instance.
        /// </param>
        /// <remarks>
        ///    The this value has been set, <see cref="Duration" /> will
        ///    return an incorrect value.
        /// </remarks>
        public void SetStreamLength(long streamLength)
        {
            _streamLength = streamLength;

            // Force the recalculation of duration if it depends on
            // the stream length.
            if (_xingHeader.TotalFrames == 0 ||
                _vbriHeader.TotalFrames == 0)
                _duration = TimeSpan.Zero;
        }

        /// <summary>
        ///    Searches for an audio header in a <see cref="TagLib.File"
        ///    /> starting at a specified position and searching through
        ///    a specified number of bytes.
        /// </summary>
        /// <param name="header">
        ///    A <see cref="AudioHeader" /> object in which the found
        ///    header will be stored.
        /// </param>
        /// <param name="file">
        ///    A <see cref="TagLib.File" /> object to search.
        /// </param>
        /// <param name="position">
        ///    A <see cref="long" /> value specifying the seek position
        ///    in <paramref name="file" /> at which to start searching.
        /// </param>
        /// <param name="length">
        ///    A <see cref="int" /> value specifying the maximum number
        ///    of bytes to search before aborting.
        /// </param>
        /// <returns>
        ///    A <see cref="bool" /> value indicating whether or not a
        ///    header was found.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///    <paramref name="file" /> is <see langword="null" />.
        /// </exception>
        public static bool Find(out AudioHeader header,
                                 TagLib.File file, long position,
                                 int length)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));

            long end = position + length;
            header = Unknown;

            file.Seek(position);

            ByteVector buffer = file.ReadBlock(3);

            if (buffer.Count < 3)
                return false;

            do
            {
                file.Seek(position + 3);
                buffer = buffer.Mid(buffer.Count - 3);
                buffer.Add(file.ReadBlock(
                    (int)TagLib.File.BufferSize));

                for (int i = 0; i < buffer.Count - 3 && (length < 0 || position + i < end); i++)
                    if (buffer[i] == 0xFF && buffer[i + 1] > 0xE0)
                    {
                        ByteVector data = buffer.Mid(i, 4);

                        if (GetHeaderError(data) != null)
                            continue;
                        try
                        {
                            header = new AudioHeader(
                                data,
                                file, position + i);
                            return true;
                        }
                        catch (CorruptFileException)
                        {
                        }
                    }

                position += TagLib.File.BufferSize;
            } while (buffer.Count > 3 && (length < 0 || position < end));

            return false;
        }

        /// <summary>
        ///    Searches for an audio header in a <see cref="TagLib.File"
        ///    /> starting at a specified position and searching to the
        ///    end of the file.
        /// </summary>
        /// <param name="header">
        ///    A <see cref="AudioHeader" /> object in which the found
        ///    header will be stored.
        /// </param>
        /// <param name="file">
        ///    A <see cref="TagLib.File" /> object to search.
        /// </param>
        /// <param name="position">
        ///    A <see cref="long" /> value specifying the seek position
        ///    in <paramref name="file" /> at which to start searching.
        /// </param>
        /// <returns>
        ///    A <see cref="bool" /> value indicating whether or not a
        ///    header was found.
        /// </returns>
        /// <remarks>
        ///    Searching to the end of the file can be very, very slow
        ///    especially for corrupt or non-MPEG files. It is
        ///    recommended to use <see
        ///    cref="Find(AudioHeader,TagLib.File,long,int)" />
        ///    instead.
        /// </remarks>
        public static bool Find(out AudioHeader header,
                                 TagLib.File file, long position)
        {
            return Find(out header, file, position, -1);
        }

        private static string GetHeaderError(ByteVector data)
        {
            if (data.Count < 4)
                return "Insufficient header length.";

            if (data[0] != 0xFF)
                return "First byte did not match MPEG synch.";

            // Checking bits from high to low:
            //
            // First 3 bits MUST be set. Bits 4 and 5 can
            // be 00, 10, or 11 but not 01. One or more of
            // bits 6 and 7 must be set. Bit 8 can be
            // anything.
            if ((data[1] & 0xE6) <= 0xE0 || (data[1] & 0x18) == 0x08)
                return "Second byte did not match MPEG synch.";

            uint flags = data.ToUInt();

            if (((flags >> 12) & 0x0F) == 0x0F)
                return "Header uses invalid bitrate index.";

            if (((flags >> 10) & 0x03) == 0x03)
                return "Invalid sample rate.";

            return null;
        }
    }
}