//
// SubtitleTrack.cs:
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
    /// Describes a Matroska Subtitle Track.
    /// </summary>
    public class SubtitleTrack : Track
    {
        private readonly List<EbmlElement> _unknownElems = new List<EbmlElement>();

        /// <summary>
        /// Constructs a <see cref="SubtitleTrack" /> parsing from provided
        /// file data.
        /// Parsing will be done reading from _file at position references by
        /// parent element's data section.
        /// </summary>
        /// <param name="file"><see cref="File" /> instance to read from.</param>
        /// <param name="element">Parent <see cref="EbmlElement" />.</param>
        public SubtitleTrack(File file, EbmlElement element)
            : base(file, element)
        {
            // Here we handle the unknown elements we know, and store the rest
            foreach (EbmlElement elem in base.UnknownElements)
            {
                MatroskaId matroskaId = (MatroskaId)elem.Id;

                switch (matroskaId)
                {
                    default:
                        _unknownElems.Add(elem);
                        break;
                }
            }
        }

        /// <summary>
        /// List of unknown elements encountered while parsing.
        /// </summary>
        public new List<EbmlElement> UnknownElements => _unknownElems;

        /// <summary>
        /// This type of track only has text media type.
        /// </summary>
        public override MediaTypes MediaTypes => MediaTypes.Text;
    }
}