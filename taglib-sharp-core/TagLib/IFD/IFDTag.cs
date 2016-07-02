//
// IFDTag.cs: Basic Tag-class to handle an IFD (Image File Directory) with
// its image-tags.
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
using TagLib.IFD.Entries;
using TagLib.IFD.Tags;
using TagLib.Image;

namespace TagLib.IFD
{
    /// <summary>
    ///    Contains the metadata for one IFD (Image File Directory).
    /// </summary>
    public class IfdTag : ImageTag
    {
        /// <summary>
        ///    A reference to the Exif IFD (which can be found by following the
        ///    pointer in IFD0, ExifIFD tag). This variable should not be used
        ///    directly, use the <see cref="ExifIfd"/> property instead.
        /// </summary>
        private IfdStructure _exifIfd = null;

        /// <summary>
        ///    A reference to the GPS IFD (which can be found by following the
        ///    pointer in IFD0, GPSIFD tag). This variable should not be used
        ///    directly, use the <see cref="Gpsifd"/> property instead.
        /// </summary>
        private IfdStructure _gpsIfd = null;

        /// <value>
        ///    The IFD structure referenced by the current instance
        /// </value>
        public IfdStructure Structure { get; }

        /// <summary>
        ///    The Exif IFD. Will create one if the file doesn't alread have it.
        /// </summary>
        /// <remarks>
        ///    <para>Note how this also creates an empty IFD for exif, even if
        ///    you don't set a value. That's okay, empty nested IFDs get ignored
        ///    when rendering.</para>
        /// </remarks>
        public IfdStructure ExifIfd
        {
            get
            {
                if (_exifIfd == null)
                {
                    var entry = Structure.GetEntry(0, IfdEntryTag.ExifIfd) as SubIfdEntry;
                    if (entry == null)
                    {
                        _exifIfd = new IfdStructure();
                        entry = new SubIfdEntry((ushort)IfdEntryTag.ExifIfd, (ushort)IfdEntryType.Long, 1, _exifIfd);
                        Structure.SetEntry(0, entry);
                    }

                    _exifIfd = entry.Structure;
                }

                return _exifIfd;
            }
        }

        /// <summary>
        ///    The GPS IFD. Will create one if the file doesn't alread have it.
        /// </summary>
        /// <remarks>
        ///    <para>Note how this also creates an empty IFD for GPS, even if
        ///    you don't set a value. That's okay, empty nested IFDs get ignored
        ///    when rendering.</para>
        /// </remarks>
        public IfdStructure Gpsifd
        {
            get
            {
                if (_gpsIfd == null)
                {
                    var entry = Structure.GetEntry(0, IfdEntryTag.Gpsifd) as SubIfdEntry;
                    if (entry == null)
                    {
                        _gpsIfd = new IfdStructure();
                        entry = new SubIfdEntry((ushort)IfdEntryTag.Gpsifd, (ushort)IfdEntryType.Long, 1, _gpsIfd);
                        Structure.SetEntry(0, entry);
                    }

                    _gpsIfd = entry.Structure;
                }

                return _gpsIfd;
            }
        }

        /// <summary>
        ///    Gets the tag types contained in the current instance.
        /// </summary>
        /// <value>
        ///    Always <see cref="TagTypes.TiffIFD" />.
        /// </value>
        public override TagTypes TagTypes => TagTypes.TiffIfd;

        /// <summary>
        ///    Constructor. Creates an empty IFD tag. Can be populated manually, or via
        ///    <see cref="IfdReader"/>.
        /// </summary>
        public IfdTag()
        {
            Structure = new IfdStructure();
        }

