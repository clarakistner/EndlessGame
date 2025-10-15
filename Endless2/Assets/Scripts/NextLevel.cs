using UnityEngine;
using UnityEngine.SceneManagement;

public class NextLevel : MonoBehaviour
{
    public string nomeProximaCena = "Level2";

    public void Acionar()
    {
        Time.timeScale = 1f;                // garante que a próxima cena não entra pausada
        SceneManager.LoadScene(nomeProximaCena);
    }
}
