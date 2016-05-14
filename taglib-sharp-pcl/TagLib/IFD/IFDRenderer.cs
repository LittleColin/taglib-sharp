//
// IFDRenderer.cs: Outputs an IFD structure into TIFF IFD bytes.
//
// Author:
//   Ruben Vermeersch (ruben@savanne.be)
//   Mike Gemuende (mike@gemuende.de)
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

using System;
using System.Collections.Generic;
using TagLib.IFD.Entries;

namespace TagLib.IFD
{
    /// <summary>
    ///     This class contains all the IFD rendering code.
    /// </summary>
    public class IfdRenderer
    {
        /// <summary>
        ///    A <see cref="System.UInt32"/> value with the offset of the
        ///    current IFD. All offsets inside the IFD must be adjusted
        ///    according to this given offset.
        /// </summary>
        private readonly uint _ifdOffset;

        /// <summary>
        ///    If IFD should be encoded in BigEndian or not.
        /// </summary>
        private readonly bool _isBigendian;

        /// <summary>
        ///    The IFD structure that will be rendered.
        /// </summary>
        private readonly IfdStructure _structure;

        /// <summary>
        ///    Constructor. Will render the given IFD structure.
        /// </summary>
        /// <param name="isBigendian">
        ///    If IFD should be encoded in BigEndian or not.
        /// </param>
        /// <param name="structure">
        ///    The IFD structure that will be rendered.
        /// </param>
        /// <param name="ifdOffset">
        ///    A <see cref="System.UInt32"/> value with the offset of the
        ///    current IFD. All offsets inside the IFD must be adjusted
        ///    according to this given offset.
        /// </param>
        public IfdRenderer(bool isBigendian, IfdStructure structure, uint ifdOffset)
        {
            _isBigendian = isBigendian;
            _structure = structure;
            _ifdOffset = ifdOffset;
        }

        /// <summary>
        ///    Renders the current instance to a <see cref="ByteVector"/>.
        /// </summary>
        /// <returns>
        ///    A <see cref="ByteVector"/> containing the rendered IFD.
        /// </returns>
        public ByteVector Render()
        {
            ByteVector ifdData = new ByteVector();

            uint currentOffset = _ifdOffset;
            var directories = _structure.directories;

            for (int index = 0; index < directories.Count; index++)
            {
                ByteVector data = RenderIfd(directories[index], currentOffset, index == directories.Count - 1);
                currentOffset += (uint)data.Count;
                ifdData.Add(data);
            }

            return ifdData;
        }

        /// <summary>
        ///    Constructs a new IFD Renderer used to render a <see cref="SubIfdEntry"/>.
        /// </summary>
        /// <param name="isBigendian">
        ///    If IFD should be encoded in BigEndian or not.
        /// </param>
        /// <param name="structure">
        ///    The IFD structure that will be rendered.
        /// </param>
        /// <param name="ifdOffset">
        ///    A <see cref="System.UInt32"/> value with the offset of the
        ///    current IFD. All offsets inside the IFD must be adjusted
        ///    according to this given offset.
        /// </param>
        protected virtual IfdRenderer CreateSubRenderer(bool isBigendian, IfdStructure structure, uint ifdOffset)
        {
            return new IfdRenderer(isBigendian, structure, ifdOffset);
        }

        /// <summary>
        ///    Adds the data of a single entry to <paramref name="entryData"/>.
        /// </summary>
        /// <param name="entryData">
        ///    A <see cref="ByteVector"/> to add the entry to.
        /// </param>
        /// <param name="tag">
        ///    A <see cref="System.UInt16"/> with the tag of the entry.
        /// </param>
        /// <param name="type">
        ///    A <see cref="System.UInt16"/> with the type of the entry.
        /// </param>
        /// <param name="count">
        ///    A <see cref="System.UInt32"/> with the data count of the entry,
        /// </param>
        /// <param name="offset">
        ///    A <see cref="System.UInt32"/> with the offset field of the entry.
        /// </param>
        protected void RenderEntry(ByteVector entryData, ushort tag, ushort type, uint count, uint offset)
        {
            entryData.Add(ByteVector.FromUShort(tag, _isBigendian));
            entryData.Add(ByteVector.FromUShort(type, _isBigendian));
            entryData.Add(ByteVector.FromUInt(count, _isBigendian));
            entryData.Add(ByteVector.FromUInt(offset, _isBigendian));
        }

