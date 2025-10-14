using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject canvasProximaFase;

    public TMP_Text textoMoedas;

    [Header("Configuração do Jogo")]
    public int coinsCounter = 0;
    public int moedasParaVencer = 5;

    [Header("Tempo e fases")]
    public float delayProximaFase = 2f;

    private bool jogoAtivo = true;
    private string textoOriginal;

    void Start()
    {
        Debug.Log("TextoMoedas é: " + textoMoedas.text);
        textoOriginal = textoMoedas.text;
        textoMoedas.text = "TESTE";
        canvasProximaFase.SetActive(false);

    }


    void Update()
    {

        textoMoedas.text = $"{textoOriginal} {coinsCounter}";
        Debug.Log("TextoMoedas é: " + textoMoedas.text);

        if (coinsCounter >= moedasParaVencer && jogoAtivo)
        {
            Vencer();
        }
    }

    public void Vencer()
    {
        Time.timeScale = 0;
        jogoAtivo = false;
        canvasProximaFase.SetActive(true);

    }


}