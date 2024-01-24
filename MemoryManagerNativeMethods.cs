// Copyright Koninklijke Philips N.V. 2011

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;
using Microsoft.Win32.SafeHandles;

namespace CTHarmonyAdapters {
    /// <summary>
    /// Memory manager API.
    /// </summary>
    internal static class MemoryManagerNativeMethods {
        
        #region Enums and Constants

        /// <summary>
        /// Invalid Set file pointer
        /// </summary>
        internal const uint INVALID_SET_FILE_POINTER = 0xFFFFFFFF;

        /// <summary>
        /// holds the current process
        /// </summary>
        internal static readonly Process currentProcess = Process.GetCurrentProcess();

        /// <summary>
        /// Move method
        /// </summary>
        internal enum MoveMethod : uint {
            /// <summary>
            /// Begin of file
            /// </summary>
            FILE_BEGIN = 0,
            /// <summary>
            /// current file position
            /// </summary>
            FILE_CURRENT = 1,
            /// <summary>
            /// End of file
            /// </summary>
            FILE_END = 2
        }
        //@shoscarOff C4@106ECoding  Document internal
        //@shoscarOff C7@101 Coding  Protect non-constant instance/class variable

        /// <summary>
        /// MEMORYSTATUSEX
        /// </summary>
        [StructLayoutAttribute(LayoutKind.Sequential)]
        internal struct MEMORYSTATUSEX {

            /// DWORD->unsigned int
            public uint dwLength;

            /// DWORD->unsigned int
            public uint dwMemoryLoad;

            /// DWORDLONG->ULONGLONG->unsigned __int64
            public ulong ullTotalPhys;

            /// DWORDLONG->ULONGLONG->unsigned __int64
            public ulong ullAvailPhys;

            /// DWORDLONG->ULONGLONG->unsigned __int64
            public ulong ullTotalPageFile;

            /// DWORDLONG->ULONGLONG->unsigned __int64
            public ulong ullAvailPageFile;

            /// DWORDLONG->ULONGLONG->unsigned __int64
            public ulong ullTotalVirtual;

            /// DWORDLONG->ULONGLONG->unsigned __int64
            public ulong ullAvailVirtual;

            /// DWORDLONG->ULONGLONG->unsigned __int64
            public ulong ullAvailExtendedVirtual;
        }

        //@shoscarOn C4@106ECoding  Document internal
        //@shoscarOn C7@101 Coding  Protect non-constant instance/class variable

        /// <summary>
        /// Specifies the type of access to the file view and, 
        /// therefore, the protection of the pages mapped by 
        /// the file. The following table shows possible values 
        /// for this parameter. These flags are used in parameter 
        /// dwDesiredAccess of MapViewOfFile() system call.
        /// </summary>
        [Flags]
        internal enum FILE_MAP_ACCESS {
            /// <summary>
            /// Copy-on-write access. The mapping object must be created with 
            /// PAGE_WRITECOPY protection. The system commits physical storage 
            /// from the paging file at the time that MapViewOfFile is called. 
            /// The actual physical storage is not used until a thread in the 
            /// process writes to an address in the view. At that time, the 
            /// system copies the original page to a new page that is backed 
            /// by the paging file, maps the page into the process address space,
            /// and changes the page protection to PAGE_READWRITE. The threads 
            /// in the process can access only the local copy of the data, not 
            /// the original data. If the page is ever trimmed from the working 
            /// set of the process, it can be written to the paging file storage that is
            /// committed when MapViewOfFile is called.
            /// 
            /// This process only allocates physical memory when a virtual address 
            /// is actually written to. Changes are never written back to the original 
            /// file, and are freed when the thread in your process unmaps the view.
            /// 
            /// Paging file space for the entire view is committed when copy-on-write 
            /// access is specified, because the thread in the process can write to 
            /// every single page. Therefore, enough physical storage space must be 
            /// obtained at the time MapViewOfFile is called.
            /// </summary>
            FILE_MAP_COPY = 0x0001,
            /// <summary>
            /// Read/write access. The mapping object must be 
            /// created with PAGE_READWRITE protection.
            /// A read/write view of the file is mapped.
            /// </summary>
            FILE_MAP_WRITE = 0x0002,
            /// <summary>
            /// Read-only access. The mapping object must be 
            /// created with PAGE_READWRITE or PAGE_READONLY protection.
            /// A read-only view of the file is mapped.
            /// </summary>
            FILE_MAP_READ = 0x0004
        }

