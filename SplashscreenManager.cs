using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashscreenManager : MonoBehaviour
{
    public float waktuTunggu;
    public string halamanSelanjutnya;
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    void Start()
    {
        StartCoroutine(PindahHalaman());
    }

    IEnumerator PindahHalaman()
    {
        yield return new WaitForSeconds(waktuTunggu);
        SceneManager.LoadScene(halamanSelanjutnya);
    }
}
