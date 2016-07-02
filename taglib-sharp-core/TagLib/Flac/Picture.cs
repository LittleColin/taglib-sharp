//
// Picture.cs:
//
// Author:
//   Brian Nickel (brian.nickel@gmail.com)
//
// Copyright (C) 2007 Brian Nickel
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

namespace TagLib.Flac
{
    /// <summary>
    ///    This class implements <see cref="IPicture" /> to provide support
    ///    for reading and writing Flac picture metadata.
    /// </summary>
    public class Picture : IPicture
    {
        /// <summary>
        ///    Contains the picture type.
        /// </summary>
        private PictureType _type;

        /// <summary>
        ///    Contains the mime-type.
        /// </summary>
        private string _mimeType;

        /// <summary>
        ///    Contains the description.
        /// </summary>
        private string _description;

        /// <summary>
        ///    Contains the width.
        /// </summary>
        private int _width = 0;

        /// <summary>
        ///    Contains the height.
        /// </summary>
        private int _height = 0;

        /// <summary>
        ///    Contains the color depth.
        /// </summary>
        private int _colorDepth = 0;

        /// <summary>
        ///    Contains the number of indexed colors.
        /// </summary>
        private int _indexedColors = 0;

        /// <summary>
        ///    Contains the picture data.
        /// </summary>
        private ByteVector _pictureData;

        /// <summary>
        ///    Constructs and initializes a new instance of <see
        ///    cref="Picture" /> by reading the contents of a raw Flac
        ///    image structure.
        /// </summary>
        /// <param name="data">
        ///    A <see cref="ByteVector" /> object containing the raw
        ///    Flac image.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///    <paramref name="data" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="CorruptFileException">
        ///    <paramref name="data" /> contains less than 32 bytes.
        /// </exception>
        public Picture(ByteVector data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            if (data.Count < 32)
                throw new CorruptFileException(
                    "Data must be at least 32 bytes long");

            int pos = 0;
            _type = (PictureType)data.Mid(pos, 4).ToUInt();
            pos += 4;

            int mimetypeLength = (int)data.Mid(pos, 4).ToUInt();
            pos += 4;

            _mimeType = data.ToString(StringType.Latin1, pos,
                mimetypeLength);
            pos += mimetypeLength;

            int descriptionLength = (int)data.Mid(pos, 4)
                .ToUInt();
            pos += 4;

            _description = data.ToString(StringType.Utf8, pos,
                descriptionLength);
            pos += descriptionLength;

            _width = (int)data.Mid(pos, 4).ToUInt();
            pos += 4;

            _height = (int)data.Mid(pos, 4).ToUInt();
            pos += 4;

            _colorDepth = (int)data.Mid(pos, 4).ToUInt();
            pos += 4;

            _indexedColors = (int)data.Mid(pos, 4).ToUInt();
            pos += 4;

            int dataLength = (int)data.Mid(pos, 4).ToUInt();
            pos += 4;

            _pictureData = data.Mid(pos, dataLength);
        }

        /// <summary>
        ///    Constructs and initializes a new instance of <see
        ///    cref="Picture" /> by copying the properties of a <see
        ///    cref="IPicture" /> object.
        /// </summary>
        /// <param name="picture">
        ///    A <see cref="IPicture" /> object to use for the new
        ///    instance.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///    <paramref name="picture" /> is <see langword="null" />.
        /// </exception>
        public Picture(IPicture picture)
        {
            if (picture == null)
                throw new ArgumentNullException(nameof(picture));

            _type = picture.Type;
            _mimeType = picture.MimeType;
            _description = picture.Description;
            _pictureData = picture.Data;

            Picture flacPicture =
                picture as Picture;

            if (flacPicture == null)
                return;

            _width = flacPicture.Width;
            _height = flacPicture.Height;
            _colorDepth = flacPicture.ColorDepth;
            _indexedColors = flacPicture.IndexedColors;
        }