        private const int MemoryCommit = 0x1000;
        private const int MemoryReserve = 0x2000;
        private const int MemoryTopDown = 0x100000;
        private const int MemoryRelease = 0x8000;
        private const int PageReadWrite = 0x04;
        /// <summary>
        /// Normal allocation behavior
        /// </summary>
        private const int Normal = 0x0;
        /// <summary>
        /// Allocate at high address
        /// </summary>
        private const int HighMem = 0x1;

        #endregion

        
        /// <summary>
        /// Allocates the virtual memory
        /// </summary>
        /// <param name="lpAddress">The address of memory.</param>
        /// <param name="dwSize">Size of the memory to create.</param>
        /// <param name="flAllocationType">Type of the allocation.</param>
        /// <param name="flProtect">protection mode.</param>
        /// <returns></returns>
        [
        SuppressMessage(
            "Microsoft.Security",
            "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage",
            Justification =
            "The usage of SuppressUnmanagedCodeSecurity has been reviewed for this" +
            "method and is accepted as this unmanaged method will be called from within the" +
            " known platform code."
        )
        ]
        [SuppressUnmanagedCodeSecurity]
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern IntPtr VirtualAlloc(
            IntPtr lpAddress,
            UIntPtr dwSize,
            int flAllocationType,
            int flProtect);

        /// <summary>
        /// Frees the virtual memory created before using VirtualAlloc
        /// </summary>
        /// <param name="lpAddress">The address of memory.</param>
        /// <param name="dwSize">Size of the memory.</param>
        /// <param name="dwFreeType">Type of freeing.</param>
        /// <returns></returns>
        [
        SuppressMessage(
            "Microsoft.Security",
            "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage",
            Justification =
            "The usage of SuppressUnmanagedCodeSecurity has been reviewed for this" +
            "method and is accepted as this unmanaged method will be called from within the" +
            " known platform code."
        )
        ]
        [SuppressUnmanagedCodeSecurity]
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool VirtualFree(
            IntPtr lpAddress,
            UIntPtr dwSize,
            int dwFreeType);

        /// <summary>
        /// Allocate a block of memory aligned for raw IO
        /// </summary>
        /// <param name="size">size in bytes of block to allocate</param>
        /// <param name="errorCodeIfAny">Errorcode if any.</param>
        /// <returns>pointer to allocated block</returns>
        internal static IntPtr AllocateForRawIO(int size, out int errorCodeIfAny) {
            errorCodeIfAny = 0;
            IntPtr allocated = VirtualAlloc(
                IntPtr.Zero, // let system decide where to allocate
                (UIntPtr)size,
                MemoryCommit | MemoryReserve,
                PageReadWrite);
            if (allocated == IntPtr.Zero) {
                errorCodeIfAny = Marshal.GetLastWin32Error();
            }
            return allocated;
        }

        /// <summary>
        /// Free a block of memory
        /// </summary>
        /// <param name="address">address of block to free</param>
        internal static void FreeRawIO(IntPtr address) {
            int errorCode = 0;
            FreeRawIO(address, out errorCode);
        }

