using UnityEngine;
using UnityEngine.SceneManagement;

public class NextLevel : MonoBehaviour
{
    public string nomeProximaCena = "Level2";

    public void Acionar()
    {
        Time.timeScale = 1f;                // garante que a pr�xima cena n�o entra pausada
        SceneManager.LoadScene(nomeProximaCena);
    }
}
