//
// AudioTrack.cs:
//
// Author:
//   Julien Moutte <julien@fluendo.com>
//
// Copyright (C) 2011 FLUENDO S.A.
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

using System.Collections.Generic;

namespace TagLib.Matroska
{
    /// <summary>
    /// Describes a Matroska Audio track.
    /// </summary>
    public class AudioTrack : Track, IAudioCodec
    {
#pragma warning disable 414 // Assigned, never used
        private readonly double _rate;
        private readonly uint _channels;
        private uint _depth;
#pragma warning restore 414

        private readonly List<EbmlElement> _unknownElems = new List<EbmlElement>();

        /// <summary>
        ///  Construct a <see cref="AudioTrack" /> reading information from
        ///  provided file data.
        /// Parsing will be done reading from _file at position references by
        /// parent element's data section.
        /// </summary>
        /// <param name="file"><see cref="File" /> instance to read from.</param>
        /// <param name="element">Parent <see cref="EbmlElement" />.</param>
        public AudioTrack(File file, EbmlElement element)
            : base(file, element)
        {
            MatroskaId matroskaId;

            // Here we handle the unknown elements we know, and store the rest
            foreach (EbmlElement elem in base.UnknownElements)
            {
                matroskaId = (MatroskaId)elem.Id;

                if (matroskaId == MatroskaId.MatroskaTrackAudio)
                {
                    ulong i = 0;

                    while (i < elem.DataSize)
                    {
                        EbmlElement child = new EbmlElement(file, elem.DataOffset + i);

                        matroskaId = (MatroskaId)child.Id;

                        switch (matroskaId)
                        {
                            case MatroskaId.MatroskaAudioChannels:
                                _channels = child.ReadUInt();
                                break;

                            case MatroskaId.MatroskaAudioBitDepth:
                                _depth = child.ReadUInt();
                                break;

                            case MatroskaId.MatroskaAudioSamplingFreq:
                                _rate = child.ReadDouble();
                                break;

                            default:
                                _unknownElems.Add(child);
                                break;
                        }

                        i += child.Size;
                    }
                }
                else
                {
                    _unknownElems.Add(elem);
                }
            }
        }

        /// <summary>
        /// List of unknown elements encountered while parsing.
        /// </summary>
        public new List<EbmlElement> UnknownElements => _unknownElems;

        /// <summary>
        /// This type of track only has audio media type.
        /// </summary>
        public override MediaTypes MediaTypes => MediaTypes.Audio;

        /// <summary>
        /// Audio track bitrate.
        /// </summary>
        public int AudioBitrate => 0;

        /// <summary>
        /// Audio track sampling rate.
        /// </summary>
        public int AudioSampleRate => (int)_rate;

        /// <summary>
        /// Number of audio channels in this track.
        /// </summary>
        public int AudioChannels => (int)_channels;
    }
}