        /// <summary>
        /// Free a block of memory
        /// </summary>
        /// <param name="address">address of block to free</param>
        /// <param name="errorCode">error code if any</param>
        internal static void FreeRawIO(IntPtr address, out int errorCode) {
            errorCode = 0;
            if (address == IntPtr.Zero) {
                return;
            }
            bool result = VirtualFree(address, UIntPtr.Zero, MemoryRelease);
            // if Virtual free fails then return value will be 0
            if (!result) {
                errorCode = Marshal.GetLastWin32Error();
                //LogHelper.DevelopmentWarningLog(
                //    ModuleIDs.MemoryManagerAPI,
                //    EventIDs.Error,
                //    "Unable to free memory using VirtualFree. Address = " + address +
                //    ", Errorcode = " + errorCode + ", Message = " +
                //    MemoryManagerNativeMethods.GetMessageForErrorCode(errorCode)
                //);
            }
        }

        /// <summary>
        /// Gets and logs the last formatted Win32 API error
        /// </summary>
        /// <param name="operationName">formatted error string</param>
        //internal static void WriteLogLastWin32Error(string operationName) {
        //    string msg = GetFormattedLastError(operationName);
        //    LogHelper.DevelopmentErrorLog(
        //        ModuleIDs.MemoryManagerAPI, EventIDs.Unknown, msg);
        //}

        /// <summary>
        /// Gets the formatted last error for Win32 API
        /// </summary>
        /// <param name="operationName">API name</param>
        /// <returns>formatted error string</returns>
        internal static string GetFormattedLastError(string operationName) {
            int lastError = Marshal.GetLastWin32Error();
            return (operationName + ", ErrorCode = " + lastError + 
                ", Message = " +  new Win32Exception(lastError).Message);
        }

        /// <summary>
        /// Gets the message for error code.
        /// </summary>
        /// <param name="errorCode">The error code.</param>
        /// <returns></returns>
        internal static string GetMessageForErrorCode(int errorCode) {
            return new Win32Exception(errorCode).Message;
        }

        /// <summary>
        /// Gets the exception for error code.
        /// </summary>
        /// <param name="errorCode">The error code.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        //internal static Exception GetExceptionForErrorCode(int errorCode, string errorMessage) {
        //    Exception exp = null;
        //    if (errorCode == MemoryManagerConstants.NotEnoughResources) {
        //        exp = new InsufficientMemoryException(errorMessage);
        //    } else if (errorCode == MemoryManagerConstants.FileTruncated) {
        //        exp = new FileTruncatedException(errorMessage);
        //    } else if (errorCode == MemoryManagerConstants.ErrorSharingViolation) {
        //        exp = new IOException(errorMessage);
        //    } else {
        //        exp = new FailException(errorMessage);
        //    }
        //    return exp;
        //}
        #region DllImports

        /// <summary>
        /// Gets the system information
        /// </summary>
        /// <param name="lpSystemInfo">reference to SYSTEM_INFO structure</param>
        [
        SuppressMessage(
            "Microsoft.Security",
            "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage",
            Justification =
            "The usage of SuppressUnmanagedCodeSecurity has been reviewed for this" +
            "method and is accepted as this unmanaged method will be called from within the" +
            " known platform code."
        )
        ]
        [SuppressUnmanagedCodeSecurity]
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern void GetSystemInfo(
            [MarshalAs(UnmanagedType.Struct)] ref SYSTEM_INFO lpSystemInfo);

        /// <summary>
        /// SystemInfo structure
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct SYSTEM_INFO {
            /// <summary>
            /// processor info
            /// </summary>
            internal _PROCESSOR_INFO_UNION uProcessorInfo;
            /// <summary>
            /// page sise
            /// </summary>
            public uint dwPageSize;
            /// <summary>
            /// minimum allocation address
            /// </summary>
            public IntPtr lpMinimumApplicationAddress;
            /// <summary>
            /// maximum allocation address
            /// </summary>
            public IntPtr lpMaximumApplicationAddress;
            /// <summary>
            /// active processor mask
            /// </summary>
            public IntPtr dwActiveProcessorMask;
            /// <summary>
            /// no.of processors
            /// </summary>
            public uint dwNumberOfProcessors;
            /// <summary>
            /// processor type
            /// </summary>
            public uint dwProcessorType;
            /// <summary>
            /// allocation granularity
            /// </summary>
            public uint dwAllocationGranularity;
            /// <summary>
            /// processor level
            /// </summary>
            public ushort dwProcessorLevel;
            /// <summary>
            /// processor revision
            /// </summary>
            public ushort dwProcessorRevision;
        }

