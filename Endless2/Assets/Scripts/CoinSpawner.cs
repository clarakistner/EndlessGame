using UnityEngine;

/// Spawner exclusivo de moedas SEM coroutine/SEM yield.
/// - Agenda spawns pelo Update() usando Time.time (escalado) ou Time.realtimeSinceStartup (não-escalado)
/// - Suporta área (BoxCollider2D), pontos (Transforms) ou posição do próprio spawner
/// - Limite opcional de moedas vivas (decrementa ao destruir/coletar)
public class CoinSpawner : MonoBehaviour
{
    [Header("Prefabs de Moeda (obrigatório)")]
    [SerializeField] private GameObject[] coinPrefabs;

    [Header("Intervalo entre spawns (segundos)")]
    [SerializeField] private float spawnMin = 0.6f;
    [SerializeField] private float spawnMax = 1.2f;

    [Header("Relógio")]
    [Tooltip("Se true, usa tempo NÃO-escalado (funciona mesmo com Time.timeScale = 0).")]
    [SerializeField] private bool usarTempoNaoEscalado = true;

    [Header("Onde spawnar")]
    [Tooltip("Se definido, spawna aleatoriamente DENTRO deste BoxCollider2D.")]
    [SerializeField] private BoxCollider2D areaDeSpawn;
    [Tooltip("Se houver, spawna aleatoriamente EM UM DESTES pontos.")]
    [SerializeField] private Transform[] pontosDeSpawn;
    [Tooltip("Se nenhum dos dois acima estiver definido, usa a posição do spawner.")]
    [SerializeField] private bool parentearNoSpawner = false;

    [Header("Limites e Execução")]
    [Tooltip("0 = ilimitado. Se > 0, respeita limite de moedas vivas.")]
    [SerializeField] private int maxMoedasVivas = 0;
    [Tooltip("Inicia automaticamente no OnEnable.")]
    [SerializeField] private bool autoStartOnEnable = true;

    [Header("Depuração (opcional)")]
    [SerializeField] private bool logErros = false;
    [SerializeField] private bool logSpawns = false;

    // --- estado interno ---
    private bool ativo = false;
    private int vivos = 0;
    private float proximoSpawnTS = 0f; // timestamp do próximo spawn, no relógio escolhido

    private void OnEnable()
    {
        if (autoStartOnEnable) Iniciar();
    }

    private void OnDisable()
    {
        ativo = false;
    }

    public void Iniciar()
    {
        if (!ValidarSetup())
        {
            if (logErros) Debug.LogError("[CoinSpawner_NoYield] Setup inválido. Desabilitando componente.", this);
            enabled = false;
            return;
        }

        vivos = 0;
        ativo = true;
        AgendarProximoSpawn( /*primeiro*/ Mathf.Max(0.01f, spawnMin * 0.5f));
    }

    public void Parar()
    {
        ativo = false;
    }

    private void Update()
    {
        if (!ativo) return;

        float agora = Agora();
        if (agora < proximoSpawnTS) return;

        // Respeita limite de vivos
        if (maxMoedasVivas > 0 && vivos >= maxMoedasVivas)
        {
            // re-agenda um pequeno retry
            AgendarProximoSpawn(0.1f);
            return;
        }

        // Spawn único
        if (!SpawnOnce())
        {
            // se falhou (ex.: todos prefabs nulos), tenta de novo mais tarde
            AgendarProximoSpawn(0.5f);
            return;
        }

        // Agenda o próximo
        AgendarProximoSpawn(Random.Range(Mathf.Max(0.01f, spawnMin), Mathf.Max(spawnMin, spawnMax)));
    }

    // -------- núcleo --------

