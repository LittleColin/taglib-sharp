//
// PngTag.cs:
//
// Author:
//   Mike Gemuende (mike@gemuende.de)
//
// Copyright (C) 2010 Mike Gemuende
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
using System.Collections;
using System.Collections.Generic;
using TagLib.Image;

namespace TagLib.Png
{
    /// <summary>
    ///    Native Png Keywords
    /// </summary>
    public class PngTag : ImageTag, IEnumerable
    {
        /// <summary>
        ///    Name of image's creator
        /// </summary>
        public static readonly string Author = "Author";

        /// <summary>
        ///    Miscellaneous comment
        /// </summary>
        public static readonly string COMMENT = "Comment";

        /// <summary>
        ///    Copyright notice
        /// </summary>
        public static readonly string COPYRIGHT = "Copyright";

        /// <summary>
        ///    Time of original image creation
        /// </summary>
        public static readonly string CreationTime = "Creation Time";

        /// <summary>
        ///    Description of image (possibly long)
        /// </summary>
        public static readonly string Description = "Description";

        /// <summary>
        ///    Legal disclaimer
        /// </summary>
        public static readonly string Disclaimer = "Disclaimer";

        /// <summary>
        ///    Software used to create the image
        /// </summary>
        public static readonly string SOFTWARE = "Software";

        /// <summary>
        ///    Device used to create the image
        /// </summary>
        public static readonly string Source = "Source";

        /// <summary>
        ///    Short (one line) title or caption for image
        /// </summary>
        public static readonly string TITLE = "Title";

        /// <summary>
        ///    Warning of nature of content
        /// </summary>
        public static readonly string Warning = "Warning";

        /// <summary>
        ///    Store the keywords with their values
        /// </summary>
        private readonly Dictionary<string, string> _keywordStore = new Dictionary<string, string>();

        /// <summary>
        ///    Constructor.
        /// </summary>
        public PngTag()
        {
        }

        /// <summary>
        ///    Gets or sets the comment for the image described
        ///    by the current instance.
        /// </summary>
        /// <value>
        ///    A <see cref="string" /> containing the comment of the
        ///    current instace.
        /// </value>
        /// <remarks>
        ///    We use here both keywords Description and Comment of the
        ///    PNG specification to store the comment.
        /// </remarks>
        public override string Comment
        {
            get
            {
                string description = GetKeyword(Description);

                if (!string.IsNullOrEmpty(description))
                    return description;

                return GetKeyword(COMMENT);
            }
            set
            {
                SetKeyword(Description, value);
                SetKeyword(COMMENT, value);
            }
        }

        /// <summary>
        ///    Gets and sets the copyright information for the media
        ///    represented by the current instance.
        /// </summary>
        /// <value>
        ///    A <see cref="string" /> object containing the copyright
        ///    information for the media represented by the current
        ///    instance or <see langword="null" /> if no value present.
        /// </value>
        public override string Copyright
        {
            get { return GetKeyword(COPYRIGHT); }
            set { SetKeyword(COPYRIGHT, value); }
        }

        /// <summary>
        ///    Gets or sets the creator of the image.
        /// </summary>
        /// <value>
        ///    A <see cref="string" /> with the name of the creator.
        /// </value>
        public override string Creator
        {
            get { return GetKeyword(Author); }
            set { SetKeyword(Author, value); }
        }

        /// <summary>
        ///    Gets or sets the time when the image, the current instance
        ///    belongs to, was taken.
        /// </summary>
        /// <value>
        ///    A <see cref="System.Nullable"/> with the time the image was taken.
        /// </value>
        public override DateTime? DateTime
        {
            get
            {
                DateTime ret;
                string date = GetKeyword(CreationTime);

                if (System.DateTime.TryParse(date, out ret))
                    return ret;

                return null;
            }
            set
            {
                string date = null;

                if (value != null)
                {
                    // Creation Date is stored in RFC 822 for PNG
                    date = value.Value.ToString("R");
                }

                SetKeyword(CreationTime, date);
            }
        }

        /// <summary>
        ///    Gets or sets the software the image, the current instance
        ///    belongs to, was created with.
        /// </summary>
        /// <value>
        ///    A <see cref="string" /> containing the name of the
        ///    software the current instace was created with.
        /// </value>
        public override string Software
        {
            get { return GetKeyword(SOFTWARE); }
            set { SetKeyword(SOFTWARE, value); }
        }

        /// <summary>
        ///    Gets the tag types contained in the current instance.
        /// </summary>
        /// <value>
        ///    Always <see cref="TagTypes.Png" />.
        /// </value>
        public override TagTypes TagTypes => TagTypes.Png;

        /// <summary>
        ///    Gets and sets the title for the media described by the
        ///    current instance.
        /// </summary>
        /// <value>
        ///    A <see cref="string" /> object containing the title for
        ///    the media described by the current instance or <see
        ///    langword="null" /> if no value is present.
        /// </value>
        public override string Title
        {
            get { return GetKeyword(TITLE); }
            set { SetKeyword(TITLE, value); }
        }

        /// <summary>
        ///    Clears the values stored in the current instance.
        /// </summary>
        public override void Clear()
        {
            _keywordStore.Clear();
        }

        /// <summary>
        ///    Returns an enumerator to enumerate all keywords.
        /// </summary>
        /// <returns>
        ///    A <see cref="System.Collections.IEnumerator"/> to enumerate
        ///    the keywords.
        /// </returns>
        public IEnumerator GetEnumerator()
        {
            return _keywordStore.GetEnumerator();
        }

        /// <summary>
        ///    Gets a value of a keyword.
        /// </summary>
        /// <param name="keyword">
        ///    A <see cref="System.String"/> with the keyword to get the value for.
        /// </param>
        /// <returns>
        ///    A <see cref="System.String"/> with the value or  <see langword="null" />
        ///    if the keyword is not contained.
        /// </returns>
        public string GetKeyword(string keyword)
        {
            string ret = null;

            _keywordStore.TryGetValue(keyword, out ret);

            return ret;
        }

        /// <summary>
        ///    Sets a keyword of to the given value.
        /// </summary>
        /// <param name="keyword">
        ///    A <see cref="System.String"/> with the keyword to set.
        /// </param>
        /// <param name="value">
        ///    A <see cref="System.String"/> with the value.
        /// </param>
        public void SetKeyword(string keyword, string value)
        {
            if (string.IsNullOrEmpty(keyword))
                throw new ArgumentException("keyword is null or empty");

            _keywordStore.Remove(keyword);

            if (value != null)
            {
                _keywordStore.Add(keyword, value);
            }
        }
    }
}