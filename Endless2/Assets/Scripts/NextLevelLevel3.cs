using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public class NextLevelLevel3 : MonoBehaviour
{
    [Header("Cena de destino")]
    [Tooltip("Nome exato da cena na Build Settings (File -> Build Settings -> Scenes In Build)")]
    public string nomeProximaCena = "Level3";

    [Header("Opções")]
    [Tooltip("Carrega de forma assíncrona (recomendado)")]
    public bool usarCarregamentoAssincrono = true;

    [Tooltip("Evita acionar duas vezes enquanto carrega")]
    public bool bloquearReentradas = true;

    private bool _carregando = false;

    /// <summary>
    /// Chame este método (por botão UI, trigger, etc.) para ir ao Level3.
    /// </summary>
    public void Acionar()
    {
        if (bloquearReentradas && _carregando) return;

        if (string.IsNullOrWhiteSpace(nomeProximaCena))
        {
            Debug.LogError("[NextLevelLevel3] nomeProximaCena está vazio.");
            return;
        }

        if (!CenaExisteNaBuild(nomeProximaCena))
        {
            Debug.LogError($"[NextLevelLevel3] A cena \"{nomeProximaCena}\" não está em Build Settings (File → Build Settings → Scenes In Build).");
            return;
        }

        _carregando = true;

        // Garante que a próxima cena não entre pausada
        Time.timeScale = 1f;

        if (usarCarregamentoAssincrono)
        {
            StartCoroutine(CarregarCenaAssincrona(nomeProximaCena));
        }
        else
        {
            SceneManager.LoadScene(nomeProximaCena, LoadSceneMode.Single);
        }
    }

    private System.Collections.IEnumerator CarregarCenaAssincrona(string cena)
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(cena, LoadSceneMode.Single);
        op.allowSceneActivation = true;

        while (!op.isDone)
        {
            // Se quiser, monitore op.progress (0..0.9 antes de ativar)
            yield return null;
        }
    }

    // Opcional: chamar automaticamente quando o Player encostar num portal
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            Acionar();
    }

    // Valida se a cena está na Build Settings
    private bool CenaExisteNaBuild(string sceneName)
    {
        int count = SceneManager.sceneCountInBuildSettings;
        for (int i = 0; i < count; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i); // ex: Assets/Scenes/Level3.unity
            string name = Path.GetFileNameWithoutExtension(path);
            if (name == sceneName) return true;
        }
        return false;
    }
}
