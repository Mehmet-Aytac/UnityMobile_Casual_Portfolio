using UnityEngine;

[CreateAssetMenu(fileName = "GateType", menuName = "Gates/New Gate Type")]
public class GateType : ScriptableObject
{
    public string id;
    public GateStats stats;
    public GateFunction gateFunction;

    [HideInInspector] public int idHash;

    void OnValidate()
    {
        idHash = Animator.StringToHash(id);
    }
}