using UnityEngine;
using UnityEngine.SceneManagement;

public class NextLevelDoorInteraction : MonoBehaviour
{
    // Reference to next Scene
    [SerializeField] private string next_scene_name;

    public void OnInteract()
    {
        // Check if the player has completed the required goals
        if (ChecklistHandler.instance.AreAllRequiredGoalsCompleted())
        {
            // Load the next level
            LoadScene();
        }
    }

    private void LoadScene()
    {
        if (!string.IsNullOrEmpty(next_scene_name))
        {
            if (Application.CanStreamedLevelBeLoaded(next_scene_name))
            {
                SceneManager.LoadScene(next_scene_name);
            }
            else
            {
                Debug.LogError($"Scene '{next_scene_name}' is not in Build Settings or has a typo.");
            }
        }
        else
        {
            Debug.LogWarning("No scene name assigned to 'scene_to_load' in NextLevelDoorInteraction.");
        }
    }

}
