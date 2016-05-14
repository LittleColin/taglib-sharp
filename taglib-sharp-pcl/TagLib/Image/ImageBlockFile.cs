//
// ImageBlockFile.cs: Base class for Images files which are organized
//                    which are organized as blocks.
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
using System.Collections.Generic;

namespace TagLib.Image
{
    /// <summary>
    ///    Some image file formats are organized as a sequence of mostly
    ///    independent data blocks whose order can be changed. Metadata is
    ///    stored in some of those blocks and when metadata is saved, often the
    ///    same task remains: Delete some blocks which contain metadata and
    ///    overwrite some blocks with other metadata.
    ///    This class extends <see cref="TagLib.Image.File" /> to provide this
    ///    functionality. Blocks can be marked as metadata and when metadata is
    ///    saved their space is used or they are deleted.
    /// </summary>
    public abstract class ImageBlockFile : File
    {
        /// <summary>
        ///    An odered list of the metadata blocks. The blocks do not overlap.
        /// </summary>
        private readonly List<MetadataBlock> _metadataBlocks = new List<MetadataBlock>();

        /// <summary>
        ///    Constructs and initializes a new instance for a specified
        ///    path in the local file system.
        /// </summary>
        /// <param name="path">
        ///    A <see cref="string" /> object containing the path of the
        ///    file to use in the new instance.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///    <paramref name="path" /> is <see langword="null" />.
        /// </exception>
        protected ImageBlockFile(string path) : base(path)
        {
        }

        /// <summary>
        ///    Constructs and initializes a new instance for a specified
        ///    file abstraction.
        /// </summary>
        /// <param name="abstraction">
        ///    A <see cref="IFileAbstraction" /> object to use when
        ///    reading from and writing to the file.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///    <paramref name="abstraction" /> is <see langword="null"
        ///    />.
        /// </exception>
        protected ImageBlockFile(IFileAbstraction abstraction) : base(abstraction)
        {
        }

        /// <summary>
        ///    Adds a range to be treated as metadata.
        /// </summary>
        /// <param name="start">
        ///    A <see cref="System.Int64"/> with the start index of the metadata block
        /// </param>
        /// <param name="length">
        ///    A <see cref="System.Int64"/> with the length of the metadata block
        /// </param>
        protected void AddMetadataBlock(long start, long length)
        {
            MetadataBlock newBlock = new MetadataBlock(start, length);

            // We keep the list sorted and unique. Therefore, we add the new block to
            // the list and join overlapping blocks if necessary.

            // iterate through all existing blocks.
            for (int i = 0; i < _metadataBlocks.Count; i++)
            {
                var block = _metadataBlocks[i];

                // if one block overlaps with the new one, join them.
                if (newBlock.OverlapsWith(block))
                {
                    block.Add(newBlock);

                    // Since we joined two blocks, they may overlap with
                    // other blocks which follows in the list. Therfore,
                    // we iterate through the tail of the list and join
                    // blocks which are now contained.
                    i++;
                    while (i < _metadataBlocks.Count)
                    {
                        var nextBlock = _metadataBlocks[i];

                        if (block.OverlapsWith(nextBlock))
                        {
                            block.Add(nextBlock);
                            _metadataBlocks.Remove(nextBlock);
                        }
                        else
                        {
                            return;
                        }
                    }

                    return;

                    // if the new block is 'smaller' than the one in the list,
                    // just add it to the list.
                }
                else if (newBlock.Before(block))
                {
                    _metadataBlocks.Insert(i, newBlock);
                    return;
                }
            }

            // if the new block is 'bigger' than all other blocks, at it to the end.
            _metadataBlocks.Add(newBlock);
        }

