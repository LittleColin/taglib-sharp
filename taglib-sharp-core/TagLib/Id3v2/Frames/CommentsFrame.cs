//
// CommentsFrame.cs:
//
// Author:
//   Brian Nickel (brian.nickel@gmail.com)
//
// Original Source:
//   id3v2commentsframe.cpp from TagLib
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

namespace TagLib.Id3v2
{
    /// <summary>
    ///    This class extends <see cref="Frame" />, implementing support for
    ///    ID3v2 Comments (COMM) Frames.
    /// </summary>
    /// <remarks>
    ///    <para>A <see cref="CommentsFrame" /> should be used for storing
    ///    user readable comments on the media file.</para>
    ///    <para>When reading comments from a file, <see cref="GetPreferred"
    ///    /> should be used as it gracefully falls back to comments that
    ///    you, as a developer, may not be expecting. When writing comments,
    ///    however, it is best to use <see cref="Get" /> as it forces it to
    ///    be written in the exact version you are expecting.</para>
    /// </remarks>
    public class CommentsFrame : Frame
    {
        /// <summary>
        ///    Contains the description of the current instance.
        /// </summary>
        private string _description = null;

        /// <summary>
        ///    Contains the text encoding to use when rendering the
        ///    current instance.
        /// </summary>
        private StringType _encoding = Tag.DefaultEncoding;

        /// <summary>
        ///    Contains the ISO-639-2 language code of the current
        ///    instance.
        /// </summary>
        private string _language = null;

        /// <summary>
        ///    Contains the comment text of the current instance.
        /// </summary>
        private string _text = null;

        /// <summary>
        ///    Constructs and initializes a new instance of <see
        ///    cref="CommentsFrame" /> with a specified description,
        ///    ISO-639-2 language code, and text encoding.
        /// </summary>
        /// <param name="description">
        ///    A <see cref="string" /> containing the description of the
        ///    new frame.
        /// </param>
        /// <param name="language">
        ///    A <see cref="string" /> containing the ISO-639-2 language
        ///    code of the new frame.
        /// </param>
        /// <param name="encoding">
        ///    A <see cref="StringType" /> containing the text encoding
        ///    to use when rendering the new frame.
        /// </param>
        /// <remarks>
        ///    When a frame is created, it is not automatically added to
        ///    the tag. Consider using <see cref="Get" /> for more
        ///    integrated frame creation.
        /// </remarks>
        public CommentsFrame(string description, string language,
                              StringType encoding)
            : base(FrameType.Comm, 4)
        {
            _encoding = encoding;
            _language = language;
            _description = description;
        }

        /// <summary>
        ///    Constructs and initializes a new instance of <see
        ///    cref="CommentsFrame" /> with a specified description and
        ///    ISO-639-2 language code.
        /// </summary>
        /// <param name="description">
        ///    A <see cref="string" /> containing the description of the
        ///    new frame.
        /// </param>
        /// <param name="language">
        ///    A <see cref="string" /> containing the ISO-639-2 language
        ///    code of the new frame.
        /// </param>
        /// <remarks>
        ///    When a frame is created, it is not automatically added to
        ///    the tag. Consider using <see cref="Get" /> for more
        ///    integrated frame creation.
        /// </remarks>
        public CommentsFrame(string description, string language)
            : this(description, language,
                    Tag.DefaultEncoding)
        {
        }

        /// <summary>
        ///    Constructs and initializes a new instance of <see
        ///    cref="CommentsFrame" /> with a specified description.
        /// </summary>
        /// <param name="description">
        ///    A <see cref="string" /> containing the description of the
        ///    new frame.
        /// </param>
        /// <remarks>
        ///    When a frame is created, it is not automatically added to
        ///    the tag. Consider using <see cref="Get" /> for more
        ///    integrated frame creation.
        /// </remarks>
        public CommentsFrame(string description)
            : this(description, null)
        {
        }

        /// <summary>
        ///    Constructs and initializes a new instance of <see
        ///    cref="CommentsFrame" /> by reading its raw data in a
        ///    specified ID3v2 version.
        /// </summary>
        /// <param name="data">
        ///    A <see cref="ByteVector" /> object starting with the raw
        ///    representation of the new frame.
        /// </param>
        /// <param name="version">
        ///    A <see cref="byte" /> indicating the ID3v2 version the
        ///    raw frame is encoded in.
        /// </param>
        public CommentsFrame(ByteVector data, byte version)
            : base(data, version)
        {
            SetData(data, 0, version, true);
        }

