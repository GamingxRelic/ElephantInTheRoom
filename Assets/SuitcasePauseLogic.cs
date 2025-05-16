using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SuitcasePauseLogic : MonoBehaviour
{
    [SerializeField] private PauseMenuLogic pause_logic;
    [SerializeField] private Image image;
    [SerializeField] private Sprite suitcase_opened;
    public void OnSuitcaseOpened()
    {
        pause_logic.ShowButtons();
        image.sprite = suitcase_opened;
    }
}
