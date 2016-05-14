//
// File.cs: Provides tagging for Panasonic Rw2 files
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

using System;
using TagLib.IFD;
using TagLib.Image;

namespace TagLib.Tiff.Rw2
{
    /// <summary>
    ///    This class extends <see cref="TagLib.Tiff.BaseTiffFile" /> to provide tagging
    ///    for RW2 image files.
    /// </summary>
    [SupportedMimeType("taglib/rw2", "rw2")]
    [SupportedMimeType("image/rw2")]
    [SupportedMimeType("taglib/raw", "raw")]
    [SupportedMimeType("image/raw")]
    [SupportedMimeType("image/x-raw")]
    [SupportedMimeType("image/x-panasonic-raw")]
    public class File : BaseTiffFile
    {
        /// <summary>
        ///    The Properties of the image
        /// </summary>
        private Properties _properties;

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
            Magic = 85; // Panasonic uses 0x55
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
        ///     The JPEG file that's embedded in the RAW file.
        /// </summary>
        public Jpeg.File JpgFromRaw
        {
            get;
            internal set;
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
        ///    Indicates if tags can be written back to the current file or not
        /// </summary>
        /// <value>
        ///    A <see cref="bool" /> which is true if tags can be written to the
        ///    current file, otherwise false.
        /// </value>
        public override bool Writeable => false;

        /// <summary>
        ///    Gets a tag of a specified type from the current instance,
        ///    optionally creating a new tag if possible.
        /// </summary>
        /// <param name="type">
        ///    A <see cref="TagLib.TagTypes" /> value indicating the
        ///    type of tag to read.
        /// </param>
        /// <param name="create">
        ///    A <see cref="bool" /> value specifying whether or not to
        ///    try and create the tag if one is not found.
        /// </param>
        /// <returns>
        ///    A <see cref="Tag" /> object containing the tag that was
        ///    found in or added to the current instance. If no
        ///    matching tag was found and none was created, <see
        ///    langword="null" /> is returned.
        /// </returns>
        public override Tag GetTag(TagTypes type,
                                           bool create)
        {
            Tag tag = base.GetTag(type, false);
            if (tag != null)
            {
                return tag;
            }

            if (!create || (type & ImageTag.AllowedTypes) == 0)
                return null;

            if (type != TagTypes.TiffIfd)
                return base.GetTag(type, create);

            ImageTag newTag = new IfdTag(this);
            ImageTag.AddTag(newTag);
            return newTag;
        }

        /// <summary>
        ///    Saves the changes made in the current instance to the
        ///    file it represents.
        /// </summary>
        public override void Save()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        ///    Creates an IFD reader to parse the file.
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
        protected override IFD.IfdReader CreateIfdReader(BaseTiffFile file, bool isBigendian, IfdStructure structure, long baseOffset, uint ifdOffset, uint maxOffset)
        {
            return new IfdReader(file, isBigendian, structure, baseOffset, ifdOffset, maxOffset);
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
            int width = 0, height = 0;

            IfdTag tag = GetTag(TagTypes.TiffIfd) as IfdTag;
            IfdStructure structure = tag.Structure;

            width = (int)(structure.GetLongValue(0, 0x07) ?? 0);
            height = (int)(structure.GetLongValue(0, 0x06) ?? 0);

            var vendor = ImageTag.Make;
            if (vendor == "LEICA")
                vendor = "Leica";
            var desc = string.Format("{0} RAW File", vendor);

            if (width > 0 && height > 0)
            {
                return new Properties(TimeSpan.Zero, new Codec(width, height, desc));
            }

            return null;
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
                ImageTag = new CombinedImageTag(TagTypes.TiffIfd);

                ReadFile();

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
        ///   Reads and validates the RW2 header started at the current position.
        /// </summary>
        /// <returns>
        ///    A <see cref="System.UInt32"/> with the offset to the IFD with the RAW data.
        /// </returns>
        private uint ReadAdditionalRw2Header()
        {
            // RW2 Header
            //
            // Seems to be 16 bytes, no idea on the meaning of these.

            ByteVector header = ReadBlock(16);

            if (header.Count != 16)
                throw new CorruptFileException("Unexpected end of RW2 header");

            return (uint)Tell;
        }

        /// <summary>
        ///    Parses the RW2 file
        /// </summary>
        private void ReadFile()
        {
            // A RW2 file starts with a Tiff header followed by a RW2 header
            uint firstIfdOffset = ReadHeader();
            uint rawIfdOffset = ReadAdditionalRw2Header();

            ReadIFD(firstIfdOffset, 3);
            ReadIFD(rawIfdOffset, 1);
        }
    }
}