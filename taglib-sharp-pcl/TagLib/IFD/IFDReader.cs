//
// IFDReader.cs: Parses TIFF IFDs and populates an IFD structure.
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
using System.IO;
using TagLib.IFD.Entries;
using TagLib.IFD.Makernotes;
using TagLib.IFD.Tags;

namespace TagLib.IFD
{
    /// <summary>
    ///     This class contains all the IFD reading and parsing code.
    /// </summary>
    public class IfdReader
    {
        /// <summary>
        ///     A <see cref="System.Int64"/> value describing the base were the IFD offsets
        ///     refer to. E.g. in Jpegs the IFD are located in an Segment and the offsets
        ///     inside the IFD refer from the beginning of this segment. So base_offset must
        ///     contain the beginning of the segment.
        /// </summary>
        protected readonly long BaseOffset;

        /// <summary>
        ///    The <see cref="TagLib.File" /> where this IFD is found in.
        /// </summary>
        protected readonly File File;

        /// <summary>
        ///     A <see cref="System.UInt32"/> value with the beginning of the IFD relative to
        ///     base_offset.
        /// </summary>
        protected readonly uint IfdOffset;

        /// <summary>
        ///    If IFD is encoded in BigEndian or not
        /// </summary>
        protected readonly bool IsBigendian;

        /// <summary>
        ///    A <see cref="System.UInt32"/> with the maximal offset, which should occur in the
        ///    IFD. Greater offsets, would reference beyond the considered data.
        /// </summary>
        protected readonly uint MaxOffset;

        /// <summary>
        ///    The IFD structure that will be populated
        /// </summary>
        protected readonly IfdStructure Structure;

        /// <summary>
        ///    Whether or not the makernote should be parsed.
        /// </summary>
        protected bool parse_makernote = true;

        private static readonly Dictionary<File, int> _ifdLoopdetectRefs = new Dictionary<File, int>();
        private static readonly Dictionary<File, List<long>> _ifdOffsets = new Dictionary<File, List<long>>();
        private static readonly string LeicaHeader = "LEICA\0\0\0";
        private static readonly string NikonHeader = "Nikon\0";
        private static readonly string Olympus1Header = "OLYMP\0";
        private static readonly string Olympus2Header = "OLYMPUS\0";
        private static readonly string PanasonicHeader = "Panasonic\0\0\0";
        private static readonly string PentaxHeader = "AOC\0";
        private static readonly string SonyHeader = "SONY DSC \0\0\0";

        /// <summary>
        ///    Constructor. Reads an IFD from given file, using the given endianness.
        /// </summary>
        /// <param name="file">
        ///    A <see cref="TagLib.File"/> to read from.
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
        public IfdReader(File file, bool isBigendian, IfdStructure structure, long baseOffset, uint ifdOffset, uint maxOffset)
        {
            File = file;
            IsBigendian = isBigendian;
            Structure = structure;
            BaseOffset = baseOffset;
            IfdOffset = ifdOffset;
            MaxOffset = maxOffset;
        }

        /// <summary>
        ///    Whether or not the makernote should be parsed.
        /// </summary>
        internal bool ShouldParseMakernote
        {
            get { return parse_makernote; }
            set { parse_makernote = value; }
        }

        /// <summary>
        ///    Read all IFD segments from the file.
        /// </summary>
        public void Read()
        {
            Read(-1);
        }

        /// <summary>
        ///    Read IFD segments from the file.
        /// </summary>
        /// <para>
        ///    The number of IFDs that may be read can be restricted using the count
        ///    parameter. This might be needed for fiels that have invalid next-ifd
        ///    pointers (such as some IFDs in the Nikon Makernote). This condition is
        ///    tested in the Nikon2 unit test, which contains such a file.
        /// </para>
        /// <param name="count">
        ///     A <see cref="System.Int32"/> with the maximal number of IFDs to read.
        ///     Passing -1 means unlimited.
        /// </param>
        public void Read(int count)
        {
            if (count == 0)
                return;

            uint nextOffset = IfdOffset;
            int i = 0;

            lock (File)
            {
                StartIfdLoopDetect();
                do
                {
                    if (DetectIfdLoop(BaseOffset + nextOffset))
                    {
                        File.MarkAsCorrupt("IFD loop detected");
                        break;
                    }
                    nextOffset = ReadIfd(BaseOffset, nextOffset, MaxOffset);
                } while (nextOffset > 0 && (count == -1 || ++i < count));

                StopIfdLoopDetect();
            }
        }

