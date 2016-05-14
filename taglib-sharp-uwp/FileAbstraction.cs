using System;

namespace TagLib.Uwp
{
    public class FileAbstraction : File.IFileAbstraction
    {
        /// <summary>
        ///    Constructs and initializes a new instance of
        ///    <see cref="FileAbstraction" /> for a
        ///    specified path in the local file system.
        /// </summary>
        /// <param name="path">
        ///    A <see cref="string" /> object containing the
        ///    path of the file to use in the new instance.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///    <paramref name="path" /> is <see langword="null"
        ///    />.
        /// </exception>
        public FileAbstraction(string path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            Name = path;
        }

        /// <summary>
        ///    Gets the path of the file represented by the
        ///    current instance.
        /// </summary>
        /// <value>
        ///    A <see cref="string" /> object containing the
        ///    path of the file represented by the current
        ///    instance.
        /// </value>
        public string Name { get; }

        /// <summary>
        ///    Gets a new readable, seekable stream from the
        ///    file represented by the current instance.
        /// </summary>
        /// <value>
        ///    A new <see cref="System.IO.Stream" /> to be used
        ///    when reading the file represented by the current
        ///    instance.
        /// </value>
        public System.IO.Stream ReadStream => System.IO.File.Open(Name, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read);

        /// <summary>
        ///    Gets a new writable, seekable stream from the
        ///    file represented by the current instance.
        /// </summary>
        /// <value>
        ///    A new <see cref="System.IO.Stream" /> to be used
        ///    when writing to the file represented by the
        ///    current instance.
        /// </value>
        public System.IO.Stream WriteStream => System.IO.File.Open(Name, System.IO.FileMode.Open, System.IO.FileAccess.ReadWrite);

        /// <summary>
        ///    Closes a stream created by the current instance.
        /// </summary>
        /// <param name="stream">
        ///    A <see cref="System.IO.Stream" /> object
        ///    created by the current instance.
        /// </param>
        public void CloseStream(System.IO.Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            stream.Dispose();
        }
    }
}
