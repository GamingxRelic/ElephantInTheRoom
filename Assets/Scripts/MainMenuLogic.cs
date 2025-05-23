using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuLogic : MonoBehaviour
{

    [SerializeField] RectTransform tie_anchor;
    [SerializeField] GameObject main_menu_buttons;
    [SerializeField] GameObject options_menu;

    private bool main_menu_visible = true; // Flag to track if the main menu is visible
    private bool options_menu_visible = false; // Flag to track if the options menu is visible

    [SerializeField] TMP_Text start_text;

    private void Start()
    {
        // Set the main menu buttons active and options menu inactive at the start
        main_menu_buttons.SetActive(main_menu_visible);
        options_menu.SetActive(options_menu_visible);
    }

    public void StartGameButtonPressed() // Start game and go to level 01
    {
        start_text.text = "Loading...";
        // Load the first level
        SceneManager.LoadScene("Level01");
    }

    public void OptionsMenuButtonPressed()
    {
        main_menu_buttons.SetActive(false); // Hide main menu buttons
        options_menu.SetActive(true); // Show options menu
    }

    public void OnQuitButtonPressed()
    {
        // Quit the application
        Application.Quit();
        print("Quit game");
    }

    private void Update()
    {
        if (tie_anchor != null)
        {
            Vector2 localMousePos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                tie_anchor.parent as RectTransform, 
                Input.mousePosition,
                null,
                out localMousePos
            );

            Vector2 direction = localMousePos - ((RectTransform)tie_anchor).anchoredPosition;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            tie_anchor.rotation = Quaternion.Euler(0, 0, angle);
        }

    }


    // Options Menu
    public void OnOptionsBackButtonPressed()
    {
        options_menu.SetActive(false); // Hide options menu
        main_menu_buttons.SetActive(true); // Show main menu buttons
    }



}

