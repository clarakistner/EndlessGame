using UnityEngine;
using System.Collections;

public class ImaColetavel : MonoBehaviour
{
    [Header("Duração e atração")]
    public float duracao = 5f;                 // segundos (ignora timeScale)
    public float velocidadeAtracao = 18f;      // m/s
    public float raioAtracao = 10f;            // metros
    public float pickupAssistDistance = 0.25f; // “gruda” no player nesta distância

    [Header("Detecção (opcional)")]
    public LayerMask coletavelMask;            // defina as layers (Coin/Fire). Se 0, usa Find por tag.

    [Header("Posicionamento acima da cabeça")]
    public float padding = 0.05f;
    public Transform ancoraNaCabeca;           // arraste um Empty no topo da cabeça se quiser (HeadAnchor)

    [Header("Escala de MUNDO do ímã")]
    public Vector3 escalaMundoDesejada = new Vector3(4f, 4f, 4f);

    private bool ativo = false;
    private Transform jogador;
    private float fimTempoReal = 0f;           // usa Time.realtimeSinceStartup
    private Vector3 escalaInicialLocal;

    private void Awake()
    {
        escalaInicialLocal = transform.localScale;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (ativo) return;

        if (other.CompareTag("Player"))
        {
            jogador = other.transform;

            // Fallback automático: tenta achar "HeadAnchor" no player se não foi setado no Inspector
            if (ancoraNaCabeca == null)
            {
                var achou = jogador.Find("HeadAnchor");
                if (achou != null) ancoraNaCabeca = achou;
            }

            // Mantém posição/rotação atuais em MUNDO ao parentear
            transform.SetParent(jogador, worldPositionStays: true);

            // Corrige rotação e escala de MUNDO
            transform.rotation = Quaternion.identity;
            AplicarEscalaMundoConstante(escalaMundoDesejada);

            // Inicia o efeito (timer em tempo REAL, ignora timeScale)
            fimTempoReal = Time.realtimeSinceStartup + duracao;
            ativo = true;

            // Posiciona acima da cabeça já no primeiro frame
            AtualizarPosicaoAcimaDaCabeca();
        }
    }

    private void LateUpdate()
    {
        if (!ativo || jogador == null) return;

        // Mantém escala de mundo = (4,4,4)
        AplicarEscalaMundoConstante(escalaMundoDesejada);

        // Reposiciona acima da cabeça (animações/flip)
        AtualizarPosicaoAcimaDaCabeca();

        // Atrai objetos todo frame (sem depender da física)
        Atrair("Coin");

        // Timer em tempo real (independente do timeScale)
        if (Time.realtimeSinceStartup >= fimTempoReal)
        {
            Destroy(gameObject);
        }
    }

    private void AplicarEscalaMundoConstante(Vector3 escalaMundoAlvo)
    {
        if (transform.parent == null)
        {
            transform.localScale = escalaMundoAlvo;
            return;
        }

        Vector3 p = transform.parent.lossyScale;
        float sx = (p.x != 0f) ? escalaMundoAlvo.x / p.x : escalaMundoAlvo.x;
        float sy = (p.y != 0f) ? escalaMundoAlvo.y / p.y : escalaMundoAlvo.y;
        float sz = (p.z != 0f) ? escalaMundoAlvo.z / p.z : escalaMundoAlvo.z;
        transform.localScale = new Vector3(sx, sy, sz);
    }

    private void AtualizarPosicaoAcimaDaCabeca()
    {
        if (jogador == null) return;

        if (ancoraNaCabeca != null)
        {
            Vector3 alvo = ancoraNaCabeca.position;
            alvo.y += ObterMeiaAlturaDoIman() + padding;
            transform.position = alvo;
            return;
        }

        Bounds b = ObterBoundsDoPlayer();
        float magnetHalf = ObterMeiaAlturaDoIman();

        Vector3 alvoMundo = new Vector3(
            b.center.x,
            b.max.y + magnetHalf + padding,
            transform.position.z
        );
        transform.position = alvoMundo;
    }

    private Bounds ObterBoundsDoPlayer()
    {
        // Prefere colliders NÃO-trigger e habilitados, do próprio player (evita groundCheck/sensors)
        var cols = jogador.GetComponentsInChildren<Collider2D>();
        Collider2D melhor = null;
        var rbPlayer = jogador.GetComponent<Rigidbody2D>();

        foreach (var c in cols)
        {
            if (!c || !c.enabled) continue;
            if (c.isTrigger) continue; // ignora sensores
            // restringe ao corpo do player (mesmo rigidbody ou no root)
            if (c.transform == jogador || (rbPlayer != null && c.attachedRigidbody == rbPlayer))
            {
                melhor = c; break;
            }
        }
        if (melhor != null) return melhor.bounds;

        // Senão, usa o SpriteRenderer principal do player
        var sr = jogador.GetComponent<SpriteRenderer>();
        if (sr != null) return sr.bounds;

        // Fallback
        return new Bounds(jogador.position, Vector3.one);
    }

    private float ObterMeiaAlturaDoIman()
    {
        float h = 0f;
        var col = GetComponentInChildren<Collider2D>();
        var sr = GetComponentInChildren<SpriteRenderer>();
        if (col != null) h = col.bounds.extents.y;
        else if (sr != null) h = sr.bounds.extents.y;
        return h;
    }

    private void Atrair(string tag)
    {
        if (jogador == null) return;

        float dt = (Time.timeScale > 0f ? Time.deltaTime : Time.unscaledDeltaTime);
        if (dt <= 0f) dt = 0.016f; // fallback mínimo

        if (coletavelMask.value != 0)
        {
            // Detecção por LayerMask (mais performática/precisa)
            var hits = Physics2D.OverlapCircleAll((Vector2)jogador.position, raioAtracao, coletavelMask);
            for (int i = 0; i < hits.Length; i++)
            {
                var h = hits[i];
                if (!h || !h.CompareTag(tag)) continue;
                PuxarTransform(h.transform, dt);
            }
        }
        else
        {
            // Fallback por tag
            var gos = GameObject.FindGameObjectsWithTag(tag);
            for (int i = 0; i < gos.Length; i++)
            {
                var go = gos[i];
                if (!go) continue;
                if (Vector2.Distance(go.transform.position, jogador.position) <= raioAtracao)
                    PuxarTransform(go.transform, dt);
            }
        }
    }

    private void PuxarTransform(Transform t, float dt)
    {
        if (t == null) return;

        // Durante a sucção, ignore física e mova por transform (funciona mesmo com timeScale=0)
        var rb = t.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.isKinematic = true;
            rb.gravityScale = 0f;
        }

        Vector3 dir = (jogador.position - t.position).normalized;
        t.position += dir * (velocidadeAtracao * dt);

        float d = Vector2.Distance(t.position, jogador.position);
        if (d <= pickupAssistDistance)
        {
            // Snap suave pra dentro do player pra garantir o gatilho de coleta
            t.position = Vector3.MoveTowards(t.position, jogador.position, 100f * dt);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 centro = (jogador != null) ? jogador.position : transform.position;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(centro, raioAtracao);
    }
}
