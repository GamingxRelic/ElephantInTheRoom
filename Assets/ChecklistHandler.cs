using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChecklistHandler : MonoBehaviour
{
    public static ChecklistHandler instance;

    [System.Serializable]
    public struct Goal
    {
        public string id;
        public string checklist_text;
        public bool optional;

        public List<string> required_goals;     // IDs of goals that must be completed first
        public string fail_override_goal;       // If this goal fails, trigger this one instead
    }

    [SerializeField]
    public List<Goal> all_goals = new List<Goal>();

    private HashSet<string> completed_goals = new HashSet<string>();

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void TriggerGoal(string goal_id)
    {
        if (completed_goals.Contains(goal_id))
            return;

        Goal g = all_goals.Find(x => x.id == goal_id);

        // Check dependencies
        foreach (string required_id in g.required_goals)
        {
            if (!completed_goals.Contains(required_id))
            {
                Debug.LogWarning($"Cannot complete goal '{goal_id}' — required goal '{required_id}' not completed.");
                return;
            }
        }

        completed_goals.Add(goal_id);
        string display_text = !string.IsNullOrEmpty(g.checklist_text) ? g.checklist_text : goal_id;

        Debug.Log("Goal Completed: " + display_text);
        // Optional: trigger UI or sound here
    }

    public void FailGoal(string goal_id)
    {
        // Don't fail a goal that was already completed
        if (completed_goals.Contains(goal_id))
        {
            Debug.Log($"Cannot fail goal '{goal_id}' — it has already been completed.");
            return;
        }

        Goal g = all_goals.Find(x => x.id == goal_id);

        if (!string.IsNullOrEmpty(g.fail_override_goal))
        {
            Debug.Log($"Goal '{goal_id}' failed. Triggering fail override: {g.fail_override_goal}");
            TriggerGoal(g.fail_override_goal);
        }
        else
        {
            Debug.Log($"Goal '{goal_id}' failed. No override specified.");
        }
    }
}
