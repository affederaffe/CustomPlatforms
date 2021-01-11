using System.IO;

using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;

using Zenject;

namespace CustomFloorPlugin.UI {


    internal class NewScriptWarningView : BSMLResourceViewController {

        [Inject]
        private readonly NewScriptWarningFlowCoordinator newScriptWarningFlowCoordinator;


        /// <summary>
        /// Path to the embedded resource
        /// </summary>
        public override string ResourceName => "CustomFloorPlugin.Views.NewScriptWarning.bsml";


        [UIValue("warningText")]
        public const string warningText = "At least one new CustomScript has been detected.\nCustomScripts are a potential risk and may contain Viruses.\nOnly use CustomScripts of trusted sources.\nDo you wish to continue?";


        [UIAction("Continue")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Called by Unity")]
        private void Continue() {
            BeatSaberUI.MainFlowCoordinator.DismissFlowCoordinator(newScriptWarningFlowCoordinator);
            File.WriteAllLines(PlatformLoader.ScriptHashesPath, PlatformLoader.scriptHashList.ToArray());
            PlatformManager.Reload();
        }


        [UIAction("Cancel")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Called by Unity")]
        private void Cancel() {
            PlatformLoader.newScriptsFound = false;
            BeatSaberUI.MainFlowCoordinator.DismissFlowCoordinator(newScriptWarningFlowCoordinator);
        }
    }
}
