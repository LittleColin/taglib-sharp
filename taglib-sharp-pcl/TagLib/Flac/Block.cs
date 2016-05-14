//
// Block.cs: Represents a Flac metadata block.
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
    ///    This class represents a Flac metadata block.
    /// </summary>
    public class Block
    {
        /// <summary>
        ///    Contains the block header.
        /// </summary>
        private BlockHeader _header;

        /// <summary>
        ///    Contains the block data.
        /// </summary>
        private readonly ByteVector _data;

        /// <summary>
        ///    Constructs and initializes a new instance of <see
        ///    cref="Block" /> with a specified header and internal
        ///    data.
        /// </summary>
        /// <param name="header">
        ///    A <see cref="BlockHeader" /> object containing the
        ///    header to use for the new instance.
        /// </param>
        /// <param name="data">
        ///    A <see cref="ByteVector" /> object containing the data
        ///    to be contained in the new instance.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///    <paramref name="data" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="CorruptFileException">
        ///    The size of <paramref name="data" /> does not match the
        ///    size specified in <paramref name="header" />.
        /// </exception>
        public Block(BlockHeader header, ByteVector data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            if (header.BlockSize != data.Count)
                throw new CorruptFileException(
                    "Data count not equal to block size.");

            _header = header;
            _data = data;
        }

        /// <summary>
        ///    Constructs and initializes a new instance of <see
        ///    cref="Block" /> with of a specified type and internal
        ///    data.
        /// </summary>
        /// <param name="type">
        ///    A <see cref="BlockType" /> value indicating the type of
        ///    data stored in <paramref name="data" />.
        /// </param>
        /// <param name="data">
        ///    A <see cref="ByteVector" /> object containing the data
        ///    to be contained in the new instance.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///    <paramref name="data" /> is <see langword="null" />.
        /// </exception>
        public Block(BlockType type, ByteVector data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            _header = new BlockHeader(type, (uint)data.Count);

            _data = data;
        }

        /// <summary>
        ///    Gets the type of data contained in the current instance.
        /// </summary>
        /// <value>
        ///    A <see cref="BlockType" /> value indicating the type of
        ///    data contained in <see cref="Data" />.
        /// </value>
        public BlockType Type => _header.BlockType;

        /// <summary>
        ///    Gets whether or not the block represented by the current
        ///    instance is the last metadata block in the Flac stream.
        /// </summary>
        /// <value>
        ///    <see langword="true" /> if the block represented by the
        ///    current instance was the last one to appear in the file
        ///    and is followed immediately by the audio data, or <see
        ///    langword="false" /> if another block appears after the
        ///    current one or the block was not read from disk.
        /// </value>
        public bool IsLastBlock => _header.IsLastBlock;

        /// <summary>
        ///    Gets the size of the data contained in the current
        ///    instance.
        /// </summary>
        public uint DataSize => _header.BlockSize;

        /// <summary>
        ///    Gets the total size of the block represented by the
        ///    current instance as it appears on disk.
        /// </summary>
        public uint TotalSize => DataSize + BlockHeader.Size;

        /// <summary>
        ///    Gets the data contained in the current instance.
        /// </summary>
        /// <value>
        ///    A <see cref="ByteVector" /> object containing the data
        ///    stored in the current instance.
        /// </value>
        public ByteVector Data => _data;

        /// <summary>
        ///    Renders the current instance as a raw Flac metadata
        ///    block.
        /// </summary>
        /// <param name="isLastBlock">
        ///    A <see cref="bool" /> value indicating whether or not the
        ///    block is to be marked as the last metadata block.
        /// </param>
        /// <returns>
        ///    A <see cref="ByteVector" /> object containing the
        ///    rendered version of the current instance.
        /// </returns>
        public ByteVector Render(bool isLastBlock)
        {
            if (_data == null)
                throw new InvalidOperationException(
                    "Cannot render empty blocks.");

            ByteVector data = _header.Render(isLastBlock);
            data.Add(_data);
            return data;
        }
    }
}