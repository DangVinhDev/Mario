using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Coin : MonoBehaviour
{
    public int value = 10;
    public AudioClip pickupSfx;

    void Reset()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
        gameObject.tag = "Coin";
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (ScoreManager.Instance != null)
                ScoreManager.Instance.Add(value);

            if (pickupSfx)
                AudioSource.PlayClipAtPoint(pickupSfx, transform.position);

            Destroy(gameObject);
        }
    }
}
