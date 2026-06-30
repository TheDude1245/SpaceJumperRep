using UnityEngine;

public class SaveDebugTester : MonoBehaviour
{
    private void Update()
    {
        if (CurrentSaveManager.Instance == null)
            return;

        if (Input.GetKeyDown(KeyCode.T))
        {
            CurrentSaveManager.Instance.UnlockTrinket();
            Debug.Log("Debug: Unlocked trinket.");
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            CurrentSaveManager.Instance.UnlockCosmetic();
            Debug.Log("Debug: Unlocked cosmetic.");
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            CurrentSaveManager.Instance.SetStoryProgress(25);
            Debug.Log("Debug: Set story progress to 25%.");
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            CurrentSaveManager.Instance.SetStoryProgress(50);
            Debug.Log("Debug: Set story progress to 50%.");
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            CurrentSaveManager.Instance.SetStoryProgress(100);
            Debug.Log("Debug: Set story progress to 100%.");
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            CurrentSaveManager.Instance.SetBonusProgress(15);
            Debug.Log("Debug: Set bonus progress to 15%.");
        }
    }
}