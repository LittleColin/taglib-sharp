//
// File.cs: Provides tagging for PNG files
//
// Author:
//   Mike Gemuende (mike@gemuende.be)
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
using System.IO;
using System.IO.Compression;
using TagLib.Image;

namespace TagLib.Png
{
    /// <summary>
    ///    This class extends <see cref="TagLib.Image.ImageBlockFile" /> to provide tagging
    ///    for PNG image files.
    /// </summary>
    /// <remarks>
    ///    This implementation is based on http://www.w3.org/TR/PNG
    /// </remarks>
    [SupportedMimeType("taglib/png", "png")]
    [SupportedMimeType("image/png")]
    public class File : ImageBlockFile
    {
        /// <summary>
        ///    Table for faster computation of CRC.
        /// </summary>
        private static uint[] _crcTable;

        /// <summary>
        ///    The PNG Header every png file starts with.
        /// </summary>
        private readonly byte[] _header = new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 };

        /// <summary>
        ///    byte sequence to indicate a IEND Chunk
        /// </summary>
        private readonly byte[] _iendChunkType = new byte[] { 73, 69, 78, 68 };

        /// <summary>
        ///    byte sequence to indicate a IHDR Chunk
        /// </summary>
        private readonly byte[] _ihdrChunkType = new byte[] { 73, 72, 68, 82 };

        /// <summary>
        ///    byte sequence to indicate a iTXt Chunk
        /// </summary>
        private readonly byte[] _iTXtChunkType = new byte[] { 105, 84, 88, 116 };

        /// <summary>
        ///    byte sequence to indicate a tEXt Chunk
        /// </summary>
        private readonly byte[] _tEXtChunkType = new byte[] { 116, 69, 88, 116 };

        /// <summary>
        ///    header of a iTXt which contains XMP data.
        /// </summary>
        private readonly byte[] _xmpChunkHeader = new byte[] {
			// Keyword ("XML:com.adobe.xmp")
			0x58, 0x4D, 0x4C, 0x3A, 0x63, 0x6F, 0x6D, 0x2E, 0x61, 0x64, 0x6F, 0x62, 0x65, 0x2E, 0x78, 0x6D, 0x70,

			// Null Separator
			0x00,

			// Compression Flag
			0x00,

			// Compression Method
			0x00,

			// Language Tag Null Separator
			0x00,

			// Translated Keyword Null Separator
			0x00
        };

        /// <summary>
        ///    byte sequence to indicate a zTXt Chunk
        /// </summary>
        private readonly byte[] _zTXtChunkType = new byte[] { 122, 84, 88, 116 };

        /// <summary>
        ///    The height of the image
        /// </summary>
        private int _height;

        /// <summary>
        ///    The Properties of the image
        /// </summary>
        private Properties _properties;

        /// <summary>
        ///    The width of the image
        /// </summary>
        private int _width;

        /// <summary>
        ///    Constructs and initializes a new instance of <see
        ///    cref="File" /> for a specified path in the local file
        ///    system and specified read style.
        /// </summary>
        /// <param name="path">
        ///    A <see cref="string" /> object containing the path of the
        ///    file to use in the new instance.
        /// </param>
        /// <param name="propertiesStyle">
        ///    A <see cref="ReadStyle" /> value specifying at what level
        ///    of accuracy to read the media properties, or <see
        ///    cref="ReadStyle.None" /> to ignore the properties.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///    <paramref name="path" /> is <see langword="null" />.
        /// </exception>
        public File(string path, ReadStyle propertiesStyle)
            : this(new LocalFileAbstraction(path),
                propertiesStyle)
        {
        }

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
        public File(string path) : this(path, ReadStyle.Average)
        {
        }

