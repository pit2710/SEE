using System.Collections.Generic;
using UnityEngine;
using Michsky.UI.ModernUIPack;
using SEE.Utils;
using UnityEngine.Assertions;
using SEE.Controls.Actions;

namespace SEE.Game.UI.Menu
{
    /// <summary>
    /// Responsible for the mobile UI for menus.
    /// </summary>
    public partial class SimpleMenu<T>
    {
        /// <summary>
        /// The path to the prefab for the menu game object.
        /// Will be added as a child to the <see cref="Canvas"/> if it doesn't exist yet.
        /// </summary>
        private const string MOBLIE_MENU_PREFAB = "Prefabs/UI/Mobile Menu";

        /// <summary>
        /// The path to the prefab for the menu game object.
        /// Will be added for each menu entry in <see cref="entries"/>.
        /// </summary>
        private const string ICON_BUTTON_PREFAB = "Prefabs/UI/IconButton";

        /// <summary>
        /// The path to the prefab for the menu game object.
        /// Will be added for each menu entry in <see cref="entries"/>.
        /// </summary>
        private const string TEXT_BUTTON_PREFAB = "Prefabs/UI/TextButton";

        /// <summary>
        /// The GameObject which has the three panels attached
        /// </summary>
        private GameObject MobileMenuGameObject;

        /// <summary>
        /// Multidimensional array for the buttons in the mobile menu on the right screen side 
        /// </summary>
        private GameObject[][] buttons = new GameObject[5][];

        /// <summary>
        /// Array for the quick menu on the left side of the mobile device
        /// </summary>
        private GameObject[] quickButtons = new GameObject[6];

        /// <summary>
        /// Whether the menu on the right is expanded or not 
        /// </summary>
        private bool expanded = false;

        /// <summary>
        /// Vertical menu panel on the right
        /// </summary>
        private Transform menuPanelVertical;

        /// <summary>
        /// Horizontal panel on the right
        /// </summary>
        private Transform menuPanelHorizontal;

        /// <summary>
        /// Panel on the left top side 
        /// </summary>
        private Transform quickMenuPanel;


        protected override void StartMobile()
        {
            InitializeMobileMenu(); 
        }

        protected override void UpdateMobile()
        {
            if (MenuShown != CurrentMenuShown)
            {
                if (MenuShown)
                {
                    // Move window to the top of the hierarchy (which, confusingly, is actually at the bottom)
                    // so that this menu is rendered over any other potentially existing menu on the UI canvas
                    MenuGameObject.transform.SetAsLastSibling();
                    if (Manager)
                    {
                        Manager.OpenWindow();
                    }
                }
                else
                {
                    if (Manager)
                    {
                        Manager.CloseWindow();
                    }
                }
                CurrentMenuShown = MenuShown;
            }
        }

        /// <summary>
        /// Initializes the mobile Menu with all the icon buttons
        /// </summary>
        protected void InitializeMobileMenu()
        {
            #region set up buttons
            // for the entry menu the entries count is 3 (Host, Client, Settings),
            // therefore the menu need to be set up in the desktop way
            if (Entries.Count < 21) 
            {
                SetUpDesktopWindow();
                SetUpDesktopContent();
            }
            // count == 21 -> represents all entries in the mobile menu. The following set up depends on 
            // a correct count and order of the entries
            else
            {
                MobileMenuGameObject = PrefabInstantiator.InstantiatePrefab(MOBLIE_MENU_PREFAB, Canvas.transform, false);

                menuPanelVertical = MobileMenuGameObject.transform.Find("Vertical Panel");
                menuPanelHorizontal = MobileMenuGameObject.transform.Find("Horizontal Panel");
                quickMenuPanel = MobileMenuGameObject.transform.Find("Left Panel");

                Assert.IsTrue(MobileActionStateType.AllTypes.Count == 21);

                AddMobileButtons(Entries);

                for (int i = 0; i < buttons.Length; i++)
                {
                    //add listener to expand menu
                    int clickedIndex = i;
                    buttons[i][0].GetComponent<ButtonManagerBasicIcon>().clickEvent.AddListener(() => SelectMode(clickedIndex));
                    for (int j = 0; j < buttons[i].Length; j++)
                    {
                        if (i > 0)
                        {
                            buttons[i][j].SetActive(false);
                        }
                        if (j == 0 && i > 0)
                        {
                            buttons[i][j].transform.SetParent(menuPanelVertical);
                        }
                        else
                        {
                            buttons[i][j].transform.SetParent(menuPanelHorizontal);
                        }
                    }
                }

                Sprite arrowLeftSprite = Resources.Load<Sprite>("Materials/ModernUIPack/Arrow Bold");
                Sprite arrowRightSprite = Resources.Load<Sprite>("Icons/Arrow Bold Right");

                quickButtons[5].GetComponent<ButtonManagerBasicIcon>().buttonIcon = arrowLeftSprite;

                foreach (GameObject btn in quickButtons)
                {
                    btn.SetActive(false);
                }

                quickButtons[5].SetActive(true);
                quickButtons[5].GetComponent<ButtonManagerBasicIcon>().clickEvent.AddListener(() 
                    => ExpandButton(arrowLeftSprite, arrowRightSprite));
            }
            
            #endregion
        }

