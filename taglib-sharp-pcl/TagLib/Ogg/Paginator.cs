//
// Paginator.cs:
//
// Author:
//   Brian Nickel (brian.nickel@gmail.com)
//
// Original Source:
//   oggpage.cpp from TagLib
//
// Copyright (C) 2006-2007 Brian Nickel
// Copyright (C) 2003 Scott Wheeler (Original Implementation)
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

namespace TagLib.Ogg
{
    /// <summary>
    ///    This class accepts a sequence of pages for a single Ogg stream,
    ///    accepts changes, and produces a new sequence of pages to write to
    ///    disk.
    /// </summary>
    public class Paginator
    {
        /// <summary>
        ///    Contains the packets to paginate.
        /// </summary>
        private readonly ByteVectorCollection _packets =
            new ByteVectorCollection();

        /// <summary>
        ///    Contains the first page header.
        /// </summary>
        private PageHeader? _firstPageHeader = null;

        /// <summary>
        ///    Contains the codec to use.
        /// </summary>
        private readonly Codec _codec;

        /// <summary>
        ///    contains the number of pages read.
        /// </summary>
        private int _pagesRead = 0;

        /// <summary>
        ///    Constructs and initializes a new instance of <see
        ///    cref="Paginator" /> for a given <see cref="Codec" />
        ///    object.
        /// </summary>
        /// <param name="codec">
        ///    A <see cref="Codec"/> object to use when processing
        ///    packets.
        /// </param>
        public Paginator(Codec codec)
        {
            _codec = codec;
        }

        /// <summary>
        ///    Adds the next page to the current instance.
        /// </summary>
        /// <param name="page">
        ///    The next <see cref="Page" /> object found in the stream.
        /// </param>
        public void AddPage(Page page)
        {
            _pagesRead++;

            if (_firstPageHeader == null)
                _firstPageHeader = page.Header;

            if (page.Packets.Length == 0)
                return;

            ByteVector[] pagePackets = page.Packets;

            for (int i = 0; i < pagePackets.Length; i++)
            {
                if ((page.Header.Flags & PageFlags
                    .FirstPacketContinued) != 0 && i == 0 &&
                    _packets.Count > 0)
                    _packets[_packets.Count - 1].Add(pagePackets[0]);
                else
                    _packets.Add(pagePackets[i]);
            }
        }

        /// <summary>
        ///    Stores a Xiph comment in the codec-specific comment
        ///    packet.
        /// </summary>
        /// <param name="comment">
        ///    A <see cref="XiphComment" /> object to store in the
        ///    comment packet.
        /// </param>
        public void SetComment(XiphComment comment)
        {
            _codec.SetCommentPacket(_packets, comment);
        }

        /// <summary>
        ///    Repaginates the pages passed into the current instance to
        ///    handle changes made to the Xiph comment.
        /// </summary>
        /// <returns>
        ///    A <see cref="Page[]" /> containing the new page
        ///    collection.
        /// </returns>
        [Obsolete("Use Paginator.Paginate(out int)")]
        public Page[] Paginate()
        {
            int dummy;
            return Paginate(out dummy);
        }

        /// <summary>
        ///    Repaginates the pages passed into the current instance to
        ///    handle changes made to the Xiph comment.
        /// </summary>
        /// <param name="change">
        ///    A <see cref="int" /> value reference containing the
        ///    the difference between the number of pages returned and
        ///    the number of pages that were added to the class.
        /// </param>
        /// <returns>
        ///    A <see cref="Page[]" /> containing the new page
        ///    collection.
        /// </returns>
        public Page[] Paginate(out int change)
        {
            // Ogg Pagination: Welcome to sucksville!
            // If you don't understand this, you're not alone.
            // It is confusing as Hell.

            // TODO: Document this method, in the mean time, there
            // is always http://xiph.org/ogg/doc/framing.html

            if (_pagesRead == 0)
            {
                change = 0;
                return new Page[0];
            }

            int count = _pagesRead;
            ByteVectorCollection packets = new ByteVectorCollection(
                _packets);
            PageHeader firstHeader = (PageHeader)_firstPageHeader;
            List<Page> pages = new List<Page>();
            uint index = 0;
            bool bos = firstHeader.PageSequenceNumber == 0;

            if (bos)
            {
                pages.Add(new Page(new ByteVectorCollection(packets[0]), firstHeader));
                index++;
                packets.RemoveAt(0);
                count--;
            }

            int lacingPerPage = 0xfc;
            if (count > 0)
            {
                int totalLacingBytes = 0;

                for (int i = 0; i < packets.Count; i++)
                    totalLacingBytes += GetLacingValueLength(
                        packets, i);

                lacingPerPage = Math.Min(totalLacingBytes / count + 1, lacingPerPage);
            }

            int lacingBytesUsed = 0;
            ByteVectorCollection pagePackets = new ByteVectorCollection();
            bool firstPacketContinued = false;

            while (packets.Count > 0)
            {
                int packetBytes = GetLacingValueLength(packets, 0);
                int remaining = lacingPerPage - lacingBytesUsed;
                bool wholePacket = packetBytes <= remaining;
                if (wholePacket)
                {
                    pagePackets.Add(packets[0]);
                    lacingBytesUsed += packetBytes;
                    packets.RemoveAt(0);
                }
                else
                {
                    pagePackets.Add(packets[0].Mid(0, remaining * 0xff));
                    packets[0] = packets[0].Mid(remaining * 0xff);
                    lacingBytesUsed += remaining;
                }

                if (lacingBytesUsed == lacingPerPage)
                {
                    pages.Add(new Page(pagePackets,
                        new PageHeader(firstHeader,
                            index, firstPacketContinued ?
                            PageFlags.FirstPacketContinued :
                            PageFlags.None)));
                    pagePackets = new ByteVectorCollection();
                    lacingBytesUsed = 0;
                    index++;
                    count--;
                    firstPacketContinued = !wholePacket;
                }
            }

            if (pagePackets.Count > 0)
            {
                pages.Add(new Page(pagePackets,
                    new PageHeader(
                        firstHeader.StreamSerialNumber,
                        index, firstPacketContinued ?
                        PageFlags.FirstPacketContinued :
                        PageFlags.None)));
                index++;
                count--;
            }
            change = -count;
            return pages.ToArray();
        }

        /// <summary>
        ///    Gets the number of lacing value bytes that would be
        ///    required for a given packet.
        /// </summary>
        /// <param name="packets">
        ///    A <see cref="ByteVectorCollection" /> object containing
        ///    the packet.
        /// </param>
        /// <param name="index">
        ///    A <see cref="int" /> value containing the index of the
        ///    packet to compute.
        /// </param>
        /// <returns>
        ///    A <see cref="int" /> value containing the number of bytes
        ///    needed to store the length.
        /// </returns>
        private static int GetLacingValueLength(ByteVectorCollection packets,
                                                 int index)
        {
            int size = packets[index].Count;
            return size / 0xff + ((index + 1 < packets.Count ||
                size % 0xff > 0) ? 1 : 0);
        }
    }
}