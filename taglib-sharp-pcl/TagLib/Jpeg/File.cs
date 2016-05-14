//
// File.cs: Provides tagging for Jpeg files
//
// Author:
//   Ruben Vermeersch (ruben@savanne.be)
//   Mike Gemuende (mike@gemuende.de)
//   Stephane Delcroix (stephane@delcroix.org)
//
// Copyright (C) 2009 Ruben Vermeersch
// Copyright (C) 2009 Mike Gemuende
// Copyright (c) 2009 Stephane Delcroix
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
using System.IO;
using TagLib.IFD;
using TagLib.Image;

namespace TagLib.Jpeg
{
    /// <summary>
    ///    This class extends <see cref="TagLib.Image.ImageBlockFile" /> to provide tagging
    ///    and properties support for Jpeg files.
    /// </summary>
    [SupportedMimeType("taglib/jpg", "jpg")]
    [SupportedMimeType("taglib/jpeg", "jpeg")]
    [SupportedMimeType("taglib/jpe", "jpe")]
    [SupportedMimeType("taglib/jif", "jif")]
    [SupportedMimeType("taglib/jfif", "jfif")]
    [SupportedMimeType("taglib/jfi", "jfi")]
    [SupportedMimeType("image/jpeg")]
    public class File : ImageBlockFile
    {
        /// <summary>
        ///    Standard (empty) JFIF header to add, if no one is contained
        /// </summary>
        private static readonly byte[] BasicJfifHeader = new byte[] {
			// segment maker
			0xFF, (byte) Marker.App0,

			// segment size
			0x00, 0x10,

			// segment data
			0x4A, 0x46, 0x49, 0x46, 0x00, 0x01, 0x01,
            0x01, 0x00, 0x48, 0x00, 0x48, 0x00, 0x00
        };

        /// <summary>
        ///    Contains the media properties.
        /// </summary>
        private Properties _properties;

        /// <summary>
        ///    For now, we do not allow to change the jfif header. As long as this is
        ///    the case, the header is kept as it is.
        /// </summary>
        private ByteVector _jfifHeader = null;

        /// <summary>
        ///    The image width, as parsed from the Frame
        /// </summary>
        private ushort _width;

        /// <summary>
        ///    The image height, as parsed from the Frame
        /// </summary>
        private ushort _height;

        /// <summary>
        ///    Quality of the image, stored as we parse the file
        /// </summary>
        private int _quality;

        /// <summary>
        ///    Constructs and initializes a new instance of <see
        ///    cref="File" /> for a specified path in the local file
        ///    system and specified read style.
        /// </summary>
        /// <param name="path">
        ///    A <see cref="string" /> object containing the path of the
        ///    file to use in the new instance.
        /// </param>
        /// <param name="propertiesStyle">
        ///    A <see cref="ReadStyle" /> value specifying at what level
        ///    of accuracy to read the media properties, or <see
        ///    cref="ReadStyle.None" /> to ignore the properties.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///    <paramref name="path" /> is <see langword="null" />.
        /// </exception>
        public File(string path, ReadStyle propertiesStyle)
            : this(new LocalFileAbstraction(path),
                propertiesStyle)
        {
        }

        /// <summary>
        ///    Constructs and initializes a new instance of <see
        ///    cref="File" /> for a specified path in the local file
        ///    system.
        /// </summary>
        /// <param name="path">
        ///    A <see cref="string" /> object containing the path of the
        ///    file to use in the new instance.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///    <paramref name="path" /> is <see langword="null" />.
        /// </exception>
        public File(string path) : this(path, ReadStyle.Average)
        {
        }

        /// <summary>
        ///    Constructs and initializes a new instance of <see
        ///    cref="File" /> for a specified file abstraction and
        ///    specified read style.
        /// </summary>
        /// <param name="abstraction">
        ///    A <see cref="IFileAbstraction" /> object to use when
        ///    reading from and writing to the file.
        /// </param>
        /// <param name="propertiesStyle">
        ///    A <see cref="ReadStyle" /> value specifying at what level
        ///    of accuracy to read the media properties, or <see
        ///    cref="ReadStyle.None" /> to ignore the properties.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///    <paramref name="abstraction" /> is <see langword="null"
        ///    />.
        /// </exception>
        public File(IFileAbstraction abstraction,
                     ReadStyle propertiesStyle) : base(abstraction)
        {
            Read(propertiesStyle);
        }

        /// <summary>
        ///    Constructs and initializes a new instance of <see
        ///    cref="File" /> for a specified file abstraction.
        /// </summary>
        /// <param name="abstraction">
        ///    A <see cref="IFileAbstraction" /> object to use when
        ///    reading from and writing to the file.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///    <paramref name="abstraction" /> is <see langword="null"
        ///    />.
        /// </exception>
        protected File(IFileAbstraction abstraction)
            : this(abstraction, ReadStyle.Average)
        {
        }

