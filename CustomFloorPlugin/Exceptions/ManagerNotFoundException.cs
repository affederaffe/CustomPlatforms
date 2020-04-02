using System;

namespace CustomFloorPlugin.Exceptions {


    /// <summary>
    /// The manager class you were looking for does not appear to be instantiated right now
    /// </summary>
    public class ManagerNotFoundException:Exception {
        internal ManagerNotFoundException() :
            base("No such Manager could be found!") {

        }
        internal ManagerNotFoundException(Exception e) :
            base("No such Manager could be found!", e) {

        }
    }
}
