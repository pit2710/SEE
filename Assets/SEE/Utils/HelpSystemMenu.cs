using SEE.Game.UI.Menu;
using SEE.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

public class HelpSystemMenu : MonoBehaviour
{
    /// <summary>
    /// The name of the PersonalAssistant-GameObject
    /// </summary>
    public const string PersonalAssistant = "PersonalAssistant";

    /// <summary>
    /// The name of the HelpSystem-GameObject
    /// </summary>
    public const string HelpSystem = "HelpSystem";

    public NestedMenu mainMenu;

    public static bool newRendering = false;

    // Start is called before the first frame update
    void Start()
    {
        CreateStartMenu();
    }

    public void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
                if (hit.transform == GameObject.Find(PersonalAssistant).transform)
                {
                    mainMenu.ToggleMenu();
                }
        }
    }

    private void CreateStartMenu()
    {
        mainMenu = GameObject.Find(HelpSystem).AddComponent<NestedMenu>();
        mainMenu.Title = "Test";
        mainMenu.Description = "Testdescription";
        mainMenu.Icon = Resources.Load<Sprite>("Materials/Notification/info");

        ToggleMenuEntry entry1 = new ToggleMenuEntry(
            active: true,
                    entryAction: null,
                    exitAction: null,
                    title: "Entry1",
                    description: "Des Entry1",
                    entryColor: Color.red,
                    icon: Resources.Load<Sprite>("Materials/Notification/info"));
        ToggleMenuEntry entry2 = new ToggleMenuEntry(
    active: true,
            entryAction: null,
            exitAction: null,
            title: "Entry2",
            description: "Des Entry2",
            entryColor: Color.red,
            icon: Resources.Load<Sprite>("Materials/Notification/info"));
        ToggleMenuEntry entry3 = new ToggleMenuEntry(
    active: true,
            entryAction: null,
            exitAction: null,
            title: "Entry3",
            description: "Des Entry3",
            entryColor: Color.red,
            icon: Resources.Load<Sprite>("Materials/Notification/info"));

        List<ToggleMenuEntry> a = new List<ToggleMenuEntry>();
        a.Add(entry1);
        a.Add(entry2);

        NestedMenuEntry entry4 = new NestedMenuEntry(
             innerEntries: a,
                title: "Entry4 ref",
                description: "Des Entry4 ref",
                entryColor: Color.red,
                enabled: true,
                icon: Resources.Load<Sprite>("Materials/Notification/info"));

        mainMenu.AddEntry(entry4);
        mainMenu.AddEntry(entry3);
        mainMenu.ToggleMenu();
    }
}