        /// <summary>
        ///    Clears the values stored in the current instance.
        /// </summary>
        public override void Clear()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///    Gets or sets the comment for the image described
        ///    by the current instance.
        /// </summary>
        /// <value>
        ///    A <see cref="string" /> containing the comment of the
        ///    current instace.
        /// </value>
        public override string Comment
        {
            get
            {
                var commentEntry = ExifIfd.GetEntry(0, (ushort)ExifEntryTag.UserComment) as UserCommentIfdEntry;

                if (commentEntry == null)
                {
                    var description = Structure.GetEntry(0, IfdEntryTag.ImageDescription) as StringIfdEntry;
                    return description == null ? null : description.Value;
                }

                return commentEntry.Value;
            }
            set
            {
                if (value == null)
                {
                    ExifIfd.RemoveTag(0, (ushort)ExifEntryTag.UserComment);
                    Structure.RemoveTag(0, (ushort)IfdEntryTag.ImageDescription);
                    return;
                }

                ExifIfd.SetEntry(0, new UserCommentIfdEntry((ushort)ExifEntryTag.UserComment, value));
                Structure.SetEntry(0, new StringIfdEntry((ushort)IfdEntryTag.ImageDescription, value));
            }
        }

        /// <summary>
        ///    Gets and sets the copyright information for the media
        ///    represented by the current instance.
        /// </summary>
        /// <value>
        ///    A <see cref="string" /> object containing the copyright
        ///    information for the media represented by the current
        ///    instance or <see langword="null" /> if no value present.
        /// </value>
        public override string Copyright
        {
            get
            {
                return Structure.GetStringValue(0, (ushort)IfdEntryTag.Copyright);
            }
            set
            {
                if (value == null)
                {
                    Structure.RemoveTag(0, (ushort)IfdEntryTag.Copyright);
                    return;
                }

                Structure.SetEntry(0, new StringIfdEntry((ushort)IfdEntryTag.Copyright, value));
            }
        }

        /// <summary>
        ///    Gets or sets the creator of the image.
        /// </summary>
        /// <value>
        ///    A <see cref="string" /> with the name of the creator.
        /// </value>
        public override string Creator
        {
            get
            {
                return Structure.GetStringValue(0, (ushort)IfdEntryTag.Artist);
            }
            set
            {
                Structure.SetStringValue(0, (ushort)IfdEntryTag.Artist, value);
            }
        }

        /// <summary>
        ///    Gets or sets the software the image, the current instance
        ///    belongs to, was created with.
        /// </summary>
        /// <value>
        ///    A <see cref="string" /> containing the name of the
        ///    software the current instace was created with.
        /// </value>
        public override string Software
        {
            get
            {
                return Structure.GetStringValue(0, (ushort)IfdEntryTag.Software);
            }
            set
            {
                Structure.SetStringValue(0, (ushort)IfdEntryTag.Software, value);
            }
        }

        /// <summary>
        ///    Gets or sets the time when the image, the current instance
        ///    belongs to, was taken.
        /// </summary>
        /// <value>
        ///    A <see cref="System.Nullable"/> with the time the image was taken.
        /// </value>
        public override DateTime? DateTime
        {
            get { return DateTimeOriginal; }
            set { DateTimeOriginal = value; }
        }

        /// <summary>
        ///    The time of capturing.
        /// </summary>
        /// <value>
        ///    A <see cref="System.Nullable"/> with the time of capturing.
        /// </value>
        public DateTime? DateTimeOriginal
        {
            get
            {
                return ExifIfd.GetDateTimeValue(0, (ushort)ExifEntryTag.DateTimeOriginal);
            }
            set
            {
                if (value == null)
                {
                    ExifIfd.RemoveTag(0, (ushort)ExifEntryTag.DateTimeOriginal);
                    return;
                }

                ExifIfd.SetDateTimeValue(0, (ushort)ExifEntryTag.DateTimeOriginal, value.Value);
            }
        }

        /// <summary>
        ///    The time of digitization.
        /// </summary>
        /// <value>
        ///    A <see cref="System.Nullable"/> with the time of digitization.
        /// </value>
        public DateTime? DateTimeDigitized
        {
            get
            {
                return ExifIfd.GetDateTimeValue(0, (ushort)ExifEntryTag.DateTimeDigitized);
            }
            set
            {
                if (value == null)
                {
                    ExifIfd.RemoveTag(0, (ushort)ExifEntryTag.DateTimeDigitized);
                    return;
                }

                ExifIfd.SetDateTimeValue(0, (ushort)ExifEntryTag.DateTimeDigitized, value.Value);
            }
        }

