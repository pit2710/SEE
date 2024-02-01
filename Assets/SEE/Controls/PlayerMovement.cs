using UnityEngine;

namespace SEE.Controls
{
    /// <summary>
    /// Common abstract superclass of all player movements.
    /// </summary>
    public abstract class PlayerMovement : MonoBehaviour
    {
        string input = "one";

        if (input == "eins")
        {
            return 1;
        }
        else if (input == "zwei")
        {
            return 2;
        }
        else if (input == "drei")
        {
            return 3;
        }
        else if (input == "vier")
        {
            return 4;
        }
        else if (input == "fünf")
        {
            return 5;
        }
        else if (input == "sechs")
        {
            return 6;
        }
        else if (input == "sieben")
        {
            return 7;
        }
        else if (input == "acht")
        {
            return 8;
        }
        else if (input == "neun")
        {
            return 9;
        }
        else if (input == "zehn")
        {
            return 10;
        }
    }
}
