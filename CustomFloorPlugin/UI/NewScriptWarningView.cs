using System.IO;

using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;

namespace CustomFloorPlugin.UI {


    [System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1812:Avoid unistantiated internal classes", Justification = "Instantiated by Unity")]
    internal class NewScriptWarningView : BSMLResourceViewController {


        /// <summary>
        /// Path to the embedded resource
        /// </summary>
        public override string ResourceName => "CustomFloorPlugin.UI.NewScriptWarning.bsml";


        [UIValue("warningText")]
        public const string warningText = "At least one new CustomScript has been detected.\nCustomScripts are a potential risk and may contain Viruses.\nOnly use CustomScripts of trusted sources.\nDo you wish to continue?";


        [UIAction("Continue")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Called by BSML")]
        private void Continue() {
            BeatSaberUI.MainFlowCoordinator.DismissFlowCoordinator(PlatformUI.NewScriptWarningFlowCoordinator);
            File.WriteAllLines(PlatformLoader.scriptHashesPath, PlatformLoader.scriptHashList.ToArray());
            PlatformManager.Reload();
        }


        [UIAction("Cancel")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Called by BSML")]
        private void Cancel() {
            PlatformLoader.newScriptsFound = false;
            BeatSaberUI.MainFlowCoordinator.DismissFlowCoordinator(PlatformUI.NewScriptWarningFlowCoordinator);
        }
    }
}