        /// <summary>
        ///    Create a reader for Sub IFD entries.
        /// </summary>
        /// <param name="file">
        ///    A <see cref="TagLib.File"/> to read from.
        /// </param>
        /// <param name="isBigendian">
        ///     A <see cref="System.Boolean"/>, it must be true, if the data of the IFD should be
        ///     read as bigendian, otherwise false.
        /// </param>
        /// <param name="structure">
        ///    A <see cref="IfdStructure"/> that will be populated.
        /// </param>
        /// <param name="baseOffset">
        ///    A <see cref="System.Int64"/> with the base offset which every offsets in the
        ///    IFD are relative to.
        /// </param>
        /// <param name="offset">
        ///    A <see cref="System.UInt32"/> with the offset of the entry.
        /// </param>
        /// <param name="maxOffset">
        ///    A <see cref="System.UInt32"/> with the maximal offset to consider for
        ///    the IFD.
        /// </param>
        /// <returns>
        ///    A <see cref="IfdReader"/> which can be used to read the specified sub IFD.
        /// </returns>
        protected virtual IfdReader CreateSubIfdReader(File file, bool isBigendian, IfdStructure structure, long baseOffset, uint offset, uint maxOffset)
        {
            return new IfdReader(file, isBigendian, structure, baseOffset, offset, maxOffset);
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
        protected virtual IFdEntry ParseIfdEntry(ushort tag, ushort type, uint count, long baseOffset, uint offset)
        {
            if (tag == (ushort)ExifEntryTag.MakerNote && parse_makernote)
                return ParseMakernote(tag, type, count, baseOffset, offset);

            if (tag == (ushort)IfdEntryTag.SubIfDs)
            {
                var entries = new List<IfdStructure>();

                uint[] data;
                if (count >= 2)
                {
                    // This is impossible right?
                    if (baseOffset + offset > File.Length)
                    {
                        File.MarkAsCorrupt("Length of SubIFD is too long");
                        return null;
                    }

                    File.Seek(baseOffset + offset, SeekOrigin.Begin);
                    data = ReadUIntArray(count);
                }
                else
                {
                    data = new uint[] { offset };
                }

                foreach (var subOffset in data)
                {
                    var subStructure = new IfdStructure();
                    var subReader = CreateSubIfdReader(File, IsBigendian, subStructure, baseOffset, subOffset, MaxOffset);
                    subReader.Read();

                    entries.Add(subStructure);
                }
                return new SubIfdArrayEntry(tag, entries);
            }

            IfdStructure ifdStructure = new IfdStructure();
            IfdReader reader = CreateSubIfdReader(File, IsBigendian, ifdStructure, baseOffset, offset, MaxOffset);

            // Sub IFDs are either identified by the IFD-type ...
            if (type == (ushort)IfdEntryType.Ifd)
            {
                reader.Read();
                return new SubIfdEntry(tag, type, (uint)ifdStructure.Directories.Length, ifdStructure);
            }

            // ... or by one of the following tags
            switch (tag)
            {
                case (ushort)IfdEntryTag.ExifIfd:
                case (ushort)IfdEntryTag.InteroperabilityIfd:
                case (ushort)IfdEntryTag.Gpsifd:
                    reader.Read();
                    return new SubIfdEntry(tag, (ushort)IfdEntryType.Long, 1, ifdStructure);

                default:
                    return null;
            }
        }

        /// <summary>
        ///    Creates an IFDEntry from the given values. This method is used for
        ///    every entry. Custom parsing can be hooked in by overriding the
        ///    <see cref="ParseIfdEntry"/> method.
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
        ///    A <see cref="System.Int64"/> with the base offset which every
        ///    offsets in the IFD are relative to.
        /// </param>
        /// <param name="offsetData">
        ///    A <see cref="ByteVector"/> containing exactly 4 byte with the data
        ///    of the offset of the entry. Since this field isn't interpreted as
        ///    an offset if the data can be directly stored in the 4 byte, we
        ///    pass the <see cref="ByteVector"/> to easier interpret it.
        /// </param>
        /// <param name="maxOffset">
        ///    A <see cref="System.UInt32"/> with the maximal offset to consider for
        ///    the IFD.
        /// </param>
        /// <returns>
        ///    A <see cref="IFdEntry"/> with the given parameter.
        /// </returns>
        private IFdEntry CreateIfdEntry(ushort tag, ushort type, uint count, long baseOffset, ByteVector offsetData, uint maxOffset)
        {
            uint offset = offsetData.ToUInt(IsBigendian);

            // Fix the type for the IPTC tag.
            // From http://www.awaresystems.be/imaging/tiff/tifftags/iptc.html
            // "Often times, the datatype is incorrectly specified as LONG. "
            if (tag == (ushort)IfdEntryTag.Iptc && type == (ushort)IfdEntryType.Long)
            {
                type = (ushort)IfdEntryType.Byte;
            }

            var ifdEntry = ParseIfdEntry(tag, type, count, baseOffset, offset);
            if (ifdEntry != null)
                return ifdEntry;

            if (count > 0x10000000)
            {
                // Some Nikon files are known to exhibit this corruption (or "feature").
                File.MarkAsCorrupt("Impossibly large item count");
                return null;
            }

            // then handle the values stored in the offset data itself
            if (count == 1)
            {
                if (type == (ushort)IfdEntryType.Byte)
                    return new ByteIfdEntry(tag, offsetData[0]);

                if (type == (ushort)IfdEntryType.SByte)
                    return new SByteIfdEntry(tag, (sbyte)offsetData[0]);

                if (type == (ushort)IfdEntryType.Short)
                    return new ShortIfdEntry(tag, offsetData.Mid(0, 2).ToUShort(IsBigendian));

                if (type == (ushort)IfdEntryType.SShort)
                    return new SShortIfdEntry(tag, (short)offsetData.Mid(0, 2).ToUShort(IsBigendian));

                if (type == (ushort)IfdEntryType.Long)
                    return new LongIfdEntry(tag, offsetData.ToUInt(IsBigendian));

                if (type == (ushort)IfdEntryType.SLong)
                    return new SLongIfdEntry(tag, offsetData.ToInt(IsBigendian));
            }

            if (count == 2)
            {
                if (type == (ushort)IfdEntryType.Short)
                {
                    ushort[] data = new ushort[] {
                        offsetData.Mid (0, 2).ToUShort (IsBigendian),
                        offsetData.Mid (2, 2).ToUShort (IsBigendian)
                    };

                    return new ShortArrayIfdEntry(tag, data);
                }

                if (type == (ushort)IfdEntryType.SShort)
                {
                    short[] data = new short[] {
                        (short) offsetData.Mid (0, 2).ToUShort (IsBigendian),
                        (short) offsetData.Mid (2, 2).ToUShort (IsBigendian)
                    };

                    return new SShortArrayIfdEntry(tag, data);
                }
            }

            if (count <= 4)
            {
                if (type == (ushort)IfdEntryType.Undefined)
                    return new UndefinedIfdEntry(tag, offsetData.Mid(0, (int)count));

                if (type == (ushort)IfdEntryType.Ascii)
                {
                    string data = offsetData.Mid(0, (int)count).ToString();
                    int term = data.IndexOf('\0');

                    if (term > -1)
                        data = data.Substring(0, term);

                    return new StringIfdEntry(tag, data);
                }

                if (type == (ushort)IfdEntryType.Byte)
                    return new ByteVectorIfdEntry(tag, offsetData.Mid(0, (int)count));
            }

            // FIXME: create correct type.
            if (offset > maxOffset)
                return new UndefinedIfdEntry(tag, new ByteVector());

            // then handle data referenced by the offset
            File.Seek(baseOffset + offset, SeekOrigin.Begin);

            if (count == 1)
            {
                if (type == (ushort)IfdEntryType.Rational)
                    return new RationalIfdEntry(tag, ReadRational());

                if (type == (ushort)IfdEntryType.SRational)
                    return new SRationalIfdEntry(tag, ReadSRational());
            }

            if (count > 1)
            {
                if (type == (ushort)IfdEntryType.Long)
                {
                    uint[] data = ReadUIntArray(count);

                    return new LongArrayIfdEntry(tag, data);
                }

                if (type == (ushort)IfdEntryType.SLong)
                {
                    int[] data = ReadIntArray(count);

                    return new SLongArrayIfdEntry(tag, data);
                }

                if (type == (ushort)IfdEntryType.Rational)
                {
                    Rational[] entries = new Rational[count];

                    for (int i = 0; i < count; i++)
                        entries[i] = ReadRational();

                    return new RationalArrayIfdEntry(tag, entries);
                }

                if (type == (ushort)IfdEntryType.SRational)
                {
                    SRational[] entries = new SRational[count];

                    for (int i = 0; i < count; i++)
                        entries[i] = ReadSRational();

                    return new SRationalArrayIfdEntry(tag, entries);
                }
            }

            if (count > 2)
            {
                if (type == (ushort)IfdEntryType.Short)
                {
                    ushort[] data = ReadUShortArray(count);

                    return new ShortArrayIfdEntry(tag, data);
                }

                if (type == (ushort)IfdEntryType.SShort)
                {
                    short[] data = ReadShortArray(count);

                    return new SShortArrayIfdEntry(tag, data);
                }
            }

            if (count > 4)
            {
                if (type == (ushort)IfdEntryType.Long)
                {
                    uint[] data = ReadUIntArray(count);

                    return new LongArrayIfdEntry(tag, data);
                }

                if (type == (ushort)IfdEntryType.Byte)
                {
                    ByteVector data = File.ReadBlock((int)count);

                    return new ByteVectorIfdEntry(tag, data);
                }

                if (type == (ushort)IfdEntryType.Ascii)
                {
                    string data = ReadAsciiString((int)count);

                    return new StringIfdEntry(tag, data);
                }

                if (tag == (ushort)ExifEntryTag.UserComment)
                {
                    ByteVector data = File.ReadBlock((int)count);

                    return new UserCommentIfdEntry(tag, data, File);
                }

                if (type == (ushort)IfdEntryType.Undefined)
                {
                    ByteVector data = File.ReadBlock((int)count);

                    return new UndefinedIfdEntry(tag, data);
                }
            }

            if (type == (ushort)IfdEntryType.Float)
                return null;

            if (type == 0 || type > 12)
            {
                // Invalid type
                File.MarkAsCorrupt("Invalid item type");
                return null;
            }

            // TODO: We should ignore unreadable values, erroring for now until we have sufficient coverage.
            throw new NotImplementedException(string.Format("Unknown type/count {0}/{1} ({2})", type, count, offset));
        }

        /// <summary>
        ///    Attempts to detect whether or not this file has an endless IFD loop.
        /// </summary>
        /// <param name="offset">
        ///    A <see cref="System.UInt32"/> with the offset at which the next IFD
        ///    can be found.
        /// </param>
        /// <returns>
        ///    True if we have gone into a loop, false otherwise.
        /// </returns>
        private bool DetectIfdLoop(long offset)
        {
            if (offset == 0)
                return false;
            if (_ifdOffsets[File].Contains(offset))
                return true;
            _ifdOffsets[File].Add(offset);
            return false;
        }

        /// <summary>
        ///    Performs some fixups to a read <see cref="IfdDirectory"/>. For some
        ///    special cases multiple <see cref="IFdEntry"/> instances contained
        ///    in the directory are needed. Therfore, we do the fixups after reading the
        ///    whole directory to be sure, all entries are present.
        /// </summary>
        /// <param name="baseOffset">
        ///    A <see cref="System.Int64"/> value with the base offset, all offsets in the
        ///    directory refers to.
        /// </param>
        /// <param name="directory">
        ///    A <see cref="IfdDirectory"/> instance which was read and needs fixes.
        /// </param>
        private void FixupDirectory(long baseOffset, IfdDirectory directory)
        {
            // The following two entries refer to thumbnail data, where one is  the offset
            // to the data and the other is the length. Unnaturally both are used to describe
            // the data. So it is needed to keep both entries in sync and keep the thumbnail data
            // for writing it back.
            // We determine the position of the data, read it and store it in an ThumbnailDataIFDEntry
            // which replaces the offset-entry to thumbnail data.
            ushort offsetTag = (ushort)IfdEntryTag.JpegInterchangeFormat;
            ushort lengthTag = (ushort)IfdEntryTag.JpegInterchangeFormatLength;
            if (directory.ContainsKey(offsetTag) && directory.ContainsKey(lengthTag))
            {
                var offsetEntry = directory[offsetTag] as LongIfdEntry;
                var lengthEntry = directory[lengthTag] as LongIfdEntry;

                if (offsetEntry != null && lengthEntry != null)
                {
                    uint offset = offsetEntry.Value;
                    uint length = lengthEntry.Value;

                    File.Seek(baseOffset + offset, SeekOrigin.Begin);
                    ByteVector data = File.ReadBlock((int)length);

                    directory.Remove(offsetTag);
                    directory.Add(offsetTag, new ThumbnailDataIfdEntry(offsetTag, data));
                }
            }

            // create a StripOffsetIFDEntry if necessary
            ushort stripOffsetsTag = (ushort)IfdEntryTag.StripOffsets;
            ushort stripByteCountsTag = (ushort)IfdEntryTag.StripByteCounts;
            if (directory.ContainsKey(stripOffsetsTag) && directory.ContainsKey(stripByteCountsTag))
            {
                uint[] stripOffsets = null;
                uint[] stripByteCounts = null;

                var stripOffsetsEntry = directory[stripOffsetsTag];
                var stripByteCountsEntry = directory[stripByteCountsTag];

                if (stripOffsetsEntry is LongIfdEntry)
                    stripOffsets = new uint[] { (stripOffsetsEntry as LongIfdEntry).Value };
                else if (stripOffsetsEntry is LongArrayIfdEntry)
                    stripOffsets = (stripOffsetsEntry as LongArrayIfdEntry).Values;

                if (stripOffsets == null)
                    return;

                if (stripByteCountsEntry is LongIfdEntry)
                    stripByteCounts = new uint[] { (stripByteCountsEntry as LongIfdEntry).Value };
                else if (stripByteCountsEntry is LongArrayIfdEntry)
                    stripByteCounts = (stripByteCountsEntry as LongArrayIfdEntry).Values;

                if (stripByteCounts == null)
                    return;

                directory.Remove(stripOffsetsTag);
                directory.Add(stripOffsetsTag, new StripOffsetsIfdEntry(stripOffsetsTag, stripOffsets, stripByteCounts, File));
            }
        }

        private IFdEntry ParseMakernote(ushort tag, ushort type, uint count, long baseOffset, uint offset)
        {
            long makernoteOffset = baseOffset + offset;
            IfdStructure ifdStructure = new IfdStructure();

            // This is the minimum size a makernote should have
            // The shortest header is PENTAX_HEADER (4)
            // + IFD entry count (2)
            // + at least one IFD etry (12)
            // + next IFD pointer (4)
            // = 22 ....
            // we use this number to read a header which is big used
            // to identify the makernote types
            int headerSize = 18;

            long length = 0;
            try
            {
                length = File.Length;
            }
            catch (Exception)
            {
                // Use a safety-value of 4 gigabyte.
                length = 1073741824L * 4;
            }

            if (makernoteOffset > length)
            {
                File.MarkAsCorrupt("offset to makernote is beyond file size");
                return null;
            }

            if (makernoteOffset + headerSize > length)
            {
                File.MarkAsCorrupt("data is to short to contain a maker note ifd");
                return null;
            }

            // read header
            File.Seek(makernoteOffset, SeekOrigin.Begin);
            ByteVector header = File.ReadBlock(headerSize);

            if (header.StartsWith(PanasonicHeader))
            {
                IfdReader reader =
                    new IfdReader(File, IsBigendian, ifdStructure, baseOffset, offset + 12, MaxOffset);

                reader.ReadIfd(baseOffset, offset + 12, MaxOffset);
                return new MakernoteIfdEntry(tag, ifdStructure, MakernoteType.Panasonic, PanasonicHeader, 12, true, null);
            }

            if (header.StartsWith(PentaxHeader))
            {
                IfdReader reader =
                    new IfdReader(File, IsBigendian, ifdStructure, baseOffset, offset + 6, MaxOffset);

                reader.ReadIfd(baseOffset, offset + 6, MaxOffset);
                return new MakernoteIfdEntry(tag, ifdStructure, MakernoteType.Pentax, header.Mid(0, 6), 6, true, null);
            }

            if (header.StartsWith(Olympus1Header))
            {
                IfdReader reader =
                    new IfdReader(File, IsBigendian, ifdStructure, baseOffset, offset + 8, MaxOffset);

                reader.Read();
                return new MakernoteIfdEntry(tag, ifdStructure, MakernoteType.Olympus1, header.Mid(0, 8), 8, true, null);
            }

            if (header.StartsWith(Olympus2Header))
            {
                IfdReader reader =
                    new IfdReader(File, IsBigendian, ifdStructure, makernoteOffset, 12, count);

                reader.Read();
                return new MakernoteIfdEntry(tag, ifdStructure, MakernoteType.Olympus2, header.Mid(0, 12), 12, false, null);
            }

            if (header.StartsWith(SonyHeader))
            {
                IfdReader reader =
                    new IfdReader(File, IsBigendian, ifdStructure, baseOffset, offset + 12, MaxOffset);

                reader.ReadIfd(baseOffset, offset + 12, MaxOffset);
                return new MakernoteIfdEntry(tag, ifdStructure, MakernoteType.Sony, SonyHeader, 12, true, null);
            }

            if (header.StartsWith(NikonHeader))
            {
                ByteVector endianBytes = header.Mid(10, 2);

                if (endianBytes.ToString() == "II" || endianBytes.ToString() == "MM")
                {
                    bool makernoteEndian = endianBytes.ToString().Equals("MM");
                    ushort magic = header.Mid(12, 2).ToUShort(IsBigendian);

                    if (magic == 42)
                    {
                        // TODO: the max_offset value is not correct here. However, some nikon files have offsets to a sub-ifd
                        // (preview image) which are not stored with the other makernote data. Therfore, we keep the max_offset
                        // for now. (It is just an upper bound for some checks. So if it is too big, it doesn't matter)
                        var reader =
                            new Nikon3MakernoteReader(File, makernoteEndian, ifdStructure, makernoteOffset + 10, 8, MaxOffset - offset - 10);

                        reader.Read();
                        return new MakernoteIfdEntry(tag, ifdStructure, MakernoteType.Nikon3, header.Mid(0, 18), 8, false, makernoteEndian);
                    }
                }
            }

            if (header.StartsWith(LeicaHeader))
            {
                IfdReader reader = new IfdReader(File, IsBigendian, ifdStructure, makernoteOffset, 8, count);

                reader.Read();
                return new MakernoteIfdEntry(tag, ifdStructure, MakernoteType.Leica, header.Mid(0, 8), 10, false, null);
            }

            try
            {
                IfdReader reader =
                    new IfdReader(File, IsBigendian, ifdStructure, baseOffset, offset, MaxOffset);

                reader.Read();
                return new MakernoteIfdEntry(tag, ifdStructure, MakernoteType.Canon);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        ///    Reads an ASCII string from the current file.
        /// </summary>
        /// <returns>
        ///    A <see cref="string" /> read from the current instance.
        /// </returns>
        /// <remarks>
        ///    The exif standard allows to store multiple string separated
        ///    by '\0' in one ASCII-field. On the other hand some programs
        ///    (e.g. CanonZoomBrowser) fill some ASCII fields by trailing
        ///    '\0's.
        ///    We follow the Adobe practice as described in XMP Specification
        ///    Part 3 (Storeage in Files), and process the ASCII string only
        ///    to the first '\0'.
        /// </remarks>
        private string ReadAsciiString(int count)
        {
            string str = File.ReadBlock(count).ToString();
            int term = str.IndexOf('\0');

            if (term > -1)
                str = str.Substring(0, term);

            return str;
        }

        /// <summary>
        ///    Reads an IFD from file at position <paramref name="offset"/> relative
        ///    to <paramref name="baseOffset"/>.
        /// </summary>
        /// <param name="baseOffset">
        ///    A <see cref="System.Int64"/> with the base offset which every offset
        ///    in IFD is relative to.
        /// </param>
        /// <param name="offset">
        ///    A <see cref="System.UInt32"/> with the offset of the IFD relative to
        ///    <paramref name="baseOffset"/>
        /// </param>
        /// <param name="maxOffset">
        ///    A <see cref="System.UInt32"/> with the maximal offset to consider for
        ///    the IFD.
        /// </param>
        /// <returns>
        ///    A <see cref="System.UInt32"/> with the offset of the next IFD, the
        ///    offset is also relative to <paramref name="baseOffset"/>
        /// </returns>
        private uint ReadIfd(long baseOffset, uint offset, uint maxOffset)
        {
            long length = 0;
            try
            {
                length = File.Length;
            }
            catch (Exception)
            {
                // Use a safety-value of 4 gigabyte.
                length = 1073741824L * 4;
            }

            if (baseOffset + offset > length)
            {
                File.MarkAsCorrupt("Invalid IFD offset");
                return 0;
            }

            var directory = new IfdDirectory();

            File.Seek(baseOffset + offset, SeekOrigin.Begin);
            ushort entryCount = ReadUShort();

            if (File.Tell + 12 * entryCount > baseOffset + maxOffset)
            {
                File.MarkAsCorrupt("Size of entries exceeds possible data size");
                return 0;
            }

            ByteVector entryDatas = File.ReadBlock(12 * entryCount);
            uint nextOffset = ReadUInt();

            for (int i = 0; i < entryCount; i++)
            {
                ByteVector entryData = entryDatas.Mid(i * 12, 12);

                ushort entryTag = entryData.Mid(0, 2).ToUShort(IsBigendian);
                ushort type = entryData.Mid(2, 2).ToUShort(IsBigendian);
                uint valueCount = entryData.Mid(4, 4).ToUInt(IsBigendian);
                ByteVector offsetData = entryData.Mid(8, 4);

                IFdEntry entry = CreateIfdEntry(entryTag, type, valueCount, baseOffset, offsetData, maxOffset);

                if (entry == null)
                    continue;

                if (directory.ContainsKey(entry.Tag))
                    directory.Remove(entry.Tag);

                directory.Add(entry.Tag, entry);
            }

            FixupDirectory(baseOffset, directory);

            Structure.directories.Add(directory);
            return nextOffset;
        }

        /// <summary>
        ///    Reads a 4-byte int from the current file.
        /// </summary>
        /// <returns>
        ///    A <see cref="uint" /> value containing the int read
        ///    from the current instance.
        /// </returns>
        private int ReadInt()
        {
            return File.ReadBlock(4).ToInt(IsBigendian);
        }

        /// <summary>
        ///    Reads an array of 4-byte int from the current file.
        /// </summary>
        /// <returns>
        ///    An array of <see cref="int" /> values containing the
        ///    shorts read from the current instance.
        /// </returns>
        private int[] ReadIntArray(uint count)
        {
            int[] data = new int[count];
            for (int i = 0; i < count; i++)
                data[i] = ReadInt();
            return data;
        }

        /// <summary>
        ///    Reads a <see cref="Rational"/> by two following unsigned
        ///    int from the current file.
        /// </summary>
        /// <returns>
        ///    A <see cref="Rational"/> value created by the read values.
        /// </returns>
        private Rational ReadRational()
        {
            uint numerator = ReadUInt();
            uint denominator = ReadUInt();

            // correct illegal value
            if (denominator == 0)
            {
                numerator = 0;
                denominator = 1;
            }

            return new Rational(numerator, denominator);
        }

        /// <summary>
        ///    Reads a 2-byte signed short from the current file.
        /// </summary>
        /// <returns>
        ///    A <see cref="short" /> value containing the short read
        ///    from the current instance.
        /// </returns>
        private short ReadShort()
        {
            return File.ReadBlock(2).ToShort(IsBigendian);
        }

        /// <summary>
        ///    Reads an array of 2-byte signed shorts from the current file.
        /// </summary>
        /// <returns>
        ///    An array of <see cref="short" /> values containing the
        ///    shorts read from the current instance.
        /// </returns>
        private short[] ReadShortArray(uint count)
        {
            short[] data = new short[count];
            for (int i = 0; i < count; i++)
                data[i] = ReadShort();
            return data;
        }

        /// <summary>
        ///    Reads a <see cref="SRational"/> by two following unsigned
        ///    int from the current file.
        /// </summary>
        /// <returns>
        ///    A <see cref="SRational"/> value created by the read values.
        /// </returns>
        private SRational ReadSRational()
        {
            int numerator = ReadInt();
            int denominator = ReadInt();

            // correct illegal value
            if (denominator == 0)
            {
                numerator = 0;
                denominator = 1;
            }

            return new SRational(numerator, denominator);
        }

        /// <summary>
        ///    Reads a 4-byte unsigned int from the current file.
        /// </summary>
        /// <returns>
        ///    A <see cref="uint" /> value containing the int read
        ///    from the current instance.
        /// </returns>
        private uint ReadUInt()
        {
            return File.ReadBlock(4).ToUInt(IsBigendian);
        }

        /// <summary>
        ///    Reads an array of 4-byte unsigned int from the current file.
        /// </summary>
        /// <returns>
        ///    An array of <see cref="uint" /> values containing the
        ///    shorts read from the current instance.
        /// </returns>
        private uint[] ReadUIntArray(uint count)
        {
            uint[] data = new uint[count];
            for (int i = 0; i < count; i++)
                data[i] = ReadUInt();
            return data;
        }

        /// <summary>
        ///    Reads a 2-byte unsigned short from the current file.
        /// </summary>
        /// <returns>
        ///    A <see cref="ushort" /> value containing the short read
        ///    from the current instance.
        /// </returns>
        private ushort ReadUShort()
        {
            return File.ReadBlock(2).ToUShort(IsBigendian);
        }

        /// <summary>
        ///    Reads an array of 2-byte shorts from the current file.
        /// </summary>
        /// <returns>
        ///    An array of <see cref="ushort" /> values containing the
        ///    shorts read from the current instance.
        /// </returns>
        private ushort[] ReadUShortArray(uint count)
        {
            ushort[] data = new ushort[count];
            for (int i = 0; i < count; i++)
                data[i] = ReadUShort();
            return data;
        }

        /// <summary>
        ///    Add to the reference count for the IFD loop detection.
        /// </summary>
        private void StartIfdLoopDetect()
        {
            if (!_ifdOffsets.ContainsKey(File))
            {
                _ifdOffsets[File] = new List<long>();
                _ifdLoopdetectRefs[File] = 1;
            }
            else
            {
                _ifdLoopdetectRefs[File]++;
            }
        }

        /// <summary>
        ///    End the IFD loop detection, cleanup if we're the last.
        /// </summary>
        private void StopIfdLoopDetect()
        {
            _ifdLoopdetectRefs[File]--;
            if (_ifdLoopdetectRefs[File] == 0)
            {
                _ifdOffsets.Remove(File);
                _ifdLoopdetectRefs.Remove(File);
            }
        }
    }
}