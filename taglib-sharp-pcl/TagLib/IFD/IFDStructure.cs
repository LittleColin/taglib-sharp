//
// IFDStructure.cs: A structure resembling the logical structure of a TIFF IFD
// file. This is the same structure as used by Exif.
//
// Author:
//   Ruben Vermeersch (ruben@savanne.be)
//   Mike Gemuende (mike@gemuende.de)
//   Paul Lange (palango@gmx.de)
//
// Copyright (C) 2009 Ruben Vermeersch
// Copyright (C) 2009 Mike Gemuende
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
using System.Globalization;
using TagLib.IFD.Entries;
using TagLib.IFD.Tags;

namespace TagLib.IFD
{
    /// <summary>
    ///    This class resembles the structure of a TIFF file. It can either be a
    ///    top-level IFD, or a nested IFD (in the case of Exif).
    /// </summary>
    public class IfdStructure
    {
        /// <summary>
        ///    Contains the IFD directories in this tag.
        /// </summary>
        internal readonly List<IfdDirectory> directories = new List<IfdDirectory>();

        private static readonly string DatetimeFormat = "yyyy:MM:dd HH:mm:ss";

        /// <summary>
        ///    Gets the IFD directories contained in the current instance.
        /// </summary>
        /// <value>
        ///    An array of <see cref="IfdDirectory"/> instances.
        /// </value>
        public IfdDirectory[] Directories => directories.ToArray();

        /// <summary>
        ///    Adds an <see cref="IFdEntry"/> to the IFD, if it is not already
        ///    contained in, it fails otherwise.
        /// </summary>
        /// <param name="directory">
        ///    A <see cref="System.Int32"/> value with the directory index that
        ///    should contain the tag that will be added.
        /// </param>
        /// <param name="entry">
        ///    A <see cref="IFdEntry"/> to add to the IFD.
        /// </param>
        public void AddEntry(int directory, IFdEntry entry)
        {
            while (directory >= directories.Count)
                directories.Add(new IfdDirectory());

            directories[directory].Add(entry.Tag, entry);
        }

        /// <summary>
        ///    Checks, if a value for the given tag is contained in the IFD.
        /// </summary>
        /// <param name="directory">
        ///    A <see cref="System.Int32"/> value with the directory index that
        ///    contains the tag.
        /// </param>
        /// <param name="tag">
        ///    A <see cref="System.UInt16"/> value with the tag.
        /// </param>
        /// <returns>
        ///    A <see cref="System.Boolean"/>, which is true, if the tag is already
        ///    contained in the IFD, otherwise false.
        /// </returns>
        public bool ContainsTag(int directory, ushort tag)
        {
            if (directory >= directories.Count)
                return false;
            return directories[directory].ContainsKey(tag);
        }

        /// <summary>
        ///    Returns a <see cref="System.Nullable"/> containing the
        ///    <see cref="System.Byte"/> stored in the entry defined
        ///    by <paramref name="entryTag"/>.
        /// </summary>
        /// <param name="directory">
        ///    A <see cref="System.Int32"/> with the number of the directory
        ///    to search for the entry.
        /// </param>
        /// <param name="entryTag">
        ///    A <see cref="System.UInt16"/> with the tag of the entry
        /// </param>
        /// <returns>
        ///    A <see cref="System.Nullable"/> containing the
        ///    <see cref="System.Byte"/> stored in the entry, or
        ///    <see langword="null" /> if no such entry is contained or it
        ///    does not contain a <see cref="System.Byte"/> value.
        /// </returns>
        public byte? GetByteValue(int directory, ushort entryTag)
        {
            var entry = GetEntry(directory, entryTag);

            if (entry != null && entry is ByteIfdEntry)
                return (entry as ByteIfdEntry).Value;

            return null;
        }

        /// <summary>
        ///    Returns a <see cref="System.Nullable"/> containing the
        ///    <see cref="System.DateTime"/> stored in the entry defined
        ///    by <paramref name="entryTag"/>. The entry must be of type
        ///    <see cref="StringIfdEntry"/> and contain an datestring
        ///    according to the Exif specification.
        /// </summary>
        /// <param name="directory">
        ///    A <see cref="System.Int32"/> with the number of the directory
        ///    to search for the entry.
        /// </param>
        /// <param name="entryTag">
        ///    A <see cref="System.UInt16"/> with the tag of the entry
        /// </param>
        /// <returns>
        ///    A <see cref="System.Nullable"/> containing the
        ///    <see cref="System.DateTime"/> stored in the entry, or
        ///    <see langword="null" /> if no such entry is contained or it
        ///    does not contain a valid value.
        /// </returns>
        public DateTime? GetDateTimeValue(int directory, ushort entryTag)
        {
            string dateString = GetStringValue(directory, entryTag);

            try
            {
                DateTime dateTime = DateTime.ParseExact(dateString,
                        DatetimeFormat, CultureInfo.InvariantCulture);

                return dateTime;
            }
            catch { }

            return null;
        }

