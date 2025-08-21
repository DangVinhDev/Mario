using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverOnFall : MonoBehaviour
{
    public float minY = -20f;

    void Update()
    {
        if (transform.position.y < minY)
        {
            Scene current = SceneManager.GetActiveScene();
            SceneManager.LoadScene(current.buildIndex);
        }
    }
}
