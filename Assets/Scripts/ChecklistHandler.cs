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
        public bool hidden_until_completed;

        public List<string> required_goals;     // IDs of goals that must be completed first
        public string fail_override_goal;       // If this goal fails, trigger this one instead

    }

    [SerializeField]
    public List<Goal> all_goals = new List<Goal>();

    private HashSet<string> completed_goals = new HashSet<string>();

    private int current_page = 0;
    private const int goals_per_page = 5;

    [SerializeField] private List<TMP_Text> checklist_text_slots; // 5 text objects for displaying task names
    [SerializeField] private List<GameObject> checkmarks; // 5 checkmark game objects

    [SerializeField] private AudioSource scribble_sound;
    [SerializeField] private float delay_before_check = 0.7f;
    [SerializeField] private float delay_before_reorder = 1.15f;


    [SerializeField] private ChecklistAnim checklist_anim;

    // Buttons
    [SerializeField] private GameObject next_page_button;
    [SerializeField] private GameObject previous_page_button;


    private void Update()
    {
        if (Input.GetKey(KeyCode.Alpha2))
        {
            ShowNextPage();
        }
        else if (Input.GetKey(KeyCode.Alpha1))
        {
            ShowPreviousPage();
        }
    }


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
        scribble_sound = GetComponent<AudioSource>();
        checklist_anim = GetComponent<ChecklistAnim>();
        UpdateChecklistUI();
    }

    public void TriggerGoal(string goal_id)
    {
        // Already completed? Do nothing
        if (completed_goals.Contains(goal_id))
            return;

        Goal g = all_goals.Find(x => x.id == goal_id);

        // Check dependencies
        foreach (string required_id in g.required_goals)
        {
            if (!completed_goals.Contains(required_id))
                return;
        }

        // Final goal dependency check
        if (goal_id == "final_goal" && !AreAllRequiredGoalsCompleted())
            return;

        StartCoroutine(CompleteAndReorderGoal(goal_id));
    }

    private IEnumerator CompleteAndReorderGoal(string goal_id)
    {
        StartCoroutine(checklist_anim.ShowThenHide());

        // Scroll to page containing this goal before marking it
        current_page = GetPageIndexForGoal(goal_id);
        UpdateChecklistUI();

        completed_goals.Add(goal_id);

        // Wait BEFORE visually marking goal as completed
        yield return new WaitForSeconds(delay_before_check);
        UpdateChecklistUI();
        scribble_sound.Play();

        // Wait again BEFORE updating checklist
        yield return new WaitForSeconds(delay_before_reorder);
        SortCompletedGoalsToBottom();

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
            if (goal.optional || goal.id == "final_goal")
                continue;

            bool is_completed = completed_goals.Contains(goal.id);
            bool is_fail_override_completed = !string.IsNullOrEmpty(goal.fail_override_goal)
                                              && completed_goals.Contains(goal.fail_override_goal);

            if (!is_completed && !is_fail_override_completed)
            {
                return false;
            }
        }

        return true;
    }

   


    private List<Goal> GetVisibleGoalsRaw() // Get all visible goals, including completed ones
    {
        // Filter out hidden goals and the final goal if not all required goals are completed
        {
            List<Goal> visible = new List<Goal>();

            foreach (var goal in all_goals)
            {
                bool is_completed = completed_goals.Contains(goal.id);

                if (goal.hidden_until_completed && !is_completed)
                    continue;

                if (goal.id == "final_goal" && !AreAllRequiredGoalsCompleted())
                    continue;

                visible.Add(goal);
            }

            return visible;
        }
    }


    private List<Goal> GetVisibleGoalsInOrder() // Get all visible goals, including completed ones, in the correct order (completed at the bottom)
    {
        List<Goal> incomplete_visible = new List<Goal>();
        List<Goal> completed_visible = new List<Goal>();

        foreach (var goal in all_goals)
        {
            bool is_completed = completed_goals.Contains(goal.id);

            if (goal.hidden_until_completed && !is_completed)
                continue;

            if (goal.id == "final_goal" && !AreAllRequiredGoalsCompleted())
                continue;

            if (is_completed)
                completed_visible.Add(goal);
            else
                incomplete_visible.Add(goal);
        }

        List<Goal> ordered = new List<Goal>();
        ordered.AddRange(incomplete_visible);
        ordered.AddRange(completed_visible);
        return ordered;
    }

    public void SortCompletedGoalsToBottom()
    {
        List<Goal> incomplete = new List<Goal>();
        List<Goal> completed = new List<Goal>();

        foreach (var goal in all_goals)
        {
            if (completed_goals.Contains(goal.id))
                completed.Add(goal);
            else
                incomplete.Add(goal);
        }

        all_goals.Clear();
        all_goals.AddRange(incomplete);
        all_goals.AddRange(completed);
    }



    private void UpdateChecklistUI()
    {
        List<Goal> visible_goals = GetVisibleGoalsRaw();

        int start_index = current_page * goals_per_page;

        for (int i = 0; i < goals_per_page; i++)
        {
            int goal_index = start_index + i;

            if (goal_index < visible_goals.Count)
            {
                Goal goal = visible_goals[goal_index];
                checklist_text_slots[i].text = goal.checklist_text;
                checkmarks[i].SetActive(completed_goals.Contains(goal.id));
            }
            else
            {
                checklist_text_slots[i].text = "";
                checkmarks[i].SetActive(false);
            }
        }
        UpdatePageButtons();
    }


    private void UpdatePageButtons()
    {
        int max_page = GetMaxPageIndex();

        bool has_multiple_pages = max_page > 0;

        next_page_button.SetActive(has_multiple_pages && current_page < max_page);
        previous_page_button.SetActive(current_page > 0);
    }

    public void ResetChecklistPage()
    {
        current_page = 0;
        UpdateChecklistUI();
    }

    private int GetPageIndexForGoal(string goal_id)
    {
        List<Goal> visible_goals = GetVisibleGoalsRaw();

        for (int i = 0; i < visible_goals.Count; i++)
        {
            if (visible_goals[i].id == goal_id)
                return i / goals_per_page;
        }

        return 0;
    }
    public void ShowNextPage()
    {
        int max_page = GetMaxPageIndex();
        if (current_page < max_page)
        {
            current_page++;
            UpdateChecklistUI();
        }
    }

    public void ShowPreviousPage()
    {
        if (current_page > 0)
        {
            current_page--;
            UpdateChecklistUI();
        }
    }

    private int GetMaxPageIndex()
    {
        int total_visible = GetVisibleGoalsInOrder().Count;
        return Mathf.Max(0, (total_visible - 1) / goals_per_page);
    }

}