        /// <summary>
        ///    Renders the current instance as a raw Flac picture.
        /// </summary>
        /// <returns>
        ///    A <see cref="ByteVector" /> object containing the
        ///    rendered version of the current instance.
        /// </returns>
        public ByteVector Render()
        {
            ByteVector data = new ByteVector();

            data.Add(ByteVector.FromUInt((uint)Type));

            ByteVector mimeData = ByteVector.FromString(MimeType,
                StringType.Latin1);
            data.Add(ByteVector.FromUInt((uint)mimeData.Count));
            data.Add(mimeData);

            ByteVector decriptionData = ByteVector.FromString(
                Description, StringType.Utf8);
            data.Add(ByteVector.FromUInt((uint)
                decriptionData.Count));
            data.Add(decriptionData);

            data.Add(ByteVector.FromUInt((uint)Width));
            data.Add(ByteVector.FromUInt((uint)Height));
            data.Add(ByteVector.FromUInt((uint)ColorDepth));
            data.Add(ByteVector.FromUInt((uint)IndexedColors));

            data.Add(ByteVector.FromUInt((uint)Data.Count));
            data.Add(Data);

            return data;
        }

        /// <summary>
        ///    Gets and sets the mime-type of the picture data
        ///    stored in the current instance.
        /// </summary>
        /// <value>
        ///    A <see cref="string" /> object containing the mime-type
        ///    of the picture data stored in the current instance.
        /// </value>
        public string MimeType
        {
            get { return _mimeType; }
            set { _mimeType = value; }
        }

        /// <summary>
        ///    Gets and sets the type of content visible in the picture
        ///    stored in the current instance.
        /// </summary>
        /// <value>
        ///    A <see cref="PictureType" /> containing the type of
        ///    content visible in the picture stored in the current
        ///    instance.
        /// </value>
        public PictureType Type
        {
            get { return _type; }
            set { _type = value; }
        }

        /// <summary>
        ///    Gets and sets a description of the picture stored in the
        ///    current instance.
        /// </summary>
        /// <value>
        ///    A <see cref="string" /> object containing a description
        ///    of the picture stored in the current instance.
        /// </value>
        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        /// <summary>
        ///    Gets and sets the picture data stored in the current
        ///    instance.
        /// </summary>
        /// <value>
        ///    A <see cref="ByteVector" /> object containing the picture
        ///    data stored in the current instance.
        /// </value>
        public ByteVector Data
        {
            get { return _pictureData; }
            set { _pictureData = value; }
        }

        /// <summary>
        ///    Gets and sets the width of the picture in the current
        ///    instance.
        /// </summary>
        /// <value>
        ///    A <see cref="int" /> value containing width of the
        ///    picture stored in the current instance.
        /// </value>
        public int Width
        {
            get { return _width; }
            set { _width = value; }
        }

        /// <summary>
        ///    Gets and sets the height of the picture in the current
        ///    instance.
        /// </summary>
        /// <value>
        ///    A <see cref="int" /> value containing height of the
        ///    picture stored in the current instance.
        /// </value>
        public int Height
        {
            get { return _height; }
            set { _height = value; }
        }

        /// <summary>
        ///    Gets and sets the color depth of the picture in the
        ///    current instance.
        /// </summary>
        /// <value>
        ///    A <see cref="int" /> value containing color depth of the
        ///    picture stored in the current instance.
        /// </value>
        public int ColorDepth
        {
            get { return _colorDepth; }
            set { _colorDepth = value; }
        }

        /// <summary>
        ///    Gets and sets the number of indexed colors in the picture
        ///    in the current instance.
        /// </summary>
        /// <value>
        ///    A <see cref="int" /> value containing number of indexed
        ///    colors in the picture, or zero if the picture is not
        ///    stored in an indexed format.
        /// </value>
        public int IndexedColors
        {
            get { return _indexedColors; }
            set { _indexedColors = value; }
        }
    }
}