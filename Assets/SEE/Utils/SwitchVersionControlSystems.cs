using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Switches the version control system, to the one that the user gives as input to the DiffCity.
/// </summary>
public class SwitchVersionControlSystems
{
    /// <summary>
    /// Returns the functionality of the given version control system.
    /// </summary>
    /// <param name="system">the version control system</param>
    /// <returns>the functionality for the given version control system</returns>
    public static IVersionControl CreateVersionControl(string system)
    {
        switch (system.ToLower())
        {
            case "git":
                return new VersionControlSystems.GitVersionControl();
            case "svn":
                return new VersionControlSystems.SvnVersionControl();
            // Add cases for other version control systems
            default:
                throw new ArgumentException("Unsupported version control system", nameof(system));
        }
    }
}