        /// <summary>
        ///    Gets the media properties of the file represented by the
        ///    current instance.
        /// </summary>
        /// <value>
        ///    A <see cref="TagLib.Properties" /> object containing the
        ///    media properties of the file represented by the current
        ///    instance.
        /// </value>
        public override Properties Properties => _properties;

        /// <summary>
        ///  Gets a tag of a specified type from the current instance, optionally creating a
        /// new tag if possible.
        /// </summary>
        public override Tag GetTag(TagTypes type, bool create)
        {
            if (type == TagTypes.Xmp)
            {
                foreach (Tag tag in ImageTag.AllTags)
                {
                    if ((tag.TagTypes & type) == type || (tag.TagTypes & TagTypes.Iptciim) != 0)
                        return tag;
                }
            }
            if (type == TagTypes.Iptciim && create)
            {
                // FIXME: don't know how to create IPTCIIM tags
                return base.GetTag(type, false);
            }

            return base.GetTag(type, create);
        }

        /// <summary>
        ///    Saves the changes made in the current instance to the
        ///    file it represents.
        /// </summary>
        public override void Save()
        {
            if (!Writeable || PossiblyCorrupt)
                throw new InvalidOperationException("File not writeable. Corrupt metadata?");

            Mode = AccessMode.Write;
            try
            {
                WriteMetadata();

                TagTypesOnDisk = TagTypes;
            }
            finally
            {
                Mode = AccessMode.Closed;
            }
        }

        /// <summary>
        ///    Reads the information from file with a specified read style.
        /// </summary>
        /// <param name="propertiesStyle">
        ///    A <see cref="ReadStyle" /> value specifying at what level
        ///    of accuracy to read the media properties, or <see
        ///    cref="ReadStyle.None" /> to ignore the properties.
        /// </param>
        private void Read(ReadStyle propertiesStyle)
        {
            Mode = AccessMode.Read;
            try
            {
                ImageTag = new CombinedImageTag(TagTypes.Xmp | TagTypes.TiffIfd | TagTypes.JpegComment | TagTypes.Iptciim);

                ValidateHeader();
                ReadMetadata();

                TagTypesOnDisk = TagTypes;

                if (propertiesStyle != ReadStyle.None)
                    _properties = ExtractProperties();
            }
            finally
            {
                Mode = AccessMode.Closed;
            }
        }

        /// <summary>
        ///    Attempts to extract the media properties of the main
        ///    photo.
        /// </summary>
        /// <returns>
        ///    A <see cref="Properties" /> object with a best effort guess
        ///    at the right values. When no guess at all can be made,
        ///    <see langword="null" /> is returned.
        /// </returns>
        private Properties ExtractProperties()
        {
            if (_width > 0 && _height > 0)
                return new Properties(TimeSpan.Zero, new Codec(_width, _height, _quality));

            return null;
        }

        /// <summary>
        ///    Validates if the opened file is actually a JPEG.
        /// </summary>
        private void ValidateHeader()
        {
            ByteVector segment = ReadBlock(2);
            if (segment.ToUShort() != 0xFFD8)
                throw new CorruptFileException("Expected SOI marker at the start of the file.");
        }

        /// <summary>
        ///    Reads a segment marker for a segment starting at current position.
        ///    The second byte of the marker is returned, since the first is equal
        ///    to 0xFF in every case.
        /// </summary>
        /// <returns>
        ///    A <see cref="TagLib.Jpeg.Marker"/> with the second byte of the segment marker.
        /// </returns>
        private Marker ReadSegmentMarker()
        {
            ByteVector segmentHeader = ReadBlock(2);

            if (segmentHeader.Count != 2)
                throw new CorruptFileException("Could not read enough bytes for segment maker");

            if (segmentHeader[0] != 0xFF)
                throw new CorruptFileException("Start of Segment expected at " + (Tell - 2));

            return (Marker)segmentHeader[1];
        }

        /// <summary>
        ///    Reads the size of a segment at the current position.
        /// </summary>
        /// <returns>
        ///    A <see cref="System.UInt16"/> with the size of the current segment.
        /// </returns>
        private ushort ReadSegmentSize()
        {
            long position = Tell;

            ByteVector segmentSizeBytes = ReadBlock(2);

            if (segmentSizeBytes.Count != 2)
                throw new CorruptFileException("Could not read enough bytes to determine segment size");

            ushort segmentSize = segmentSizeBytes.ToUShort();

            // the size itself must be contained in the segment size
            // so the smallest (theoretically) possible number of bytes if 2
            if (segmentSize < 2)
                throw new CorruptFileException(string.Format("Invalid segment size ({0} bytes)", segmentSize));

            long length = 0;
            try
            {
                length = Length;
            }
            catch (Exception)
            {
                // Probably not supported by stream.
            }

            if (length > 0 && position + segmentSize >= length)
                throw new CorruptFileException("Segment size exceeds file size");

            return segmentSize;
        }

