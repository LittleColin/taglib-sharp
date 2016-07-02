//
// File.cs:
//
// Author:
//   Ruben Vermeersch (ruben@savanne.be)
//
// Copyright (C) 2009 Ruben Vermeersch
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
using TagLib.IFD.Tags;
using TagLib.Image;

namespace TagLib.Tiff
{
    /// <summary>
    ///    This class extends <see cref="TagLib.Tiff.BaseTiffFile" /> to provide tagging
    ///    and properties support for Tiff files.
    /// </summary>
    [SupportedMimeType("taglib/tiff", "tiff")]
    [SupportedMimeType("taglib/tif", "tif")]
    [SupportedMimeType("image/tiff")]
    public class File : BaseTiffFile
    {
        /// <summary>
        ///    Contains the media properties.
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
        public File(string path) : base(path)
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
            ImageTag = new CombinedImageTag(TagTypes.TiffIfd | TagTypes.Xmp);

            Mode = AccessMode.Read;
            try
            {
                Read(propertiesStyle);
                TagTypesOnDisk = TagTypes;
            }
            finally
            {
                Mode = AccessMode.Closed;
            }
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
        protected File(IFileAbstraction abstraction) : base(abstraction)
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
        ///    Saves the changes made in the current instance to the
        ///    file it represents.
        /// </summary>
        public override void Save()
        {
            Mode = AccessMode.Write;
            try
            {
                WriteFile();

                TagTypesOnDisk = TagTypes;
            }
            finally
            {
                Mode = AccessMode.Closed;
            }
        }

        /// <summary>
        ///    Render the whole file and write it back.
        /// </summary>
        private void WriteFile()
        {
            // Check, if IFD0 is contained
            IfdTag exif = ImageTag.Exif;
            if (exif == null)
                throw new Exception("Tiff file without tags");

            // first IFD starts at 8
            uint firstIfdOffset = 8;
            ByteVector data = RenderHeader(firstIfdOffset);

            var renderer = new IfdRenderer(IsBigEndian, exif.Structure, firstIfdOffset);

            data.Add(renderer.Render());

            Insert(data, 0, Length);
        }

        /// <summary>
        ///    Reads the file with a specified read style.
        /// </summary>
        /// <param name="propertiesStyle">
        ///    A <see cref="ReadStyle" /> value specifying at what level
        ///    of accuracy to read the media properties, or <see
        ///    cref="ReadStyle.None" /> to ignore the properties.
        /// </param>
        protected void Read(ReadStyle propertiesStyle)
        {
            Mode = AccessMode.Read;
            try
            {
                uint firstIfdOffset = ReadHeader();
                ReadIFD(firstIfdOffset);

                if (propertiesStyle == ReadStyle.None)
                    return;

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
        protected virtual Properties ExtractProperties()
        {
            int width = 0, height = 0;

            IfdTag tag = GetTag(TagTypes.TiffIfd) as IfdTag;
            IfdStructure structure = tag.Structure;

            width = (int)(structure.GetLongValue(0, (ushort)IfdEntryTag.ImageWidth) ?? 0);
            height = (int)(structure.GetLongValue(0, (ushort)IfdEntryTag.ImageLength) ?? 0);

            if (width > 0 && height > 0)
            {
                return new Properties(TimeSpan.Zero, CreateCodec(width, height));
            }

            return null;
        }

        /// <summary>
        ///    Create a codec that describes the photo properties.
        /// </summary>
        /// <returns>
        ///    A <see cref="Codec" /> object.
        /// </returns>
        protected virtual Codec CreateCodec(int width, int height)
        {
            return new Codec(width, height);
        }
    }
}