        /// <summary>
        ///    Gets or sets the latitude of the GPS coordinate the current
        ///    image was taken.
        /// </summary>
        /// <value>
        ///    A <see cref="System.Nullable"/> with the latitude ranging from -90.0
        ///    to +90.0 degrees.
        /// </value>
        public override double? Latitude
        {
            get
            {
                var gpsIfd = Gpsifd;
                var degreeEntry = gpsIfd.GetEntry(0, (ushort)GpsEntryTag.GpsLatitude) as RationalArrayIfdEntry;
                var degreeRef = gpsIfd.GetStringValue(0, (ushort)GpsEntryTag.GpsLatitudeRef);

                if (degreeEntry == null || degreeRef == null)
                    return null;

                Rational[] values = degreeEntry.Values;
                if (values.Length != 3)
                    return null;

                double deg = values[0] + values[1] / 60.0d + values[2] / 3600.0d;

                if (degreeRef == "S")
                    deg *= -1.0d;

                return Math.Max(Math.Min(deg, 90.0d), -90.0d);
            }
            set
            {
                var gpsIfd = Gpsifd;

                if (value == null)
                {
                    gpsIfd.RemoveTag(0, (ushort)GpsEntryTag.GpsLatitudeRef);
                    gpsIfd.RemoveTag(0, (ushort)GpsEntryTag.GpsLatitude);
                    return;
                }

                double angle = value.Value;

                if (angle < -90.0d || angle > 90.0d)
                    throw new ArgumentException("value");

                InitGpsDirectory();

                gpsIfd.SetStringValue(0, (ushort)GpsEntryTag.GpsLatitudeRef, angle < 0 ? "S" : "N");

                var entry =
                    new RationalArrayIfdEntry((ushort)GpsEntryTag.GpsLatitude,
                                               DegreeToRationals(Math.Abs(angle)));
                gpsIfd.SetEntry(0, entry);
            }
        }

        /// <summary>
        ///    Gets or sets the longitude of the GPS coordinate the current
        ///    image was taken.
        /// </summary>
        /// <value>
        ///    A <see cref="System.Nullable"/> with the longitude ranging from -180.0
        ///    to +180.0 degrees.
        /// </value>
        public override double? Longitude
        {
            get
            {
                var gpsIfd = Gpsifd;
                var degreeEntry = gpsIfd.GetEntry(0, (ushort)GpsEntryTag.GpsLongitude) as RationalArrayIfdEntry;
                var degreeRef = gpsIfd.GetStringValue(0, (ushort)GpsEntryTag.GpsLongitudeRef);

                if (degreeEntry == null || degreeRef == null)
                    return null;

                Rational[] values = degreeEntry.Values;
                if (values.Length != 3)
                    return null;

                double deg = values[0] + values[1] / 60.0d + values[2] / 3600.0d;

                if (degreeRef == "W")
                    deg *= -1.0d;

                return Math.Max(Math.Min(deg, 180.0d), -180.0d);
            }
            set
            {
                var gpsIfd = Gpsifd;

                if (value == null)
                {
                    gpsIfd.RemoveTag(0, (ushort)GpsEntryTag.GpsLongitudeRef);
                    gpsIfd.RemoveTag(0, (ushort)GpsEntryTag.GpsLongitude);
                    return;
                }

                double angle = value.Value;

                if (angle < -180.0d || angle > 180.0d)
                    throw new ArgumentException("value");

                InitGpsDirectory();

                gpsIfd.SetStringValue(0, (ushort)GpsEntryTag.GpsLongitudeRef, angle < 0 ? "W" : "E");

                var entry =
                    new RationalArrayIfdEntry((ushort)GpsEntryTag.GpsLongitude,
                                               DegreeToRationals(Math.Abs(angle)));
                gpsIfd.SetEntry(0, entry);
            }
        }