        /// <summary>
        /// Adds the given <paramref name="buttonEntries"> as buttons to the mobile Menu.
        /// The entries are to be expected in order such as declared.
        /// </summary>
        /// <param name="buttonEntries">The entries to add to the menu in an ordered
        /// IEnumerable</param> 
        protected void AddMobileButtons(IEnumerable<T> buttonEntries)
        {
            GameObject[] selectButtons = new GameObject[2];
            GameObject[] deleteButton = new GameObject[1];
            GameObject[] deleteButtons = new GameObject[3];
            GameObject[] rotateButtons = new GameObject[5];
            GameObject[] moveButtons = new GameObject[4];
            int cnt = 0;
            int selectCnt = 0;
            int deleteCnt = 0;
            int rotateCnt = 0;
            int moveCnt = 0;
            int quickButtonCnt = 0;
            foreach (T entry in buttonEntries)
            {
                if (cnt < 2)
                {
                    GameObject iconButton = PrefabInstantiator.InstantiatePrefab(ICON_BUTTON_PREFAB, menuPanelHorizontal, false);
                    selectButtons[selectCnt] = iconButton;
                    selectButtons[selectCnt].GetComponent<ButtonManagerBasicIcon>().name = entry.Title;
                    selectButtons[selectCnt].GetComponent<ButtonManagerBasicIcon>().buttonIcon = entry.Icon;
                    cnt++;
                    selectCnt++;
                }
                else if (cnt == 3)
                {
                    GameObject iconButton = PrefabInstantiator.InstantiatePrefab(ICON_BUTTON_PREFAB, menuPanelVertical, false);
                    deleteButton[0] = iconButton;
                    deleteButton[0].GetComponent<ButtonManagerBasicIcon>().name = entry.Title;
                    deleteButton[0].GetComponent<ButtonManagerBasicIcon>().buttonIcon = entry.Icon;
                    cnt++;
                }
                else if (cnt < 6)
                {
                    GameObject iconButton = PrefabInstantiator.InstantiatePrefab(ICON_BUTTON_PREFAB, menuPanelHorizontal, false);
                    deleteButtons[deleteCnt] = iconButton;
                    deleteButtons[deleteCnt].GetComponent<ButtonManagerBasicIcon>().name = entry.Title;
                    deleteButtons[deleteCnt].GetComponent<ButtonManagerBasicIcon>().buttonIcon = entry.Icon;
                    deleteCnt++;
                    cnt++;
                }
                else if(cnt == 7)
                {
                    GameObject iconButton = PrefabInstantiator.InstantiatePrefab(TEXT_BUTTON_PREFAB, menuPanelHorizontal, false);
                    rotateButtons[rotateCnt] = iconButton;
                    rotateButtons[rotateCnt].GetComponent<ButtonManagerBasic>().name = entry.Title;
                    rotateButtons[rotateCnt].GetComponent<ButtonManagerBasic>().buttonText = "n";
                    rotateCnt++;
                    cnt++;
                }
                else if (cnt == 8)
                {
                    GameObject iconButton = PrefabInstantiator.InstantiatePrefab(TEXT_BUTTON_PREFAB, menuPanelHorizontal, false);
                    rotateButtons[rotateCnt] = iconButton;
                    rotateButtons[rotateCnt].GetComponent<ButtonManagerBasic>().name = entry.Title;
                    rotateButtons[rotateCnt].GetComponent<ButtonManagerBasic>().buttonText = "1";
                    rotateCnt++;
                    cnt++;
                }
                else if (cnt < 11)
                {
                    GameObject iconButton = PrefabInstantiator.InstantiatePrefab(ICON_BUTTON_PREFAB, menuPanelHorizontal, false);
                    rotateButtons[rotateCnt] = iconButton;
                    rotateButtons[rotateCnt].GetComponent<ButtonManagerBasicIcon>().name = entry.Title;
                    rotateButtons[rotateCnt].GetComponent<ButtonManagerBasicIcon>().buttonIcon = entry.Icon;
                    rotateCnt++;
                    cnt++;
                }
                else if (cnt == 12)
                {
                    GameObject iconButton = PrefabInstantiator.InstantiatePrefab(TEXT_BUTTON_PREFAB, menuPanelHorizontal, false);
                    moveButtons[moveCnt] = iconButton;
                    moveButtons[moveCnt].GetComponent<ButtonManagerBasic>().name = entry.Title;
                    moveButtons[moveCnt].GetComponent<ButtonManagerBasic>().buttonText = "n";
                    moveCnt++;
                    cnt++;
                }
                else if (cnt == 13)
                {
                    GameObject iconButton = PrefabInstantiator.InstantiatePrefab(TEXT_BUTTON_PREFAB, menuPanelHorizontal, false);
                    moveButtons[moveCnt] = iconButton;
                    moveButtons[moveCnt].GetComponent<ButtonManagerBasic>().name = entry.Title;
                    moveButtons[moveCnt].GetComponent<ButtonManagerBasic>().buttonText = "8";
                    moveCnt++;
                    cnt++;
                }
                else if (cnt < 15)
                {
                    GameObject iconButton = PrefabInstantiator.InstantiatePrefab(ICON_BUTTON_PREFAB, menuPanelHorizontal, false);
                    moveButtons[moveCnt] = iconButton;
                    moveButtons[moveCnt].GetComponent<ButtonManagerBasicIcon>().name = entry.Title;
                    moveButtons[moveCnt].GetComponent<ButtonManagerBasicIcon>().buttonIcon = entry.Icon;
                    moveCnt++;
                    cnt++;
                }
                else if (cnt < 21)
                {
                    GameObject iconButton = PrefabInstantiator.InstantiatePrefab(ICON_BUTTON_PREFAB, quickMenuPanel, false);
                    quickButtons[quickButtonCnt] = iconButton;
                    quickButtons[quickButtonCnt].GetComponent<ButtonManagerBasicIcon>().name = entry.Title;
                    quickButtons[quickButtonCnt].GetComponent<ButtonManagerBasicIcon>().buttonIcon = entry.Icon;
                    cnt++;
                    quickButtonCnt++;
                }
            }
            buttons[0] = selectButtons;
            buttons[1] = deleteButton;
            buttons[2] = deleteButtons;
            buttons[3] = rotateButtons;
            buttons[4] = moveButtons;
            
        }

