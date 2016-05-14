//
// IFDReader.cs: Panasonic Rw2-specific IFD reader
//
// Author:
//   Ruben Vermeersch (ruben@savanne.be)
//
// Copyright (C) 2010 Ruben Vermeersch
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

using System.IO;
using TagLib.IFD;

namespace TagLib.Tiff.Rw2
{
    /// <summary>
    ///     Panasonic Rw2-specific IFD reader
    /// </summary>
    public class IfdReader : IFD.IfdReader
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
        public IfdReader(BaseTiffFile file, bool isBigendian, IfdStructure structure, long baseOffset, uint ifdOffset, uint maxOffset) :
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
            if (tag == 0x002e && !_seenJpgfromraw)
            {
                // FIXME: JpgFromRaw

                File.Seek(baseOffset + offset, SeekOrigin.Begin);
                var data = File.ReadBlock((int)count);
                var memStream = new MemoryStream(data.Data);
                var res = new StreamJpgAbstraction(memStream);
                (File as File).JpgFromRaw = new Jpeg.File(res, ReadStyle.Average);

                _seenJpgfromraw = true;
                return null;
            }

            return base.ParseIfdEntry(tag, type, count, baseOffset, offset);
        }

        private bool _seenJpgfromraw = false;
    }

    internal class StreamJpgAbstraction : TagLib.File.IFileAbstraction
    {
        private readonly Stream _stream;

        public StreamJpgAbstraction(Stream stream)
        {
            _stream = stream;
        }

        public string Name => "JpgFromRaw.jpg";

        public void CloseStream(Stream stream)
        {
            stream.Dispose();
        }

        public Stream ReadStream => _stream;

        public Stream WriteStream => _stream;
    }
}