        /// <summary>
        ///    Constructs and initializes a new instance of <see
        ///    cref="File" /> for a specified file abstraction and
        ///    specified read style.
        /// </summary>
        /// <param name="abstraction">
        ///    A <see cref="IFileAbstraction" /> object to use when
        ///    reading from and writing to the file.
        /// </param>
        /// <param name="propertiesStyle">
        ///    A <see cref="ReadStyle" /> value specifying at what level
        ///    of accuracy to read the media properties, or <see
        ///    cref="ReadStyle.None" /> to ignore the properties.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///    <paramref name="abstraction" /> is <see langword="null"
        ///    />.
        /// </exception>
        public File(IFileAbstraction abstraction,
                     ReadStyle propertiesStyle) : base(abstraction)
        {
            Read(propertiesStyle);
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
        protected File(IFileAbstraction abstraction)
            : this(abstraction, ReadStyle.Average)
        {
        }

        /// <summary>
        ///    Gets the media properties of the file represented by the
        ///    current instance.
        /// </summary>
        /// <value>
        ///    A <see cref="TagLib.Properties" /> object containing the
        ///    media properties of the file represented by the current
        ///    instance.
        /// </value>
        public override Properties Properties => _properties;

        /// <summary>
        ///    Saves the changes made in the current instance to the
        ///    file it represents.
        /// </summary>
        public override void Save()
        {
            Mode = AccessMode.Write;
            try
            {
                SaveMetadata();

                TagTypesOnDisk = TagTypes;
            }
            finally
            {
                Mode = AccessMode.Closed;
            }
        }

        /// <summary>
        ///    Initializes the CRC Table.
        /// </summary>
        private static void BuildCrcTable()
        {
            uint polynom = 0xEDB88320;

            _crcTable = new uint[256];

            for (int i = 0; i < 256; i++)
            {
                uint c = (uint)i;
                for (int k = 0; k < 8; k++)
                {
                    if ((c & 0x00000001) != 0x00)
                        c = polynom ^ (c >> 1);
                    else
                        c = c >> 1;
                }
                _crcTable[i] = c;
            }
        }

        /// <summary>
        ///    Checks the CRC for a Chunk.
        /// </summary>
        /// <param name="chunkType">
        ///    A <see cref="ByteVector"/> whith the Chunk type
        /// </param>
        /// <param name="chunkData">
        ///    A <see cref="ByteVector"/> with the Chunk data.
        /// </param>
        /// <param name="crcData">
        ///    A <see cref="ByteVector"/> with the read CRC data.
        /// </param>
        private static void CheckCrc(ByteVector chunkType, ByteVector chunkData, ByteVector crcData)
        {
            ByteVector computedCrc = ComputeCrc(chunkType, chunkData);

            if (computedCrc != crcData)
                throw new CorruptFileException(
                    string.Format("CRC check failed for {0} Chunk (expected: 0x{1:X4}, read: 0x{2:X4}",
                                   chunkType.ToString(), computedCrc.ToUInt(), crcData.ToUInt()));
        }

        /// <summary>
        ///    Computes a 32bit CRC for the given data.
        /// </summary>
        /// <param name="datas">
        ///    A <see cref="ByteVector[]"/> with data to compute
        ///    the CRC for.
        /// </param>
        /// <returns>
        ///    A <see cref="ByteVector"/> with 4 bytes (32bit) containing the CRC.
        /// </returns>
        private static ByteVector ComputeCrc(params ByteVector[] datas)
        {
            uint crc = 0xFFFFFFFF;

            if (_crcTable == null)
                BuildCrcTable();

            foreach (var data in datas)
            {
                foreach (byte b in data)
                {
                    crc = _crcTable[(crc ^ b) & 0xFF] ^ (crc >> 8);
                }
            }

            // Invert
            return ByteVector.FromUInt(crc ^ 0xFFFFFFFF);
        }

        private static ByteVector Decompress(byte compressionMethod, ByteVector compressedData)
        {
            // there is currently just one compression method specified
            // for PNG.
            switch (compressionMethod)
            {
                case 0:
                    return Inflate(compressedData);

                default:
                    return null;
            }
        }

        private static ByteVector Inflate(ByteVector data)
        {
            using (MemoryStream outStream = new MemoryStream())
            using (var input = new MemoryStream(data.Data))
            {
                input.Seek(2, SeekOrigin.Begin); // First 2 bytes are properties deflate does not need (or handle)
                using (var zipstream = new DeflateStream(input, CompressionMode.Decompress))
                {
                    //zipstream.CopyTo (out_stream); Cleaner with .NET 4
                    byte[] buffer = new byte[1024];
                    int writtenBytes;

                    while ((writtenBytes = zipstream.Read(buffer, 0, 1024)) > 0)
                        outStream.Write(buffer, 0, writtenBytes);

                    return new ByteVector(outStream.ToArray());
                }
            }
        }

        /// <summary>
        ///    Attempts to extract the media properties of the main
        ///    photo.
        /// </summary>
        /// <returns>
        ///    A <see cref="Properties" /> object with a best effort guess
        ///    at the right values. When no guess at all can be made,
        ///    <see langword="null" /> is returned.
        /// </returns>
        private Properties ExtractProperties()
        {
            if (_width > 0 && _height > 0)
                return new Properties(TimeSpan.Zero, new Codec(_width, _height));

            return null;
        }

        /// <summary>
        ///    Reads the information from file with a specified read style.
        /// </summary>
        /// <param name="propertiesStyle">
        ///    A <see cref="ReadStyle" /> value specifying at what level
        ///    of accuracy to read the media properties, or <see
        ///    cref="ReadStyle.None" /> to ignore the properties.
        /// </param>
        private void Read(ReadStyle propertiesStyle)
        {
            Mode = AccessMode.Read;
            try
            {
                ImageTag = new CombinedImageTag(TagTypes.Xmp | TagTypes.Png);

                ValidateHeader();
                ReadMetadata();

                TagTypesOnDisk = TagTypes;

                if (propertiesStyle != ReadStyle.None)
                    _properties = ExtractProperties();
            }
            finally
            {
                Mode = AccessMode.Closed;
            }
        }

        /// <summary>
        ///    Reads the whole Chunk data starting from the current position.
        /// </summary>
        /// <param name="dataLength">
        ///    A <see cref="System.Int32"/> with the length of the Chunk Data.
        /// </param>
        /// <returns>
        ///    A <see cref="ByteVector"/> with the Chunk Data which is read.
        /// </returns>
        private ByteVector ReadChunkData(int dataLength)
        {
            ByteVector data = ReadBlock(dataLength);

            if (data.Count != dataLength)
                throw new CorruptFileException(string.Format("Chunk Data of Length {0} expected", dataLength));

            return data;
        }

        /// <summary>
        ///    Reads the length of data of a chunk from the current position
        /// </summary>
        /// <returns>
        ///    A <see cref="System.Int32"/> with the length of data.
        /// </returns>
        /// <remarks>
        ///    The length is stored in a 4-byte unsigned integer in the file,
        ///    but due to the PNG specification this value does not exceed
        ///    2^31-1 and can therfore be safely returned as an signed integer.
        ///    This prevents unsafe casts for using the length as parameter
        ///    for other methods.
        /// </remarks>
        private int ReadChunkLength()
        {
            ByteVector data = ReadBlock(4);

            if (data.Count != 4)
                throw new CorruptFileException("Unexpected end of Chunk Length");

            uint length = data.ToUInt(true);

            if (length > int.MaxValue)
                throw new CorruptFileException("PNG limits the Chunk Length to 2^31-1");

            return (int)length;
        }

        /// <summary>
        ///    Reads the type of a chunk from the current position.
        /// </summary>
        /// <returns>
        ///    A <see cref="ByteVector"/> with 4 bytes containing the type of
        ///    the Chunk.
        /// </returns>
        private ByteVector ReadChunkType()
        {
            ByteVector data = ReadBlock(4);

            if (data.Count != 4)
                throw new CorruptFileException("Unexpected end of Chunk Type");

            return data;
        }

        /// <summary>
        ///    Reads the CRC value for a chunk from the current position.
        /// </summary>
        /// <returns>
        ///    A <see cref="ByteVector"/> with 4 bytes with the CRC value.
        /// </returns>
        private ByteVector ReadCrc()
        {
            ByteVector data = ReadBlock(4);

            if (data.Count != 4)
                throw new CorruptFileException("Unexpected end of CRC");

            return data;
        }

        /// <summary>
        ///    Reads the IHDR Chunk from file and extracts some image information
        ///    like width and height. The current position must be set to the start
        ///    of the Chunk Data.
        /// </summary>
        /// <param name="dataLength">
        ///     A <see cref="System.Int32"/> with the length of the Chunk Data.
        /// </param>
        private void ReadIhdrChunk(int dataLength)
        {
            // IHDR Chunk
            //
            // 4 Bytes     Width
            // 4 Bytes     Height
            // 1 Byte      Bit depth
            // 1 Byte      Colour type
            // 1 Byte      Compression method
            // 1 Byte      Filter method
            // 1 Byte      Interlace method
            //
            // Followed by 4 Bytes CRC data

            if (dataLength != 13)
                throw new CorruptFileException("IHDR chunk data length must be 13");

            ByteVector data = ReadChunkData(dataLength);

            CheckCrc(_ihdrChunkType, data, ReadCrc());

            // The PNG specification limits the size of 4-byte unsigned integers to 2^31-1.
            // That allows us to safely cast them to an signed integer.
            uint width = data.Mid(0, 4).ToUInt(true);
            uint height = data.Mid(4, 4).ToUInt(true);

            if (width > int.MaxValue || height > int.MaxValue)
                throw new CorruptFileException("PNG limits width and heigth to 2^31-1");

            _width = (int)width;
            _height = (int)height;
        }

        /// <summary>
        ///    Reads an iTXt Chunk from file. The current position must be set
        ///    to the start of the Chunk Data. Such a Chunk may contain XMP data
        ///    or translated keywords.
        /// </summary>
        /// <param name="dataLength">
        ///    A <see cref="System.Int32"/> with the length of the Chunk Data.
        /// </param>
        private void ReadiTXtChunk(int dataLength)
        {
            long position = Tell;

            // iTXt Chunk
            //
            // N Bytes     Keyword
            // 1 Byte      Null Separator
            // 1 Byte      Compression Flag (0 for uncompressed data)
            // 1 Byte      Compression Method
            // N Bytes     Language Tag
            // 1 Byte      Null Separator
            // N Bytes     Translated Keyword
            // 1 Byte      Null Terminator
            // N Bytes     Txt
            //
            // Followed by 4 Bytes CRC data

            ByteVector data = ReadChunkData(dataLength);

            CheckCrc(_iTXtChunkType, data, ReadCrc());

            int terminatorIndex;
            string keyword = ReadKeyword(data, 0, out terminatorIndex);

            if (terminatorIndex + 2 >= dataLength)
                throw new CorruptFileException("Compression Flag and Compression Method byte expected");

            byte compressionFlag = data[terminatorIndex + 1];
            byte compressionMethod = data[terminatorIndex + 2];

            //string language = ReadTerminatedString (data, terminator_index + 3, out terminator_index);
            //string translated_keyword = ReadTerminatedString (data, terminator_index + 1, out terminator_index);

            ByteVector txtData = data.Mid(terminatorIndex + 1);

            if (compressionFlag != 0x00)
            {
                txtData = Decompress(compressionMethod, txtData);

                // ignore unknown compression methods
                if (txtData == null)
                    return;
            }

            string value = txtData.ToString();
            PngTag pngTag = GetTag(TagTypes.Png, true) as PngTag;

            if (pngTag.GetKeyword(keyword) == null)
                pngTag.SetKeyword(keyword, value);

            AddMetadataBlock(position - 8, dataLength + 8 + 4);
        }

        /// <summary>
        ///    Reads a null terminated keyword from he given data from given position.
        /// </summary>
        /// <param name="data">
        ///    A <see cref="ByteVector"/> with teh data to read the string from
        /// </param>
        /// <param name="startIndex">
        ///    A <see cref="System.Int32"/> with the index to start reading
        /// </param>
        /// <param name="terminatorIndex">
        ///    A <see cref="System.Int32"/> with the index of the null byte
        /// </param>
        /// <returns>
        ///    A <see cref="System.String"/> with the read keyword. The null byte
        ///    is not included.
        /// </returns>
        private string ReadKeyword(ByteVector data, int startIndex, out int terminatorIndex)
        {
            string keyword = ReadTerminatedString(data, startIndex, out terminatorIndex);

            if (string.IsNullOrEmpty(keyword))
                throw new CorruptFileException("Keyword cannot be empty");

            return keyword;
        }

        /// <summary>
        ///    Reads the whole metadata from file. The current position must be set to
        ///    the first Chunk which is contained in the file.
        /// </summary>
        private void ReadMetadata()
        {
            int dataLength = ReadChunkLength();
            ByteVector type = ReadChunkType();

            // File should start with a header chunk
            if (!type.StartsWith(_ihdrChunkType))
                throw new CorruptFileException(
                    string.Format("IHDR Chunk was expected, but Chunk {0} was found", type.ToString()));

            ReadIhdrChunk(dataLength);

            // Read all following chunks
            while (true)
            {
                dataLength = ReadChunkLength();
                type = ReadChunkType();

                if (type.StartsWith(_iendChunkType))
                    return;
                else if (type.StartsWith(_iTXtChunkType))
                    ReadiTXtChunk(dataLength);
                else if (type.StartsWith(_tEXtChunkType))
                    ReadtEXtChunk(dataLength);
                else if (type.StartsWith(_zTXtChunkType))
                    ReadzTXtChunk(dataLength);
                else
                    SkipChunkData(dataLength);
            }
        }

        /// <summary>
        ///    Reads a null terminated string from the given data from given position.
        /// </summary>
        /// <param name="data">
        ///    A <see cref="ByteVector"/> with teh data to read the string from
        /// </param>
        /// <param name="startIndex">
        ///    A <see cref="System.Int32"/> with the index to start reading
        /// </param>
        /// <param name="terminatorIndex">
        ///    A <see cref="System.Int32"/> with the index of the null byte
        /// </param>
        /// <returns>
        ///    A <see cref="System.String"/> with the read string. The null byte
        ///    is not included.
        /// </returns>
        private string ReadTerminatedString(ByteVector data, int startIndex, out int terminatorIndex)
        {
            if (startIndex >= data.Count)
                throw new CorruptFileException("Unexpected End of Data");

            terminatorIndex = data.Find("\0", startIndex);

            if (terminatorIndex < 0)
                throw new CorruptFileException("Cannot find string terminator");

            return data.Mid(startIndex, terminatorIndex - startIndex).ToString();
        }

        /// <summary>
        ///    Reads an tEXt Chunk from file. The current position must be set
        ///    to the start of the Chunk Data. Such a Chunk contains plain
        ///    keywords.
        /// </summary>
        /// <param name="dataLength">
        ///    A <see cref="System.Int32"/> with the length of the Chunk Data.
        /// </param>
        private void ReadtEXtChunk(int dataLength)
        {
            long position = Tell;

            // tEXt Chunk
            //
            // N Bytes     Keyword
            // 1 Byte      Null Separator
            // N Bytes     Txt
            //
            // Followed by 4 Bytes CRC data

            ByteVector data = ReadChunkData(dataLength);

            CheckCrc(_tEXtChunkType, data, ReadCrc());

            int keywordTerminator;
            string keyword = ReadKeyword(data, 0, out keywordTerminator);

            string value = data.Mid(keywordTerminator + 1).ToString();

            PngTag pngTag = GetTag(TagTypes.Png, true) as PngTag;

            if (pngTag.GetKeyword(keyword) == null)
                pngTag.SetKeyword(keyword, value);

            AddMetadataBlock(position - 8, dataLength + 8 + 4);
        }

        /// <summary>
        ///    Reads an zTXt Chunk from file. The current position must be set
        ///    to the start of the Chunk Data. Such a Chunk contains compressed
        ///    keywords.
        /// </summary>
        /// <param name="dataLength">
        ///    A <see cref="System.Int32"/> with the length of the Chunk Data.
        /// </param>
        /// <remarks>
        ///    The Chunk may also contain compressed Exif data which is written
        ///    by other tools. But, since the PNG specification does not support
        ///    Exif data, we ignore it here.
        /// </remarks>
        private void ReadzTXtChunk(int dataLength)
        {
            long position = Tell;

            // zTXt Chunk
            //
            // N Bytes     Keyword
            // 1 Byte      Null Separator
            // 1 Byte      Compression Method
            // N Bytes     Txt
            //
            // Followed by 4 Bytes CRC data

            ByteVector data = ReadChunkData(dataLength);

            CheckCrc(_zTXtChunkType, data, ReadCrc());

            int terminatorIndex;
            string keyword = ReadKeyword(data, 0, out terminatorIndex);

            if (terminatorIndex + 1 >= dataLength)
                throw new CorruptFileException("Compression Method byte expected");

            byte compressionMethod = data[terminatorIndex + 1];

            ByteVector plainData = Decompress(compressionMethod, data.Mid(terminatorIndex + 2));

            // ignore unknown compression methods
            if (plainData == null)
                return;

            string value = plainData.ToString();

            PngTag pngTag = GetTag(TagTypes.Png, true) as PngTag;

            if (pngTag.GetKeyword(keyword) == null)
                pngTag.SetKeyword(keyword, value);

            AddMetadataBlock(position - 8, dataLength + 8 + 4);
        }

        /// <summary>
        ///    Creates a list of Chunks containing the PNG keywords
        /// </summary>
        /// <returns>
        ///    A <see cref="ByteVector"/> with the list of chunks, or
        ///    or <see langword="null" /> if no PNG Keywords are contained.
        /// </returns>
        private ByteVector RenderKeywordChunks()
        {
            // Check, if PngTag is contained
            PngTag pngTag = GetTag(TagTypes.Png, true) as PngTag;
            if (pngTag == null)
                return null;

            ByteVector chunks = new ByteVector();

            foreach (KeyValuePair<string, string> keyword in pngTag)
            {
                ByteVector data = new ByteVector();
                data.Add(keyword.Key);
                data.Add("\0");
                data.Add(keyword.Value);

                chunks.Add(ByteVector.FromUInt((uint)data.Count));
                chunks.Add(_tEXtChunkType);
                chunks.Add(data);
                chunks.Add(ComputeCrc(_tEXtChunkType, data));
            }

            return chunks;
        }

        /// <summary>
        ///    Save the metadata to file.
        /// </summary>
        private void SaveMetadata()
        {
            ByteVector metadataChunks = new ByteVector();

            metadataChunks.Add(RenderKeywordChunks());

            // Metadata is stored after the PNG header and the IDHR chunk.
            SaveMetadata(metadataChunks, _header.Length + 13 + 4 + 4 + 4);
        }

        /// <summary>
        ///    Skips the Chunk Data and CRC Data. The read position must be at the
        ///    beginning of the Chunk data.
        /// </summary>
        /// <param name="dataSize">
        ///    A <see cref="System.Int32"/> with the length of the chunk data read
        ///    before.
        /// </param>
        private void SkipChunkData(int dataSize)
        {
            long position = Tell;

            if (position + dataSize >= Length)
                throw new CorruptFileException(string.Format("Chunk Data of Length {0} expected", dataSize));

            Seek(Tell + dataSize);
            ReadCrc();
        }

        /// <summary>
        ///    Validates the header of a PNG file. Therfore, the current position to
        ///    read must be the start of the file.
        /// </summary>
        private void ValidateHeader()
        {
            ByteVector data = ReadBlock(8);

            if (data.Count != 8)
                throw new CorruptFileException("Unexpected end of header");

            if (!data.Equals(new ByteVector(_header)))
                throw new CorruptFileException("PNG Header was expected");
        }
    }
}