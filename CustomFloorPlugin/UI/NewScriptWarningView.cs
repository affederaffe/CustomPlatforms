using System.IO;

using BS_Utils.Utilities;

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
            BeatSaberUI.MainFlowCoordinator.DismissFlowCoordinator(PlatformUI.NewScriptWarningFlowCoordinator, null, AnimationDirection.Horizontal, false);  
            File.WriteAllLines(PlatformLoader.scriptHashesPath, PlatformLoader.scriptHashList.ToArray());
            PlatformManager.Reload();
            BeatSaberUI.MainFlowCoordinator.GetField<MenuTransitionsHelper>("_menuTransitionsHelper").RestartGame();
        }


        [UIAction("Cancel")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Called by BSML")]
        private void Cancel() {
            BeatSaberUI.MainFlowCoordinator.DismissFlowCoordinator(PlatformUI.NewScriptWarningFlowCoordinator, null, AnimationDirection.Horizontal, false);
            BeatSaberUI.MainFlowCoordinator.GetField<MenuTransitionsHelper>("_menuTransitionsHelper").RestartGame();
        }
    }
}
