using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProgressManager : MonoBehaviour
{
    public LevelListSO levelList;
    private bool[] starsCollected;


    public void Init() {
        starsCollected = PlayerPrefsX.GetBoolArray("StarsCollected", false, 0);
        if (starsCollected.Length != levelList.numStars) {
            ResetStarsCollected();
        }
    }

    // if (Managers.ProgressManager.IsStarCollected(0, 0)) { // stuff }
    public bool IsStarCollected(int levelIndex, int starIndex) {
        return starsCollected[levelIndex * levelList.starsPerLevel + starIndex];
    }


    // Managers.ProgressManager.SetStarCollected(0, 0, true);
    public void SetStarCollected(int levelIndex, int starIndex, bool isCollected) {
        starsCollected[levelIndex * levelList.starsPerLevel + starIndex] = isCollected;
        PlayerPrefsX.SetBoolArray("StarsCollected", starsCollected);
    }


    [ContextMenu("Reset Stars Collected")]
    private void ResetStarsCollected() {
        starsCollected = new bool[levelList.numStars];
        PlayerPrefsX.SetBoolArray("StarsCollected", starsCollected);
    }


}