        /// <summary>
        ///    Gets or sets the altitude of the GPS coordinate the current
        ///    image was taken. The unit is meter.
        /// </summary>
        /// <value>
        ///    A <see cref="System.Nullable"/> with the altitude. A positive value
        ///    is above sea level, a negative one below sea level. The unit is meter.
        /// </value>
        public override double? Altitude
        {
            get
            {
                var gpsIfd = Gpsifd;
                var altitude = gpsIfd.GetRationalValue(0, (ushort)GpsEntryTag.GpsAltitude);
                var refEntry = gpsIfd.GetByteValue(0, (ushort)GpsEntryTag.GpsAltitudeRef);

                if (altitude == null)
                    return null;

                if (refEntry != null && refEntry.Value == 1)
                    altitude *= -1.0d;

                return altitude;
            }
            set
            {
                var gpsIfd = Gpsifd;

                if (value == null)
                {
                    gpsIfd.RemoveTag(0, (ushort)GpsEntryTag.GpsAltitudeRef);
                    gpsIfd.RemoveTag(0, (ushort)GpsEntryTag.GpsAltitude);
                    return;
                }

                double altitude = value.Value;

                InitGpsDirectory();

                gpsIfd.SetByteValue(0, (ushort)GpsEntryTag.GpsAltitudeRef, (byte)(altitude < 0 ? 1 : 0));
                gpsIfd.SetRationalValue(0, (ushort)GpsEntryTag.GpsAltitude, Math.Abs(altitude));
            }
        }

        /// <summary>
        ///    Gets the exposure time the image, the current instance belongs
        ///    to, was taken with.
        /// </summary>
        /// <value>
        ///    A <see cref="System.Nullable"/> with the exposure time in seconds.
        /// </value>
        public override double? ExposureTime
        {
            get
            {
                return ExifIfd.GetRationalValue(0, (ushort)ExifEntryTag.ExposureTime);
            }
            set
            {
                ExifIfd.SetRationalValue(0, (ushort)ExifEntryTag.ExposureTime, value.HasValue ? (double)value : 0);
            }
        }

        /// <summary>
        ///    Gets the FNumber the image, the current instance belongs
        ///    to, was taken with.
        /// </summary>
        /// <value>
        ///    A <see cref="System.Nullable"/> with the FNumber.
        /// </value>
        public override double? FNumber
        {
            get
            {
                return ExifIfd.GetRationalValue(0, (ushort)ExifEntryTag.FNumber);
            }
            set
            {
                ExifIfd.SetRationalValue(0, (ushort)ExifEntryTag.FNumber, value.HasValue ? (double)value : 0);
            }
        }

        /// <summary>
        ///    Gets the ISO speed the image, the current instance belongs
        ///    to, was taken with.
        /// </summary>
        /// <value>
        ///    A <see cref="System.Nullable"/> with the ISO speed as defined in ISO 12232.
        /// </value>
        public override uint? IsoSpeedRatings
        {
            get
            {
                return ExifIfd.GetLongValue(0, (ushort)ExifEntryTag.IsoSpeedRatings);
            }
            set
            {
                ExifIfd.SetLongValue(0, (ushort)ExifEntryTag.IsoSpeedRatings, value.HasValue ? (uint)value : 0);
            }
        }

        /// <summary>
        ///    Gets the focal length the image, the current instance belongs
        ///    to, was taken with.
        /// </summary>
        /// <value>
        ///    A <see cref="System.Nullable"/> with the focal length in millimeters.
        /// </value>
        public override double? FocalLength
        {
            get
            {
                return ExifIfd.GetRationalValue(0, (ushort)ExifEntryTag.FocalLength);
            }
            set
            {
                ExifIfd.SetRationalValue(0, (ushort)ExifEntryTag.FocalLength, value.HasValue ? (double)value : 0);
            }
        }

