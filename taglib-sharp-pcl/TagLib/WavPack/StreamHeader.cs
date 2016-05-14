//
// StreamHeader.cs: Provides support for reading WavPack audio properties.
//
// Author:
//   Brian Nickel (brian.nickel@gmail.com)
//
// Original Source:
//   wvproperties.cpp from libtunepimp
//
// Copyright (C) 2006-2007 Brian Nickel
// Copyright (C) 2006 by Lukáš Lalinský (Original Implementation)
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

namespace TagLib.WavPack
{
    /// <summary>
    ///    This struct implements <see cref="IAudioCodec" /> to provide
    ///    support for reading WavPack audio properties.
    /// </summary>
    public struct StreamHeader : IAudioCodec, ILosslessAudioCodec, IEquatable<StreamHeader>
    {
        private static readonly uint[] SampleRates = new uint[] {
            6000, 8000, 9600, 11025, 12000, 16000, 22050, 24000,
            32000, 44100, 48000, 64000, 88200, 96000, 192000};

        private const int BytesStored = 3;
        private const int MonoFlag = 4;
        private const int ShiftLsb = 13;
        private const long ShiftMask = (0x1fL << ShiftLsb);
        private const int SrateLsb = 23;
        private const long SrateMask = (0xfL << SrateLsb);

        /// <summary>
        ///    Contains the number of bytes in the stream.
        /// </summary>
        private readonly long _streamLength;

        /// <summary>
        ///    Contains the WavPack version.
        /// </summary>
        private readonly ushort _version;

        /// <summary>
        ///    Contains the flags.
        /// </summary>
        private readonly uint _flags;

        /// <summary>
        ///    Contains the sample count.
        /// </summary>
        private readonly uint _samples;

        /// <summary>
        ///    The size of a WavPack header.
        /// </summary>
        public const uint Size = 32;

        /// <summary>
        ///    The identifier used to recognize a WavPack file.
        /// </summary>
        /// <value>
        ///    "wvpk"
        /// </value>
        public static readonly ReadOnlyByteVector FileIdentifier = "wvpk";

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
        ///    WavPack stream in bytes.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///    <paramref name="data" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="CorruptFileException">
        ///    <paramref name="data" /> does not begin with <see
        ///    cref="FileIdentifier" /> or is less than <see cref="Size"
        ///    /> bytes long.
        /// </exception>
        public StreamHeader(ByteVector data, long streamLength)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            if (!data.StartsWith(FileIdentifier))
                throw new CorruptFileException(
                    "Data does not begin with identifier.");

            if (data.Count < Size)
                throw new CorruptFileException(
                    "Insufficient data in stream header");

            _streamLength = streamLength;
            _version = data.Mid(8, 2).ToUShort(false);
            _flags = data.Mid(24, 4).ToUInt(false);
            _samples = data.Mid(12, 4).ToUInt(false);
        }

        /// <summary>
        ///    Gets the duration of the media represented by the current
        ///    instance.
        /// </summary>
        /// <value>
        ///    A <see cref="TimeSpan" /> containing the duration of the
        ///    media represented by the current instance.
        /// </value>
        public TimeSpan Duration => AudioSampleRate > 0 ?
            TimeSpan.FromSeconds(_samples /
                                  (double)AudioSampleRate + 0.5) :
            TimeSpan.Zero;

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
            "WavPack Version {0} Audio", Version);

        /// <summary>
        ///    Gets the bitrate of the audio represented by the current
        ///    instance.
        /// </summary>
        /// <value>
        ///    A <see cref="int" /> value containing a bitrate of the
        ///    audio represented by the current instance.
        /// </value>
        public int AudioBitrate => (int)(Duration > TimeSpan.Zero ?
            ((_streamLength * 8L) /
             Duration.TotalSeconds) / 1000 : 0);

        /// <summary>
        ///    Gets the sample rate of the audio represented by the
        ///    current instance.
        /// </summary>
        /// <value>
        ///    A <see cref="int" /> value containing the sample rate of
        ///    the audio represented by the current instance.
        /// </value>
        public int AudioSampleRate => (int)(SampleRates[
            (_flags & SrateMask) >> SrateLsb]);

        /// <summary>
        ///    Gets the number of channels in the audio represented by
        ///    the current instance.
        /// </summary>
        /// <value>
        ///    A <see cref="int" /> value containing the number of
        ///    channels in the audio represented by the current
        ///    instance.
        /// </value>
        public int AudioChannels => ((_flags & MonoFlag) != 0) ? 1 : 2;

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
        ///    Gets the number of bits per sample in the audio
        ///    represented by the current instance.
        /// </summary>
        /// <value>
        ///    A <see cref="int" /> value containing the number of bits
        ///    per sample in the audio represented by the current
        ///    instance.
        /// </value>
        public int BitsPerSample => (int)(((_flags & BytesStored) + 1) * 8 -
                                           ((_flags & ShiftMask) >> ShiftLsb));

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
                return (int)(_flags ^ _samples ^ _version);
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
            return _flags == other._flags &&
                _samples == other._samples &&
                _version == other._version;
        }

        /// <summary>
        ///    Gets whether or not two instances of <see
        ///    cref="StreamHeader" /> are equal to eachother.
        /// </summary>
        /// <param name="first">
        ///    The first <see cref="StreamHeader" /> object to compare.
        /// </param>
        /// <param name="second">
        ///    The second <see cref="StreamHeader" /> object to compare.
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
        ///    cref="StreamHeader" /> are unequal to eachother.
        /// </summary>
        /// <param name="first">
        ///    The first <see cref="StreamHeader" /> object to compare.
        /// </param>
        /// <param name="second">
        ///    The second <see cref="StreamHeader" /> object to compare.
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