        /// <summary>
        ///    Renders a complete entry together with the data. The entry itself
        ///    is stored in <paramref name="entryData"/> and the data of the
        ///    entry is stored in <paramref name="offsetData"/> if it cannot be
        ///    stored in the offset. This method is called for every <see
        ///    cref="IFdEntry"/> of this IFD and can be overwritten in subclasses
        ///    to provide special behavior.
        /// </summary>
        /// <param name="entry">
        ///    A <see cref="IFdEntry"/> with the entry to render.
        /// </param>
        /// <param name="entryData">
        ///    A <see cref="ByteVector"/> to add the entry to.
        /// </param>
        /// <param name="offsetData">
        ///    A <see cref="ByteVector"/> to add the entry data to if it cannot be
        ///    stored in the offset field.
        /// </param>
        /// <param name="dataOffset">
        ///    A <see cref="System.UInt32"/> with the offset, were the data of the
        ///    entries starts. It is needed to adjust the offsets of the entries
        ///    itself.
        /// </param>
        protected virtual void RenderEntryData(IFdEntry entry, ByteVector entryData, ByteVector offsetData, uint dataOffset)
        {
            ushort tag = entry.Tag;
            uint offset = (uint)(dataOffset + offsetData.Count);

            ushort type;
            uint count;
            ByteVector data = entry.Render(_isBigendian, offset, out type, out count);

            // store data in offset, if it is smaller than 4 byte
            if (data.Count <= 4)
            {
                while (data.Count < 4)
                    data.Add("\0");

                offset = data.ToUInt(_isBigendian);
                data = null;
            }

            // preserve word boundary of offsets
            if (data != null && data.Count % 2 != 0)
                data.Add("\0");

            RenderEntry(entryData, tag, type, count, offset);
            offsetData.Add(data);
        }

        /// <summary>
        ///    Renders the IFD to an ByteVector where the offset of the IFD
        ///    itself is <paramref name="ifdOffset"/> and all offsets
        ///    contained in the IFD are adjusted accroding it.
        /// </summary>
        /// <param name="directory">
        ///    A <see cref="IfdDirectory"/> with the directory to render.
        /// </param>
        /// <param name="ifdOffset">
        ///    A <see cref="System.UInt32"/> with the offset of the IFD
        /// </param>
        /// <param name="last">
        ///    A <see cref="System.Boolean"/> which is true, if the IFD is
        ///    the last one, i.e. the offset to the next IFD, which is
        ///    stored inside the IFD, is 0. If the value is false, the
        ///    offset to the next IFD is set that it starts directly after
        ///    the current one.
        /// </param>
        /// <returns>
        ///    A <see cref="ByteVector"/> with the rendered IFD.
        /// </returns>
        private ByteVector RenderIfd(IfdDirectory directory, uint ifdOffset, bool last)
        {
            if (directory.Count > ushort.MaxValue)
                throw new Exception(string.Format("Directory has too much entries: {0}", directory.Count));

            // Remove empty SUB ifds.
            var tags = new List<ushort>(directory.Keys);
            foreach (var tag in tags)
            {
                var entry = directory[tag];
                if (entry is SubIfdEntry && (entry as SubIfdEntry).ChildCount == 0)
                {
                    directory.Remove(tag);
                }
            }

            ushort entryCount = (ushort)directory.Count;

            // ifd_offset + size of entry_count + entries + next ifd offset
            uint dataOffset = ifdOffset + 2 + 12 * (uint)entryCount + 4;

            // store the entries itself
            ByteVector entryData = new ByteVector();

            // store the data referenced by the entries
            ByteVector offsetData = new ByteVector();

            entryData.Add(ByteVector.FromUShort(entryCount, _isBigendian));

            foreach (IFdEntry entry in directory.Values)
                RenderEntryData(entry, entryData, offsetData, dataOffset);

            if (last)
                entryData.Add("\0\0\0\0");
            else
                entryData.Add(ByteVector.FromUInt((uint)(dataOffset + offsetData.Count), _isBigendian));

            if (dataOffset - ifdOffset != entryData.Count)
                throw new Exception(string.Format("Expected IFD data size was {0} but is {1}", dataOffset - ifdOffset, entryData.Count));

            entryData.Add(offsetData);

            return entryData;
        }
    }
}