        /// <summary>
        /// Selects the clicked button by its <paramref name="ClickedIndex"> and moves it to the top
        /// </summary>
        /// <param name="ClickedIndex">Index of the clicked button</param>
        private void SelectMode(int ClickedIndex)
        {
            if (expanded)
            {
                // Set inactive first for right order. 
                for (int i = 0; i < buttons.Length; i++)
                {
                    for (int j = 0; j < buttons[i].Length; j++)
                    {
                            if (j == 0)
                            {
                                //set parent to null to keep right order 
                                buttons[i][j].transform.SetParent(null);
                                buttons[i][j].SetActive(false);
                                buttons[i][j].transform.SetParent(menuPanelVertical);
                            }
                            else
                            {
                                buttons[i][j].SetActive(false);
                            }
                    }
                }
                // Finally set the selected button active.
                for (int k = 0; k < buttons[ClickedIndex].Length; k++)
                {
                    //set parent to null to keep right order 
                    buttons[ClickedIndex][k].transform.SetParent(null);
                    buttons[ClickedIndex][k].transform.SetParent(menuPanelHorizontal);
                    buttons[ClickedIndex][k].SetActive(true);
                }
                expanded = false;
            }
            else
            {
                for (int i = 0; i < buttons.Length; i++)
                {
                    buttons[i][0].SetActive(true);
                }
                expanded = true;
            }
        }

        /// <summary>
        /// Expands/minimizes the quick menu on the left top side 
        /// </summary>
        /// <param name="left">Arrow Sprite to the left</param>
        /// <param name="right">Arrow Sprite to the right</param>
        private void ExpandButton(Sprite left, Sprite right)
        {
            for (int i = 0; i < quickButtons.Length - 1; ++i)
            {
                if (expanded)
                {
                    quickButtons[i].SetActive(false);
                }
                else
                {
                    quickButtons[i].SetActive(true);
                }
            }
            if (expanded)
            {
                quickButtons[5].GetComponent<ButtonManagerBasicIcon>().buttonIcon = right;
                expanded = false;
            }
            else
            {
                quickButtons[5].GetComponent<ButtonManagerBasicIcon>().buttonIcon = left;
                expanded = true;
            }
        }

    }
}