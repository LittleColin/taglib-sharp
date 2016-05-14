//
// Opus.cs:
//
// Author:
//   Les De Ridder (les@lesderid.net)
//
// Copyright (C) 2007 Brian Nickel
// Copyright (C) 2015 Les De Ridder
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

namespace TagLib.Ogg.Codecs
{
    /// <summary>
    ///    This class extends <see cref="Codec" /> and implements <see
    ///    cref="IAudioCodec" /> to provide support for processing Ogg
    ///    Opus bitstreams.
    /// </summary>
    public class Opus : Codec, IAudioCodec
    {
        /// <summary>
        ///    Contains the file identifier.
        /// </summary>
        private static readonly ByteVector _magicSignatureBase = "Opus";

        private static readonly ByteVector _magicSignatureHeader = "OpusHead";
        private static readonly ByteVector _magicSignatureComment = "OpusTags";
        private static readonly int _magicSignatureLength = 8;

        /// <summary>
        ///    Contains the header packet.
        /// </summary>
        private HeaderPacket _header;

        /// <summary>
        ///    Contains the comment data.
        /// </summary>
        private ByteVector _commentData;

        /// <summary>
        ///    Constructs and initializes a new instance of <see
        ///    cref="Opus" />.
        /// </summary>
        private Opus()
        {
        }

        /// <summary>
        ///    Reads a Ogg packet that has been encountered in the
        ///    stream.
        /// </summary>
        /// <param name="packet">
        ///    A <see cref="ByteVector" /> object containing a packet to
        ///    be read by the current instance.
        /// </param>
        /// <param name="index">
        ///    A <see cref="int" /> value containing the index of the
        ///    packet in the stream.
        /// </param>
        /// <returns>
        ///    <see langword="true" /> if the codec has read all the
        ///    necessary packets for the stream and does not need to be
        ///    called again, typically once the Xiph comment has been
        ///    found. Otherwise <see langword="false" />.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///    <paramref name="packet" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///    <paramref name="index" /> is less than zero.
        /// </exception>
        /// <exception cref="CorruptFileException">
        ///    The data does not conform to the specificiation for the
        ///    codec represented by the current instance.
        /// </exception>
        public override bool ReadPacket(ByteVector packet, int index)
        {
            if (packet == null)
                throw new ArgumentNullException(nameof(packet));

            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index),
                    "index must be at least zero.");

            ByteVector signature = MagicSignature(packet);
            if (signature != _magicSignatureHeader && index == 0)
                throw new CorruptFileException(
                    "Stream does not begin with opus header.");

            if (_commentData == null)
            {
                if (signature == _magicSignatureHeader)
                    _header = new HeaderPacket(packet);
                else if (signature == _magicSignatureComment)
                    _commentData =
                        packet.Mid(_magicSignatureLength);
                else
                    return true;
            }

