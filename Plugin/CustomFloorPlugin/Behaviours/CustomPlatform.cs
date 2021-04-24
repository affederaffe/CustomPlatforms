using System;

using UnityEngine;


namespace CustomFloorPlugin
{
    public class CustomPlatform : MonoBehaviour, IComparable<CustomPlatform>
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

        [SerializeField] internal string platHash = "";
        [SerializeField] internal string fullPath = "";
        [SerializeField] internal bool isDescriptor = true;

        private void Awake()
        {
            gameObject.SetActive(false);
        }

        public int CompareTo(CustomPlatform? other)
        {
            if (other == null) return 0;
            return string.Compare(platName, other.platName, StringComparison.Ordinal);
        }
    }
}