        /// <summary>
        ///    Saves the given data at the given position. All metadata blocks are
        ///    either deleted or overwritten.
        /// </summary>
        /// <param name="data">
        ///    A <see cref="ByteVector"/> with the metadata to write.
        /// </param>
        /// <param name="start">
        ///    A <see cref="System.Int64"/> with the index to save the metadata at.
        /// </param>
        protected void SaveMetadata(ByteVector data, long start)
        {
            long newStart = 0;

            // this ensures that the block with the start index is contained.
            AddMetadataBlock(start, 0);

            // start iterating through the metadata block from the end,
            // because deleting such blocks do not affect the smaller indices.
            for (int i = _metadataBlocks.Count - 1; i >= 0; i--)
            {
                var block = _metadataBlocks[i];

                // this is the block to save the metadata in
                if (block.Start <= start && block.Start + block.Length >= start)
                {
                    // the metadata is saved starting at the beginning of the block,
                    // because the bytes will be removed.
                    Insert(data, block.Start, block.Length);
                    newStart = block.Start;
                }
                else
                {
                    // remove block
                    Insert("", block.Start, block.Length);

                    // update start of the metadata block, if metadata was written
                    // before, i.e. we have removed a block which is before the saved
                    // metadata
                    if (block.Start < start)
                        newStart -= block.Length;
                }
            }

            // and reset the metadata blocks
            // (there is now just one block contained)
            _metadataBlocks.Clear();
            AddMetadataBlock(newStart, data.Count);
        }

        /// <summary>
        ///    This class represents a metadata block to overwrite.
        /// </summary>
        private class MetadataBlock
        {
            /// <summary>
            ///    Constructor
            /// </summary>
            /// <param name="start">
            ///    A <see cref="System.Int64"/> with the start of the block
            /// </param>
            /// <param name="length">
            ///    A <see cref="System.Int64"/> with the length of the block
            /// </param>
            public MetadataBlock(long start, long length)
            {
                if (start < 0)
                    throw new ArgumentOutOfRangeException(nameof(start));

                if (length < 0)
                    throw new ArgumentOutOfRangeException(nameof(length));

                Start = start;
                Length = length;
            }

            /// <summary>
            ///    Constructor. Creates a new instance with an empty block
            /// </summary>
            public MetadataBlock() : this(0, 0) { }

            /// <summary>
            ///    The length of the block
            /// </summary>
            public long Length { get; set; }

            /// <summary>
            ///    The start index
            /// </summary>
            public long Start { get; set; }

            /// <summary>
            ///    Adds the given block to the current instance, if this is possible.
            /// </summary>
            /// <param name="block">
            ///    A <see cref="MetadataBlock"/> with the block to add.
            /// </param>
            public void Add(MetadataBlock block)
            {
                if (block.Start >= Start && block.Start <= Start + Length)
                {
                    Length = Math.Max(Length, block.Start + block.Length - Start);
                    return;
                }

                if (Start >= block.Start && Start <= block.Start + block.Length)
                {
                    Length = Math.Max(block.Length, Start + Length - block.Start);
                    Start = block.Start;
                    return;
                }

                throw new ArgumentException(string.Format("blocks do not overlap: {0} and {1}", this, block));
            }

            /// <summary>
            ///    Checks, if the one block is before the other. That means,
            ///    if the current instance ends before the given block starts.
            /// </summary>
            /// <param name="block">
            ///    A <see cref="MetadataBlock"/> to compare with.
            /// </param>
            /// <returns>
            ///    A <see cref="System.Boolean"/> which is true if the current
            ///    instance is before the given block.
            /// </returns>
            public bool Before(MetadataBlock block)
            {
                return (Start + Length < block.Start);
            }

            /// <summary>
            ///    Checks if the given block overlaps with this instance.
            /// </summary>
            /// <param name="block">
            ///    A <see cref="MetadataBlock"/> with the block to check
            ///    overlapping.
            /// </param>
            /// <returns>
            ///    A <see cref="System.Boolean"/> which is true, if the given
            ///    block overlapps with the current instance.
            /// </returns>
            /// <remarks>
            ///    Overlapping means here also that blocks directly follow.
            /// </remarks>
            public bool OverlapsWith(MetadataBlock block)
            {
                if (block.Start >= Start && block.Start <= Start + Length)
                    return true;

                if (Start >= block.Start && Start <= block.Start + block.Length)
                    return true;

                return false;
            }

            /// <summary>
            ///    Provides a readable <see cref="System.String"/> for
            ///    the current instance.
            /// </summary>
            /// <returns>
            ///    A <see cref="System.String"/> representing the current
            ///    instance.
            /// </returns>
            public override string ToString()
            {
                return string.Format("[MetadataBlock: Start={0}, Length={1}]", Start, Length);
            }
        }
    }
}