namespace ModularSkylines
{
    //Class which determines the fire hazard of building. override it to use a differnet calculation
    public class FireAI
    {
        public void GetFireHazard(ushort buildingID, ref Building buildingData, out int fireHazard)
        {
            fireHazard = (int)(((buildingData.m_flags & Building.Flags.Active) != Building.Flags.None) ? buildingData.m_fireHazard : 0);
        }
    }
}