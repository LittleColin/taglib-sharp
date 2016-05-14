//
// StreamHeader.cs: Provides support for reading MusePack audio properties.
//
// Author:
//   Brian Nickel (brian.nickel@gmail.com)
//
// Original Source:
//   mpcproperties.cpp from TagLib
//
// Copyright (C) 2016 Helmut Wahrmann: SV8 Support based on Taglib imnplementation
// Copyright (C) 2006-2007 Brian Nickel
// Copyright (C) 2004 by Allan Sandfeld Jensen (Original Implementation)
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
using System.Globalization;
using System.IO;

namespace TagLib.MusePack
{
    /// <summary>
    ///    This struct implements <see cref="IAudioCodec" /> to provide
    ///    support for reading MusePack audio properties.
    /// </summary>
    public struct StreamHeader : IAudioCodec
    {
        private static readonly ushort[] _sftable = { 44100, 48000, 37800, 32000 };

        /// <summary>
        ///    Contains the number of bytes in the stream.
        /// </summary>
        private readonly long _streamLength;

        /// <summary>
        ///    Contains the MusePack version.
        /// </summary>
        private int _version;

        /// <summary>
        ///    Contains additional header information.
        /// </summary>
        private uint _headerData;

        /// <summary>
        ///    Contains the sample rate of the stream.
        /// </summary>
        private int _sampleRate;

        /// <summary>
        ///    Contains the number of frames in the stream.
        /// </summary>
        private uint _frames;

        /// <summary>
        ///	   Contains the number of channels in the stream.
        /// </summary>
        private int _channels;

        /// <summary>
        ///    Contains the count of frames in the stream.
        /// </summary>
        private ulong _framecount;

        /// <summary>
        ///    The size of a MusePack SV7 header.
        /// </summary>
        public const uint SizeSv7 = 56;

        /// <summary>
        ///    The identifier used to recognize a Musepack SV7 file.
        /// </summary>
        /// <value>
        ///    "MP+"
        /// </value>
        public static readonly ReadOnlyByteVector FileIdentifierSv7 = "MP+";

        /// <summary>
        ///    The identifier used to recognize a Musepack SV8 file.
        /// </summary>
        /// <value>
        ///    "MPCK"
        /// </value>
        public static readonly ReadOnlyByteVector FileIdentifierSv8 = "MPCK";

