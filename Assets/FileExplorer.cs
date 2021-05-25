using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace FileOpener {
    //All rights reserved
    internal static class FileExplorerApi {

        const uint OFN_ALLOWMULTISELECT = 0x00000200;
        const uint OFN_CREATEPROMPT = 0x00002000;
        const uint OFN_DONTADDTORECENT = 0x02000000;
        const uint OFN_ENABLEHOOK = 0x00000020;
        const uint OFN_ENABLEINCLUDENOTIFY = 0x00400000;
        const uint OFN_ENABLESIZING = 0x00800000;
        const uint OFN_ENABLETEMPLATE = 0x00000040;
        const uint OFN_ENABLETEMPLATEHANDLE = 0x00000080;
        const uint OFN_EXPLORER = 0x00080000;
        const uint OFN_EXTENSIONDIFFERENT = 0x00000400;
        const uint OFN_FILEMUSTEXIST = 0x00001000;
        const uint OFN_FORCESHOWHIDDEN = 0x10000000;
        const uint OFN_HIDEREADONLY = 0x00000004;
        const uint OFN_LONGNAMES = 0x00200000;
        const uint OFN_NOCHANGEDIR = 0x00000008;
        const uint OFN_NODEREFERENCELINKS = 0x00100000;
        const uint OFN_NOLONGNAMES = 0x00040000;
        const uint OFN_NONETWORKBUTTON = 0x00020000;
        const uint OFN_NOREADONLYRETURN = 0x00008000;
        const uint OFN_NOTESTFILECREATE = 0x00010000;
        const uint OFN_NOVALIDATE = 0x00000100;
        const uint OFN_OVERWRITEPROMPT = 0x00000002;
        const uint OFN_PATHMUSTEXIST = 0x00000800;
        const uint OFN_READONLY = 0x00000001;
        const uint OFN_SHAREAWARE = 0x00004000;
        const uint OFN_SHOWHELP = 0x00000010;


        const ushort FNERR_BUFFERTOOSMALL = 0x3003;
        const ushort FNERR_INVALIDFILENAME = 0x3002;
        const ushort FNERR_SUBCLASSFAILURE = 0x3001;


        [DllImport ("comdlg32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool GetOpenFileNameW (ref OpenFileName ofn);

        /// <summary>
        /// Jeżeli edytor lub gracz wybrał jeden plik to pierwszy element listy posiada pełną ścieżkę, jeżeli więcej niż jeden to w pierwszym ścieżka do folderu, a reszta nazwy plików
        /// </summary>
        public static List<string> Open (string startPath = "", string fileExtensions = "Text Files ( .txt )\0*.txt", string windowTitle = "", bool allowMultiSelect = true) {
#if UNITY_EDITOR
            return new List<string> () { UnityEditor.EditorUtility.OpenFilePanel (windowTitle, startPath, "") };
#else
            return OpenExplorer (startPath, fileExtensions, windowTitle, allowMultiSelect);
#endif
        }

        private static List<string> OpenExplorer (string startPath = "", string fileExtensions = "Text Files ( .txt )\0*.txt", string windowTitle = "", bool allowMultiSelect = true) {
            var ofn = new OpenFileName ();
            ofn.structSize = Marshal.SizeOf (ofn);
            ofn.filter = fileExtensions + "\0\0";

            IntPtr heapBuf = Marshal.AllocHGlobal (256 * 32 * 2);
            unsafe { *(int*)heapBuf = 0; }
            ofn.file = heapBuf;
            ofn.maxFile = 256 * 32 * 2;

            ofn.fileTitle = new string (new char[64]);
            ofn.maxFileTitle = ofn.fileTitle.Length;
            ofn.title = windowTitle;
            ofn.initialDir = startPath == "" ? Environment.GetFolderPath (Environment.SpecialFolder.Desktop) : startPath;

            if (allowMultiSelect) {
                ofn.flags = OFN_ALLOWMULTISELECT;
            }

            ofn.flags |= OFN_EXPLORER | OFN_FILEMUSTEXIST | OFN_HIDEREADONLY | OFN_NONETWORKBUTTON | OFN_NOREADONLYRETURN;

            List<string> files = new List<string> ();
            if (GetOpenFileNameW (ref ofn)) {

                unsafe {
                    if (!allowMultiSelect || ofn.fileTitle != "") {
                        files.Add (new string ((char*)ofn.file));

                        Marshal.FreeHGlobal (heapBuf);

                        return files;
                    }
                    files.Add (new string ((char*)ofn.file) + '\\');

                    byte* n = (byte*)ofn.file;
                    for (int i = 0; i < ofn.maxFile; i++) {
                        //  System.Diagnostics.Debug.Write (*(char*)n);
                        if (*n == 0) {

                            n += 2;

                            if (*n != 0) {
                                files.Add (new string ((char*)n));
                            } else {
                                break;
                            }

                        }

                        n += 2;
                    }

                }
            } else {
                //don't care
                /*
                                ushort error = CommDlgExtendedError ();
                                switch (error) {
                                    case FNERR_BUFFERTOOSMALL:

                                        break;
                                    case FNERR_SUBCLASSFAILURE:

                                        break;
                                    case FNERR_INVALIDFILENAME:

                                        break;
                                }
                */
                Marshal.FreeHGlobal (heapBuf);
                return new List<string> ();
            }
            Marshal.FreeHGlobal (heapBuf);
            return files;
        }

        [StructLayout (LayoutKind.Sequential)]
        private struct OpenFileName {
            public int      structSize;
            public IntPtr   dlgOwner;
            public IntPtr   instance;
            [MarshalAs (UnmanagedType.LPWStr)]
            public string   filter;
            string          customFilter;
            int             maxCustFilter;
            int             filterIndex;
            public IntPtr   file;
            public int      maxFile;
            [MarshalAs (UnmanagedType.LPWStr)]
            public string   fileTitle;
            public int      maxFileTitle;
            [MarshalAs (UnmanagedType.LPWStr)]
            public string   initialDir;
            [MarshalAs (UnmanagedType.LPWStr)]
            public string   title;
            public uint     flags;
            public short    fileOffset;
            public short    fileExtension;
            [MarshalAs (UnmanagedType.LPWStr)]
            public string   defExt;
            public IntPtr   custData;
            public IntPtr   hook;
            [MarshalAs (UnmanagedType.LPWStr)]
            public string   templateName;
            IntPtr          reservePtr;
            int             reserveInt;
            public int      flagEx;
        }
    }

}
