//
// Tag.cs: Provide support for reading and writing ID3v1 tags.
//
// Author:
//   Brian Nickel (brian.nickel@gmail.com)
//
// Original Source:
//   id3v1tag.cpp from TagLib
//
// Copyright (C) 2005-2007 Brian Nickel
// Copyright (C) 2002,2003 Scott Wheeler (Original Implementation)
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
using System.Globalization;

namespace TagLib.Id3v1
{
    /// <summary>
    ///    This class extends <see cref="Tag" /> to provide support for
    ///    reading and writing tags stored in the ID3v1.1 format.
    /// </summary>
    public class Tag : TagLib.Tag
    {
        /// <summary>
        ///    The size of a ID3v1 tag.
        /// </summary>
        public const uint Size = 128;

        /// <summary>
        ///    The identifier used to recognize a ID3v1 tags.
        /// </summary>
        /// <value>
        ///    "TAG"
        /// </value>
        public static readonly ReadOnlyByteVector FileIdentifier = "TAG";

        private static StringHandler _stringHandler = new StringHandler();

        /// <summary>
        ///    Contains the album name.
        /// </summary>
        private string _album;

        /// <summary>
        ///    Contains the semicolon separated performers.
        /// </summary>
        private string _artist;

        /// <summary>
        ///    Contains a comment on track.
        /// </summary>
        private string _comment;

        /// <summary>
        ///    Contains the genre index.
        /// </summary>
        private byte _genre;

        /// <summary>
        ///    Contains the title.
        /// </summary>
        private string _title;

        /// <summary>
        ///    Contains the track number in the album.
        /// </summary>
        private byte _track;

        /// <summary>
        ///    Contains the 4 digit year.
        /// </summary>
        private string _year;

        /// <summary>
        ///    Constructs and initializes a new instance of <see
        ///    cref="Tag" /> with no contents.
        /// </summary>
        public Tag()
        {
            Clear();
        }

