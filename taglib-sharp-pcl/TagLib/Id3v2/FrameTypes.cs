//
// FrameTypes.cs:
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

namespace TagLib.Id3v2
{
    /// <summary>
    ///    <see cref="FrameType" /> provides references to different frame
    ///    types used by the library.
    /// </summary>
    /// <remarks>
    ///    <para>This class is used to severely reduce the number of times
    ///    these types are created in <see cref="TagLib.Id3v2.Tag" />,
    ///    greatly improving the speed at which warm files are read. It is,
    ///    however, not necessary for external users to use this class. While
    ///    the library may use <c>GetTextAsString (FrameType.TIT2);</c> an
    ///    external user could use <c>tag.GetTextAsString ("TIT2");</c> with
    ///    the same result.</para>
    /// </remarks>
    internal static class FrameType
    {
        public static readonly ReadOnlyByteVector Apic = "APIC";
        public static readonly ReadOnlyByteVector Comm = "COMM";
        public static readonly ReadOnlyByteVector Equa = "EQUA";
        public static readonly ReadOnlyByteVector Geob = "GEOB";
        public static readonly ReadOnlyByteVector Mcdi = "MCDI";
        public static readonly ReadOnlyByteVector Pcnt = "PCNT";
        public static readonly ReadOnlyByteVector Popm = "POPM";
        public static readonly ReadOnlyByteVector Priv = "PRIV";
        public static readonly ReadOnlyByteVector Rva2 = "RVA2";
        public static readonly ReadOnlyByteVector Rvad = "RVAD";
        public static readonly ReadOnlyByteVector Sylt = "SYLT";
        public static readonly ReadOnlyByteVector Talb = "TALB";
        public static readonly ReadOnlyByteVector Tbpm = "TBPM";
        public static readonly ReadOnlyByteVector Tcom = "TCOM";
        public static readonly ReadOnlyByteVector Tcon = "TCON";
        public static readonly ReadOnlyByteVector Tcop = "TCOP";
        public static readonly ReadOnlyByteVector Tcmp = "TCMP";
        public static readonly ReadOnlyByteVector Tdrc = "TDRC";
        public static readonly ReadOnlyByteVector Tdat = "TDAT";
        public static readonly ReadOnlyByteVector Text = "TEXT";
        public static readonly ReadOnlyByteVector Tit1 = "TIT1";
        public static readonly ReadOnlyByteVector Tit2 = "TIT2";
        public static readonly ReadOnlyByteVector Time = "TIME";
        public static readonly ReadOnlyByteVector Toly = "TOLY";
        public static readonly ReadOnlyByteVector Tope = "TOPE";
        public static readonly ReadOnlyByteVector Tpe1 = "TPE1";
        public static readonly ReadOnlyByteVector Tpe2 = "TPE2";
        public static readonly ReadOnlyByteVector Tpe3 = "TPE3";
        public static readonly ReadOnlyByteVector Tpe4 = "TPE4";
        public static readonly ReadOnlyByteVector Tpos = "TPOS";
        public static readonly ReadOnlyByteVector Trck = "TRCK";
        public static readonly ReadOnlyByteVector Trda = "TRDA";
        public static readonly ReadOnlyByteVector Tsiz = "TSIZ";
        public static readonly ReadOnlyByteVector Tsoa = "TSOA"; // Album Title Sort Frame
        public static readonly ReadOnlyByteVector Tso2 = "TSO2"; // Album Artist Sort Frame
        public static readonly ReadOnlyByteVector Tsoc = "TSOC"; // Composer Sort Frame
        public static readonly ReadOnlyByteVector Tsop = "TSOP"; // Performer Sort Frame
        public static readonly ReadOnlyByteVector Tsot = "TSOT"; // Track Title Sort Frame
        public static readonly ReadOnlyByteVector Txxx = "TXXX";
        public static readonly ReadOnlyByteVector Tyer = "TYER";
        public static readonly ReadOnlyByteVector Ufid = "UFID";
        public static readonly ReadOnlyByteVector User = "USER";
        public static readonly ReadOnlyByteVector Uslt = "USLT";
    }
}