    private bool SpawnOnce()
    {
        GameObject prefab = EscolherPrefabValido();
        if (prefab == null)
        {
            if (logErros) Debug.LogError("[CoinSpawner_NoYield] Nenhum prefab válido (array vazio ou só nulos).", this);
            return false;
        }

        Vector3 pos = ObterPosicaoDeSpawn();
        Transform pai = parentearNoSpawner ? transform : null;

        GameObject moeda = Instantiate(prefab, pos, Quaternion.identity, pai);
        if (moeda == null)
        {
            if (logErros) Debug.LogError("[CoinSpawner_NoYield] Instantiate retornou null (raro).", this);
            return false;
        }

        vivos++;
        if (!moeda.activeSelf) moeda.SetActive(true);
        if (!moeda.CompareTag("Coin")) moeda.tag = "Coin";

        if (logSpawns) Debug.Log($"[CoinSpawner_NoYield] SPAWN: {moeda.name} @ {pos} | vivos={vivos}", moeda);

        // Hook para decrementar vivos quando a moeda for destruída/coletada
        var hook = moeda.GetComponent<_CoinSpawnHook_NoYield>();
        if (hook == null) hook = moeda.AddComponent<_CoinSpawnHook_NoYield>();
        hook.onDestroyed = () => { vivos = Mathf.Max(0, vivos - 1); };

        return true;
    }

    private void AgendarProximoSpawn(float delaySegundos)
    {
        float baseTime = Agora();
        proximoSpawnTS = baseTime + Mathf.Max(0.01f, delaySegundos);
    }

    private float Agora()
    {
        return usarTempoNaoEscalado ? Time.realtimeSinceStartup : Time.time;
    }

    // -------- utilidades --------

    private bool ValidarSetup()
    {
        if (coinPrefabs == null || coinPrefabs.Length == 0)
        {
            if (logErros) Debug.LogError("[CoinSpawner_NoYield] 'coinPrefabs' vazio.", this);
            return false;
        }

        bool temValido = false;
        foreach (var p in coinPrefabs) if (p != null) { temValido = true; break; }
        if (!temValido)
        {
            if (logErros) Debug.LogError("[CoinSpawner_NoYield] Todos os prefabs em 'coinPrefabs' estão nulos.", this);
            return false;
        }

        if (spawnMax < spawnMin)
        {
            float t = spawnMin; spawnMin = spawnMax; spawnMax = t;
            if (logErros) Debug.LogWarning("[CoinSpawner_NoYield] spawnMin/spawnMax invertidos; corrigi automaticamente.", this);
        }
        spawnMin = Mathf.Max(0.01f, spawnMin);
        spawnMax = Mathf.Max(spawnMin, spawnMax);

        return true;
    }

    private GameObject EscolherPrefabValido()
    {
        // tenta algumas vezes para evitar nulos aleatórios
        for (int i = 0; i < 6; i++)
        {
            var p = coinPrefabs[Random.Range(0, coinPrefabs.Length)];
            if (p != null) return p;
        }
        foreach (var p in coinPrefabs) if (p != null) return p;
        return null;
    }

    private Vector3 ObterPosicaoDeSpawn()
    {
        // 1) Área
        if (areaDeSpawn != null)
        {
            Bounds b = areaDeSpawn.bounds;
            float x = Random.Range(b.min.x, b.max.x);
            float y = Random.Range(b.min.y, b.max.y);
            return new Vector3(x, y, transform.position.z);
        }

        // 2) Pontos
        if (pontosDeSpawn != null && pontosDeSpawn.Length > 0)
        {
            Transform t = null;
            // tenta achar um ponto válido
            for (int i = 0; i < 6; i++)
            {
                var cand = pontosDeSpawn[Random.Range(0, pontosDeSpawn.Length)];
                if (cand != null) { t = cand; break; }
            }
            if (t != null) return new Vector3(t.position.x, t.position.y, transform.position.z);
        }

        // 3) Posição do spawner
        return transform.position;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (spawnMax < spawnMin) spawnMax = spawnMin;
        if (spawnMin < 0f) spawnMin = 0f;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        if (areaDeSpawn != null)
        {
            var b = areaDeSpawn.bounds;
            Gizmos.DrawWireCube(b.center, b.size);
        }
        else if (pontosDeSpawn != null && pontosDeSpawn.Length > 0)
        {
            foreach (var t in pontosDeSpawn) if (t != null) Gizmos.DrawWireSphere(t.position, 0.15f);
        }
        else
        {
            Gizmos.DrawWireSphere(transform.position, 0.2f);
        }
    }
#endif
}

/// Hook simples para informar o spawner quando a moeda foi destruída/coletada (sem coroutines).
public class _CoinSpawnHook_NoYield : MonoBehaviour
{
    public System.Action onDestroyed;
    private void OnDestroy() { onDestroyed?.Invoke(); }
}