        /// <summary>
        /// Processor info
        /// </summary>
        [StructLayout(LayoutKind.Explicit)]
        public struct _PROCESSOR_INFO_UNION {
            /// <summary>
            /// OEM Id
            /// </summary>
            [FieldOffset(0)]
            internal uint dwOemId;
            /// <summary>
            /// processor architecture, x86 or x64, etc
            /// </summary>
            [FieldOffset(0)]
            internal ushort wProcessorArchitecture;
            /// <summary>
            /// reserved, not used.
            /// </summary>
            [FieldOffset(2)]
            internal ushort wReserved;
        }

        /// <summary>
        /// Creates an input/output (I/O) completion port and associates it with 
        /// a specified file handle, or creates an I/O completion port that is not 
        /// yet associated with a file handle, allowing association at a later time.
        /// </summary>
        /// <param name="fileHandle">
        /// An open file handle or INVALID_HANDLE_VALUE.
        /// </param>
        /// <param name="existingCompletionPort">
        /// A handle to an existing I/O completion port or NULL.
        /// </param>
        /// <param name="completionKey">
        /// The per-handle user-defined completion key that is included in every I/O 
        /// completion packet for the specified file handle.
        /// </param>
        /// <param name="NumberOfConcurrentThreads">
        /// The maximum number of threads that the operating system can allow to 
        /// concurrently process I/O completion packets for the I/O completion port. 
        /// This parameter is ignored if the ExistingCompletionPort parameter is not NULL.
        /// If this parameter is zero, the system allows as many concurrently running 
        /// threads as there are processors in the system
        /// </param>
        /// <returns>the handle to an I/O completion port</returns>
        [
        SuppressMessage(
            "Microsoft.Security",
            "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage",
            Justification =
            "The usage of SuppressUnmanagedCodeSecurity has been reviewed for this" +
            "method and is accepted as this unmanaged method will be called from within the" +
            " known platform code and test code only."
        )
        ]
        [SuppressUnmanagedCodeSecurity]
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern SafeFileHandle CreateIoCompletionPort(
            IntPtr fileHandle,
            IntPtr existingCompletionPort,
            UIntPtr completionKey,
            uint NumberOfConcurrentThreads);

        /// <summary>
        /// Closes an open object handle.
        /// </summary>
        /// <param name="handle">
        /// A handle to an open object. This parameter can be a
        /// pseudo handle or INVALID_HANDLE_VALUE.
        /// </param>
        /// <remarks>
        /// For more information see MSDN documentation on CloseHandle function.
        /// </remarks>
        [
        SuppressMessage(
            "Microsoft.Security",
            "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage",
            Justification =
            "The usage of SuppressUnmanagedCodeSecurity has been reviewed for this" +
            "method and is accepted as this unmanaged method will be called from within the" +
            " known platform code."
        )
        ]
        [SuppressUnmanagedCodeSecurity]
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool CloseHandle(IntPtr handle);

        /// <summary>
        /// Gets the file size
        /// </summary>
        /// <param name="hFile">file handle</param>
        /// <param name="lpFileSize">actual size of the file</param>
        /// <returns></returns>
        [
        SuppressMessage(
            "Microsoft.Security",
            "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage",
            Justification =
            "The usage of SuppressUnmanagedCodeSecurity has been reviewed for this" +
            "method and is accepted as this unmanaged method will be called from within the" +
            " known platform code and test code only."
        )
        ]
        [SuppressUnmanagedCodeSecurity]
        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetFileSizeEx(SafeFileHandle hFile, out long lpFileSize);