        /// <summary>
        ///    Extracts the metadata from the current file by reading every segment in file.
        ///    Method should be called with read position at first segment marker.
        /// </summary>
        private void ReadMetadata()
        {
            // loop while marker is not EOI and not the data segment
            while (true)
            {
                Marker marker = ReadSegmentMarker();

                // we stop parsing when the end of file (EOI) or the begin of the
                // data segment is reached (SOS)
                // the second case is a trade-off between tolerant and fast parsing
                if (marker == Marker.Eoi || marker == Marker.Sos)
                    break;

                long position = Tell;
                ushort segmentSize = ReadSegmentSize();

                // segment size contains 2 bytes of the size itself, so the
                // pure data size is this (and the cast is save)
                ushort dataSize = (ushort)(segmentSize - 2);

                switch (marker)
                {
                    case Marker.App0:   // possibly JFIF header
                        ReadJfifHeader(dataSize);
                        break;

                    case Marker.Com:    // Comment segment found
                        ReadComSegment(dataSize);
                        break;

                    case Marker.Sof0:
                    case Marker.Sof1:
                    case Marker.Sof2:
                    case Marker.Sof3:
                    case Marker.Sof9:
                    case Marker.Sof10:
                    case Marker.Sof11:
                        ReadSofSegment(dataSize, marker);
                        break;

                    case Marker.Dqt:    // Quantization table(s), use it to guess quality
                        ReadDqtSegment(dataSize);
                        break;
                }

                // set position to next segment and start with next segment marker
                Seek(position + segmentSize, SeekOrigin.Begin);
            }
        }

        /// <summary>
        ///    Reads a JFIF header at current position
        /// </summary>
        private void ReadJfifHeader(ushort length)
        {
            // JFIF header should be contained as first segment
            // SOI marker + APP0 Marker + segment size = 6 bytes
            if (Tell != 6)
                return;

            if (ReadBlock(5).ToString().Equals("JFIF\0"))
            {
                // store the JFIF header as it is
                Seek(2, SeekOrigin.Begin);
                _jfifHeader = ReadBlock(length + 2 + 2);

                AddMetadataBlock(2, length + 2 + 2);
            }
        }

        /// <summary>
        ///    Writes the metadata back to file. All metadata is stored in the first segments
        ///    of the file.
        /// </summary>
        private void WriteMetadata()
        {
            // first render all metadata segments to a ByteVector before the
            // file is touched ...
            ByteVector data = new ByteVector();

            // existing jfif header is retained, otherwise a standard one
            // is created
            if (_jfifHeader != null)
                data.Add(_jfifHeader);
            else
                data.Add(BasicJfifHeader);

            data.Add(RenderExifSegment());
            data.Add(RenderComSegment());

            SaveMetadata(data, 2);
        }

        /// <summary>
        ///    Creates a <see cref="ByteVector"/> for the Exif segment of this file
        /// </summary>
        /// <returns>
        ///    A <see cref="ByteVector"/> with the whole Exif segment, if exif tags
        ///    exists, otherwise null.
        /// </returns>
        private ByteVector RenderExifSegment()
        {
            // Check, if IFD0 is contained
            IfdTag exif = ImageTag.Exif;
            if (exif == null)
                return null;

            // first IFD starts at 8
            uint firstIfdOffset = 8;

            // Render IFD0
            // FIXME: store endianess and use it here
            var renderer = new IfdRenderer(true, exif.Structure, firstIfdOffset);
            ByteVector exifData = renderer.Render();

            uint segmentSize = (uint)(firstIfdOffset + exifData.Count + 2 + 6);

            // do not render data segments, which cannot fit into the possible segment size
            if (segmentSize > ushort.MaxValue)
                throw new Exception("Exif Segment is too big to render");

            // Create whole segment
            ByteVector data = new ByteVector(new byte[] { 0xFF, (byte)Marker.App1 });
            data.Add(ByteVector.FromUShort((ushort)segmentSize));
            data.Add("Exif\0\0");
            data.Add(ByteVector.FromString("MM", StringType.Latin1));
            data.Add(ByteVector.FromUShort(42));
            data.Add(ByteVector.FromUInt(firstIfdOffset));

            // Add ifd data itself
            data.Add(exifData);

            return data;
        }