        /// <summary>
        ///   Returns the <see cref="IFdEntry"/> belonging to the given tag.
        /// </summary>
        /// <param name="directory">
        ///    A <see cref="System.Int32"/> with the directory that contains
        ///    the wanted tag.
        /// </param>
        /// <param name="tag">
        ///    A <see cref="System.UInt16"/> with the tag to get.
        /// </param>
        /// <returns>
        ///    A <see cref="IFdEntry"/> belonging to the given tag, or
        ///    null, if no such tag is contained in the IFD.
        /// </returns>
        public IFdEntry GetEntry(int directory, ushort tag)
        {
            if (!ContainsTag(directory, tag))
                return null;

            return directories[directory][tag];
        }

        /// <summary>
        ///   Returns the <see cref="IFdEntry"/> belonging to the given tag.
        /// </summary>
        /// <param name="directory">
        ///    A <see cref="System.Int32"/> with the directory that contains
        ///    the wanted tag.
        /// </param>
        /// <param name="entryTag">
        ///    A <see cref="IfdEntryTag"/> with the tag to get.
        /// </param>
        /// <returns>
        ///    A <see cref="IFdEntry"/> belonging to the given tag, or
        ///    null, if no such tag is contained in the IFD.
        /// </returns>
        public IFdEntry GetEntry(int directory, IfdEntryTag entryTag)
        {
            return GetEntry(directory, (ushort)entryTag);
        }

        /// <summary>
        ///    Returns a <see cref="System.Nullable"/> containing the
        ///    <see cref="System.UInt32"/> stored in the entry defined
        ///    by <paramref name="entryTag"/>.
        /// </summary>
        /// <param name="directory">
        ///    A <see cref="System.Int32"/> with the number of the directory
        ///    to search for the entry.
        /// </param>
        /// <param name="entryTag">
        ///    A <see cref="System.UInt16"/> with the tag of the entry
        /// </param>
        /// <returns>
        ///    A <see cref="System.Nullable"/> containing the
        ///    <see cref="System.UInt32"/> stored in the entry, or
        ///    <see langword="null" /> if no such entry is contained or it
        ///    does not contain a <see cref="System.UInt32"/> value.
        /// </returns>
        public uint? GetLongValue(int directory, ushort entryTag)
        {
            var entry = GetEntry(directory, entryTag);

            if (entry is LongIfdEntry)
                return (entry as LongIfdEntry).Value;

            if (entry is ShortIfdEntry)
                return (entry as ShortIfdEntry).Value;

            return null;
        }

        /// <summary>
        ///    Returns a <see cref="System.Nullable"/> containing the
        ///    <see cref="System.Double"/> stored in the entry defined
        ///    by <paramref name="entryTag"/>. The entry can be of type
        ///    <see cref="RationalIfdEntry"/> or
        ///    <see cref="SRationalIfdEntry"/>
        /// </summary>
        /// <param name="directory">
        ///    A <see cref="System.Int32"/> with the number of the directory
        ///    to search for the entry.
        /// </param>
        /// <param name="entryTag">
        ///    A <see cref="System.UInt16"/> with the tag of the entry
        /// </param>
        /// <returns>
        ///    A <see cref="System.Nullable"/> containing the
        ///    <see cref="System.Double"/> stored in the entry, or
        ///    <see langword="null" /> if no such entry is contained.
        /// </returns>
        public double? GetRationalValue(int directory, ushort entryTag)
        {
            var entry = GetEntry(directory, entryTag);

            if (entry is RationalIfdEntry)
                return (entry as RationalIfdEntry).Value;

            if (entry is SRationalIfdEntry)
                return (entry as SRationalIfdEntry).Value;

            return null;
        }

        /// <summary>
        ///    Returns the <see cref="System.String"/> stored in the
        ///    entry defined by <paramref name="entryTag"/>.
        /// </summary>
        /// <param name="directory">
        ///    A <see cref="System.Int32"/> with the number of the directory
        ///    to search for the entry.
        /// </param>
        /// <param name="entryTag">
        ///    A <see cref="System.UInt16"/> with the tag of the entry
        /// </param>
        /// <returns>
        ///    A <see cref="System.String"/> with the value stored in the entry
        ///    or <see langword="null" /> if no such entry is contained or it
        ///    does not contain a <see cref="System.String"/> value.
        /// </returns>
        public string GetStringValue(int directory, ushort entryTag)
        {
            var entry = GetEntry(directory, entryTag);

            if (entry != null && entry is StringIfdEntry)
                return (entry as StringIfdEntry).Value;

            return null;
        }

        /// <summary>
        ///    Removes a given tag from the IFD.
        /// </summary>
        /// <param name="directory">
        ///    A <see cref="System.Int32"/> value with the directory index that
        ///    contains the tag to remove.
        /// </param>
        /// <param name="tag">
        ///    A <see cref="System.UInt16"/> value with the tag to remove.
        /// </param>
        public void RemoveTag(int directory, ushort tag)
        {
            if (ContainsTag(directory, tag))
            {
                directories[directory].Remove(tag);
            }
        }

