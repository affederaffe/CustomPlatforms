using System.Collections.Generic;

using UnityEngine;

using Zenject;


namespace CustomFloorPlugin
{
    /// <summary>
    /// Instantiable wrapper class for <see cref="SpectrogramAnimationState"/>s that handles registering and de-registering, as well setting up and updating visuals of the <see cref="Spectrogram"/>
    /// </summary>
    internal class SpectrogramAnimationStateManager : MonoBehaviour
    {
        [InjectOptional]
        private readonly BasicSpectrogramData _basicSpectrogramData;

        /// <summary>
        /// <see cref="List{T}"/> of all known <see cref="SpectrogramAnimationState"/>s loaded in the game
        /// </summary>
        private List<SpectrogramAnimationState> animationStates;

        /// <summary>
        /// Updates the Provider for Spectogram Data when this object becomes active<br/>
        /// [Unity calls this when the <see cref="MonoBehaviour"/> becomes active in the hierachy]
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Called by Unity")]
        private void OnEnable()
        {
            UpdateSpectrogramDataProvider();
        }

        /// <summary>
        /// Updates the list of known <see cref="SpectrogramAnimationState"/>s
        /// </summary>
        internal void UpdateAnimationStates()
        {
            animationStates = new List<SpectrogramAnimationState>();

            foreach (SpectrogramAnimationState spec in Resources.FindObjectsOfTypeAll<SpectrogramAnimationState>())
            {
                animationStates.Add(spec);
            }
        }

        /// <summary>
        /// Passes <see cref="BasicSpectrogramData"/> on to all <see cref="SpectrogramAnimationState"/>s
        /// </summary>
        internal void UpdateSpectrogramDataProvider()
        {
            if (_basicSpectrogramData != null)
            {
                foreach (SpectrogramAnimationState specAnim in animationStates)
                {
                    specAnim.SetData(_basicSpectrogramData);
                }
            }
        }
    }
}