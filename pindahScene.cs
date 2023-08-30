using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class pindahQuis : MonoBehaviour
{
    public void berpindahScene (string namaScene)
    {
        SceneManager.LoadScene(namaScene);
    }
}
