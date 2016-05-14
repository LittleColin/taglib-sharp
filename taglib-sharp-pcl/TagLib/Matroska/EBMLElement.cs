//
// EBMLElement.cs:
//
// Author:
//   Julien Moutte <julien@fluendo.com>
//
// Copyright (C) 2011 FLUENDO S.A.
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

namespace TagLib.Matroska
{
    /// <summary>
    /// Describes a generic EBML Element.
    /// </summary>
    public class EbmlElement
    {
        private readonly ulong _dataOffset = 0;
        private readonly uint _ebmlId = 0;
        private readonly ulong _ebmlSize = 0;
        private readonly File _file = null;
        private readonly ulong _offset = 0;

        /// <summary>
        /// Constructs a <see cref="EbmlElement" /> parsing from provided
        /// file data.
        /// </summary>
        /// <param name="_file"><see cref="File" /> instance to read from.</param>
        /// <param name="position">Position to start reading from.</param>
        public EbmlElement(File _file, ulong position)
        {
            if (_file == null)
                throw new ArgumentNullException("file");

            if (position > (ulong)(_file.Length - 4))
                throw new ArgumentOutOfRangeException(nameof(position));

            // Keep a reference to the file
            this._file = _file;

            this._file.Seek((long)position);

            // Get the header byte
            ByteVector vector = this._file.ReadBlock(1);
            byte headerByte = vector[0];
            // Define a mask
            byte mask = 0x80, idLength = 1;
            // Figure out the size in bytes
            while (idLength <= 4 && (headerByte & mask) == 0)
            {
                idLength++;
                mask >>= 1;
            }

            if (idLength > 4)
            {
                throw new CorruptFileException("invalid EBML id size");
            }

            // Now read the rest of the EBML ID
            if (idLength > 1)
            {
                vector.Add(this._file.ReadBlock(idLength - 1));
            }

            _ebmlId = vector.ToUInt();

            vector.Clear();

            // Get the size length
            vector = this._file.ReadBlock(1);
            headerByte = vector[0];
            mask = 0x80;
            byte sizeLength = 1;

            // Iterate through various possibilities
            while (sizeLength <= 8 && (headerByte & mask) == 0)
            {
                sizeLength++;
                mask >>= 1;
            }

            if (sizeLength > 8)
            {
                throw new CorruptFileException("invalid EBML element size");
            }

            // Clear the marker bit
            vector[0] &= (byte)(mask - 1);

            // Now read the rest of the EBML element size
            if (sizeLength > 1)
            {
                vector.Add(this._file.ReadBlock(sizeLength - 1));
            }

            _ebmlSize = vector.ToULong();

            _offset = position;
            _dataOffset = _offset + idLength + sizeLength;
        }

        /// <summary>
        /// EBML Element data offset in bytes.
        /// </summary>
        public ulong DataOffset => _dataOffset;

        /// <summary>
        /// EBML Element data size in bytes.
        /// </summary>
        public ulong DataSize => _ebmlSize;

        /// <summary>
        /// EBML Element Identifier.
        /// </summary>
        public uint Id => _ebmlId;

        /// <summary>
        /// EBML Element offset in bytes.
        /// </summary>
        public ulong Offset => _offset;

        /// <summary>
        /// EBML Element size in bytes.
        /// </summary>
        public ulong Size => (_dataOffset - _offset) + _ebmlSize;

        /// <summary>
        /// Reads a boolean from EBML Element's data section.
        /// </summary>
        /// <returns>a bool containing the parsed value.</returns>
        public bool ReadBool()
        {
            if (_file == null)
            {
                return false;
            }

            _file.Seek((long)_dataOffset);

            ByteVector vector = _file.ReadBlock((int)_ebmlSize);

            if (vector.ToUInt() > 0)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Reads a bytes vector from EBML Element's data section.
        /// </summary>
        /// <returns>a <see cref="ByteVector" /> containing the parsed value.</returns>
        public ByteVector ReadBytes()
        {
            if (_file == null)
            {
                return null;
            }

            _file.Seek((long)_dataOffset);

            ByteVector vector = _file.ReadBlock((int)_ebmlSize);

            return vector;
        }

        /// <summary>
        /// Reads a double from EBML Element's data section.
        /// </summary>
        /// <returns>a double containing the parsed value.</returns>
        public double ReadDouble()
        {
            if (_file == null)
            {
                return 0;
            }

            if (_ebmlSize != 4 && _ebmlSize != 8)
            {
                throw new UnsupportedFormatException("Can not read a Double with sizes differing from 4 or 8");
            }

            _file.Seek((long)_dataOffset);

            ByteVector vector = _file.ReadBlock((int)_ebmlSize);

            double result = 0.0;

            if (_ebmlSize == 4)
            {
                result = vector.ToFloat();
            }
            else if (_ebmlSize == 8)
            {
                result = vector.ToDouble();
            }

            return result;
        }

        /// <summary>
        /// Reads a string from EBML Element's data section.
        /// </summary>
        /// <returns>a string object containing the parsed value.</returns>
        public string ReadString()
        {
            if (_file == null)
            {
                return null;
            }

            _file.Seek((long)_dataOffset);

            ByteVector vector = _file.ReadBlock((int)_ebmlSize);

            return vector.ToString();
        }

        /// <summary>
        /// Reads an unsigned 32 bits integer from EBML Element's data section.
        /// </summary>
        /// <returns>a uint containing the parsed value.</returns>
        public uint ReadUInt()
        {
            if (_file == null)
            {
                return 0;
            }

            _file.Seek((long)_dataOffset);

            ByteVector vector = _file.ReadBlock((int)_ebmlSize);

            return vector.ToUInt();
        }
    }
}