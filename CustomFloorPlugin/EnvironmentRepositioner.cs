using CustomFloorPlugin.UI;
using System;
using System.Collections.Generic;

using UnityEngine;


namespace CustomFloorPlugin {


    /// <summary>
    /// This class can attempt to move certain parts of the environment to positions these elements were at in Beat Saber's beta<br/>
    /// This is highly flawed and outdated, as it only differentiates objects by name, and many objects in Beat Saber share names<br/>
    /// </summary>
    internal static class EnvironmentArranger {


        /// <summary>
        /// Adjusts the position of objects based on the players choice
        /// </summary>
        internal static void RearrangeEnvironment() {
            if(Settings.EnvArr == EnvArrangement.Classic) {
                RearrangeClassic();
            }
        }

        //TODO: here
        /// <summary>
        /// Attempts to move objects back into positions seen in Beat Saber's beta
        /// </summary>
        private static void RearrangeClassic() {
            TryMove("RotatingLaserLeft0", new Vector3(-8, 0, 45));
            TryMove("RotatingLaserLeft1", new Vector3(-8, 0, 40));
            TryMove("RotatingLaserLeft2", new Vector3(-8, 0, 35));
            TryMove("RotatingLaserLeft3", new Vector3(-8, 0, 30));

            TryMove("RotatingLaserRight0", new Vector3(8, 0, 45));
            TryMove("RotatingLaserRight1", new Vector3(8, 0, 40));
            TryMove("RotatingLaserRight2", new Vector3(8, 0, 35));
            TryMove("RotatingLaserRight3", new Vector3(8, 0, 30));

            TryHide("Light (1)");
            TryHide("Light (2)");
            TryHide("Light (3)");
            TryHide("Light (6)");
        }


        /// <summary>
        /// Attempts to move a <see cref="GameObject"/>
        /// </summary>
        /// <param name="name">Name of the <see cref="GameObject"/></param>
        /// <param name="pos">New position of the <see cref="GameObject"/></param>
        private static void TryMove(string name, Vector3 pos) {
            GameObject toMove = GameObject.Find(name);
            if(toMove != null)
                toMove.transform.position = pos;
        }


        /// <summary>
        /// Attempts to hide a <see cref="GameObject"/>
        /// </summary>
        /// <param name="name">Name of the <see cref="GameObject"/></param>
        private static void TryHide(string name) {
            GameObject toHide = GameObject.Find(name);
            if(toHide != null)
                toHide.SetActive(false);
        }


        /// <summary>
        /// Returns a list of all arrangement modes
        /// </summary>
        internal static List<EnvArrangement> RepositionModes() {
            List<EnvArrangement> list = new List<EnvArrangement>();
            foreach(EnvArrangement item in Enum.GetValues(typeof(EnvArrangement))) {
                list.Add(item);
            }
            return list;
        }


        /// <summary>
        /// All known environment arrangements
        /// </summary>
        internal enum EnvArrangement {
            Default,
            Classic
        };
    }
}