        /// <summary>
        /// Creates or opens a file
        /// </summary>
        /// <param name="filename">The name of the file</param>
        /// <param name="access">The requested access to the file</param>
        /// <param name="share">The requested sharing mode of the file</param>
        /// <param name="security">A pointer to the security attributes</param>
        /// <param name="disposition">An action to take on a file</param>
        /// <param name="flags">The file attributes and flags</param>
        /// <param name="template">A valid handle to a template file</param>
        /// <returns>Handle to the opened file</returns>
        [
        SuppressMessage(
            "Microsoft.Security",
            "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage",
            Justification =
            "The usage of SuppressUnmanagedCodeSecurity has been reviewed for this" +
            "method and is accepted as this unmanaged method will be called from within the" +
            " known platform code and test code only."
        )
        ]
        [SuppressUnmanagedCodeSecurity]
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern SafeFileHandle CreateFile(
            string filename,
            uint access,
            uint share,
            IntPtr security,
            uint disposition,
            uint flags,
            IntPtr template
        );

        /// <summary>
        /// Reads data from the specified file or input/output (I/O) device
        /// </summary>
        /// <param name="handle">A handle to the file</param>
        /// <param name="bytes">A pointer to the data read from a file</param>
        /// <param name="numBytesToRead">Number of bytes to be read</param>
        /// <param name="numBytesRead">Total number of bytes read</param>
        /// <param name="overlapped">A pointer to the overlapped structure</param>
        /// <returns>Returns successful or not</returns>
        [
        SuppressMessage(
            "Microsoft.Security",
            "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage",
            Justification =
            "The usage of SuppressUnmanagedCodeSecurity has been reviewed for this" +
            "method and is accepted as this unmanaged method will be called from within the" +
            " known platform code and test code only."
        )
        ]
        [SuppressUnmanagedCodeSecurity]
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static unsafe extern bool ReadFile(
            SafeFileHandle handle,
            IntPtr bytes,
            uint numBytesToRead,
            IntPtr numBytesRead,
            NativeOverlapped* overlapped);

        /// <summary>
        /// Reads data from the specified file or input/output (I/O) device
        /// </summary>
        /// <param name="handle">A handle to the file</param>
        /// <param name="bytes">A pointer to the data read from a file</param>
        /// <param name="numBytesToRead">Number of bytes to be read</param>
        /// <param name="numBytesRead">Total number of bytes read</param>
        /// <param name="overlapped">A pointer to the overlapped structure</param>
        /// <returns>Returns successful or not</returns>
        [
        SuppressMessage(
            "Microsoft.Interoperability",
            "CA1415:DeclarePInvokesCorrectly")
        ]
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool ReadFile(
            SafeFileHandle handle,
            IntPtr bytes,
            uint numBytesToRead,
            out uint numBytesRead,
            IntPtr overlapped);

        /// <summary>
        /// Sets the file pointer.
        /// </summary>
        /// <param name="hFile">The handle of the file.</param>
        /// <param name="cbDistanceToMove">The distance to move.</param>
        /// <param name="pDistanceToMoveHigh">The distance to move high.</param>
        /// <param name="fMoveMethod">The move method.</param>
        /// <returns></returns>
        [
        SuppressMessage(
            "Microsoft.Security",
            "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage",
            Justification =
            "The usage of SuppressUnmanagedCodeSecurity has been reviewed for this" +
            "method and is accepted as this unmanaged method will be called from within the" +
            " known platform code and test code only."
        )
        ]
        [SuppressUnmanagedCodeSecurity]
        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern UInt32 SetFilePointer(
            SafeFileHandle hFile,
            Int32 cbDistanceToMove,
            IntPtr pDistanceToMoveHigh,
            MoveMethod fMoveMethod);

