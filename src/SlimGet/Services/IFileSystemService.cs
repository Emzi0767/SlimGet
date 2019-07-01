using System.IO;
using SlimGet.Data;

namespace SlimGet.Services
{
    public interface IFileSystemService
    {
        /// <summary>
        /// Returns a seekable, readable+writable stream to hold temporary data.
        /// </summary>
        /// <returns>Stream for a temporary file.</returns>
        Stream CreateTemporaryFile();

        /// <summary>
        /// Opens a package file for reading. This stream is read-only, seeking is not guaranteed.
        /// </summary>
        /// <param name="package">Package to open the package file for.</param>
        /// <returns>Stream for the package.</returns>
        Stream OpenPackageRead(PackageInfo package);

        /// <summary>
        /// Opens a package file for writing. File will be overwritten if it exists, and created otherwise. This stream is write-only, seeking is not guaranteed.
        /// </summary>
        /// <param name="package">Package to open the package file for.</param>
        /// <returns>Stream for the package.</returns>
        Stream OpenPackageWrite(PackageInfo package);

        /// <summary>
        /// Opens a manifest file for reading. This stream is read-only, seeking is not guaranteed.
        /// </summary>
        /// <param name="package">Package to open the manifest file for.</param>
        /// <returns>Stream for the manifest.</returns>
        Stream OpenManifestRead(PackageInfo package);

        /// <summary>
        /// Opens a manifest file for writing. File will be overwritten if it exists, and created otherwise. This stream is write-only, seeking is not guaranteed.
        /// </summary>
        /// <param name="package">Package to open the manifest file for.</param>
        /// <returns>Stream for the manifest.</returns>
        Stream OpenManifestWrite(PackageInfo package);

        /// <summary>
        /// Deletes a package and associated manifest from the filesystem.
        /// </summary>
        /// <param name="package">Package to delete.</param>
        /// <returns>Whether the operation was successful.</returns>
        bool DeleteWholePackage(PackageInfo package);

        /// <summary>
        /// Deletes a package from the filesystem.
        /// </summary>
        /// <param name="package">Package to delete.</param>
        /// <returns>Whether the operation was successful.</returns>
        bool DeletePackage(PackageInfo package);

        /// <summary>
        /// Deletes a package manifest from the filesystem.
        /// </summary>
        /// <param name="package">Package the manifest of which to delete.</param>
        /// <returns>Whether the operation was successful.</returns>
        bool DeleteManifest(PackageInfo package);

        /// <summary>
        /// Checks whether this filesystem has the specified package.
        /// </summary>
        /// <param name="package">Package to check for.</param>
        /// <returns>Whether the given package exists on this filesystem.</returns>
        bool HasPackage(PackageInfo package);

        /// <summary>
        /// Gets the virtual identifier of a package in a given filesystem. This is used for debugging and other diagnostic purposes.
        /// </summary>
        /// <param name="package">Package to get virtual identifier for.</param>
        /// <returns>Virtual identifier of a package, or null if specified package does not exist.</returns>
        string GetPackageFileName(PackageInfo package);

        /// <summary>
        /// Gets the virtual identifier of a manifest in a given filesystem. This is used for debugging and other diagnostic purposes.
        /// </summary>
        /// <param name="package">Package to get virtual identifier for.</param>
        /// <returns>Virtual identifier of a manifest, or null if specified package does not exist.</returns>
        string GetManifestFileName(PackageInfo package);
    }
}
