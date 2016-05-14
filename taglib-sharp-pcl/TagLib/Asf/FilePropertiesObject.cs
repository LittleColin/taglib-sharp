//
// FilePropertiesObject.cs: Provides a representation of an ASF File Properties
// object which can be read from and written to disk.
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

namespace TagLib.Asf
{
    /// <summary>
    ///    This class extends <see cref="Object" /> to provide a
    ///    representation of an ASF File Properties object which can be read
    ///    from and written to disk.
    /// </summary>
    public class FilePropertiesObject : Object
    {
        /// <summary>
        ///    Contains the GUID for the file.
        /// </summary>
        private System.Guid _fileId;

        /// <summary>
        ///    Contains the file size.
        /// </summary>
        private readonly ulong _fileSize;

        /// <summary>
        ///    Contains the creation date.
        /// </summary>
        private readonly ulong _creationDate;

        /// <summary>
        ///    Contains the packet count.
        /// </summary>
        private readonly ulong _dataPacketsCount;

        /// <summary>
        ///    Contains the play duration.
        /// </summary>
        private readonly ulong _playDuration;

        /// <summary>
        ///    Contains the send duration.
        /// </summary>
        private readonly ulong _sendDuration;

        /// <summary>
        ///    Contains the preroll.
        /// </summary>
        private readonly ulong _preroll;

        /// <summary>
        ///    Contains the file flags.
        /// </summary>
        private readonly uint _flags;

        /// <summary>
        ///    Contains the minimum packet size.
        /// </summary>
        private readonly uint _minimumDataPacketSize;

        /// <summary>
        ///    Contains the maxximum packet size.
        /// </summary>
        private readonly uint _maximumDataPacketSize;

        /// <summary>
        ///    Contains the maximum bitrate of the file.
        /// </summary>
        private readonly uint _maximumBitrate;

        /// <summary>
        ///    Constructs and initializes a new instance of <see
        ///    cref="FilePropertiesObject" /> by reading the contents
        ///    from a specified position in a specified file.
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
        public FilePropertiesObject(File file, long position)
            : base(file, position)
        {
            if (!Guid.Equals(Asf.Guid.AsfFilePropertiesObject))
                throw new CorruptFileException(
                    "Object GUID incorrect.");

            if (OriginalSize < 104)
                throw new CorruptFileException(
                    "Object size too small.");

            _fileId = file.ReadGuid();
            _fileSize = file.ReadQWord();
            _creationDate = file.ReadQWord();
            _dataPacketsCount = file.ReadQWord();
            _sendDuration = file.ReadQWord();
            _playDuration = file.ReadQWord();
            _preroll = file.ReadQWord();
            _flags = file.ReadDWord();
            _minimumDataPacketSize = file.ReadDWord();
            _maximumDataPacketSize = file.ReadDWord();
            _maximumBitrate = file.ReadDWord();
        }

        /// <summary>
        ///    Gets the GUID for the file described by the current
        ///    instance.
        /// </summary>
        /// <value>
        ///    A <see cref="System.Guid" /> value containing the GUID
        ///    for the file described by the current instance.
        /// </value>
        public System.Guid FileId => _fileId;

        /// <summary>
        ///    Gets the size of the file described by the current
        ///    instance.
        /// </summary>
        /// <value>
        ///    A <see cref="ulong" /> value containing the size of the
        ///    file described by the current instance.
        /// </value>
        public ulong FileSize => _fileSize;

        /// <summary>
        ///    Gets the creation date of the file described by the
        ///    current instance.
        /// </summary>
        /// <value>
        ///    A <see cref="DateTime" /> value containing the creation
        ///    date of the file described by the current instance.
        /// </value>
        public DateTime CreationDate => new DateTime((long)_creationDate);

        /// <summary>
        ///    Gets the number of data packets in the file described by
        ///    the current instance.
        /// </summary>
        /// <value>
        ///    A <see cref="ulong" /> value containing the number of
        ///    data packets in the file described by the current
        ///    instance.
        /// </value>
        public ulong DataPacketsCount => _dataPacketsCount;

        /// <summary>
        ///    Gets the play duration of the file described by the
        ///    current instance.
        /// </summary>
        /// <value>
        ///    A <see cref="TimeSpan" /> value containing the play
        ///    duration of the file described by the current instance.
        /// </value>
        public TimeSpan PlayDuration => new TimeSpan((long)_playDuration);

        /// <summary>
        ///    Gets the send duration of the file described by the
        ///    current instance.
        /// </summary>
        /// <value>
        ///    A <see cref="TimeSpan" /> value containing the send
        ///    duration of the file described by the current instance.
        /// </value>
        public TimeSpan SendDuration => new TimeSpan((long)_sendDuration);

        /// <summary>
        ///    Gets the pre-roll of the file described by the current
        ///    instance.
        /// </summary>
        /// <value>
        ///    A <see cref="ulong" /> value containing the pre-roll of
        ///    the file described by the current instance.
        /// </value>
        public ulong Preroll => _preroll;

        /// <summary>
        ///    Gets the flags of the file described by the current
        ///    instance.
        /// </summary>
        /// <value>
        ///    A <see cref="uint" /> value containing the flags of the
        ///    file described by the current instance.
        /// </value>
        public uint Flags => _flags;

        /// <summary>
        ///    Gets the minimum data packet size of the file described
        ///    by the current instance.
        /// </summary>
        /// <value>
        ///    A <see cref="uint" /> value containing the minimum data
        ///    packet size of the file described by the current
        ///    instance.
        /// </value>
        public uint MinimumDataPacketSize => _minimumDataPacketSize;

        /// <summary>
        ///    Gets the maximum data packet size of the file described
        ///    by the current instance.
        /// </summary>
        /// <value>
        ///    A <see cref="uint" /> value containing the maximum data
        ///    packet size of the file described by the current
        ///    instance.
        /// </value>
        public uint MaximumDataPacketSize => _maximumDataPacketSize;

        /// <summary>
        ///    Gets the maximum bitrate of the file described by the
        ///    current instance.
        /// </summary>
        /// <value>
        ///    A <see cref="uint" /> value containing the maximum
        ///    bitrate of the file described by the current instance.
        /// </value>
        public uint MaximumBitrate => _maximumBitrate;

        /// <summary>
        ///    Renders the current instance as a raw ASF object.
        /// </summary>
        /// <returns>
        ///    A <see cref="ByteVector" /> object containing the
        ///    rendered version of the current instance.
        /// </returns>
        public override ByteVector Render()
        {
            ByteVector output = _fileId.ToByteArray();
            output.Add(RenderQWord(_fileSize));
            output.Add(RenderQWord(_creationDate));
            output.Add(RenderQWord(_dataPacketsCount));
            output.Add(RenderQWord(_sendDuration));
            output.Add(RenderQWord(_playDuration));
            output.Add(RenderQWord(_preroll));
            output.Add(RenderDWord(_flags));
            output.Add(RenderDWord(_minimumDataPacketSize));
            output.Add(RenderDWord(_maximumDataPacketSize));
            output.Add(RenderDWord(_maximumBitrate));

            return Render(output);
        }
    }
}