        /// <summary>
        ///    Constructs and initializes a new instance of <see
        ///    cref="Tag" /> by reading the contents from a specified
        ///    position in a specified file.
        /// </summary>
        /// <param name="file">
        ///    A <see cref="File" /> object containing the file from
        ///    which the contents of the new instance is to be read.
        /// </param>
        /// <param name="position">
        ///    A <see cref="long" /> value specify at what position to
        ///    read the tag.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///    <paramref name="file" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///    <paramref name="position" /> is less than zero or greater
        ///    than the size of the file.
        /// </exception>
        /// <exception cref="CorruptFileException">
        ///    The file does not contain <see cref="FileIdentifier" />
        ///    at the given position.
        /// </exception>
        public Tag(File file, long position)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));

            file.Mode = File.AccessMode.Read;

            if (position < 0 ||
                position > file.Length - Size)
                throw new ArgumentOutOfRangeException(
                    nameof(position));

            file.Seek(position);

            // read the tag -- always 128 bytes

            ByteVector data = file.ReadBlock((int)Size);

            // some initial sanity checking

            if (!data.StartsWith(FileIdentifier))
                throw new CorruptFileException(
                    "ID3v1 data does not start with identifier.");

            Parse(data);
        }

        /// <summary>
        ///    Constructs and initializes a new instance of <see
        ///    cref="Tag" /> by reading the contents from a specified
        ///    <see cref="ByteVector" /> object.
        /// </summary>
        /// <param name="data">
        ///    A <see cref="ByteVector" /> object to read the tag from.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///    <paramref name="data" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="CorruptFileException">
        ///    <paramref name="data" /> is less than 128 bytes or does
        ///    not start with <see cref="FileIdentifier" />.
        /// </exception>
        public Tag(ByteVector data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            if (data.Count < Size)
                throw new CorruptFileException(
                    "ID3v1 data is less than 128 bytes long.");

            if (!data.StartsWith(FileIdentifier))
                throw new CorruptFileException(
                    "ID3v1 data does not start with identifier.");

            Parse(data);
        }

        /// <summary>
        ///    Gets and sets the <see cref="StringHandler" /> object
        ///    to use when reading and writing ID3v1 fields.
        /// </summary>
        /// <value>
        ///    A <see cref="StringHandler" /> object to use when
        ///    processing fields.
        /// </value>
        public static StringHandler DefaultStringHandler
        {
            get { return _stringHandler; }
            set { _stringHandler = value; }
        }

        /// <summary>
        ///    Gets and sets the album of the media represented by the
        ///    current instance.
        /// </summary>
        /// <value>
        ///    A <see cref="string" /> object containing the album of
        ///    the media represented by the current instance or <see
        ///    langword="null" /> if no value is present.
        /// </value>
        /// <remarks>
        ///    When stored on disk, only the first 30 bytes of the
        ///    Latin-1 encoded value will be stored. This may result in
        ///    lost data.
        /// </remarks>
        public override string Album
        {
            get
            {
                return string.IsNullOrEmpty(_album) ?
                    null : _album;
            }
            set
            {
                _album = value != null ?
                    value.Trim() : string.Empty;
            }
        }

        /// <summary>
        ///    Gets and sets a user comment on the media represented by
        ///    the current instance.
        /// </summary>
        /// <value>
        ///    A <see cref="string" /> object containing user comments
        ///    on the media represented by the current instance or <see
        ///    langword="null" /> if no value is present.
        /// </value>
        /// <remarks>
        ///    When stored on disk, only the first 28 bytes of the
        ///    Latin-1 encoded value will be stored. This may result in
        ///    lost data.
        /// </remarks>
        public override string Comment
        {
            get
            {
                return string.IsNullOrEmpty(_comment) ?
                    null : _comment;
            }
            set
            {
                _comment = value != null ?
                    value.Trim() : string.Empty;
            }
        }

        /// <summary>
        ///    Gets and sets the genres of the media represented by the
        ///    current instance.
        /// </summary>
        /// <value>
        ///    A <see cref="string[]" /> containing the genres of the
        ///    media represented by the current instance or an empty
        ///    array if no value is present.
        /// </value>
        /// <remarks>
        ///    Only first genre will be stored and only if it is an
        ///    exact match for a value appearing in <see
        ///    cref="TagLib.Genres.Audio" />. All other values will
        ///    result in the property being cleared.
        /// </remarks>
        public override string[] Genres
        {
            get
            {
                string genreName =
                    TagLib.Genres.IndexToAudio(_genre);

                return (genreName != null) ?
                    new string[] { genreName } :
                    new string[0];
            }
            set
            {
                _genre = (value == null || value.Length == 0) ?
                    (byte)255 :
                    TagLib.Genres.AudioToIndex(
                        value[0].Trim());
            }
        }

        /// <summary>
        ///    Gets and sets the performers or artists who performed in
        ///    the media described by the current instance.
        /// </summary>
        /// <value>
        ///    A <see cref="string[]" /> containing the performers or
        ///    artists who performed in the media described by the
        ///    current instance or an empty array if no value is
        ///    present.
        /// </value>
        /// <remarks>
        ///    When stored on disk, only the first 30 bytes of the
        ///    Latin-1 encoded value will be stored, minus a byte for
        ///    each additionial performer (i.e. two performers will only
        ///    have 29 bytes and three performers will only have 28
        ///    bytes).This may result in lost data.
        /// </remarks>
        public override string[] Performers
        {
            get
            {
                return string.IsNullOrEmpty(_artist) ?
                    new string[0] : _artist.Split(';');
            }
            set
            {
                _artist = value != null ?
                    string.Join(";", value) : string.Empty;
            }
        }

        /// <summary>
        ///    Gets the tag types contained in the current instance.
        /// </summary>
        /// <value>
        ///    Always <see cref="TagTypes.Id3v1" />.
        /// </value>
        public override TagTypes TagTypes => TagTypes.Id3V1;

        /// <summary>
        ///    Gets and sets the title for the media described by the
        ///    current instance.
        /// </summary>
        /// <value>
        ///    A <see cref="string" /> object containing the title for
        ///    the media described by the current instance or <see
        ///    langword="null" /> if no value is present.
        /// </value>
        /// <remarks>
        ///    When stored on disk, only the first 30 bytes of the
        ///    Latin-1 encoded value will be stored. This may result in
        ///    lost data.
        /// </remarks>
        public override string Title
        {
            get
            {
                return string.IsNullOrEmpty(_title) ?
                    null : _title;
            }
            set
            {
                _title = value != null ?
                    value.Trim() : string.Empty;
            }
        }

        /// <summary>
        ///    Gets and sets the position of the media represented by
        ///    the current instance in its containing album.
        /// </summary>
        /// <value>
        ///    A <see cref="uint" /> containing the position of the
        ///    media represented by the current instance in its
        ///    containing album or zero if not specified.
        /// </value>
        /// <remarks>
        ///    Only values between 1 and 255 will be stored, all other
        ///    values will result in the property being zeroed.
        /// </remarks>
        public override uint Track
        {
            get { return _track; }
            set { _track = (byte)(value < 256 ? value : 0); }
        }

        /// <summary>
        ///    Gets and sets the year that the media represented by the
        ///    current instance was recorded.
        /// </summary>
        /// <value>
        ///    A <see cref="uint" /> containing the year that the media
        ///    represented by the current instance was created or zero
        ///    if no value is present.
        /// </value>
        /// <remarks>
        ///    Only values between 1 and 9999 will be stored, all other
        ///    values will result in the property being zeroed.
        /// </remarks>
        public override uint Year
        {
            get
            {
                uint value;
                return uint.TryParse(_year,
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out value) ? value : 0;
            }

            set
            {
                _year = (value > 0 && value < 10000) ?
                    value.ToString(
                        CultureInfo.InvariantCulture) :
                    string.Empty;
            }
        }

        /// <summary>
        ///    Clears the values stored in the current instance.
        /// </summary>
        public override void Clear()
        {
            _title = _artist = _album = _year = _comment = null;
            _track = 0;
            _genre = 255;
        }

        /// <summary>
        ///    Renders the current instance as a raw ID3v1 tag.
        /// </summary>
        /// <returns>
        ///    A <see cref="ByteVector" /> object containing the
        ///    rendered tag.
        /// </returns>
        public ByteVector Render()
        {
            ByteVector data = new ByteVector();

            data.Add(FileIdentifier);
            data.Add(_stringHandler.Render(_title).Resize(30));
            data.Add(_stringHandler.Render(_artist).Resize(30));
            data.Add(_stringHandler.Render(_album).Resize(30));
            data.Add(_stringHandler.Render(_year).Resize(4));
            data.Add(_stringHandler.Render(_comment).Resize(28));
            data.Add(0);
            data.Add(_track);
            data.Add(_genre);

            return data;
        }

        /// <summary>
        ///    Populates the current instance by parsing the contents of
        ///    a raw ID3v1 tag.
        /// </summary>
        /// <param name="data">
        ///    A <see cref="ByteVector" /> object containing the
        ///    starting with an ID3v1 tag.
        /// </param>
        private void Parse(ByteVector data)
        {
            _title = _stringHandler.Parse(data.Mid(3, 30));
            _artist = _stringHandler.Parse(data.Mid(33, 30));
            _album = _stringHandler.Parse(data.Mid(63, 30));
            _year = _stringHandler.Parse(data.Mid(93, 4));

            // Check for ID3v1.1 -- Note that ID3v1 *does not*
            // support "track zero" -- this is not a bug in TagLib.
            // Since a zeroed byte is what we would expect to
            // indicate the end of a C-String, specifically the
            // comment string, a value of zero must be assumed to be
            // just that.

            if (data[125] == 0 && data[126] != 0)
            {
                // ID3v1.1 detected
                _comment = _stringHandler.Parse(data.Mid(97, 28));
                _track = data[126];
            }
            else
            {
                _comment = _stringHandler.Parse(data.Mid(97, 30));
            }

            _genre = data[127];
        }
    }
}