        /// <summary>
        ///    Constructs and initializes a new instance of <see
        ///    cref="StreamHeader" /> for a specified header block and
        ///    stream length.
        /// </summary>
        /// <param name="data">
        ///    A <see cref="ByteVector" /> object containing the stream
        ///    header data.
        /// </param>
        /// <param name="streamLength">
        ///    A <see cref="long" /> value containing the length of the
        ///    MusePAck stream in bytes.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///    <paramref name="data" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="CorruptFileException">
        ///    <paramref name="data" /> does not begin with <see
        ///    cref="FileIdentifierSv7" />  or with <see
        ///    cref="FileIdentifierSv8" /> or is less than
        ///    <see cref="Size" /> bytes long.
        /// </exception>
        public StreamHeader(File file, long streamLength)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));

            // Assign default values, to be able to call methods
            // in the constructor
            _streamLength = streamLength;
            _version = 7;
            _headerData = 0;
            _frames = 0;
            _sampleRate = 0;
            _channels = 2;
            _framecount = 0;

            file.Seek(0);
            ByteVector magic = file.ReadBlock(4);
            if (magic.StartsWith(FileIdentifierSv7))
                // SV7 Format has a fixed Header size
                ReadSv7Properties(magic + file.ReadBlock((int)SizeSv7 - 4));
            else if (magic.StartsWith(FileIdentifierSv8))
                // for SV8 the properties need to be read from
                // packet information inside the file
                ReadSv8Properties(file);
            else
                throw new CorruptFileException(
                    "Data does not begin with identifier.");
        }

        private void ReadSv7Properties(ByteVector data)
        {
            if (data.Count < SizeSv7)
                throw new CorruptFileException(
                    "Insufficient data in stream header");

            _version = data[3] & 15;
            _channels = 2;

            if (_version == 7)
            {
                _frames = data.Mid(4, 4).ToUInt(false);
                uint flags = data.Mid(8, 4).ToUInt(false);
                _sampleRate = _sftable[(int)(((flags >> 17) &
                    1) * 2 + ((flags >> 16) & 1))];
                _headerData = 0;
            }
            else
            {
                _headerData = data.Mid(0, 4).ToUInt(false);
                _version = (int)((_headerData >> 11) & 0x03ff);
                _sampleRate = 44100;
                _frames = data.Mid(4,
                    _version >= 5 ? 4 : 2).ToUInt(false);
            }
        }

        private void ReadSv8Properties(File file)
        {
            bool foundSh = false;

            while (!foundSh)
            {
                ByteVector packetType = file.ReadBlock(2);

                uint packetSizeLength = 0;
                bool eof = false;

                ulong packetSize = ReadSize(file, ref packetSizeLength, ref eof);
                if (eof)
                {
                    break;
                }

                ulong payloadSize = packetSize - 2 - packetSizeLength;
                ByteVector data = file.ReadBlock((int)payloadSize);

                if (packetType == "SH")
                {
                    foundSh = true;

                    if (payloadSize <= 5)
                    {
                        break;
                    }

                    int pos = 4;
                    _version = data[pos];
                    pos += 1;
                    _frames = (uint)ReadSize(data, ref pos);
                    if (pos > (uint)payloadSize - 3)
                    {
                        break;
                    }

                    ulong beginSilence = ReadSize(data, ref pos);
                    if (pos > (uint)payloadSize - 2)
                    {
                        break;
                    }

                    ushort flags = data.Mid(pos, 1).ToUShort(true);

                    _sampleRate = _sftable[(flags >> 13) & 0x07];
                    _channels = ((flags >> 4) & 0x0F) + 1;

                    _framecount = _frames - beginSilence;
                }
                else if (packetType == "SE")
                {
                    break;
                }
                else
                {
                    file.Seek((int)payloadSize, SeekOrigin.Current);
                }
            }
        }

        private ulong ReadSize(File file, ref uint packetSizeLength, ref bool eof)
        {
            uint tmp;
            ulong size = 0;

            do
            {
                ByteVector b = file.ReadBlock(1);
                if (b.IsEmpty)
                {
                    eof = true;
                    break;
                }

                tmp = b.ToUInt();
                size = (size << 7) | (tmp & 0x7F);
                packetSizeLength++;
            } while ((tmp & 0x80) == 1);

            return size;
        }

        private ulong ReadSize(ByteVector data, ref int pos)
        {
            uint tmp;
            ulong size = 0;

            do
            {
                tmp = data[pos++];
                size = (size << 7) | (tmp & 0x7F);
            } while ((tmp & 0x80) == 0x80 && pos < data.Count);
            return size;
        }

        /// <summary>
        ///    Gets the duration of the media represented by the current
        ///    instance.
        /// </summary>
        /// <value>
        ///    A <see cref="TimeSpan" /> containing the duration of the
        ///    media represented by the current instance.
        /// </value>

        public TimeSpan Duration
        {
            get
            {
                if (_sampleRate <= 0 && _streamLength <= 0)
                    return TimeSpan.Zero;

                if (_version <= 7)
                {
                    return TimeSpan.FromSeconds(
                        (_frames * 1152 - 576) /
                    (double)_sampleRate + 0.5);
                }

                return TimeSpan.FromMilliseconds(
                        _framecount * 1000.0 /
                        _sampleRate + 0.5);
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
        ///    Gets a text description of the media represented by the
        ///    current instance.
        /// </summary>
        /// <value>
        ///    A <see cref="string" /> object containing a description
        ///    of the media represented by the current instance.
        /// </value>
        public string Description => string.Format(
            CultureInfo.InvariantCulture,
            "MusePack Version {0} Audio", Version);

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
                if (_headerData != 0)
                    return (int)((_headerData >> 23) & 0x01ff);

                if (_version <= 7)
                {
                    return (int)(Duration > TimeSpan.Zero ?
                        ((_streamLength * 8L) /
                        Duration.TotalSeconds) / 1000 : 0);
                }

                return (int)(_streamLength * 8 / Duration.TotalMilliseconds + 0.5);
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
        public int AudioSampleRate => _sampleRate;

        /// <summary>
        ///    Gets the number of channels in the audio represented by
        ///    the current instance.
        /// </summary>
        /// <value>
        ///    A <see cref="int" /> value containing the number of
        ///    channels in the audio represented by the current
        ///    instance.
        /// </value>
        public int AudioChannels => _channels;

        /// <summary>
        ///    Gets the WavPack version of the audio represented by the
        ///    current instance.
        /// </summary>
        /// <value>
        ///    A <see cref="int" /> value containing the WavPack version
        ///    of the audio represented by the current instance.
        /// </value>
        public int Version => _version;

        /// <summary>
        ///    Generates a hash code for the current instance.
        /// </summary>
        /// <returns>
        ///    A <see cref="int" /> value containing the hash code for
        ///    the current instance.
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return (int)(_headerData ^ _sampleRate ^
                    _frames ^ _version);
            }
        }

        /// <summary>
        ///    Checks whether or not the current instance is equal to
        ///    another object.
        /// </summary>
        /// <param name="other">
        ///    A <see cref="object" /> to compare to the current
        ///    instance.
        /// </param>
        /// <returns>
        ///    A <see cref="bool" /> value indicating whether or not the
        ///    current instance is equal to <paramref name="other" />.
        /// </returns>
        /// <seealso cref="M:System.IEquatable`1.Equals" />
        public override bool Equals(object other)
        {
            if (!(other is StreamHeader))
                return false;

            return Equals((StreamHeader)other);
        }

        /// <summary>
        ///    Checks whether or not the current instance is equal to
        ///    another instance of <see cref="StreamHeader" />.
        /// </summary>
        /// <param name="other">
        ///    A <see cref="StreamHeader" /> object to compare to the
        ///    current instance.
        /// </param>
        /// <returns>
        ///    A <see cref="bool" /> value indicating whether or not the
        ///    current instance is equal to <paramref name="other" />.
        /// </returns>
        /// <seealso cref="M:System.IEquatable`1.Equals" />
        public bool Equals(StreamHeader other)
        {
            return _headerData == other._headerData &&
                _sampleRate == other._sampleRate &&
                _version == other._version &&
                _frames == other._frames;
        }

        /// <summary>
        ///    Gets whether or not two instances of <see
        ///    cref="StreamHeader" /> are equal to eachother.
        /// </summary>
        /// <param name="first">
        ///    A <see cref="StreamHeader" /> object to compare.
        /// </param>
        /// <param name="second">
        ///    A <see cref="StreamHeader" /> object to compare.
        /// </param>
        /// <returns>
        ///    <see langword="true" /> if <paramref name="first" /> is
        ///    equal to <paramref name="second" />. Otherwise, <see
        ///    langword="false" />.
        /// </returns>
        public static bool operator ==(StreamHeader first,
                                        StreamHeader second)
        {
            return first.Equals(second);
        }

        /// <summary>
        ///    Gets whether or not two instances of <see
        ///    cref="StreamHeader" /> differ.
        /// </summary>
        /// <param name="first">
        ///    A <see cref="StreamHeader" /> object to compare.
        /// </param>
        /// <param name="second">
        ///    A <see cref="StreamHeader" /> object to compare.
        /// </param>
        /// <returns>
        ///    <see langword="true" /> if <paramref name="first" /> is
        ///    unequal to <paramref name="second" />. Otherwise, <see
        ///    langword="false" />.
        /// </returns>
        public static bool operator !=(StreamHeader first,
                                        StreamHeader second)
        {
            return !first.Equals(second);
        }
    }
}