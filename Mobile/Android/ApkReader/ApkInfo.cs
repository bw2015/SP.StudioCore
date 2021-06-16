using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SP.StudioCore.Mobile.Android.ApkReader
{
    public class ApkInfo
    {
        public static int FINE = 0;
        public static int NULL_VERSION_CODE = 1;
        public static int NULL_VERSION_NAME = 2;
        public static int NULL_PERMISSION = 3;
        public static int NULL_ICON = 4;
        public static int NULL_CERT_FILE = 5;
        public static int BAD_CERT = 6;
        public static int NULL_SF_FILE = 7;
        public static int BAD_SF = 8;
        public static int NULL_MANIFEST = 9;
        public static int NULL_RESOURCES = 10;
        public static int NULL_DEX = 13;
        public static int NULL_METAINFO = 14;
        public static int BAD_JAR = 11;
        public static int BAD_READ_INFO = 12;
        public static int NULL_FILE = 15;
        public static int HAS_REF = 16;



        public string label;
        public string versionName;
        public string versionCode;
        public string minSdkVersion;
        public string targetSdkVersion;
        public string packageName;
        public string debuggable;
        public List<string> Permissions;
        public List<string> iconFileName;
        public List<string> iconFileNameToGet;
        public List<string> iconHash;
        public string resourcesFileName;
        public byte[] resourcesFileBytes;
        public bool hasIcon;
        public bool supportSmallScreens;
        public bool supportNormalScreens;
        public bool supportLargeScreens;
        public bool supportAnyDensity;
        public Dictionary<string, List<string>> resStrings;
        public Dictionary<string, string> layoutStrings;

        public bool IsDebuggable
        {
            get
            {
                if (debuggable == null) return false; // debugabble is not in the manifest
                if (debuggable.Equals("-1")) return true; // debuggable == true
                else return false;
            }
        }

        public static bool supportSmallScreen(byte[] dpi)
        {
            if (dpi[0] == 1)
                return true;
            return false;
        }

        public static bool supportNormalScreen(byte[] dpi)
        {
            if (dpi[1] == 1)
                return true;
            return false;
        }

        public static bool supportLargeScreen(byte[] dpi)
        {
            if (dpi[2] == 1)
                return true;
            return false;
        }
        //public byte[] getDPI()
        //{
        //    byte[] dpi = new byte[3];
        //    if (this.supportAnyDensity)
        //    {
        //        dpi[0] = 1;
        //        dpi[1] = 1;
        //        dpi[2] = 1;
        //    }
        //    else
        //    {
        //        if (this.supportSmallScreens)
        //            dpi[0] = 1;
        //        if (this.supportNormalScreens)
        //            dpi[1] = 1;
        //        if (this.supportLargeScreens)
        //            dpi[2] = 1;
        //    }
        //    return dpi;
        //}

        public ApkInfo()
        {
            hasIcon = false;
            supportSmallScreens = false;
            supportNormalScreens = false;
            supportLargeScreens = false;
            supportAnyDensity = true;
            versionCode = null;
            versionName = null;
            iconFileName = null;
            iconFileNameToGet = null;

            Permissions = new List<string>();
        }

        private bool isReference(List<string> strs)
        {
            foreach (string str in strs)
            {
                if (isReference(str))
                    return true;
            }
            return false;
        }

        private bool isReference(string str)
        {
            if (str != null && str.StartsWith("@"))
            {
                int.Parse(str, System.Globalization.NumberStyles.HexNumber);
                return true;
            }
            return false;
        }
    }
}