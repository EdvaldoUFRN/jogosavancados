using UnityEngine;
using UnityEngine.SceneManagement;

public class CaveEntrance : MonoBehaviour
{
    public string caveSceneName; // Nome da cena da caverna

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Pressione 'E' para entrar na caverna.");
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && Input.GetKeyDown(KeyCode.E))
        {
            EnterCave();
        }
    }

    private void EnterCave()
    {
        Debug.Log("Entrando na caverna...");
        SceneManager.LoadScene(caveSceneName);
    }
}
