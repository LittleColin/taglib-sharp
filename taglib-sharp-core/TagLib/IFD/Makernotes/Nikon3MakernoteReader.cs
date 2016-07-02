//
// Nikon3MakernoteReader.cs: Reads Nikon Makernotes.
//
// Author:
//   Ruben Vermeersch (ruben@savanne.be)
//
// Copyright (C) 2009 Ruben Vermeersch
// Copyright (C) 2009 Mike Gemuende
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

using TagLib.IFD.Entries;
using TagLib.IFD.Tags;

namespace TagLib.IFD.Makernotes
{
    /// <summary>
    ///     This class contains Nikon3 makernote specific reading logic.
    /// </summary>
    public class Nikon3MakernoteReader : IfdReader
    {
        /// <summary>
        ///    Constructor. Reads an IFD from given file, using the given endianness.
        /// </summary>
        /// <param name="file">
        ///    A <see cref="File"/> to read from.
        /// </param>
        /// <param name="isBigendian">
        ///     A <see cref="System.Boolean"/>, it must be true, if the data of the IFD should be
        ///     read as bigendian, otherwise false.
        /// </param>
        /// <param name="structure">
        ///    A <see cref="IfdStructure"/> that will be populated.
        /// </param>
        /// <param name="baseOffset">
        ///     A <see cref="System.Int64"/> value describing the base were the IFD offsets
        ///     refer to. E.g. in Jpegs the IFD are located in an Segment and the offsets
        ///     inside the IFD refer from the beginning of this segment. So <paramref
        ///     name="baseOffset"/> must contain the beginning of the segment.
        /// </param>
        /// <param name="ifdOffset">
        ///     A <see cref="System.UInt32"/> value with the beginning of the IFD relative to
        ///     <paramref name="baseOffset"/>.
        /// </param>
        /// <param name="maxOffset">
        /// 	A <see cref="System.UInt32"/> value with maximal possible offset. This is to limit
        ///     the size of the possible data;
        /// </param>
        public Nikon3MakernoteReader(File file, bool isBigendian, IfdStructure structure, long baseOffset, uint ifdOffset, uint maxOffset) :
            base(file, isBigendian, structure, baseOffset, ifdOffset, maxOffset)
        {
        }

        /// <summary>
        ///    Try to parse the given IFD entry, used to discover format-specific entries.
        /// </summary>
        /// <param name="tag">
        ///    A <see cref="System.UInt16"/> with the tag of the entry.
        /// </param>
        /// <param name="type">
        ///    A <see cref="System.UInt16"/> with the type of the entry.
        /// </param>
        /// <param name="count">
        ///    A <see cref="System.UInt32"/> with the data count of the entry.
        /// </param>
        /// <param name="baseOffset">
        ///    A <see cref="System.Int64"/> with the base offset which every offsets in the
        ///    IFD are relative to.
        /// </param>
        /// <param name="offset">
        ///    A <see cref="System.UInt32"/> with the offset of the entry.
        /// </param>
        /// <returns>
        ///    A <see cref="IFdEntry"/> with the given parameters, or null if none was parsed, after
        ///    which the normal TIFF parsing is used.
        /// </returns>
        protected override IFdEntry ParseIfdEntry(ushort tag, ushort type, uint count, long baseOffset, uint offset)
        {
            if (tag == (ushort)Nikon3MakerNoteEntryTag.Preview)
            {
                // SubIFD with Preview Image
                // The entry itself is usually a long
                // TODO: handle JPEGInterchangeFormat and JPEGInterchangeFormatLength correctly

                // The preview field contains a long with an offset to an IFD
                // that contains the preview image. We need to be careful
                // though: this IFD does not contain a valid next-offset
                // pointer. For this reason, we only read the first IFD and
                // ignore the rest (which is preview image data, directly
                // starting after the IFD entries).

                type = (ushort)IfdEntryType.Ifd;

                IfdStructure ifdStructure = new IfdStructure();
                IfdReader reader = CreateSubIfdReader(File, IsBigendian, ifdStructure, baseOffset, offset, MaxOffset);

                reader.Read(1);
                return new SubIfdEntry(tag, type, (uint)ifdStructure.Directories.Length, ifdStructure);
            }
            return base.ParseIfdEntry(tag, type, count, baseOffset, offset);
        }
    }
}