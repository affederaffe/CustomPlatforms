using UnityEngine;


namespace CustomFloorPlugin
{
    public class PrefabLightmapData : MonoBehaviour
    {
        [SerializeField]
        public Renderer[] m_Renderers;

        [SerializeField]
        public Vector4[] m_LightmapOffsetScales;

        [SerializeField]
        public Texture2D[] m_Lightmaps;
    }
}