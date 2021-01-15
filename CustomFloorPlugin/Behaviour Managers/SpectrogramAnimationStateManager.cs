using System.Collections.Generic;
using System.Linq;

using BS_Utils.Utilities;

using UnityEngine;


namespace CustomFloorPlugin {


    /// <summary>
    /// Instantiable wrapper class for <see cref="SpectrogramAnimationState"/>s that handles registering and de-registering, as well setting up and updating visuals of the <see cref="Spectrogram"/>
    /// </summary>
    internal class SpectrogramAnimationStateManager : MonoBehaviour {


        /// <summary>
        /// <see cref="List{T}"/> of all known <see cref="SpectrogramAnimationState"/>s loaded in the game
        /// </summary>
        private List<SpectrogramAnimationState> animationStates;


        /// <summary>
        /// Updates the Provider for Spectogram Data when this object becomes active<br/>
        /// [Unity calls this when the <see cref="MonoBehaviour"/> becomes active in the hierachy]
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Called by Unity")]
        private void OnEnable() {
            UpdateSpectrogramDataProvider();
        }


        /// <summary>
        /// Updates the list of known <see cref="SpectrogramAnimationState"/>s
        /// </summary>
        internal void UpdateAnimationStates() {
            animationStates = new List<SpectrogramAnimationState>();

            foreach (SpectrogramAnimationState spec in Resources.FindObjectsOfTypeAll<SpectrogramAnimationState>()) {
                animationStates.Add(spec);
            }
        }


        /// <summary>
        /// Passes <see cref="BasicSpectrogramData"/> on to all <see cref="SpectrogramAnimationState"/>s
        /// </summary>
        internal void UpdateSpectrogramDataProvider() {
            BasicSpectrogramData[] datas = Resources.FindObjectsOfTypeAll<BasicSpectrogramData>();
            if (datas.Length != 0) {
                BasicSpectrogramData spectrogramData = datas.FirstOrDefault(x => ((AudioSource)x.GetField("_audioSource")).clip != null);
                if (spectrogramData != null) {
                    foreach (SpectrogramAnimationState specAnim in animationStates) {
                        specAnim.SetData(spectrogramData);
                    }
                }
            }
        }
    }
}