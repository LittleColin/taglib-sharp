//
// DescriptionRecord.cs: Provides a representation of an ASF Description Record
// to be used in combination with MetadataLibaryObject.
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
    ///    This class provides a representation of an ASF Description Record
    ///    to be used in combination with <see cref="MetadataLibraryObject"
    ///    />.
    /// </summary>
    public class DescriptionRecord
    {
        /// <summary>
        ///    Contains the data type.
        /// </summary>
        private DataType _type = DataType.Unicode;

        /// <summary>
        ///    Contains the language list index.
        /// </summary>
        private ushort _langListIndex = 0;

        /// <summary>
        ///    Contains the stream number.
        /// </summary>
        private ushort _streamNumber = 0;

        /// <summary>
        ///    Contains the record name.
        /// </summary>
        private string _name = null;

        /// <summary>
        ///    Contains the string value.
        /// </summary>
        private string _strValue = null;

        /// <summary>
        ///    Contains the byte value.
        /// </summary>
        private ByteVector _byteValue = null;

        /// <summary>
        ///    Contains the long value.
        /// </summary>
        private ulong _longValue = 0;

        /// <summary>
        ///    Contains the GUID value.
        /// </summary>
        private System.Guid _guidValue = System.Guid.Empty;

        /// <summary>
        ///    Constructs and initializes a new instance of <see
        ///    cref="DescriptionRecord" /> with a specified language,
        ///    stream, name, and value.
        /// </summary>
        /// <param name="languageListIndex">
        ///    A <see cref="ushort" /> value containing the language
        ///    list index of the new instance.
        /// </param>
        /// <param name="streamNumber">
        ///    A <see cref="ushort" /> value containing the stream
        ///    number of the new instance.
        /// </param>
        /// <param name="name">
        ///    A <see cref="string" /> object containing the name of the
        ///    new instance.
        /// </param>
        /// <param name="value">
        ///    A <see cref="string" /> object containing the value for
        ///    the new instance.
        /// </param>
        public DescriptionRecord(ushort languageListIndex,
                                  ushort streamNumber, string name,
                                  string value)
        {
            _langListIndex = languageListIndex;
            _streamNumber = streamNumber;
            _name = name;
            _strValue = value;
        }

        /// <summary>
        ///    Constructs and initializes a new instance of <see
        ///    cref="DescriptionRecord" /> with a specified language,
        ///    stream, name, and value.
        /// </summary>
        /// <param name="languageListIndex">
        ///    A <see cref="ushort" /> value containing the language
        ///    list index of the new instance.
        /// </param>
        /// <param name="streamNumber">
        ///    A <see cref="ushort" /> value containing the stream
        ///    number of the new instance.
        /// </param>
        /// <param name="name">
        ///    A <see cref="string" /> object containing the name of the
        ///    new instance.
        /// </param>
        /// <param name="value">
        ///    A <see cref="ByteVector" /> object containing the value
        ///    for the new instance.
        /// </param>
        public DescriptionRecord(ushort languageListIndex,
                                  ushort streamNumber, string name,
                                  ByteVector value)
        {
            _langListIndex = languageListIndex;
            _streamNumber = streamNumber;
            _name = name;
            _type = DataType.Bytes;
            _byteValue = new ByteVector(value);
        }

        /// <summary>
        ///    Constructs and initializes a new instance of <see
        ///    cref="DescriptionRecord" /> with a specified language,
        ///    stream, name, and value.
        /// </summary>
        /// <param name="languageListIndex">
        ///    A <see cref="ushort" /> value containing the language
        ///    list index of the new instance.
        /// </param>
        /// <param name="streamNumber">
        ///    A <see cref="ushort" /> value containing the stream
        ///    number of the new instance.
        /// </param>
        /// <param name="name">
        ///    A <see cref="string" /> object containing the name of the
        ///    new instance.
        /// </param>
        /// <param name="value">
        ///    A <see cref="uint" /> value containing the value
        ///    for the new instance.
        /// </param>
        public DescriptionRecord(ushort languageListIndex,
                                  ushort streamNumber, string name,
                                  uint value)
        {
            _langListIndex = languageListIndex;
            _streamNumber = streamNumber;
            _name = name;
            _type = DataType.DWord;
            _longValue = value;
        }

        /// <summary>
        ///    Constructs and initializes a new instance of <see
        ///    cref="DescriptionRecord" /> with a specified language,
        ///    stream, name, and value.
        /// </summary>
        /// <param name="languageListIndex">
        ///    A <see cref="ushort" /> value containing the language
        ///    list index of the new instance.
        /// </param>
        /// <param name="streamNumber">
        ///    A <see cref="ushort" /> value containing the stream
        ///    number of the new instance.
        /// </param>
        /// <param name="name">
        ///    A <see cref="string" /> object containing the name of the
        ///    new instance.
        /// </param>
        /// <param name="value">
        ///    A <see cref="ulong" /> value containing the value
        ///    for the new instance.
        /// </param>
        public DescriptionRecord(ushort languageListIndex,
                                  ushort streamNumber, string name,
                                  ulong value)
        {
            _langListIndex = languageListIndex;
            _streamNumber = streamNumber;
            _name = name;
            _type = DataType.QWord;
            _longValue = value;
        }

        /// <summary>
        ///    Constructs and initializes a new instance of <see
        ///    cref="DescriptionRecord" /> with a specified language,
        ///    stream, name, and value.
        /// </summary>
        /// <param name="languageListIndex">
        ///    A <see cref="ushort" /> value containing the language
        ///    list index of the new instance.
        /// </param>
        /// <param name="streamNumber">
        ///    A <see cref="ushort" /> value containing the stream
        ///    number of the new instance.
        /// </param>
        /// <param name="name">
        ///    A <see cref="string" /> object containing the name of the
        ///    new instance.
        /// </param>
        /// <param name="value">
        ///    A <see cref="ushort" /> value containing the value
        ///    for the new instance.
        /// </param>
        public DescriptionRecord(ushort languageListIndex,
                                  ushort streamNumber, string name,
                                  ushort value)
        {
            _langListIndex = languageListIndex;
            _streamNumber = streamNumber;
            _name = name;
            _type = DataType.Word;
            _longValue = value;
        }

        /// <summary>
        ///    Constructs and initializes a new instance of <see
        ///    cref="DescriptionRecord" /> with a specified language,
        ///    stream, name, and value.
        /// </summary>
        /// <param name="languageListIndex">
        ///    A <see cref="ushort" /> value containing the language
        ///    list index of the new instance.
        /// </param>
        /// <param name="streamNumber">
        ///    A <see cref="ushort" /> value containing the stream
        ///    number of the new instance.
        /// </param>
        /// <param name="name">
        ///    A <see cref="string" /> object containing the name of the
        ///    new instance.
        /// </param>
        /// <param name="value">
        ///    A <see cref="bool" /> value containing the value
        ///    for the new instance.
        /// </param>
        public DescriptionRecord(ushort languageListIndex,
                                  ushort streamNumber, string name,
                                  bool value)
        {
            _langListIndex = languageListIndex;
            _streamNumber = streamNumber;
            _name = name;
            _type = DataType.Bool;
            _longValue = value ? 1uL : 0;
        }

        /// <summary>
        ///    Constructs and initializes a new instance of <see
        ///    cref="DescriptionRecord" /> with a specified language,
        ///    stream, name, and value.
        /// </summary>
        /// <param name="languageListIndex">
        ///    A <see cref="ushort" /> value containing the language
        ///    list index of the new instance.
        /// </param>
        /// <param name="streamNumber">
        ///    A <see cref="ushort" /> value containing the stream
        ///    number of the new instance.
        /// </param>
        /// <param name="name">
        ///    A <see cref="string" /> object containing the name of the
        ///    new instance.
        /// </param>
        /// <param name="value">
        ///    A <see cref="System.Guid" /> value containing the value
        ///    for the new instance.
        /// </param>
        public DescriptionRecord(ushort languageListIndex,
                                  ushort streamNumber, string name,
                                  System.Guid value)
        {
            _langListIndex = languageListIndex;
            _streamNumber = streamNumber;
            _name = name;
            _type = DataType.Guid;
            _guidValue = value;
        }

        /// <summary>
        ///    Constructs and initializes a new instance of <see
        ///    cref="DescriptionRecord" /> by reading its contents from
        ///    a file.
        /// </summary>
        /// <param name="file">
        ///    A <see cref="Asf.File" /> object to read the raw ASF
        ///    Description Record from.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///    <paramref name="file" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="CorruptFileException">
        ///    A valid record could not be read.
        /// </exception>
        /// <remarks>
        ///    <paramref name="file" /> must be at a seek position at
        ///    which the record can be read.
        /// </remarks>
        protected internal DescriptionRecord(File file)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));

            if (!Parse(file))
                throw new CorruptFileException(
                    "Failed to parse description record.");
        }

        /// <summary>
        ///    Gets the index of the language associated with the
        ///    current instance.
        /// </summary>
        /// <value>
        ///    A <see cref="ushort" /> value containing the index of the
        ///    language associated with the current instance.
        /// </value>
        public ushort LanguageListIndex => _langListIndex;

        /// <summary>
        ///    Gets the index of the stream associated with the current
        ///    instance.
        /// </summary>
        /// <value>
        ///    A <see cref="ushort" /> value containing the index of the
        ///    stream associated with the current instance.
        /// </value>
        public ushort StreamNumber => _streamNumber;

        /// <summary>
        ///    Gets the name of the current instance.
        /// </summary>
        /// <value>
        ///    A <see cref="string" /> object containing the name of the
        ///    current instance.
        /// </value>
        public string Name => _name;

        /// <summary>
        ///    Gets the type of data contained in the current instance.
        /// </summary>
        /// <value>
        ///    A <see cref="DataType" /> value indicating type of data
        ///    contained in the current instance.
        /// </value>
        public DataType Type => _type;

        /// <summary>
        ///    Gets a string representation of the current instance.
        /// </summary>
        /// <returns>
        ///    A <see cref="string" /> object containing the value of
        ///    the current instance.
        /// </returns>
        public override string ToString()
        {
            if (_type == DataType.Unicode)
                return _strValue;

            if (_type == DataType.Bytes)
                return _byteValue.ToString(StringType.Utf16Le);

            return _longValue.ToString();
        }

        /// <summary>
        ///    Gets the binary contents of the current instance.
        /// </summary>
        /// <returns>
        ///    A <see cref="ByteVector" /> object containing the
        ///    contents of the current instance, or <see langword="null"
        ///    /> if <see cref="Type" /> is unequal to <see
        ///    cref="DataType.Bytes" />.
        /// </returns>
        public ByteVector ToByteVector()
        {
            return _byteValue;
        }

        /// <summary>
        ///    Gets the boolean value contained in the current instance.
        /// </summary>
        /// <returns>
        ///    A <see cref="bool" /> value containing the value of the
        ///    current instance.
        /// </returns>
        public bool ToBool()
        {
            return _longValue != 0;
        }

        /// <summary>
        ///    Gets the DWORD value contained in the current instance.
        /// </summary>
        /// <returns>
        ///    A <see cref="uint" /> value containing the value of the
        ///    current instance.
        /// </returns>
        public uint ToDWord()
        {
            uint value;
            if (_type == DataType.Unicode && _strValue != null &&
                uint.TryParse(_strValue, out value))
                return value;

            return (uint)_longValue;
        }

        /// <summary>
        ///    Gets the QWORD value contained in the current instance.
        /// </summary>
        /// <returns>
        ///    A <see cref="ulong" /> value containing the value of the
        ///    current instance.
        /// </returns>
        public ulong ToQWord()
        {
            ulong value;
            if (_type == DataType.Unicode && _strValue != null &&
                ulong.TryParse(_strValue, out value))
                return value;

            return _longValue;
        }

        /// <summary>
        ///    Gets the WORD value contained in the current instance.
        /// </summary>
        /// <returns>
        ///    A <see cref="ushort" /> value containing the value of the
        ///    current instance.
        /// </returns>
        public ushort ToWord()
        {
            ushort value;
            if (_type == DataType.Unicode && _strValue != null &&
                ushort.TryParse(_strValue, out value))
                return value;

            return (ushort)_longValue;
        }

        /// <summary>
        ///    Gets the GUID value contained in the current instance.
        /// </summary>
        /// <returns>
        ///    A <see cref="System.Guid" /> value containing the value
        ///    of the current instance.
        /// </returns>
        public System.Guid ToGuid()
        {
            return _guidValue;
        }

        /// <summary>
        ///    Renders the current instance as a raw ASF Description
        ///    Record.
        /// </summary>
        /// <returns>
        ///    A <see cref="ByteVector" /> object containing the
        ///    rendered version of the current instance.
        /// </returns>
        public ByteVector Render()
        {
            ByteVector value = null;

            switch (_type)
            {
                case DataType.Unicode:
                    value = Object.RenderUnicode(_strValue);
                    break;

                case DataType.Bytes:
                    value = _byteValue;
                    break;

                case DataType.Bool:
                case DataType.DWord:
                    value = Object.RenderDWord((uint)_longValue);
                    break;

                case DataType.QWord:
                    value = Object.RenderQWord(_longValue);
                    break;

                case DataType.Word:
                    value = Object.RenderWord((ushort)_longValue);
                    break;

                case DataType.Guid:
                    value = _guidValue.ToByteArray();
                    break;

                default:
                    return null;
            }

            ByteVector name = Object.RenderUnicode(_name);

            ByteVector output = new ByteVector();
            output.Add(Object.RenderWord(_langListIndex));
            output.Add(Object.RenderWord(_streamNumber));
            output.Add(Object.RenderWord((ushort)name.Count));
            output.Add(Object.RenderWord((ushort)_type));
            output.Add(Object.RenderDWord((uint)value.Count));
            output.Add(name);
            output.Add(value);

            return output;
        }

        /// <summary>
        ///    Populates the current instance by reading in the contents
        ///    from a file.
        /// </summary>
        /// <param name="file">
        ///    A <see cref="Asf.File" /> object to read the raw ASF
        ///    Description Record from.
        /// </param>
        /// <returns>
        ///    <see langword="true" /> if the data was read correctly.
        ///    Otherwise <see langword="false" />.
        /// </returns>
        protected bool Parse(File file)
        {
            // Field name          Field type Size (bits)
            // Language List Index WORD       16
            // Stream Number       WORD       16
            // Name Length         WORD       16
            // Data Type           WORD       16
            // Data Length         DWORD      32
            // Name                WCHAR      varies
            // Data                See below  varies

            _langListIndex = file.ReadWord();
            _streamNumber = file.ReadWord();
            ushort nameLength = file.ReadWord();
            _type = (DataType)file.ReadWord();
            int dataLength = (int)file.ReadDWord();
            _name = file.ReadUnicode(nameLength);

            switch (_type)
            {
                case DataType.Word:
                    _longValue = file.ReadWord();
                    break;

                case DataType.Bool:
                case DataType.DWord:
                    _longValue = file.ReadDWord();
                    break;

                case DataType.QWord:
                    _longValue = file.ReadQWord();
                    break;

                case DataType.Unicode:
                    _strValue = file.ReadUnicode(dataLength);
                    break;

                case DataType.Bytes:
                    _byteValue = file.ReadBlock(dataLength);
                    break;

                case DataType.Guid:
                    _guidValue = file.ReadGuid();
                    break;

                default:
                    return false;
            }

            return true;
        }
    }
}