        /// <summary>
        ///    Constructs and initializes a new instance of <see
        ///    cref="CommentsFrame" /> by reading its raw data in a
        ///    specified ID3v2 version.
        /// </summary>
        /// <param name="data">
        ///    A <see cref="ByteVector" /> object containing the raw
        ///    representation of the new frame.
        /// </param>
        /// <param name="offset">
        ///    A <see cref="int" /> indicating at what offset in
        ///    <paramref name="data" /> the frame actually begins.
        /// </param>
        /// <param name="header">
        ///    A <see cref="FrameHeader" /> containing the header of the
        ///    frame found at <paramref name="offset" /> in the data.
        /// </param>
        /// <param name="version">
        ///    A <see cref="byte" /> indicating the ID3v2 version the
        ///    raw frame is encoded in.
        /// </param>
        protected internal CommentsFrame(ByteVector data, int offset,
                                          FrameHeader header,
                                          byte version) : base(header)
        {
            SetData(data, offset, version, false);
        }

        /// <summary>
        ///    Gets and sets the description stored in the current
        ///    instance.
        /// </summary>
        /// <value>
        ///    A <see cref="string" /> containing the description
        ///    stored in the current instance.
        /// </value>
        /// <remarks>
        ///    There should only be one frame with a matching
        ///    description and ISO-639-2 language code per tag.
        /// </remarks>
        public string Description
        {
            get
            {
                if (_description != null)
                    return _description;

                return string.Empty;
            }
            set
            {
                _description = value;
            }
        }

        /// <summary>
        ///    Gets and sets the ISO-639-2 language code stored in the
        ///    current instance.
        /// </summary>
        /// <value>
        ///    A <see cref="string" /> containing the ISO-639-2 language
        ///    code stored in the current instance.
        /// </value>
        /// <remarks>
        ///    There should only be one file with a matching description
        ///    and ISO-639-2 language code per tag.
        /// </remarks>
        public string Language
        {
            get
            {
                if (_language != null && _language.Length > 2)
                    return _language.Substring(0, 3);

                return "XXX";
            }
            set
            {
                _language = value;
            }
        }

        /// <summary>
        ///    Gets and sets the comment text stored in the current
        ///    instance.
        /// </summary>
        /// <value>
        ///    A <see cref="string" /> containing the comment text
        ///    stored in the current instance.
        /// </value>
        public string Text
        {
            get
            {
                if (_text != null)
                    return _text;

                return string.Empty;
            }
            set
            {
                _text = value;
            }
        }

        /// <summary>
        ///    Gets and sets the text encoding to use when storing the
        ///    current instance.
        /// </summary>
        /// <value>
        ///    A <see cref="string" /> containing the text encoding to
        ///    use when storing the current instance.
        /// </value>
        /// <remarks>
        ///    This encoding is overridden when rendering if <see
        ///    cref="Tag.ForceDefaultEncoding" /> is <see
        ///    langword="true" /> or the render version does not support
        ///    it.
        /// </remarks>
        public StringType TextEncoding
        {
            get { return _encoding; }
            set { _encoding = value; }
        }

        /// <summary>
        ///    Gets a specified comments frame from the specified tag,
        ///    optionally creating it if it does not exist.
        /// </summary>
        /// <param name="tag">
        ///    A <see cref="Tag" /> object to search in.
        /// </param>
        /// <param name="description">
        ///    A <see cref="string" /> specifying the description to
        ///    match.
        /// </param>
        /// <param name="language">
        ///    A <see cref="string" /> specifying the ISO-639-2 language
        ///   code to match.
        /// </param>
        /// <param name="create">
        ///    A <see cref="bool" /> specifying whether or not to create
        ///    and add a new frame to the tag if a match is not found.
        /// </param>
        /// <returns>
        ///    A <see cref="CommentsFrame" /> object containing the
        ///    matching frame, or <see langword="null" /> if a match
        ///    wasn't found and <paramref name="create" /> is <see
        ///    langword="false" />.
        /// </returns>
        public static CommentsFrame Get(Tag tag, string description,
                                         string language, bool create)
        {
            CommentsFrame comm;
            foreach (Frame frame in tag.GetFrames(FrameType.Comm))
            {
                comm = frame as CommentsFrame;

                if (comm == null)
                    continue;

                if (comm.Description != description)
                    continue;

                if (language != null && language != comm.Language)
                    continue;

                return comm;
            }

            if (!create)
                return null;

            comm = new CommentsFrame(description, language);
            tag.AddFrame(comm);
            return comm;
        }

