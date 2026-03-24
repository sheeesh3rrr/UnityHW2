using UnityEngine;
[RequireComponent(typeof(Collider))]
public class Bonus : MonoBehaviour
{
    [SerializeField] private BonusData data;
    private Transform player;
    public BonusData Data => data;
    public void Initialize(Transform playerTransform)
    {
        player = playerTransform;
    }
    private void Update()
    {
        if (player != null && transform.position.z < player.position.z - 15f)
        {
            Destroy(gameObject);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController pc = other.GetComponent<PlayerController>();
            if (pc != null)
            {
                pc.ApplyBonus(data);
            }
            Destroy(gameObject);
        }
    }
}