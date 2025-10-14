using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class NextLevel : MonoBehaviour
{
    [Header("Nome da próxima cena")]
    public string nomeProximaCena;

    [Header("Tempo antes de mudar de cena")]
    public float delayProximaFase = 2f;


    public void Acionar()
    {
        StartCoroutine(IrParaProximaFase());
    }

    private IEnumerator IrParaProximaFase()
    {
        yield return new WaitForSeconds(delayProximaFase);

        if (!string.IsNullOrEmpty(nomeProximaCena))
        {
            SceneManager.LoadScene(nomeProximaCena);
        }
        else
        {
            Debug.LogWarning("Nenhum nome de cena definido em NextLevel!");
        }
    }
}