        /// <summary>
        ///    Removes a given tag from the IFD.
        /// </summary>
        /// <param name="directory">
        ///    A <see cref="System.Int32"/> value with the directory index that
        ///    contains the tag to remove.
        /// </param>
        /// <param name="entryTag">
        ///    A <see cref="IfdEntryTag"/> value with the tag to remove.
        /// </param>
        public void RemoveTag(int directory, IfdEntryTag entryTag)
        {
            RemoveTag(directory, (ushort)entryTag);
        }

        /// <summary>
        ///    Adds a <see cref="ByteIfdEntry"/> to the directory with tag
        ///    given by <paramref name="entryTag"/> and value given by <paramref name="value"/>
        /// </summary>
        /// <param name="directory">
        ///    A <see cref="System.Int32"/> with the number of the directory
        ///    to add the entry to.
        /// </param>
        /// <param name="entryTag">
        ///    A <see cref="System.UInt16"/> with the tag of the entry
        /// </param>
        /// <param name="value">
        ///    A <see cref="System.Byte"/> with the value to add.
        /// </param>
        public void SetByteValue(int directory, ushort entryTag, byte value)
        {
            SetEntry(directory, new ByteIfdEntry(entryTag, value));
        }

        /// <summary>
        ///    Adds a <see cref="StringIfdEntry"/> to the directory with tag
        ///    given by <paramref name="entryTag"/> and value given by <paramref name="value"/>.
        ///    The value is stored as a date string according to the Exif specification.
        /// </summary>
        /// <param name="directory">
        ///    A <see cref="System.Int32"/> with the number of the directory
        ///    to add the entry to.
        /// </param>
        /// <param name="entryTag">
        ///    A <see cref="System.UInt16"/> with the tag of the entry
        /// </param>
        /// <param name="value">
        ///    A <see cref="DateTime"/> with the value to add.
        /// </param>
        public void SetDateTimeValue(int directory, ushort entryTag, DateTime value)
        {
            string dateString = value.ToString(DatetimeFormat);

            SetStringValue(directory, entryTag, dateString);
        }

        /// <summary>
        ///    Adds an <see cref="IFdEntry"/> to the IFD. If it is already contained
        ///    in the IFD, it is overwritten.
        /// </summary>
        /// <param name="directory">
        ///    A <see cref="System.Int32"/> value with the directory index that
        ///    contains the tag that will be set.
        /// </param>
        /// <param name="entry">
        ///    A <see cref="IFdEntry"/> to add to the IFD.
        /// </param>
        public void SetEntry(int directory, IFdEntry entry)
        {
            if (ContainsTag(directory, entry.Tag))
                RemoveTag(directory, entry.Tag);

            AddEntry(directory, entry);
        }

        /// <summary>
        ///    Adds a <see cref="LongIfdEntry"/> to the directory with tag
        ///    given by <paramref name="entryTag"/> and value given by <paramref name="value"/>
        /// </summary>
        /// <param name="directory">
        ///    A <see cref="System.Int32"/> with the number of the directory
        ///    to add the entry to.
        /// </param>
        /// <param name="entryTag">
        ///    A <see cref="System.UInt16"/> with the tag of the entry
        /// </param>
        /// <param name="value">
        ///    A <see cref="System.UInt32"/> with the value to add.
        /// </param>
        public void SetLongValue(int directory, ushort entryTag, uint value)
        {
            SetEntry(directory, new LongIfdEntry(entryTag, value));
        }

        /// <summary>
        ///    Adds a <see cref="RationalIfdEntry"/> to the directory with tag
        ///    given by <paramref name="entryTag"/> and value given by <paramref name="value"/>
        /// </summary>
        /// <param name="directory">
        ///    A <see cref="System.Int32"/> with the number of the directory
        ///    to add the entry to.
        /// </param>
        /// <param name="entryTag">
        ///    A <see cref="System.UInt16"/> with the tag of the entry
        /// </param>
        /// <param name="value">
        ///    A <see cref="System.Double"/> with the value to add. It must be possible to
        ///    represent the value by a <see cref="Entries.Rational"/>.
        /// </param>
        public void SetRationalValue(int directory, ushort entryTag, double value)
        {
            if (value < 0.0d || value > uint.MaxValue)
                throw new ArgumentException("value");

            uint scale = (value >= 1.0d) ? 1 : uint.MaxValue;

            Rational rational = new Rational((uint)(scale * value), scale);

            SetEntry(directory, new RationalIfdEntry(entryTag, rational));
        }

        /// <summary>
        ///    Adds a <see cref="StringIfdEntry"/> to the directory with tag
        ///    given by <paramref name="entryTag"/> and value given by <paramref name="value"/>
        /// </summary>
        /// <param name="directory">
        ///    A <see cref="System.Int32"/> with the number of the directory
        ///    to add the entry to.
        /// </param>
        /// <param name="entryTag">
        ///    A <see cref="System.UInt16"/> with the tag of the entry
        /// </param>
        /// <param name="value">
        ///    A <see cref="System.String"/> with the value to add. If it is <see langword="null" />
        ///    an possibly already contained entry is removed for given tag.
        /// </param>
        public void SetStringValue(int directory, ushort entryTag, string value)
        {
            if (value == null)
            {
                RemoveTag(directory, entryTag);
                return;
            }

            SetEntry(directory, new StringIfdEntry(entryTag, value));
        }
    }
}