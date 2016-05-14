//
// VideoTrack.cs:
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
    /// Enumeration describing supported Video Aspect Ratio types.
    /// </summary>
    public enum VideoAspectRatioType
    {
        /// <summary>
        /// Free Aspect Ratio.
        /// </summary>
        AspectRatioModeFree = 0x0,

        /// <summary>
        /// Keep Aspect Ratio.
        /// </summary>
        AspectRatioModeKeep = 0x1,

        /// <summary>
        /// Fixed Aspect Ratio.
        /// </summary>
        AspectRatioModeFixed = 0x2
    }

    /// <summary>
    /// Describes a Matroska Video Track.
    /// </summary>
    public class VideoTrack : Track, IVideoCodec
    {
#pragma warning disable 414 // Assigned, never used
        private readonly uint _width;
        private readonly uint _height;
        private uint _dispWidth;
        private uint _dispHeight;
        private double _framerate;
        private bool _interlaced;
        private VideoAspectRatioType _ratioType;
        private ByteVector _fourcc;
#pragma warning restore 414

        private readonly List<EbmlElement> _unknownElems = new List<EbmlElement>();

        /// <summary>
        /// Constructs a <see cref="VideoTrack" /> parsing from provided
        /// file data.
        /// Parsing will be done reading from _file at position references by
        /// parent element's data section.
        /// </summary>
        /// <param name="file"><see cref="File" /> instance to read from.</param>
        /// <param name="element">Parent <see cref="EbmlElement" />.</param>
        public VideoTrack(File file, EbmlElement element)
            : base(file, element)
        {
            MatroskaId matroskaId;

            // Here we handle the unknown elements we know, and store the rest
            foreach (EbmlElement elem in base.UnknownElements)
            {
                matroskaId = (MatroskaId)elem.Id;

                if (matroskaId == MatroskaId.MatroskaTrackVideo)
                {
                    ulong i = 0;

                    while (i < elem.DataSize)
                    {
                        EbmlElement child = new EbmlElement(file, elem.DataOffset + i);

                        matroskaId = (MatroskaId)child.Id;

                        switch (matroskaId)
                        {
                            case MatroskaId.MatroskaVideoDisplayWidth:
                                _dispWidth = child.ReadUInt();
                                break;

                            case MatroskaId.MatroskaVideoDisplayHeight:
                                _dispHeight = child.ReadUInt();
                                break;

                            case MatroskaId.MatroskaVideoPixelWidth:
                                _width = child.ReadUInt();
                                break;

                            case MatroskaId.MatroskaVideoPixelHeight:
                                _height = child.ReadUInt();
                                break;

                            case MatroskaId.MatroskaVideoFrameRate:
                                _framerate = child.ReadDouble();
                                break;

                            case MatroskaId.MatroskaVideoFlagInterlaced:
                                _interlaced = child.ReadBool();
                                break;

                            case MatroskaId.MatroskaVideoAspectRatioType:
                                _ratioType = (VideoAspectRatioType)child.ReadUInt();
                                break;

                            case MatroskaId.MatroskaVideoColourSpace:
                                _fourcc = child.ReadBytes();
                                break;

                            default:
                                _unknownElems.Add(child);
                                break;
                        }

                        i += child.Size;
                    }
                }
                else if (matroskaId == MatroskaId.MatroskaTrackDefaultDuration)
                {
                    uint tmp = elem.ReadUInt();
                    _framerate = 1000000000.0 / tmp;
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
        /// This type of track only has video media type.
        /// </summary>
        public override MediaTypes MediaTypes => MediaTypes.Video;

        /// <summary>
        /// Describes video track width in pixels.
        /// </summary>
        public int VideoWidth => (int)_width;

        /// <summary>
        /// Describes video track height in pixels.
        /// </summary>
        public int VideoHeight => (int)_height;
    }
}