        /// <summary>
        ///    Reads a COM segment to find the JPEG comment.
        /// </summary>
        /// <param name="length">
        ///    The length of the segment that will be read.
        /// </param>
        private void ReadComSegment(int length)
        {
            if ((ImageTag.TagTypes & TagTypes.JpegComment) != 0x00)
                return;

            long position = Tell;

            JpegCommentTag comTag;

            if (length == 0)
            {
                comTag = new JpegCommentTag();
            }
            else
            {
                ByteVector data = ReadBlock(length);

                int terminator = data.Find("\0", 0);

                if (terminator < 0)
                    comTag = new JpegCommentTag(data.ToString());
                else
                    comTag = new JpegCommentTag(data.Mid(0, terminator).ToString());
            }

            ImageTag.AddTag(comTag);
            AddMetadataBlock(position - 4, length + 4);
        }

        /// <summary>
        ///    Creates a <see cref="ByteVector"/> for the comment segment of this file
        /// </summary>
        /// <returns>
        ///    A <see cref="ByteVector"/> with the whole comment segment, if a comment tag
        ///    exists, otherwise null.
        /// </returns>
        private ByteVector RenderComSegment()
        {
            // check, if Comment is contained
            JpegCommentTag comTag = GetTag(TagTypes.JpegComment) as JpegCommentTag;
            if (comTag == null)
                return null;

            // create comment data
            ByteVector comData =
                ByteVector.FromString(comTag.Value + "\0", StringType.Latin1);

            uint segmentSize = (uint)(2 + comData.Count);

            // do not render data segments, which cannot fit into the possible segment size
            if (segmentSize > ushort.MaxValue)
                throw new Exception("Comment Segment is too big to render");

            // create segment
            ByteVector data = new ByteVector(new byte[] { 0xFF, (byte)Marker.Com });
            data.Add(ByteVector.FromUShort((ushort)segmentSize));

            data.Add(comData);

            return data;
        }

        /// <summary>
        ///    Reads and parse a SOF segment
        /// </summary>
        /// <param name="length">
        ///    The length of the segment that will be read.
        /// </param>
        /// <param name="marker">
        ///    The SOFx marker.
        /// </param>
        private void ReadSofSegment(int length, Marker marker)
        {
#pragma warning disable 219 // Assigned, never read
            byte p = ReadBlock(1)[0];   //precision
#pragma warning restore 219

            //FIXME: according to specs, height could be 0 here, and should be retrieved from the DNL marker
            _height = ReadBlock(2).ToUShort();
            _width = ReadBlock(2).ToUShort();
        }

        /// <summary>
        ///    Reads the DQT Segment, and Guesstimate the image quality from it
        /// </summary>
        /// <param name="length">
        ///    The length of the segment that will be read
        /// </param>
        private void ReadDqtSegment(int length)
        {
            // See CCITT Rec. T.81 (1992 E), B.2.4.1 (p39) for DQT syntax
            while (length > 0)
            {
                byte pqtq = ReadBlock(1)[0]; length--;
                byte pq = (byte)(pqtq >> 4);    //0 indicates 8-bit Qk, 1 indicates 16-bit Qk
                byte tq = (byte)(pqtq & 0x0f);  //table index;
                int[] table = null;
                switch (tq)
                {
                    case 0:
                        table = Table.StandardLuminanceQuantization;
                        break;

                    case 1:
                        table = Table.StandardChrominanceQuantization;
                        break;
                }

                bool allones = true; //check for all-ones tables (q=100)
                double cumsf = 0.0;
                //double cumsf2 = 0.0;
                for (int row = 0; row < 8; row++)
                {
                    for (int col = 0; col < 8; col++)
                    {
                        ushort val = ReadBlock(pq == 1 ? 2 : 1).ToUShort(); length -= (pq + 1);
                        if (table != null)
                        {
                            double x = 100.0 * val / table[row * 8 + col]; //Scaling factor in percent
                            cumsf += x;
                            //cumsf2 += x*x;
                            allones = allones && (val == 1);
                        }
                    }
                }

                if (table != null)
                {
                    double localQ;
                    cumsf /= 64.0;      // mean scale factor
                                        //cumfs2 /= 64.0;
                                        //double variance = cumsf2 - (cumsf * cumsf);

                    if (allones)
                        localQ = 100.0;
                    else if (cumsf <= 100.0)
                        localQ = (200.0 - cumsf) / 2.0;
                    else
                        localQ = 5000.0 / cumsf;
                    _quality = Math.Max(_quality, (int)localQ);
                }
            }
        }
    }
}