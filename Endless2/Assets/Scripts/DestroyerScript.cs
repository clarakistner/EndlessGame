using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyerScript : MonoBehaviour
{

    public Canvas GameOver;

    private void Start()
    {
        GameOver.gameObject.SetActive(false);
        Time.timeScale = 1;
    }

    void OnCollisionEnter2D(Collision2D outro)
    {

        if (outro.gameObject.tag == "Player")
        {
            Time.timeScale = 0;
            GameOver.gameObject.SetActive(true);
            return;
        }

    }
}