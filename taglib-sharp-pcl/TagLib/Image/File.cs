//
// File.cs: Base class for Image types.
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
using System.Collections.Generic;
using TagLib.Gif;
using TagLib.IFD;
using TagLib.Jpeg;
using TagLib.Png;

namespace TagLib.Image
{
    /// <summary>
    ///    This class extends <see cref="TagLib.File" /> to provide basic
    ///    functionality common to all image types.
    /// </summary>
    public abstract class File : TagLib.File
    {
        private CombinedImageTag _imageTag;

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
        protected File(string path) : base(path)
        {
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
        ///    Gets a abstract representation of all tags stored in the
        ///    current instance.
        /// </summary>
        /// <value>
        ///    A <see cref="TagLib.Image.CombinedImageTag" /> object
        ///    representing all image tags stored in the current instance.
        /// </value>
        public CombinedImageTag ImageTag
        {
            get { return _imageTag; }
            protected set { _imageTag = value; }
        }

        /// <summary>
        ///    Gets a abstract representation of all tags stored in the
        ///    current instance.
        /// </summary>
        /// <value>
        ///    A <see cref="TagLib.Tag" /> object representing all tags
        ///    stored in the current instance.
        /// </value>
        public override Tag Tag => ImageTag;

        /// <summary>
        ///    The method creates all tags which are allowed for the current
        ///    instance of the image file. This method can be used to ensure,
        ///    that all tags are in place and properties can be safely used
        ///    to set values.
        /// </summary>
        public void EnsureAvailableTags()
        {
            foreach (TagTypes type in Enum.GetValues(typeof(TagTypes)))
            {
                if ((type & ImageTag.AllowedTypes) != 0x00 && type != TagTypes.AllTags)
                    GetTag(type, true);
            }
        }

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
            foreach (Tag tag in ImageTag.AllTags)
            {
                if ((tag.TagTypes & type) == type)
                    return tag;
            }

            if (!create || (type & ImageTag.AllowedTypes) == 0)
                return null;

            ImageTag newTag = null;
            switch (type)
            {
                case TagTypes.JpegComment:
                    newTag = new JpegCommentTag();
                    break;

                case TagTypes.GifComment:
                    newTag = new GifCommentTag();
                    break;

                case TagTypes.Png:
                    newTag = new PngTag();
                    break;

                case TagTypes.TiffIfd:
                    newTag = new IfdTag();
                    break;
            }

            if (newTag != null)
            {
                ImageTag.AddTag(newTag);
                return newTag;
            }

            throw new NotImplementedException(string.Format("Adding tag of type {0} not supported!", type));
        }

        /// <summary>
        ///    Removes a set of tag types from the current instance.
        /// </summary>
        /// <param name="types">
        ///    A bitwise combined <see cref="TagLib.TagTypes" /> value
        ///    containing tag types to be removed from the file.
        /// </param>
        /// <remarks>
        ///    In order to remove all tags from a file, pass <see
        ///    cref="TagTypes.AllTags" /> as <paramref name="types" />.
        /// </remarks>
        public override void RemoveTags(TagTypes types)
        {
            List<ImageTag> toDelete = new List<ImageTag>();

            foreach (ImageTag tag in ImageTag.AllTags)
            {
                if ((tag.TagTypes & types) == tag.TagTypes)
                    toDelete.Add(tag);
            }

            foreach (ImageTag tag in toDelete)
                ImageTag.RemoveTag(tag);
        }
    }
}