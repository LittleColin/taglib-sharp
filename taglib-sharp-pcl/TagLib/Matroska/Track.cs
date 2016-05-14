//
// Track.cs:
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

using System;
using System.Collections.Generic;

namespace TagLib.Matroska
{
    /// <summary>
    /// Describes a Matroska Track.
    /// </summary>
    public class Track : ICodec
    {
#pragma warning disable 414 // Assigned, never used
        private uint _trackNumber;
        private uint _trackUid;
        private string _trackCodecId;
        private readonly string _trackCodecName;
        private string _trackName;
        private readonly string _trackLanguage;
        private bool _trackEnabled;
        private bool _trackDefault;
        private ByteVector _codecData;
#pragma warning restore 414

        private readonly List<EbmlElement> _unknownElems = new List<EbmlElement>();

        /// <summary>
        /// Constructs a <see cref="Track" /> parsing from provided
        /// file data.
        /// Parsing will be done reading from _file at position references by
        /// parent element's data section.
        /// </summary>
        /// <param name="file"><see cref="File" /> instance to read from.</param>
        /// <param name="element">Parent <see cref="EbmlElement" />.</param>
        public Track(File file, EbmlElement element)
        {
            ulong i = 0;

            while (i < element.DataSize)
            {
                EbmlElement child = new EbmlElement(file, element.DataOffset + i);

                MatroskaId matroskaId = (MatroskaId)child.Id;

                switch (matroskaId)
                {
                    case MatroskaId.MatroskaTrackNumber:
                        _trackNumber = child.ReadUInt();
                        break;

                    case MatroskaId.MatroskaTrackUid:
                        _trackUid = child.ReadUInt();
                        break;

                    case MatroskaId.MatroskaCodecId:
                        _trackCodecId = child.ReadString();
                        break;

                    case MatroskaId.MatroskaCodecName:
                        _trackCodecName = child.ReadString();
                        break;

                    case MatroskaId.MatroskaTrackName:
                        _trackName = child.ReadString();
                        break;

                    case MatroskaId.MatroskaTrackLanguage:
                        _trackLanguage = child.ReadString();
                        break;

                    case MatroskaId.MatroskaTrackFlagEnabled:
                        _trackEnabled = child.ReadBool();
                        break;

                    case MatroskaId.MatroskaTrackFlagDefault:
                        _trackDefault = child.ReadBool();
                        break;

                    case MatroskaId.MatroskaCodecPrivate:
                        _codecData = child.ReadBytes();
                        break;

                    default:
                        _unknownElems.Add(child);
                        break;
                }

                i += child.Size;
            }
        }

        /// <summary>
        /// List of unknown elements encountered while parsing.
        /// </summary>
        public List<EbmlElement> UnknownElements => _unknownElems;

        /// <summary>
        /// Describes track duration.
        /// </summary>
        public virtual TimeSpan Duration => TimeSpan.Zero;

        /// <summary>
        /// Describes track media types.
        /// </summary>
        public virtual MediaTypes MediaTypes => MediaTypes.None;

        /// <summary>
        /// Track description.
        /// </summary>
        public virtual string Description => string.Format("{0} {1}", _trackCodecName, _trackLanguage);
    }
}