//
// Marker.cs:
//
// Author:
//   Ruben Vermeersch (ruben@savanne.be)
//   Stephane Delcroix (stephane@delcroix.org)
//
// Copyright (C) 2009 Ruben Vermeersch
// Copyright (c) 2009 Stephane Delcroix
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

namespace TagLib.Jpeg
{
    /// <summary>
    ///    This enum defines the different markers used in JPEG segments.
    ///
    ///    See CCITT Rec. T.81 (1992 E), Table B.1 (p.32)
    /// </summary>
    public enum Marker : byte
    {
        /// <summary>
        ///    Start Of Frame marker, non-differential, Huffman coding, Baseline DCT
        /// </summary>
        Sof0 = 0xc0,

        /// <summary>
        ///    Start Of Frame marker, non-differential, Huffman coding, Extended Sequential DCT
        /// </summary>
        Sof1,

        /// <summary>
        ///    Start Of Frame marker, non-differential, Huffman coding, Progressive DCT
        /// </summary>
        Sof2,

        /// <summary>
        ///    Start Of Frame marker, non-differential, Huffman coding, Lossless (sequential)
        /// </summary>
        Sof3,

        /// <summary>
        ///    Start Of Frame marker, differential, Huffman coding, Differential Sequential DCT
        /// </summary>
        Sof5 = 0xc5,

        /// <summary>
        ///    Start Of Frame marker, differential, Huffman coding, Differential Progressive DCT
        /// </summary>
        Sof6,

        /// <summary>
        ///    Start Of Frame marker, differential, Huffman coding, Differential Lossless (sequential)
        /// </summary>
        Sof7,

        /// <summary>
        ///    Reserved for JPG extensions
        /// </summary>
        Jpg,

        /// <summary>
        ///    Start Of Frame marker, non-differential, arithmetic coding, Extended Sequential DCT
        /// </summary>
        Sof9,

        /// <summary>
        ///    Start Of Frame marker, non-differential, arithmetic coding, Progressive DCT
        /// </summary>
        Sof10,

        /// <summary>
        ///    Start Of Frame marker, non-differential, arithmetic coding, Lossless (sequential)
        /// </summary>
        Sof11,

        /// <summary>
        ///    Start Of Frame marker, differential, arithmetic coding, Differential Sequential DCT
        /// </summary>
        Sof13 = 0xcd,

        /// <summary>
        ///    Start Of Frame marker, differential, arithmetic coding, Differential Progressive DCT
        /// </summary>
        Sof14,

        /// <summary>
        ///    Start Of Frame marker, differential, arithmetic coding, Differential Lossless (sequential)
        /// </summary>
        Sof15,

        /// <summary>
        ///    Define Huffman table(s)
        /// </summary>
        Dht = 0xc4,

        /// <summary>
        ///    Define arithmetic coding conditioning(s)
        /// </summary>
        Dac = 0xcc,

        //Restart interval termination with modulo 8 count "m"
        /// <summary>
        ///    Restart
        /// </summary>
        Rst0 = 0xd0,

        /// <summary>
        ///    Restart
        /// </summary>
        Rst1,

        /// <summary>
        ///    Restart
        /// </summary>
        Rst2,

        /// <summary>
        ///    Restart
        /// </summary>
        Rst3,

        /// <summary>
        ///    Restart
        /// </summary>
        Rst4,

        /// <summary>
        ///    Restart
        /// </summary>
        Rst5,

        /// <summary>
        ///    Restart
        /// </summary>
        Rst6,

        /// <summary>
        ///    Restart
        /// </summary>
        Rst7,

        /// <summary>
        ///    Start of Image
        /// </summary>
        Soi = 0xd8,

        /// <summary>
        ///    End of Image
        /// </summary>
        Eoi,

        /// <summary>
        ///    Start of scan
        /// </summary>
        Sos,

        /// <summary>
        ///    Define quantization table (s)
        /// </summary>
        Dqt,

        /// <summary>
        ///    Define number of lines
        /// </summary>
        Dnl,

        /// <summary>
        ///    Define restart interval
        /// </summary>
        Dri,

        /// <summary>
        ///    Define hierarchical progression
        /// </summary>
        Dhp,

        /// <summary>
        ///    Define reference component
        /// </summary>
        Exp,

        /// <summary>
        ///    Reserved for application segment
        /// </summary>
        App0 = 0xe0,

        /// <summary>
        ///    Reserved for application segment
        /// </summary>
        App1,

        /// <summary>
        ///    Reserved for application segment
        /// </summary>
        App2,

        /// <summary>
        ///    Reserved for application segment
        /// </summary>
        App3,

        /// <summary>
        ///    Reserved for application segment
        /// </summary>
        App4,

        /// <summary>
        ///    Reserved for application segment
        /// </summary>
        App5,

        /// <summary>
        ///    Reserved for application segment
        /// </summary>
        App6,

        /// <summary>
        ///    Reserved for application segment
        /// </summary>
        App7,

        /// <summary>
        ///    Reserved for application segment
        /// </summary>
        App8,

        /// <summary>
        ///    Reserved for application segment
        /// </summary>
        App9,

        /// <summary>
        ///    Reserved for application segment
        /// </summary>
        App10,

        /// <summary>
        ///    Reserved for application segment
        /// </summary>
        App11,

        /// <summary>
        ///    Reserved for application segment
        /// </summary>
        App12,

        /// <summary>
        ///    Reserved for application segment
        /// </summary>
        App13,

        /// <summary>
        ///    Reserved for application segment
        /// </summary>
        App14,

        /// <summary>
        ///    Reserved for application segment
        /// </summary>
        App15,

        /// <summary>
        ///    Reserved for JPEG extension
        /// </summary>
        Jpg0 = 0xf0,

        /// <summary>
        ///    Reserved for JPEG extension
        /// </summary>
        Jpg1,

        /// <summary>
        ///    Reserved for JPEG extension
        /// </summary>
        Jpg2,

        /// <summary>
        ///    Reserved for JPEG extension
        /// </summary>
        Jpg3,

        /// <summary>
        ///    Reserved for JPEG extension
        /// </summary>
        Jpg4,

        /// <summary>
        ///    Reserved for JPEG extension
        /// </summary>
        Jpg5,

        /// <summary>
        ///    Reserved for JPEG extension
        /// </summary>
        Jpg6,

        /// <summary>
        ///    Reserved for JPEG extension
        /// </summary>
        Jpg7,

        /// <summary>
        ///    Reserved for JPEG extension
        /// </summary>
        Jpg8,

        /// <summary>
        ///    Reserved for JPEG extension
        /// </summary>
        Jpg9,

        /// <summary>
        ///    Reserved for JPEG extension
        /// </summary>
        Jpg10,

        /// <summary>
        ///    Reserved for JPEG extension
        /// </summary>
        Jpg11,

        /// <summary>
        ///    Reserved for JPEG extension
        /// </summary>
        Jpg12,

        /// <summary>
        ///    Reserved for JPEG extension
        /// </summary>
        Jpg13,

        /// <summary>
        ///   Comment
        /// </summary>
        Com = 0xfe,
    }
}