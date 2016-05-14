//
// UserCommentIFDEntry.cs:
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

namespace TagLib.IFD.Entries
{
    /// <summary>
    ///    Contains an ASCII STRING value.
    /// </summary>
    public class UserCommentIfdEntry : IFdEntry
    {
        /// <summary>
        ///   Marker for an ASCII-encoded UserComment tag.
        /// </summary>
        public static readonly ByteVector CommentAsciiCode = new byte[] { 0x41, 0x53, 0x43, 0x49, 0x49, 0x00, 0x00, 0x00 };

        /// <summary>
        ///   Corrupt marker that seems to be resembling unicode.
        /// </summary>
        public static readonly ByteVector CommentBadUnicodeCode = new byte[] { 0x55, 0x6E, 0x69, 0x63, 0x6F, 0x64, 0x65, 0x00 };

        /// <summary>
        ///   Marker for a JIS-encoded UserComment tag.
        /// </summary>
        public static readonly ByteVector CommentJisCode = new byte[] { 0x4A, 0x49, 0x53, 0x00, 0x00, 0x00, 0x00, 0x00 };

        /// <summary>
        ///   Marker for a UserComment tag with undefined encoding.
        /// </summary>
        public static readonly ByteVector CommentUndefinedCode = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

        /// <summary>
        ///   Marker for a UNICODE-encoded UserComment tag.
        /// </summary>
        public static readonly ByteVector CommentUnicodeCode = new byte[] { 0x55, 0x4E, 0x49, 0x43, 0x4F, 0x44, 0x45, 0x00 };

        /// <summary>
        ///    Construcor.
        /// </summary>
        /// <param name="tag">
        ///    A <see cref="System.UInt16"/> with the tag ID of the entry this instance
        ///    represents
        /// </param>
        /// <param name="value">
        ///    A <see cref="string"/> to be stored
        /// </param>
        public UserCommentIfdEntry(ushort tag, string value)
        {
            Tag = tag;
            Value = value;
        }

        /// <summary>
        ///    Construcor.
        /// </summary>
        /// <param name="tag">
        ///    A <see cref="System.UInt16"/> with the tag ID of the entry this instance
        ///    represents
        /// </param>
        /// <param name="data">
        ///    A <see cref="ByteVector"/> to be stored
        /// </param>
        /// <param name="file">
        ///    The file that's currently being parsed, used for reporting corruptions.
        /// </param>
        public UserCommentIfdEntry(ushort tag, ByteVector data, File file)
        {
            Tag = tag;

            if (data.StartsWith(CommentAsciiCode))
            {
                Value = TrimNull(data.ToString(StringType.Latin1, CommentAsciiCode.Count, data.Count - CommentAsciiCode.Count));
                return;
            }

            if (data.StartsWith(CommentUnicodeCode))
            {
                Value = TrimNull(data.ToString(StringType.Utf8, CommentUnicodeCode.Count, data.Count - CommentUnicodeCode.Count));
                return;
            }

            var trimmed = data.ToString().Trim();
            if (trimmed.Length == 0 || trimmed == "\0")
            {
                Value = string.Empty;
                return;
            }

            // Some programs like e.g. CanonZoomBrowser inserts just the first 0x00-byte
            // followed by 7-bytes of trash.
            if (data.StartsWith((byte)0x00) && data.Count >= 8)
            {
                // And CanonZoomBrowser fills some trailing bytes of the comment field
                // with '\0'. So we return only the characters before the first '\0'.
                int term = data.Find("\0", 8);
                if (term != -1)
                {
                    Value = data.ToString(StringType.Latin1, 8, term - 8);
                }
                else
                {
                    Value = data.ToString(StringType.Latin1, 8, data.Count - 8);
                }
                return;
            }

            if (data.Data.Length == 0)
            {
                Value = string.Empty;
                return;
            }

            // Try to parse anyway
            int offset = 0;
            int length = data.Count - offset;

            // Corruption that starts with a Unicode header and a count byte.
            if (data.StartsWith(CommentBadUnicodeCode))
            {
                offset = CommentBadUnicodeCode.Count;
                length = data.Count - offset;
            }

            file.MarkAsCorrupt("UserComment with other encoding than Latin1 or Unicode");
            Value = TrimNull(data.ToString(StringType.Utf8, offset, length));
        }

        /// <value>
        ///    The ID of the tag, the current instance belongs to
        /// </value>
        public ushort Tag { get; }

        /// <value>
        ///    The value which is stored by the current instance
        /// </value>
        public string Value { get; }

        /// <summary>
        ///    Renders the current instance to a <see cref="ByteVector"/>
        /// </summary>
        /// <param name="isBigendian">
        ///    A <see cref="System.Boolean"/> indicating the endianess for rendering.
        /// </param>
        /// <param name="offset">
        ///    A <see cref="System.UInt32"/> with the offset, the data is stored.
        /// </param>
        /// <param name="type">
        ///    A <see cref="System.UInt16"/> the ID of the type, which is rendered
        /// </param>
        /// <param name="count">
        ///    A <see cref="System.UInt32"/> with the count of the values which are
        ///    rendered.
        /// </param>
        /// <returns>
        ///    A <see cref="ByteVector"/> with the rendered data.
        /// </returns>
        public ByteVector Render(bool isBigendian, uint offset, out ushort type, out uint count)
        {
            type = (ushort)IfdEntryType.Undefined;

            ByteVector data = new ByteVector();
            data.Add(CommentUnicodeCode);
            data.Add(ByteVector.FromString(Value, StringType.Utf8));

            count = (uint)data.Count;

            return data;
        }

        private string TrimNull(string value)
        {
            int term = value.IndexOf('\0');
            if (term > -1)
                value = value.Substring(0, term);
            return value;
        }
    }
}