// Copyright Koninklijke Philips N.V. 2011

using System;
using System.Security.Principal;
using System.Threading;

namespace CTHarmonyAdapters
{
    /// <summary>
    /// Holds various constants required for MemoryManager
    /// </summary>
    internal static class MemoryManagerConstants {
        /// <summary>
        /// is ready
        /// </summary>
        private static bool ready;

        /// <summary>
        /// holds every one string
        /// </summary>
        private static string everyOne;

        /// <summary>
        /// Holds the name for the PID Header
        /// </summary>
        public const string PIDHeaderName = "PIDHeader";

        /// <summary>
        /// Holds the namespace value for the PID Header
        /// </summary>
        public const string HeaderNameSpace = "PFMemoryManager";

        /// <summary>
        /// Specifies the MemoryManager server URI.
        /// </summary>
        public static readonly string MemoryManagerServerUri =
            "net.pipe://localhost/MemoryManagerServer";

        /// <summary>
        /// MONOCHROME1:Pixel data represent a single monochrome image plane
        /// </summary>
        public static readonly string Monochrome1 = "MONOCHROME1";

        /// <summary>
        /// MONOCHROME2: Pixel data represent a single monochrome image plane
        /// </summary>
        public static readonly string Monochrome2 = "MONOCHROME2";

        /// <summary>
        ///  Pixel data represent a color image described by red, green, and blue image planes.
        /// </summary>
        public static readonly string RGB = "RGB";

        /// <summary>
        /// PALETTE COLOR: Pixel data describe a color image with a single sample per pixel 
        /// (single image plane)
        /// </summary>
        public static readonly string PaletteColor = "PALETTE COLOR";

        internal static readonly string Derived = "DERIVED";

        internal static readonly string Secondary = "SECONDARY";

        internal static readonly string SBI = "SBI";

        /// <summary>
        /// Gets the common timeout used in internal services.
        /// </summary>
        /// <remarks>
        /// When this value is changed we want cients to also get recompiled. Hence keeping this
        /// as readonly instead of const.
        /// </remarks>
        public static readonly TimeSpan ServiceTimeout = TimeSpan.FromMinutes(120);

        /// <summary>
        /// Gets the common maximum timeout used in internal services.
        /// </summary>
        /// <remarks>
        /// When this value is changed we want cients to also get recompiled. Hence keeping this
        /// as readonly instead of const.
        /// the maximum time allowed is int.MaxValue miiliseconds (2147483647 ~ 24 days).
        /// TimeSpan.MaxValue doesn't seem to work.
        /// </remarks>
        public static readonly TimeSpan MaximumTimeout = TimeSpan.FromDays(15);
        
        /// <summary>
        /// Specifies a constant for windows error code 8, i.e. Not enough resources
        /// </summary>
        public const int NotEnoughResources = 8;

        /// <summary>
        /// Specifies a constant for windows error code 38, i.e. End Of File Reached.
        /// </summary>
        public const int FileTruncated = 38;

        /// <summary>
        /// Access denied.
        /// </summary>
        public const int AccessDenied = 5;

        /// <summary>
        /// ERROR_IO_PENDING
        /// </summary>
        public const int ErrorIOPending = 997;

        /// <summary>
        /// FILE_NOT_FOUND
        /// </summary>
        public const int FileOrMmfNotFound = 2;

        /// <summary>
        /// ERROR_SHARING_VIOLATION
        /// </summary>
        public const int ErrorSharingViolation = 32;

        /// <summary>
        /// PATH_NOT_FOUND
        /// </summary>
        public const int PathNotFound = 3;

        /// <summary>
        /// Defines the minimum physical memory threshold for 
        /// memory pressure cleanup helper to use lower set of Quota values
        /// </summary>
        public const int PhysicalMemoryThreshold = 4;

        private static readonly object staticObjectCreationSyncLock = new object();

        /// <summary>
        /// Holds the culture specific value of \"EverOne useraccount\".
        /// </summary>
        public static string EveryOne {
            get {
                //TICS -COV_CS_GUARDED_BY_VIOLATION
                //TICS -COV_CS_LOCK_EVASION
                if (!ready) {
                    lock (staticObjectCreationSyncLock) {
                        if (!ready) {
                            SecurityIdentifier sid = new SecurityIdentifier(
                                WellKnownSidType.WorldSid, null);
                            NTAccount userAccount = sid.Translate(typeof(NTAccount)) as NTAccount;
                            everyOne = userAccount == null ? "Everyone" : userAccount.Value;
                            Thread.MemoryBarrier();
                            ready = true;
                        }
                    }
                }
                //TICS +COV_CS_LOCK_EVASION
                //TICS +COV_CS_GUARDED_BY_VIOLATION
                return everyOne;
            }
        }

        /// <summary>
        /// Defines the maximum page size for 12-Bit
        /// </summary>
        public const int MaxPageSize = 4096;

        /// <summary>
        /// Defines the maximum pixel value for 12-Bit
        /// </summary>
        public const int MaxPixelValue = 4095;

        /// <summary>
        /// Boolean to indicate if the WCF exception details should
        /// be included in faults.
        /// Value is true in Debug Mode and false otherwise.
        /// </summary>
        public const bool IncludeExceptionDetailInFaults = true;

        /// <summary>
        /// Read flags used for file async read.
        /// FILE_ATTRIBUTE_NORMAL | FILE_FLAG_NO_BUFFERING | 
        /// FILE_FLAG_OVERLAPPED
        /// </summary>
        /// <remarks>
        /// Even though using sequential scan may buffer the files in OS cache, we will loose
        /// performance while doing async read. Hence removed.
        /// </remarks>
        public const uint AsyncReadFileFlags = 0x00000080 | 0x20000000 | 0x40000000;

        /// <summary>
        /// Read flags used for file synchronous sequential read.
        /// FILE_ATTRIBUTE_NORMAL | FILE_FLAG_SEQUENTIAL_SCAN
        /// </summary>
        public const uint SyncSequentialReadFileFlags = 0x00000080 | 0x08000000;

        /// <summary>
        /// Read flags used for file synchronous random read.
        /// FILE_ATTRIBUTE_NORMAL | FILE_FLAG_RANDOM_ACCESS
        /// </summary>
        public const uint SyncRandomReadFileFlags = 0x00000080 | 0x10000000;

        ///// <summary>
        ///// Name of the event that will be triggered when memory manager server host starts
        ///// </summary>
        //public static readonly string MmServerHostStartedEventName = DeploymentConstants.Prefix +
        //    "MMServerHostStarted_Event";
    }
}