        /// <summary>
        ///    Gets the focal length the image, the current instance belongs
        ///    to, was taken with, assuming a 35mm film camera.
        /// </summary>
        /// <value>
        ///    A <see cref="System.Nullable"/> with the focal length in 35mm equivalent in millimeters.
        /// </value>
        public override uint? FocalLengthIn35MmFilm
        {
            get
            {
                return ExifIfd.GetLongValue(0, (ushort)ExifEntryTag.FocalLengthIn35MmFilm);
            }
            set
            {
                if (value.HasValue)
                {
                    ExifIfd.SetLongValue(0, (ushort)ExifEntryTag.FocalLengthIn35MmFilm, (uint)value);
                }
                else
                {
                    ExifIfd.RemoveTag(0, (ushort)ExifEntryTag.FocalLengthIn35MmFilm);
                }
            }
        }

        /// <summary>
        ///    Gets or sets the orientation of the image described
        ///    by the current instance.
        /// </summary>
        /// <value>
        ///    A <see cref="TagLib.Image.ImageOrientation" /> containing the orientation of the
        ///    image
        /// </value>
        public override ImageOrientation Orientation
        {
            get
            {
                var orientation = Structure.GetLongValue(0, (ushort)IfdEntryTag.Orientation);

                if (orientation.HasValue)
                    return (ImageOrientation)orientation;

                return ImageOrientation.None;
            }
            set
            {
                if ((uint)value < 1U || (uint)value > 8U)
                {
                    Structure.RemoveTag(0, (ushort)IfdEntryTag.Orientation);
                    return;
                }

                Structure.SetLongValue(0, (ushort)IfdEntryTag.Orientation, (uint)value);
            }
        }

        /// <summary>
        ///    Gets the manufacture of the recording equipment the image, the
        ///    current instance belongs to, was taken with.
        /// </summary>
        /// <value>
        ///    A <see cref="string" /> with the manufacture name.
        /// </value>
        public override string Make
        {
            get
            {
                return Structure.GetStringValue(0, (ushort)IfdEntryTag.Make);
            }
            set
            {
                Structure.SetStringValue(0, (ushort)IfdEntryTag.Make, value);
            }
        }

        /// <summary>
        ///    Gets the model name of the recording equipment the image, the
        ///    current instance belongs to, was taken with.
        /// </summary>
        /// <value>
        ///    A <see cref="string" /> with the model name.
        /// </value>
        public override string Model
        {
            get
            {
                return Structure.GetStringValue(0, (ushort)IfdEntryTag.Model);
            }
            set
            {
                Structure.SetStringValue(0, (ushort)IfdEntryTag.Model, value);
            }
        }

        /// <summary>
        ///    Initilazies the GPS IFD with some basic entries.
        /// </summary>
        private void InitGpsDirectory()
        {
            Gpsifd.SetStringValue(0, (ushort)GpsEntryTag.GpsVersionId, "2 0 0 0");
            Gpsifd.SetStringValue(0, (ushort)GpsEntryTag.GpsMapDatum, "WGS-84");
        }

        /// <summary>
        ///    Converts a given (positive) angle value to three rationals like they
        ///    are used to store an angle for GPS data.
        /// </summary>
        /// <param name="angle">
        ///    A <see cref="System.Double"/> between 0.0d and 180.0d with the angle
        ///    in degrees
        /// </param>
        /// <returns>
        ///    A <see cref="Rational"/> representing the same angle by degree, minutes
        ///    and seconds of the angle.
        /// </returns>
        private Rational[] DegreeToRationals(double angle)
        {
            if (angle < 0.0 || angle > 180.0)
                throw new ArgumentException("angle");

            uint deg = (uint)Math.Floor(angle);
            uint min = (uint)((angle - Math.Floor(angle)) * 60.0);
            uint sec = (uint)((angle - Math.Floor(angle) - (min / 60.0)) * 360000000.0);

            Rational[] rationals = new Rational[] {
                new Rational (deg, 1),
                new Rational (min, 1),
                new Rational (sec, 100000)
            };

            return rationals;
        }
    }
}