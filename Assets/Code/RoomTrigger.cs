using UnityEngine;

public class RoomTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            GameManager.Instance.ActivateWavePhase();
            gameObject.SetActive(false);
        }
    }
}