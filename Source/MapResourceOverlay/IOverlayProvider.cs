﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MapResourceOverlay
{
    public interface IOverlayProvider :IConfigNode
    {
        Color32 CalculateColor32(double latitude, double longitude, CelestialBody body, bool useScansat, bool bright, double cutoff);
        OverlayTooltip TooltipContent(double latitude, double longitude, CelestialBody body);
        bool IsCoveredAt(double latitude, double longitude, CelestialBody body);
        string GuiName { get; }
        void Activate(CelestialBody body);
        void Deactivate();
        event EventHandler RedrawRequired;
        void DrawGui(MapOverlayGui gui);
        bool CanActivate();
    }

    public abstract class OverlayProviderBase : IOverlayProvider
    {
        protected CelestialBody _body;

        public virtual void Load(ConfigNode node)
        {
            
        }

        public virtual void Save(ConfigNode node)
        {
            
        }

        public virtual Color32 CalculateColor32(double latitude, double longitude, CelestialBody body, bool useScansat, bool bright,
            double cutoff)
        {
            return new Color32();
        }

        public virtual OverlayTooltip TooltipContent(double latitude, double longitude, CelestialBody body)
        {
            return new OverlayTooltip("",new GUIContent());
        }

        public virtual bool IsCoveredAt(double latitude, double longitude, CelestialBody body)
        {
            return true;
        }

        protected virtual void RequiresRedraw()
        {
            if (RedrawRequired != null)
            {
                RedrawRequired(this, null);
            }
        }

        public virtual string GuiName { get; private set; }
        public virtual void Activate(CelestialBody body)
        {
            _body = body;
            RequiresRedraw();
        }

        public virtual void Deactivate()
        {
            _body = null;
        }

        public event EventHandler RedrawRequired;
        public virtual void DrawGui(MapOverlayGui gui)
        {
            
        }

        public virtual bool CanActivate()
        {
            return true;
        }
    }

    public class OverlayTooltip
    {
        public OverlayTooltip(string title, GUIContent content, Vector2 size = new Vector2())
        {
            Title = title;
            Content = content;
            Size = size;
        }

        public string Title { get; set; }
        public GUIContent Content { get; set; }
        public Vector2 Size { get; set; }
    }

    class HeightmapProvider : OverlayProviderBase
    {
        public byte Alpha { get; set; }

        public override Color32 CalculateColor32(double latitude, double longitude, CelestialBody body, bool useScansat,
            bool bright, double cutoff)
        {
            if (useScansat && ScanSatWrapper.Instance.Active() && !IsCoveredAt(latitude, longitude, body))
            {
                return new Color32(0, 0, 0, 0);
            }
            var scanSat = ScanSatWrapper.Instance;
            var color = scanSat.GetElevationColor32(body, longitude, latitude);
            color.a = Alpha;
            return color;
        }

        public override OverlayTooltip TooltipContent(double latitude, double longitude, CelestialBody body)
        {
            return new OverlayTooltip("",new GUIContent("Height: "+ScanSatWrapper.GetElevation(body, longitude, latitude)+"m"));
        }

        public override bool IsCoveredAt(double latitude, double longitude, CelestialBody body)
        {
            return ScanSatWrapper.Instance.IsCovered(longitude, latitude, body, "AltimetryHiRes");
        }

        public override string GuiName { get { return "Height Map"; } }

        public override void Load(ConfigNode node)
        {
            try
            {
                if (!node.HasNode("Heightmap"))
                {
                    Alpha = 100;
                    return;
                }
                var myNode = node.GetNode("Heightmap");
                byte result;
                if (!Byte.TryParse(myNode.GetValue("Alpha"),out result))
                {
                    result = 100;
                }
                Alpha = result;
            }
            catch (Exception e)
            {
                this.Log("Couldnt Load " + GetType().FullName + " " + e);
            }
        }

        public override void Save(ConfigNode node)
        {
            var myNode = node.AddNode("Heightmap");
            myNode.AddValue("Alpha",Alpha);
        }
    }

    class BiomeOverlayProvider : OverlayProviderBase
    {
        public byte Alpha { get; set; }
        public override Color32 CalculateColor32(double latitude, double longitude, CelestialBody body, bool useScansat,
            bool bright, double cutoff)
        {
            if (useScansat && ScanSatWrapper.Instance.Active() && !IsCoveredAt(latitude, longitude, body))
            {
                return new Color32(0,0,0,0);
            }
            var scanSat = ScanSatWrapper.Instance;
            var biome = scanSat.GetBiome(longitude, latitude, body);
            if (biome != null)
            {
                Color32 color = biome.mapColor;
                color.a = Alpha;
                return color;
            }
            return new Color32(0, 0, 0, 0);
        }

        public override OverlayTooltip TooltipContent(double latitude, double longitude, CelestialBody body)
        {
            var scanSat = ScanSatWrapper.Instance;
            var biome = scanSat.GetBiome(longitude, latitude, body);
            return new OverlayTooltip("",new GUIContent("Biome: "+biome.name));
        }

        public override bool IsCoveredAt(double latitude, double longitude, CelestialBody body)
        {
            return ScanSatWrapper.Instance.IsCovered(longitude, latitude, body, "Biome");
        }

        public override string GuiName { get { return "Biome Map"; } }

        public override void Load(ConfigNode node)
        {
            try
            {
                if (!node.HasNode("Biomemap"))
                {
                    Alpha = 100;
                    return;
                }
                var myNode = node.GetNode("Biomemap");
                byte result;
                if (!Byte.TryParse(myNode.GetValue("Alpha"), out result))
                {
                    result = 100;
                }
                Alpha = result;
            }
            catch (Exception e)
            {
                this.Log("Couldnt Load "+GetType().FullName+ " "+e);
            }
        }

        public override void Save(ConfigNode node)
        {
            var myNode = node.AddNode("Biomemap");
            myNode.AddValue("Alpha", Alpha);
        }
    }
}