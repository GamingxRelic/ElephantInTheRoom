using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuLogic : MonoBehaviour
{

    [SerializeField] Transform tie_anchor;
    [SerializeField] GameObject main_menu_buttons;
    [SerializeField] GameObject options_menu;

    private bool main_menu_visible = true; // Flag to track if the main menu is visible
    private bool options_menu_visible = false; // Flag to track if the options menu is visible

    private void Start()
    {
        // Set the main menu buttons active and options menu inactive at the start
        main_menu_buttons.SetActive(main_menu_visible);
        options_menu.SetActive(options_menu_visible);
    }

    public void StartGameButtonPressed() // Start game and go to level 01
    {
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
        // Rotate tie_anchor towards mouse position
        if (tie_anchor != null)
        {
            Vector3 mousePosition = Input.mousePosition;
            mousePosition.z = 0;
            Vector3 worldMousePosition = Camera.main.ScreenToWorldPoint(mousePosition);

            Vector3 direction = worldMousePosition - tie_anchor.position;
            direction.z = 0; 

            tie_anchor.up = direction.normalized; // Rotate tie_anchor to face the mouse
        }
    }


    // Options Menu
    public void OnOptionsBackButtonPressed()
    {
        options_menu.SetActive(false); // Hide options menu
        main_menu_buttons.SetActive(true); // Show main menu buttons
    }



}

