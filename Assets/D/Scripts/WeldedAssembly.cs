using UnityEngine;

public class WeldedAssembly : MonoBehaviour
{
    public bool IsWelded { get; private set; }

    public void CompleteWelding()
    {
        IsWelded = true;

        Debug.Log("[WELDING] Ensamblaje bloqueado.");
    }
}