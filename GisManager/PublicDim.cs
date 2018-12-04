using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.DataSourcesGDB;
using Microsoft.Win32;

namespace GisManager
{
    [Guid("875ec57f-39b1-49fb-a600-7b95d8317545")]
    [ClassInterface(ClassInterfaceType.None)]
    [ProgId("GisManager.PublicDim")]
    public class PublicDim
    {
        public static void ReverseMouseWheel()
        {
            try
            {
                RegistryKey setKey = Registry.CurrentUser.OpenSubKey(@"Software\ESRI\ArcMap\Settings", true);
                if (setKey != null)
                {
                    if (setKey.GetValue("ReverseMouseWheel") == null)
                    {
                        setKey.SetValue("ReverseMouseWheel", 0, RegistryValueKind.DWord);
                    }
                    else if (setKey.GetValue("ReverseMouseWheel").ToString() != "0")
                    {
                        setKey.SetValue("ReverseMouseWheel", 0);
                    }
                }
            }
            catch { }
        }

        
    }
}
