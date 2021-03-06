﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/*
 * Most of this is a modified version taken from SCANSat. License should be distributed with this code.
 * Any changes were made by Lukas Domagala and are under the project License.
 */
namespace MapResourceOverlay
{
    internal class ScanSatWrapper
    {
        private static ScanSatWrapper _instance;
        public static ScanSatWrapper Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ScanSatWrapper();
                }
                return _instance;
            } 
        }

        private ScanSatWrapper()
        {
            InitializeScansatIntegration();
        }

        private delegate bool IsCoveredDelegate(double lon, double lat, CelestialBody body, int mask);
        private IsCoveredDelegate _scansatIsCoveredDelegate;
        private Type _scansatEnum;
        private int GetScansatId(string resourceName)
        {
            if (_scansatEnum != null)
            {
                return (int)_scansatEnum.GetField(resourceName).GetValue(null);
            }
            return 0;
        }



        private void InitializeScansatIntegration()
        {
            var scanutil = AssemblyLoader.loadedAssemblies.SelectMany(x => x.assembly.GetExportedTypes())
                        .FirstOrDefault(x => x.FullName == "SCANsat.SCANUtil");
            var scandata =
                AssemblyLoader.loadedAssemblies.SelectMany(x => x.assembly.GetExportedTypes())
                    .FirstOrDefault(x => x.FullName == "SCANsat.SCANdata");
            if (scanutil != null && scandata != null)
            {
                var method = scanutil.GetMethod("isCovered",
                    new[] { typeof(double), typeof(double), typeof(CelestialBody), typeof(int) });
                if (method != null)
                {
                    _scansatIsCoveredDelegate = (IsCoveredDelegate)Delegate.CreateDelegate(typeof(IsCoveredDelegate), method);
                }
                _scansatEnum = scandata.GetNestedType("SCANtype");
            }
        }

        public bool Active()
        {
            return _scansatIsCoveredDelegate != null;
        }

        internal static int getBiomeIndex(CelestialBody body, double lon, double lat)
        {
            if (body.BiomeMap == null) return -1;
            if (body.BiomeMap.Map == null) return -1;
            //double u = fixLon(lon);
            //double v = fixLat(lat);

            //if (badDLonLat(u, v))
            //    return -1;
            CBAttributeMap.MapAttribute att = body.BiomeMap.GetAtt(Mathf.Deg2Rad * lat, Mathf.Deg2Rad * lon);
            for (int i = 0; i < body.BiomeMap.Attributes.Length; ++i)
            {
                if (body.BiomeMap.Attributes[i] == att)
                {
                    return i;
                }
            }
            return -1;
        }
        public bool IsCovered(double longitude, double latitude, CelestialBody body, Resource resource)
        {
            if (_scansatIsCoveredDelegate == null)
            {
                return false;
            }
            return _scansatIsCoveredDelegate(longitude, latitude, body, GetScansatId(resource.ScansatName));
        }

        public CBAttributeMap.MapAttribute GetBiome(double longitude, double latitude, CelestialBody body)
        {
            if (body.BiomeMap == null) return null;
            if (body.BiomeMap.Map == null) return body.BiomeMap.defaultAttribute;
            int i = getBiomeIndex(body, longitude, latitude);
            if (i < 0) return body.BiomeMap.defaultAttribute;
            else return body.BiomeMap.Attributes[i];
        }

        public Color32 GetElevationColor32(CelestialBody body, double lon, double lat)
        {
            return heightToColor((float)GetElevation(body, lon, lat), 0, 7500);
        }

        public static double GetElevation(CelestialBody body, double lon, double lat)
        {
            if (body.pqsController == null) return 0;
            double rlon = Mathf.Deg2Rad * lon;
            double rlat = Mathf.Deg2Rad * lat;
            Vector3d rad = new Vector3d(Math.Cos(rlat) * Math.Cos(rlon), Math.Sin(rlat), Math.Cos(rlat) * Math.Sin(rlon));
            return Math.Round(body.pqsController.GetSurfaceHeight(rad) - body.pqsController.radius, 1);
        }
        public static Color heightToColor(float val, double low, double high)
        {
            Color c = Color.black;
            int sealevel = (int)low;
            float max = (float) high;
            if (val <= sealevel)
            {
                val = (Mathf.Clamp(val, -1500, sealevel) + 1500) / 1000f;
                c = Color.Lerp(xkcd_DarkPurple, xkcd_Cerulean, val);
            }
            else
            {
                val = (heightGradient.Length - 2) * Mathf.Clamp(val, sealevel, (sealevel + max)) / (sealevel + max); // 4*val / 7500
                c = Color.Lerp(heightGradient[(int)val], heightGradient[(int)val + 1], val - (int)val);
            }
            return c;
        }
        public static Color xkcd_Amber = XKCDColors.Amber;
        public static Color xkcd_ArmyGreen = XKCDColors.ArmyGreen;
        public static Color xkcd_PukeGreen = XKCDColors.PukeGreen;
        public static Color xkcd_Lemon = XKCDColors.Lemon;
        public static Color xkcd_OrangeRed = XKCDColors.OrangeRed;
        public static Color xkcd_CamoGreen = XKCDColors.CamoGreen;
        public static Color xkcd_Marigold = XKCDColors.Marigold;
        public static Color xkcd_Puce = XKCDColors.Puce;
        public static Color xkcd_DarkTeal = XKCDColors.DarkTeal;
        public static Color xkcd_DarkPurple = XKCDColors.DarkPurple;
        public static Color xkcd_DarkGrey = XKCDColors.DarkGrey;
        public static Color xkcd_LightGrey = XKCDColors.LightGrey;
        public static Color xkcd_PurplyPink = XKCDColors.PurplyPink;
        public static Color xkcd_Magenta = XKCDColors.Magenta;
        public static Color xkcd_YellowGreen = XKCDColors.YellowGreen;
        public static Color xkcd_LightRed = XKCDColors.LightRed;
        public static Color xkcd_Cerulean = XKCDColors.Cerulean;
        public static Color xkcd_Yellow = XKCDColors.Yellow;
        public static Color xkcd_Red = XKCDColors.Red;
        public static Color xkcd_White = XKCDColors.White;
        public static Color[] heightGradient = {
			xkcd_ArmyGreen,
			xkcd_Yellow,
			xkcd_Red,
			xkcd_Magenta,
			xkcd_White,
			xkcd_White
		};

        // XKCD Colors
        // 	(these are collected here for the same reason)


        public bool IsCovered(double longitude, double latitude, CelestialBody body,string str)
        {
            if (!Active())
            {
                return false;
            }
            return _scansatIsCoveredDelegate(longitude, latitude, body, GetScansatId(str));
        }
    }
}
