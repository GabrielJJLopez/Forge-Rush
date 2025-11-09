using UnityEngine;
using UnityEngine.EventSystems;

public class DeliveryZone : MonoBehaviour, IDropHandler
{
    public ForgeManager manager;
    public void OnDrop(PointerEventData eventData) { manager.Deliver(); }
}
