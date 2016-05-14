//
// EBMLIDs.cs:
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

namespace TagLib.Matroska
{
    /// <summary>
    /// Public enumeration listing the possible EBML element identifiers.
    /// </summary>
    public enum Ebmlid
    {
        /// <summary>
        /// Indicates an EBML Header element.
        /// </summary>
        EbmlHeader = 0x1A45DFA3,

        /// <summary>
        /// Indicates an EBML Version element.
        /// </summary>
        EbmlVersion = 0x4286,

        /// <summary>
        /// Indicates an EBML Read Version element.
        /// </summary>
        EbmlReadVersion = 0x42F7,

        /// <summary>
        /// Indicates an EBML Max ID Length element.
        /// </summary>
        EbmlMaxIdLength = 0x42F2,

        /// <summary>
        /// Indicates an EBML Max Size Length element.
        /// </summary>
        EbmlMaxSizeLength = 0x42F3,

        /// <summary>
        /// Indicates an EBML Doc Type element.
        /// </summary>
        EbmlDocType = 0x4282,

        /// <summary>
        /// Indicates an EBML Doc Type Version element.
        /// </summary>
        EbmlDocTypeVersion = 0x4287,

        /// <summary>
        /// Indicates an EBML Doc Type Read Version element.
        /// </summary>
        EbmlDocTypeReadVersion = 0x4285,

        /// <summary>
        /// Indicates an EBML Void element.
        /// </summary>
        EbmlVoid = 0xEC
    }
}