            return _commentData != null;
        }

        /// <summary>
        ///    Computes the duration of the stream using the first and
        ///    last granular positions of the stream.
        /// </summary>
        /// <param name="firstGranularPosition">
        ///    A <see cref="long" /> value containing the first granular
        ///    position of the stream.
        /// </param>
        /// <param name="lastGranularPosition">
        ///    A <see cref="long" /> value containing the last granular
        ///    position of the stream.
        /// </param>
        /// <returns>
        ///    A <see cref="TimeSpan" /> value containing the duration
        ///    of the stream.
        /// </returns>
        public override TimeSpan GetDuration(long firstGranularPosition,
                                              long lastGranularPosition)
        {
            return TimeSpan.FromSeconds((lastGranularPosition -
                                          firstGranularPosition
                                          - 2 * _header.PreSkip) /
                    (double)48000);
        }

        /// <summary>
        ///    Replaces the comment packet in a collection of packets
        ///    with the rendered version of a Xiph comment or inserts a
        ///    comment packet if the stream lacks one.
        /// </summary>
        /// <param name="packets">
        ///    A <see cref="ByteVectorCollection" /> object containing
        ///    a collection of packets.
        /// </param>
        /// <param name="comment">
        ///    A <see cref="XiphComment" /> object to store the rendered
        ///    version of in <paramref name="packets" />.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///    <paramref name="packets" /> or <paramref name="comment"
        ///    /> is <see langword="null" />.
        /// </exception>
        public override void SetCommentPacket(ByteVectorCollection packets,
                                               XiphComment comment)
        {
            if (packets == null)
                throw new ArgumentNullException(nameof(packets));

            if (comment == null)
                throw new ArgumentNullException(nameof(comment));

            ByteVector data = new ByteVector();
            data.Add(_magicSignatureComment);
            data.Add(comment.Render(true));
            if (packets.Count > 1 && MagicSignature(packets[1])
                          == _magicSignatureComment)
                packets[1] = data;
            else
                packets.Insert(1, data);
        }

        /// <summary>
        ///    Gets the bitrate of the audio represented by the current
        ///    instance.
        /// </summary>
        /// <value>
        ///    A <see cref="int" /> value containing a bitrate of the
        ///    audio represented by the current instance.
        /// </value>
        /// <remarks>
        ///    Always returns zero, since bitrate is variable and no
        ///    information is stored in the Ogg header (unlike e.g. Vorbis).
        /// </remarks>
        public int AudioBitrate => 0;

        /// <summary>
        ///    Gets the sample rate of the audio represented by the
        ///    current instance.
        /// </summary>
        /// <value>
        ///    A <see cref="int" /> value containing the original
        ///    sample rate of the audio represented by the current instance.
        /// </value>
        public int AudioSampleRate => (int)_header.InputSampleRate;

        /// <summary>
        ///    Gets the number of channels in the audio represented by
        ///    the current instance.
        /// </summary>
        /// <value>
        ///    A <see cref="int" /> value containing the number of
        ///    channels in the audio represented by the current
        ///    instance.
        /// </value>
        public int AudioChannels => (int)_header.ChannelCount;

        /// <summary>
        ///    Gets the types of media represented by the current
        ///    instance.
        /// </summary>
        /// <value>
        ///    Always <see cref="MediaTypes.Audio" />.
        /// </value>
        public override MediaTypes MediaTypes => MediaTypes.Audio;

        /// <summary>
        ///    Gets the raw Xiph comment data contained in the codec.
        /// </summary>
        /// <value>
        ///    A <see cref="ByteVector" /> object containing a raw Xiph
        ///    comment or <see langword="null"/> if none was found.
        /// </value>
        public override ByteVector CommentData => _commentData;

        /// <summary>
        ///    Gets a text description of the media represented by the
        ///    current instance.
        /// </summary>
        /// <value>
        ///    A <see cref="string" /> object containing a description
        ///    of the media represented by the current instance.
        /// </value>
        public override string Description => string.Format(
            "Opus Version {0} Audio",
            _header.OpusVersion);

        /// <summary>
        ///    Implements the <see cref="CodecProvider" /> delegate to
        ///    provide support for recognizing a Opus stream from the
        ///    header packet.
        /// </summary>
        /// <param name="packet">
        ///    A <see cref="ByteVector" /> object containing the stream
        ///    header packet.
        /// </param>
        /// <returns>
        ///    A <see cref="Codec"/> object containing a codec capable
        ///    of parsing the stream of <see langref="null" /> if the
        ///    stream is not a Opus stream.
        /// </returns>
        public static Codec FromPacket(ByteVector packet)
        {
            return (MagicSignature(packet) == _magicSignatureHeader) ?
                new Opus() : null;
        }

        /// <summary>
        ///    Gets the magic signature for a specified Opus packet.
        /// </summary>
        /// <param name="packet">
        ///    A <see cref="ByteVector" /> object containing a Opus
        ///    packet.
        /// </param>
        /// <returns>
        ///    A <see cref="ByteVector" /> value containing the magic
        ///    signature or null if the packet is invalid.
        /// </returns>
        private static ByteVector MagicSignature(ByteVector packet)
        {
            if (packet.Count < _magicSignatureLength)
                return null;

            for (int i = 0; i < _magicSignatureBase.Count; i++)
                if (packet[i] != _magicSignatureBase[i])
                    return null;

            return packet.Mid(0, _magicSignatureLength);
        }

        /// <summary>
        ///    This structure represents a Opus header packet.
        /// </summary>
        private struct HeaderPacket
        {
            public readonly uint OpusVersion;
            public readonly uint ChannelCount;
            public readonly uint PreSkip;
            public readonly uint InputSampleRate;
            public uint OutputGain;
            public readonly uint ChannelMap;
            public uint StreamCount;
            public uint TwoChannelStreamCount;
            public readonly uint[] ChannelMappings;

            public HeaderPacket(ByteVector data)
            {
                OpusVersion = data[8];
                ChannelCount = data[9];
                PreSkip = data.Mid(10, 2).ToUInt(false);
                InputSampleRate = data.Mid(12, 4).ToUInt(false);
                OutputGain = data.Mid(16, 2).ToUInt(false);
                ChannelMap = data[18];

                if (ChannelMap == 0)
                {
                    StreamCount = 1;
                    TwoChannelStreamCount = ChannelCount - 1;

                    ChannelMappings = new uint[ChannelCount];
                    ChannelMappings[0] = 0;
                    if (ChannelCount == 2)
                    {
                        ChannelMappings[1] = 1;
                    }
                }
                else
                {
                    StreamCount = data[19];
                    TwoChannelStreamCount = data[20];

                    ChannelMappings = new uint[ChannelCount];
                    for (int i = 0; i < ChannelCount; i++)
                    {
                        ChannelMappings[i] = data[21 + i];
                    }
                }
            }
        }
    }
}