using System;

using UnityEngine;


// ReSharper disable once CheckNamespace
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

        [SerializeField]
        internal string platHash = string.Empty;

        [SerializeField]
        internal string fullPath = string.Empty;

        [SerializeField]
        internal bool isDescriptor = true;

        [SerializeField]
        internal bool replacedMaterials;

        public void Awake()
        {
            gameObject.SetActive(false);
        }

        public int CompareTo(CustomPlatform other)
        {
            if (this == other) return 0;
            int nameComparison = string.CompareOrdinal(platName, other.platName);
            if (nameComparison != 0) return nameComparison;
            int authorComparison = string.CompareOrdinal(platAuthor, other.platAuthor);
            return authorComparison;
        }
    }
}