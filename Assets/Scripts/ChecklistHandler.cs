using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


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

    [SerializeField] private TMP_Text checklist_display;


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

    private void Start()
    {
        UpdateChecklistUI();
    }

    public void TriggerGoal(string goal_id)
    {
        if (completed_goals.Contains(goal_id))
            return;

        if(goal_id == "final_goal" && !AreAllRequiredGoalsCompleted())
            return;

        Goal g = all_goals.Find(x => x.id == goal_id);

        // Check dependencies
        foreach (string required_id in g.required_goals)
        {
            if (!completed_goals.Contains(required_id))
            {
                //Debug.LogWarning($"Cannot complete goal '{goal_id}' — required goal '{required_id}' not completed.");
                return;
            }
        }

        completed_goals.Add(goal_id);
        string display_text = !string.IsNullOrEmpty(g.checklist_text) ? g.checklist_text : goal_id;

        // Check if all non-optional goals are completed (excluding final_goal)
        



        //Debug.Log("Goal Completed: " + display_text);
        UpdateChecklistUI();
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

    public bool AreAllRequiredGoalsCompleted()
    {
        foreach (Goal goal in all_goals)
        {
            if (!goal.optional && goal.id != "final_goal" && !completed_goals.Contains(goal.id))
            {
                return false;
            }
        }
        return true;
    }


    private void UpdateChecklistUI()
    {
        List<string> checklist_lines = new List<string>();

        // Show required goals first
        foreach (Goal g in all_goals)
        {
            bool is_completed = completed_goals.Contains(g.id);
            bool is_failed = !string.IsNullOrEmpty(g.fail_override_goal) && completed_goals.Contains(g.fail_override_goal);

            if (!g.optional)
            {
                if (is_completed)
                {
                    checklist_lines.Add($"<color=green><s>{g.checklist_text}</s></color>");
                }
                else if (is_failed)
                {
                    checklist_lines.Add($"<color=red><s>{g.checklist_text}</s></color>");
                }
                else
                {
                    if (!AreAllRequiredGoalsCompleted() && g.id == "final_goal")
                        continue;
                    else
                        checklist_lines.Add($"{g.checklist_text}");
                }
            }
        }

        // Then show completed optional goals only
        foreach (Goal g in all_goals)
        {
            if (g.optional && completed_goals.Contains(g.id))
            {
                checklist_lines.Add($"<color=green><s>(Optional) {g.checklist_text}</s></color>");
            }
        }

        checklist_display.text = string.Join("\n", checklist_lines);
    }




}
