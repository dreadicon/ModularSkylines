using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ModularSkylines
{
    //This class handles how decorations are managed.
    public abstract class DecorationAI
    {
        public abstract string name();
        public abstract void GetDecorationArea(out int width, out int length, out float offset, BuildingInfo info);
        public abstract void GetDecorationDirections(out bool negX, out bool posX, out bool negZ, out bool posZ, BuildingInfo info);
    }

    public class VanillaGrowableDecorationAI : DecorationAI
    {
        public override string name()
        {
            return "Vanilla Growable";
        }

        public override void GetDecorationArea(out int width, out int length, out float offset, BuildingInfo info)
        {
            width = info.m_cellWidth;
            length = ((info.m_zoningMode != BuildingInfo.ZoningMode.Straight) ? info.m_cellLength : 4);
            offset = (float)(length - info.m_cellLength) * 4f;
            if (!info.m_expandFrontYard)
            {
                offset = -offset;
            }
        }

        public override void GetDecorationDirections(out bool negX, out bool posX, out bool negZ, out bool posZ, BuildingInfo info)
        {
            negX = (info.m_zoningMode == BuildingInfo.ZoningMode.CornerRight);
            posX = (info.m_zoningMode == BuildingInfo.ZoningMode.CornerLeft);
            negZ = false;
            posZ = true;
        }
    }
}
