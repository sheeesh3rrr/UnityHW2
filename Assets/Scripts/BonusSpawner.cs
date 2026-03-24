using UnityEngine;
public class BonusSpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] bonusPrefabs;
    [SerializeField] private float spawnInterval = 4f;
    [SerializeField] private float spawnDistance = 35f;
    [SerializeField] private float[] lanes;
    private float timer;
    private Transform player;
    private void Start()
    {
        player = GetComponentInParent<Transform>();
    }
    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            Spawn();
            timer = 0f;
        }
    }
    private void Spawn()
    {
        int lane = Random.Range(0, lanes.Length);
        int index = Random.Range(0, bonusPrefabs.Length);
        Vector3 pos = new Vector3(
        lanes[lane],
        1.5f,
        player.position.z + spawnDistance
        );
        GameObject obj = Instantiate(bonusPrefabs[index], pos,
       Quaternion.identity);
        obj.GetComponent<Bonus>().Initialize(player);
    }
}