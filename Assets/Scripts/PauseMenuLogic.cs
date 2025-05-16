using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenuLogic : MonoBehaviour
{
    public GameObject pause_menu_ui;

    public static bool is_paused = false;

    bool options_menu_open = false;

    private PlayerInputActions player_input;
    private InputAction pause;

    public GameObject main_options_ui;
    public GameObject options_menu_ui;

    [SerializeField] private Animator anim;


    private void Awake()
    {
        player_input = new PlayerInputActions();
    }

    private void Start()
    {
        pause_menu_ui.SetActive(false);
    }

    private void OnEnable()
    {
        pause = player_input.Player.Pause;
        pause.Enable();
        pause.performed += _ => Pause();
    }

    private void OnDisable()
    {
        pause.Disable();
        pause.performed -= _ => Pause();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (is_paused)
                Resume();
            else
                Pause();
        }
    }

    public void Resume()
    {
        pause_menu_ui.SetActive(false);
        is_paused = false;
    }

    public void Pause()
    {
        if (is_paused)
        {
            Resume();
            return;
        }

        else if (options_menu_open)
        {
            options_menu_ui.SetActive(false);
            anim.SetTrigger("Open");
            options_menu_open = false;
        }

        pause_menu_ui.SetActive(true);
        main_options_ui.SetActive(false);
        options_menu_ui.SetActive(false);
        is_paused = true;
    }

    public void QuitToMainMenu()
    {
        is_paused = false;
        SceneManager.LoadScene("MainMenu");
    }

    public void OpenOptionsMenu()
    {
        options_menu_ui.SetActive(true);
        main_options_ui.SetActive(false);
        options_menu_open = true;
    }

    public void CloseOptionsMenu()
    {
        options_menu_ui.SetActive(false);
        main_options_ui.SetActive(true);
        options_menu_open = false;
    }

    public void ShowButtons()
    {
        main_options_ui.SetActive(true);
    }



}