        /// <summary>
        ///    Gets a specified comments frame from the specified tag,
        ///    trying to to match the description and language but
        ///    accepting an incomplete match.
        /// </summary>
        /// <param name="tag">
        ///    A <see cref="Tag" /> object to search in.
        /// </param>
        /// <param name="description">
        ///    A <see cref="string" /> specifying the description to
        ///    match.
        /// </param>
        /// <param name="language">
        ///    A <see cref="string" /> specifying the ISO-639-2 language
        ///   code to match.
        /// </param>
        /// <returns>
        ///    A <see cref="CommentsFrame" /> object containing the
        ///    matching frame, or <see langword="null" /> if a match
        ///    wasn't found.
        /// </returns>
        /// <remarks>
        ///    <para>The method tries matching with the following order
        ///    of precidence:</para>
        ///    <list type="number">
        ///       <item><term>The first frame with a matching
        ///       description and language.</term></item>
        ///       <item><term>The first frame with a matching
        ///       language.</term></item>
        ///       <item><term>The first frame with a matching
        ///       description.</term></item>
        ///       <item><term>The first frame.</term></item>
        ///    </list>
        /// </remarks>
        public static CommentsFrame GetPreferred(Tag tag,
                                                  string description,
                                                  string language)
        {
            // This is weird, so bear with me. The best thing we can
            // have is something straightforward and in our own
            // language. If it has a description, then it is
            // probably used for something other than an actual
            // comment. If that doesn't work, we'd still rather have
            // something in our language than something in another.
            // After that all we have left are things in other
            // languages, so we'd rather have one with actual
            // content, so we try to get one with no description
            // first.

            bool skipItunes = description == null ||
                !description.StartsWith("iTun");

            int bestValue = -1;
            CommentsFrame bestFrame = null;

            foreach (Frame frame in tag.GetFrames(FrameType.Comm))
            {
                CommentsFrame comm = frame as CommentsFrame;

                if (comm == null)
                    continue;

                if (skipItunes &&
                    comm.Description.StartsWith("iTun"))
                    continue;

                bool sameName = comm.Description == description;
                bool sameLang = comm.Language == language;

                if (sameName && sameLang)
                    return comm;

                int value = sameLang ? 2 : sameName ? 1 : 0;

                if (value <= bestValue)
                    continue;

                bestValue = value;
                bestFrame = comm;
            }

            return bestFrame;
        }

        /// <summary>
        ///    Creates a deep copy of the current instance.
        /// </summary>
        /// <returns>
        ///    A new <see cref="Frame" /> object identical to the
        ///    current instance.
        /// </returns>
        public override Frame Clone()
        {
            CommentsFrame frame = new CommentsFrame(_description,
                _language, _encoding);
            frame._text = _text;
            return frame;
        }

        /// <summary>
        ///    Gets a string representation of the current instance.
        /// </summary>
        /// <returns>
        ///    A <see cref="string" /> containing the comment text.
        /// </returns>
        public override string ToString()
        {
            return Text;
        }

        /// <summary>
        ///    Populates the values in the current instance by parsing
        ///    its field data in a specified version.
        /// </summary>
        /// <param name="data">
        ///    A <see cref="ByteVector" /> object containing the
        ///    extracted field data.
        /// </param>
        /// <param name="version">
        ///    A <see cref="byte" /> indicating the ID3v2 version the
        ///    field data is encoded in.
        /// </param>
        protected override void ParseFields(ByteVector data,
                                             byte version)
        {
            if (data.Count < 4)
                throw new CorruptFileException(
                    "Not enough bytes in field.");

            _encoding = (StringType)data[0];
            _language = data.ToString(StringType.Latin1, 1, 3);

            // Instead of splitting into two string, in the format
            // [{desc}\0{value}], try splitting into three strings
            // in case of a misformatted [{desc}\0{value}\0].
            string[] split = data.ToStrings(_encoding, 4, 3);

            if (split.Length == 0)
            {
                // No data in the frame.
                _description = string.Empty;
                _text = string.Empty;
            }
            else if (split.Length == 1)
            {
                // Bad comment frame. Assume that it lacks a
                // description.
                _description = string.Empty;
                _text = split[0];
            }
            else
            {
                _description = split[0];
                _text = split[1];
            }
        }

        /// <summary>
        ///    Renders the values in the current instance into field
        ///    data for a specified version.
        /// </summary>
        /// <param name="version">
        ///    A <see cref="byte" /> indicating the ID3v2 version the
        ///    field data is to be encoded in.
        /// </param>
        /// <returns>
        ///    A <see cref="ByteVector" /> object containing the
        ///    rendered field data.
        /// </returns>
        protected override ByteVector RenderFields(byte version)
        {
            StringType encoding = CorrectEncoding(TextEncoding,
                version);
            ByteVector v = new ByteVector();

            v.Add((byte)encoding);
            v.Add(ByteVector.FromString(Language, StringType.Latin1));
            v.Add(ByteVector.FromString(_description, encoding));
            v.Add(ByteVector.TextDelimiter(encoding));
            v.Add(ByteVector.FromString(_text, encoding));

            return v;
        }
    }
}