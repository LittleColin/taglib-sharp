//
// StreamPropertiesObject.cs: Provides a representation of an ASF Stream
// Properties object which can be read from and written to disk.
//
// Author:
//   Brian Nickel (brian.nickel@gmail.com)
//
// Copyright (C) 2006-2007 Brian Nickel
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
using TagLib.Riff;

namespace TagLib.Asf
{
    /// <summary>
    ///    This class extends <see cref="Object" /> to provide a
    ///    representation of an ASF Stream Properties object which can be
    ///    read from and written to disk.
    /// </summary>
    public class StreamPropertiesObject : Object
    {
        /// <summary>
        ///    Contains the stream type GUID.
        /// </summary>
        private System.Guid _streamType;

        /// <summary>
        ///    Contains the error correction type GUID.
        /// </summary>
        private System.Guid _errorCorrectionType;

        /// <summary>
        ///    Contains the time offset of the stream.
        /// </summary>
        private readonly ulong _timeOffset;

        /// <summary>
        ///    Contains the stream flags.
        /// </summary>
        private readonly ushort _flags;

        /// <summary>
        ///    Contains the reserved data.
        /// </summary>
        private readonly uint _reserved;

        /// <summary>
        ///    Contains the type specific data.
        /// </summary>
        private readonly ByteVector _typeSpecificData;

        /// <summary>
        ///    Contains the error correction data.
        /// </summary>
        private readonly ByteVector _errorCorrectionData;

        /// <summary>
        ///    Constructs and initializes a new instance of <see
        ///    cref="PaddingObject" /> by reading the contents from a
        ///    specified position in a specified file.
        /// </summary>
        /// <param name="file">
        ///    A <see cref="Asf.File" /> object containing the file from
        ///    which the contents of the new instance are to be read.
        /// </param>
        /// <param name="position">
        ///    A <see cref="long" /> value specify at what position to
        ///    read the object.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///    <paramref name="file" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///    <paramref name="position" /> is less than zero or greater
        ///    than the size of the file.
        /// </exception>
        /// <exception cref="CorruptFileException">
        ///    The object read from disk does not have the correct GUID
        ///    or smaller than the minimum size.
        /// </exception>
        public StreamPropertiesObject(File file, long position)
            : base(file, position)
        {
            if (!Guid.Equals(Asf.Guid.AsfStreamPropertiesObject))
                throw new CorruptFileException(
                    "Object GUID incorrect.");

            if (OriginalSize < 78)
                throw new CorruptFileException(
                    "Object size too small.");

            _streamType = file.ReadGuid();
            _errorCorrectionType = file.ReadGuid();
            _timeOffset = file.ReadQWord();

            int typeSpecificDataLength = (int)file.ReadDWord();
            int errorCorrectionDataLength = (int)
                file.ReadDWord();

            _flags = file.ReadWord();
            _reserved = file.ReadDWord();
            _typeSpecificData =
                file.ReadBlock(typeSpecificDataLength);
            _errorCorrectionData =
                file.ReadBlock(errorCorrectionDataLength);
        }

        /// <summary>
        ///    Gets the codec information contained in the current
        ///    instance.
        /// </summary>
        /// <value>
        ///    A <see cref="ICodec" /> object containing the codec
        ///    information read from <see cref="TypeSpecificData" /> or
        ///    <see langword="null" /> if the data could not be decoded.
        /// </value>
        public ICodec Codec
        {
            get
            {
                if (_streamType == Asf.Guid.AsfAudioMedia)
                    return new WaveFormatEx(
                        _typeSpecificData, 0);

                if (_streamType == Asf.Guid.AsfVideoMedia)
                    return new BitmapInfoHeader(
                        _typeSpecificData, 11);

                return null;
            }
        }

        /// <summary>
        ///    Gets the stream type GUID of the current instance.
        /// </summary>
        /// <summary>
        ///    A <see cref="System.Guid" /> object containing the stream
        ///    type GUID of the current instance.
        /// </summary>
        public System.Guid StreamType => _streamType;

        /// <summary>
        ///    Gets the error correction type GUID of the current
        ///    instance.
        /// </summary>
        /// <summary>
        ///    A <see cref="System.Guid" /> object containing the error
        ///    correction type GUID of the current instance.
        /// </summary>
        public System.Guid ErrorCorrectionType => _errorCorrectionType;

        /// <summary>
        ///    Gets the time offset at which the stream described by the
        ///    current instance begins.
        /// </summary>
        /// <value>
        ///    A <see cref="TimeSpan" /> value containing the time
        ///    offset at which the stream described by the current
        ///    instance begins.
        /// </value>
        public TimeSpan TimeOffset => new TimeSpan((long)_timeOffset);

        /// <summary>
        ///    Gets the flags that apply to the current instance.
        /// </summary>
        /// <value>
        ///    A <see cref="ushort" /> value containing the flags that
        ///    apply to the current instance.
        /// </value>
        public ushort Flags => _flags;

        /// <summary>
        ///    Gets the type specific data contained in the current
        ///    instance.
        /// </summary>
        /// <value>
        ///    A <see cref="ByteVector" /> object containing the type
        ///    specific data contained in the current instance.
        /// </value>
        /// <remarks>
        ///    The contents of this value are dependant on the type
        ///    contained in <see cref="StreamType" />.
        /// </remarks>
        public ByteVector TypeSpecificData => _typeSpecificData;

        /// <summary>
        ///    Gets the error correction data contained in the current
        ///    instance.
        /// </summary>
        /// <value>
        ///    A <see cref="ByteVector" /> object containing the error
        ///    correction data contained in the current instance.
        /// </value>
        /// <remarks>
        ///    The contents of this value are dependant on the type
        ///    contained in <see cref="ErrorCorrectionType" />.
        /// </remarks>
        public ByteVector ErrorCorrectionData => _errorCorrectionData;

        /// <summary>
        ///    Renders the current instance as a raw ASF object.
        /// </summary>
        /// <returns>
        ///    A <see cref="ByteVector" /> object containing the
        ///    rendered version of the current instance.
        /// </returns>
        public override ByteVector Render()
        {
            ByteVector output = _streamType.ToByteArray();
            output.Add(_errorCorrectionType.ToByteArray());
            output.Add(RenderQWord(_timeOffset));
            output.Add(RenderDWord((uint)
                _typeSpecificData.Count));
            output.Add(RenderDWord((uint)
                _errorCorrectionData.Count));
            output.Add(RenderWord(_flags));
            output.Add(RenderDWord(_reserved));
            output.Add(_typeSpecificData);
            output.Add(_errorCorrectionData);

            return Render(output);
        }
    }
}