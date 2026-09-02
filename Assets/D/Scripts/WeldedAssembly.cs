using UnityEngine;

public class WeldedAssembly : MonoBehaviour
{
    public bool IsWelded { get; private set; }

    public void CompleteWelding()
    {
        AudioManager.instance.PlayOneShot(FMODEvents.instance.soldar, transform.position);
        IsWelded = true;

        Debug.Log("[WELDING] Ensamblaje bloqueado.");
    }
}