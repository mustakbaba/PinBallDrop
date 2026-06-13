
using UnityEngine;

public class PersistData : PersistManager<PersistData>
{
  public int CurrentLevel = 1;
  public float Money = 1000;
  public int CurrentBlockerIndex = 0;
  public bool RecentlyBlockerReached;
  public float CurrentBlockerFillAmount = .1f;
  public bool IsSlotTutoShown = false;
  public bool IsBallsTutoShown = false;
}