        /// <summary>
        /// Attempts to de-queue an I/O completion packet from the specified I/O completion port. 
        /// If there is no completion packet queued, the function waits for a pending I/O 
        /// operation associated with the completion port to complete.
        /// </summary>
        /// <param name="CompletionPort">A handle to the completion port</param>
        /// <param name="lpNumberOfBytes">A pointer to a variable that receives the number of 
        /// bytes transferred during an I/O operation that has completed</param>
        /// <param name="lpCompletionKey">A pointer to a variable that receives the completion 
        /// key value associated with the file handle whose I/O operation has completed.</param>
        /// <param name="lpOverlapped">A pointer to a variable that receives the address of 
        /// the OVERLAPPED structure that was specified when the completed I/O operation 
        /// was started</param>
        /// <param name="dwMilliseconds">The number of milliseconds that the caller is willing 
        /// to wait for a completion packet to appear at the completion port</param>
        /// <returns>Returns successful or not</returns>
        [SuppressUnmanagedCodeSecurity]
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern unsafe bool GetQueuedCompletionStatus(
            IntPtr CompletionPort,
            out uint lpNumberOfBytes,
            out UIntPtr lpCompletionKey,
            void* lpOverlapped,
            int dwMilliseconds);

        /// Return Type: BOOL->int
        ///lpBuffer: LPMEMORYSTATUSEX->_MEMORYSTATUSEX*
        [
        SuppressMessage(
            "Microsoft.Security",
            "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage",
            Justification =
            "The usage of SuppressUnmanagedCodeSecurity has been reviewed for this" +
            "method and is accepted as this unmanaged method will be called from with in the" +
            " known platform code."
        )
        ]
        [SuppressUnmanagedCodeSecurity]
        [DllImport("kernel32.dll", EntryPoint = "GlobalMemoryStatusEx")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GlobalMemoryStatusEx([Out] out MEMORYSTATUSEX lpBuffer);

        /// <summary>
        /// Copies count bytes of src to dest. If the source and destination overlap, the behavior
        /// of memcpy is undefined. Use memmove to handle overlapping regions.
        /// </summary>
        /// <param name="destination">The destination.</param>
        /// <param name="val">The value.</param>
        /// <param name="size">The size.</param>
        /// <returns></returns>
        [
        SuppressMessage(
            "Microsoft.Security",
            "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage",
            Justification =
            "The usage of SuppressUnmanagedCodeSecurity has been reviewed for this" +
            "method and is accepted as this unmanaged method will be called from with in the" +
            " known platform code."
        )
        ]
        [SuppressUnmanagedCodeSecurity]
        [DllImport("ntdll.dll", EntryPoint = "memset", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr MemSet(
            IntPtr destination, 
            int val, 
            [MarshalAs(UnmanagedType.U4)]uint size
        );

        /// <summary>
        /// Copies count bytes of src to dest. If the source and destination overlap, the behavior 
        /// of memcpy is undefined. Use memmove to handle overlapping regions.
        /// </summary>
        /// <param name="dst">The destination pointer.</param>
        /// <param name="src">The source pointer.</param>
        /// <param name="count">The number of bytes to copy.</param>
        [
        SuppressMessage(
            "Microsoft.Security",
            "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage",
            Justification =
            "The usage of SuppressUnmanagedCodeSecurity has been reviewed for this" +
            "method and is accepted as this unmanaged method will be called from with in the" +
            " known platform code."
        )
        ]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(
            "msvcrt.dll",
            EntryPoint = "memcpy",
            CallingConvention = CallingConvention.Cdecl,
            SetLastError = false)
        ]
        internal static extern void MemCpy(IntPtr dst, IntPtr src, int count);

        #endregion        

        /// <summary>
        /// Specifies whether the server is hosted successfully in this process.
        /// MM server Host sets this property. So if MM is running inproc we will get this as 
        /// true.
        /// </summary>
        internal static bool ServerHostedInProc { get; set; }
    }

}
