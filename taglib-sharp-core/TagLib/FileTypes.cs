//
// FileTypes.cs: Provides a mechanism for registering file classes and mime-
// types, to be used when constructing a class via TagLib.File.Create.
//
// Author:
//   Aaron Bockover (abockover@novell.com)
//
// Copyright (C) 2006 Novell, Inc.
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
using System.Linq;
using System.Reflection;
using TagLib.Mpeg;

namespace TagLib
{
    /// <summary>
    ///    This static class provides a mechanism for registering file
    ///    classes and mime-types, to be used when constructing a class via
    ///    <see cref="File.Create(string)" />.
    /// </summary>
    /// <remarks>
    ///    <para>The default types built into the taglib-sharp.dll assembly
    ///    are registered automatically when the class is initialized. To
    ///    register your own custom types, use <see cref="Register"
    ///    />.</para>
    /// </remarks>
    /// <seealso cref="SupportedMimeType" />
    public static class FileTypes
    {
        /// <summary>
        ///    Contains a static array of file types contained in the
        ///    TagLib# assembly.
        /// </summary>
        /// <remarks>
        ///    A static Type array is used instead of getting types by
        ///    reflecting the executing assembly as Assembly.GetTypes is
        ///    very inefficient and leaks every type instance under
        ///    Mono. Not reflecting taglib-sharp.dll saves about 120KB
        ///    of heap.
        /// </remarks>
        private static readonly Type[] StaticFileTypes = {
            typeof(Aac.File),
            typeof(Aiff.File),
            typeof(Ape.File),
            typeof(Asf.File),
            typeof(Audible.File),
            typeof(Dsf.File),
            typeof(Flac.File),
            typeof(Matroska.File),
            typeof(Gif.File),
            typeof(Image.NoMetadata.File),
            typeof(Jpeg.File),
            typeof(Mpeg4.File),
            typeof(AudioFile),
            typeof(Mpeg.File),
            typeof(MusePack.File),
            typeof(Ogg.File),
            typeof(Png.File),
            typeof(Riff.File),
            typeof(Tiff.Arw.File),
            typeof(Tiff.Cr2.File),
            typeof(Tiff.Dng.File),
            typeof(Tiff.File),
            typeof(Tiff.Nef.File),
            typeof(Tiff.Pef.File),
            typeof(Tiff.Rw2.File),
            typeof(WavPack.File)
        };

        /// <summary>
        ///    Contains a mapping between mime-types and the <see
        ///    cref="File" /> subclasses that support them.
        /// </summary>
        private static Dictionary<string, Type> _fileTypes;

        /// <summary>
        ///    Constructs and initializes the <see cref="FileTypes" />
        ///    class by registering the default types.
        /// </summary>
        static FileTypes()
        {
            Init();
        }

        /// <summary>
        ///    Gets a dictionary containing all the supported mime-types
        ///    and file classes used by <see cref="File.Create(string)"
        ///    />.
        /// </summary>
        /// <value>
        ///    A <see cref="T:System.Collections.Generic.IDictionary`2" /> object containing the
        ///    supported mime-types.
        /// </value>
        public static IDictionary<string, Type> AvailableTypes => _fileTypes;

        /// <summary>
        ///    Registers a <see cref="File" /> subclass to be used when
        ///    creating files via <see cref="File.Create(string)" />.
        /// </summary>
        /// <param name="type">
        ///    A <see cref="Type" /> object for the class to register.
        /// </param>
        /// <remarks>
        ///    In order to register mime-types, the class represented by
        ///    <paramref name="type" /> should use the <see
        ///    cref="SupportedMimeType" /> custom attribute.
        /// </remarks>
        public static void Register(Type type)
        {
            IEnumerable<Attribute> attrs = type.GetTypeInfo().GetCustomAttributes(typeof(SupportedMimeType), false).ToList();

            if (attrs == null || !attrs.Any())
                return;

            foreach (var attr in attrs.Cast<SupportedMimeType>())
                _fileTypes.Add(attr.MimeType, type);
        }

        /// <summary>
        ///    Initializes the class by registering the default types.
        /// </summary>
        internal static void Init()
        {
            if (_fileTypes != null)
                return;

            _fileTypes = new Dictionary<string, Type>();

            foreach (Type type in StaticFileTypes)
                Register(type);
        }
    }
}