using UnityEngine;


namespace CustomFloorPlugin
{
    public class CustomPlatform : MonoBehaviour
    {
        [Header("Platform Description")]
        public string platName = "MyCustomPlatform";
        public string platAuthor = "MyName";
        public Sprite? icon;
        [Space]
        [Header("Hide Environment")]
        public bool hideHighway;
        public bool hideTowers;
        public bool hideDefaultPlatform;
        public bool hideEQVisualizer;
        public bool hideSmallRings;
        public bool hideBigRings;
        public bool hideBackColumns;
        public bool hideBackLasers;
        public bool hideDoubleColorLasers;
        public bool hideRotatingLasers;
        public bool hideTrackLights;
    }
}