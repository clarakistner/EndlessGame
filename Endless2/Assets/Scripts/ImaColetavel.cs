using UnityEngine;
using System.Collections;

public class ImaColetavel : MonoBehaviour
{
    public float duracao = 5f;               // Tempo que o �m� fica ativo
    public float velocidadeAtracao = 5f;     // Velocidade da atra��o
    public float raioAtracao = 10f;          // Raio de alcance do �m�

    private bool coletado = false;
    private Transform jogador;               // Refer�ncia ao jogador

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!coletado && other.CompareTag("Player"))
        {
            coletado = true;
            jogador = other.transform;

            // Gruda no jogador
            transform.SetParent(jogador);
            transform.localPosition = Vector3.up; // Pode ajustar a posi��o do �m� no corpo do jogador

            Debug.Log("�m� coletado!");
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

        Debug.Log("�m� desativado!");
        Destroy(gameObject); // Remove o �m� ap�s o tempo
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
                // Move a moeda em dire��o ao jogador
                Vector3 direcao = (jogador.position - moeda.transform.position).normalized;
                moeda.transform.position += direcao * velocidadeAtracao * Time.deltaTime;
            }
        }
        foreach (GameObject fogo in fogos)
        {
            float distancia = Vector2.Distance(fogo.transform.position, jogador.position);

            if (distancia <= raioAtracao)
            {
                // Move a moeda em dire��o ao jogador
                Vector3 direcao = (jogador.position - fogo.transform.position).normalized;
                fogo.transform.position += direcao * velocidadeAtracao * Time.deltaTime;
            }
        }
    }
}