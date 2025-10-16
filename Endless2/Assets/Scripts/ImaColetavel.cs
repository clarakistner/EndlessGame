using UnityEngine;
using System.Collections;

public class ImaColetavel : MonoBehaviour
{
    public float duracao = 5f;               // Tempo que o ímã fica ativo
    public float velocidadeAtracao = 5f;     // Velocidade da atração
    public float raioAtracao = 10f;          // Raio de alcance do ímã

    private bool coletado = false;
    private Transform jogador;               // Referência ao jogador

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!coletado && other.CompareTag("Player"))
        {
            coletado = true;
            jogador = other.transform;

            // Gruda no jogador
            transform.SetParent(jogador);
            transform.localPosition = Vector3.up; // Pode ajustar a posição do ímã no corpo do jogador

            Debug.Log("Ímã coletado!");
            StartCoroutine(AtivarImanTemporario());
        }
    }

    IEnumerator AtivarImanTemporario()
    {
        float tempoRestante = duracao;

        while (tempoRestante > 0)
        {
            AtrairMoedasParaJogador();
            tempoRestante -= Time.deltaTime;
            yield return null;
        }

        Debug.Log("Ímã desativado!");
        Destroy(gameObject); // Remove o ímã após o tempo
    }

    void AtrairMoedasParaJogador()
    {
        GameObject[] moedas = GameObject.FindGameObjectsWithTag("Coin");
        GameObject[] fogos = GameObject.FindGameObjectsWithTag("Fire");

        foreach (GameObject moeda in moedas)
        {
            float distancia = Vector2.Distance(moeda.transform.position, jogador.position);

            if (distancia <= raioAtracao)
            {
                // Move a moeda em direção ao jogador
                Vector3 direcao = (jogador.position - moeda.transform.position).normalized;
                moeda.transform.position += direcao * velocidadeAtracao * Time.deltaTime;
            }
        }
        foreach (GameObject fogo in fogos)
        {
            float distancia = Vector2.Distance(fogo.transform.position, jogador.position);

            if (distancia <= raioAtracao)
            {
                // Move a moeda em direção ao jogador
                Vector3 direcao = (jogador.position - fogo.transform.position).normalized;
                fogo.transform.position += direcao * velocidadeAtracao * Time.deltaTime;
            }
        }
    }
}