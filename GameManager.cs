using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SimpleJSON;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;
using Unity.VisualScripting;
using UnityEngine.EventSystems;
using System.IO;
using System.Net.Http.Headers;

public class GameManager : MonoBehaviour, IProgress<float>
{
    [Header("Pengaturan Umum")]
    public TextMeshProUGUI loading_progress;

    [Header("Pengaturan Halaman Login - 1")]
    public string host = "http://lab.cikarastudio.com/";
    public string project_key = "";
    public TMP_InputField login_email, login_password;
    public GameObject login_popup_gagal;

    [Header("Pengaturan Halaman Login - 2")]
    public string halaman_login;
    public string halaman_register;
    public string halaman_menu;
    public string halaman_kuis;
    public string halaman_leaderboard;
    public string halaman_hasil;


    [Header("Pengaturan Halaman Register")]
    public TMP_InputField register_email;
    public TMP_InputField register_password;
    public TMP_InputField register_nama_lengkap;
    public GameObject register_popup_berhasil;
    public GameObject register_popup_gagal;


    [Header("Pengaturan Halaman Menu")]
    [Tooltip("Ceklist Untuk membuat Soal Menjadi Random, dan jangan diceklish untuk membuat soal tampil secara berurutan")]
    public bool randomSoal;
    public TextMeshProUGUI menu_nama;
    public GameObject menu_mulai;

    [Header("Pengaturan Halaman Kuis")]
    public TextMeshProUGUI kuis_soal;
    public TextMeshProUGUI kuis_jawaban_a;
    public TextMeshProUGUI kuis_jawaban_b;
    public TextMeshProUGUI kuis_jawaban_c;
    public TextMeshProUGUI kuis_jawaban_d;
    public TextMeshProUGUI kuis_jawaban_e;
    public Image kuis_gambar;
    public TextMeshProUGUI kuis_status_gambar;
    public Texture2D kuis_gambar_error;
    public GameObject kuis_button_sound;
    string jawaban_benar;
    int bobot;
    
    public AudioClip suaraBenar;
    public AudioClip suaraSalah;
    public AudioSource suaraFX;
    string suara_url;

    public Button button_A;
    public Button button_B;
    public Button button_C;
    public Button button_D;
    public Button button_E;

    [Header("Untuk keperluan Debugging !")]
    public int kuis_nilai;

    [Header("Pengaturan Leaderboard")]
    public GameObject leaderboard_prefabs;
    public Transform leaderboard_SpawnPoint;
    [Tooltip("Jumlah Data Per Halaman. Contoh 4 maka data yang ditampilkan per halaman yaitu 4 data")]
    public int leaderboard_jumlah_data;
    int leaderboard_page = 1;
    public GameObject leaderboard_next;
    public GameObject leaderboard_prev;
    public GameObject leaderboard_loading;

    [Header("Pengaturan Halaman Hasil")]
    public GameObject[] bintang;
    public int batas_bintang_1;
    public int batas_bintang_2;
    public int batas_bintang_3;
    public TextMeshProUGUI hasil_nilai;


    // HALAMAN URL API //
    string api_login        = "api/kuis/login";
    string api_register     = "api/kuis/register";
    string api_menu         = "api/kuis/all-pertanyaan";
    string api_hasil        = "api/kuis/simpan-nilai";
    string api_leaderboard  = "api/kuis/leaderboard/page";

    string currentScene;
    JSONNode jsonData;
    string output;
    string url;

    async void Start()
    {
        
        currentScene = SceneManager.GetActiveScene().name;
        if (currentScene == halaman_login)
        {
            url = host + api_login;
            PlayerPrefs.SetString("halaman_login", halaman_login);
            PlayerPrefs.SetString("halaman_menu", halaman_menu);
            PlayerPrefs.SetString("halaman_register", halaman_register);
            PlayerPrefs.SetString("halaman_kuis", halaman_kuis);
            PlayerPrefs.SetString("project_key", project_key);
            Debug.Log(project_key);
            PlayerPrefs.SetString("halaman_hasil", halaman_hasil);
            PlayerPrefs.SetString("halaman_leaderboard", halaman_leaderboard);
            PlayerPrefs.SetString("host", host);

            if (PlayerPrefs.GetInt("login_" + project_key) == 1)
            {
                SceneManager.LoadScene(halaman_menu);
            }
        }
        else if (currentScene == PlayerPrefs.GetString("halaman_register"))
        {
            url = PlayerPrefs.GetString("host") + api_register;
        }
        else if (currentScene == PlayerPrefs.GetString("halaman_menu"))
        {
            PlayerPrefs.SetInt("step", 0);
            menu_nama.text = "Hallo, " + PlayerPrefs.GetString("nama_lengkap");
            url = PlayerPrefs.GetString("host") + api_menu;
            menu_mulai.GetComponent<Button>().enabled = false;

            Button_Proses();
        }
        else if (currentScene == PlayerPrefs.GetString("halaman_kuis"))
        {
            int step = PlayerPrefs.GetInt("step");
            if(step == 0)
            {
                PlayerPrefs.SetInt("nilai", 0);
            }
            int urutan = PlayerPrefs.GetInt("urutan_" + step);
            
            jawaban_benar       = PlayerPrefs.GetString("jawaban_benar_" + urutan);
            kuis_soal.text      = PlayerPrefs.GetString("soal_" + urutan);
            kuis_jawaban_a.text = PlayerPrefs.GetString("opsi_a_" + urutan);
            kuis_jawaban_b.text = PlayerPrefs.GetString("opsi_b_" + urutan);
            kuis_jawaban_c.text = PlayerPrefs.GetString("opsi_c_" + urutan);
            kuis_jawaban_d.text = PlayerPrefs.GetString("opsi_d_" + urutan);
            kuis_jawaban_e.text = PlayerPrefs.GetString("opsi_e_" + urutan);
            kuis_jawaban_e.text = PlayerPrefs.GetString("opsi_e_" + urutan);
            string url_gambar   = PlayerPrefs.GetString("host") + PlayerPrefs.GetString("gambar_" + urutan);
            string soundFX      = PlayerPrefs.GetString("sound_" + urutan);
            suara_url           = PlayerPrefs.GetString("host") + soundFX;

            cachedFilePath = Path.Combine(Application.persistentDataPath, "cachedAudio.mp3");

            if (soundFX == "null" || soundFX == null)
            {
                kuis_button_sound.SetActive(false);
            }
            else
            {
                kuis_button_sound.SetActive(true);
            }
           // kuis_gambar.sprite = await GetImageFromWebRequest(url_gambar, this.GetCancellationTokenOnDestroy());
            Davinci.get().load(url_gambar).into(kuis_gambar).withStartAction(() =>
               {
                   kuis_status_gambar.text = "Download has been started.";
               })
               .withDownloadProgressChangedAction((progress) =>
               {
                   kuis_status_gambar.text = "" + progress + "%";
               })
               .withDownloadedAction(() =>
               {
                   kuis_status_gambar.text = "";
               })
               .withLoadedAction(() =>
               {
                   kuis_status_gambar.text = "";
               })
               .withErrorAction((error) =>
               {
                   kuis_status_gambar.text = "Got error : " + error;
               })
               .withEndAction(() =>
               {
                   //print("Operation has been finished.");
               })
               .setErrorPlaceholder(kuis_gambar_error)
               .setFadeTime(0f)
               .setCached(true)
               .start();

            var tempColor = kuis_gambar.color;
            tempColor.a = 1f;
            kuis_gambar.color = tempColor;

            string s_bobot = PlayerPrefs.GetString("bobot_" + urutan);
            int.TryParse(s_bobot, out int int_bobot);
            bobot = int_bobot;

            kuis_nilai = PlayerPrefs.GetInt("nilai");
        }
        else if (currentScene == PlayerPrefs.GetString("halaman_hasil"))
        {
            PlayerPrefs.SetInt("step", 0);
            int nilai = PlayerPrefs.GetInt("nilai");
            hasil_nilai.text = "Nilai: " + nilai;
            if (nilai <= batas_bintang_1)
            {
                bintang[0].SetActive(true);
                bintang[1].SetActive(false);
                bintang[2].SetActive(false);
            }else if(nilai <= batas_bintang_2)
            {
                bintang[0].SetActive(true);
                bintang[1].SetActive(true);
                bintang[2].SetActive(false);
            }
            else if (nilai <= batas_bintang_3)
            {
                bintang[0].SetActive(true);
                bintang[1].SetActive(true);
                bintang[2].SetActive(true);
            }
            url = PlayerPrefs.GetString("host") + api_hasil;
            Button_Proses();
        }
        else if (currentScene == PlayerPrefs.GetString("halaman_leaderboard"))
        {
            url = PlayerPrefs.GetString("host") + api_leaderboard;
            Button_Proses();
        }

    }

    public void Button_Proses()
    {
        ProsesData();
    }

    async void ProsesData()
    {
        string result = await GetWebRequest(url, this.GetCancellationTokenOnDestroy());
        jsonData = JSON.Parse((result));
        string status = "" + jsonData["status"];

        if (status == "success")
        {
            if (currentScene == halaman_login)
            {
                string nama_lengkap = "" + jsonData["data"]["data_user"]["name"];
                string login = "login_" + project_key;
                PlayerPrefs.SetInt(login, 1);
                PlayerPrefs.SetString("nama_lengkap", nama_lengkap);
                PlayerPrefs.SetString("integrasi_id", jsonData["data"]["data_integrasi"]["id"]);
                SceneManager.LoadScene(PlayerPrefs.GetString("halaman_menu"));
            }
            else if (currentScene == PlayerPrefs.GetString("halaman_register"))
            {
                register_popup_berhasil.SetActive(true);
            }
            else if (currentScene == PlayerPrefs.GetString("halaman_menu"))
            {
                //start - Proses Seting Soal dan Jawaban //
                if(jsonData["data"].Count != 0)
                {
                    for (int i = 0; i < jsonData["data"].Count; i++)
                    {

                        PlayerPrefs.SetString("soal_" + i, jsonData["data"][i]["soal"]);
                        PlayerPrefs.SetString("gambar_" + i, jsonData["data"][i]["gambar"]);
                        PlayerPrefs.SetString("opsi_a_" + i, jsonData["data"][i]["opsi_a"]);
                        PlayerPrefs.SetString("opsi_b_" + i, jsonData["data"][i]["opsi_b"]);
                        PlayerPrefs.SetString("opsi_c_" + i, jsonData["data"][i]["opsi_c"]);
                        PlayerPrefs.SetString("opsi_d_" + i, jsonData["data"][i]["opsi_d"]);
                        PlayerPrefs.SetString("opsi_e_" + i, jsonData["data"][i]["opsi_e"]);
                        PlayerPrefs.SetString("jawaban_benar_" + i, jsonData["data"][i]["jawaban_benar"]);
                        PlayerPrefs.SetString("bobot_"  + i, jsonData["data"][i]["bobot"]);
                        PlayerPrefs.SetString("sound_"   + i, jsonData["data"][i]["sound"]);

                    }
                    PlayerPrefs.SetInt("total_soal", jsonData["data"].Count);
                    /*end - Proses Seting Soal dan Jawaban */

                    if (randomSoal)
                    {
                        AcakSoal(jsonData["data"].Count);
                    }
                    else
                    {
                        for (int i = 0; i < jsonData["data"].Count; i++)
                        {
                            PlayerPrefs.SetInt("urutan_"+i, i);
                        }
                    }
                }
                else
                {
                    menu_mulai.SetActive(false);
                }
                
            }
            else if (currentScene == PlayerPrefs.GetString("halaman_hasil"))
            {
                Debug.Log("Nilai Telah Berhasil Disimpan");
            }
            else if (currentScene == PlayerPrefs.GetString("halaman_leaderboard"))
            {
                int.TryParse(jsonData["total_data"], out int total_data);

                if (total_data != 0)
                {
                    for (int i = 0; i < jsonData["data"].Count; i++)
                    {
                        int nomor_urut = ((leaderboard_page * leaderboard_jumlah_data) - leaderboard_jumlah_data) + (i + 1);
                        string nama;
                        if ("" + jsonData["data"][i]["integrasi_id"] == PlayerPrefs.GetString("integrasi_id"))
                        {
                            nama = jsonData["data"][i]["nama_lengkap"] + " (You)";
                        }
                        else
                        {
                            nama = jsonData["data"][i]["nama_lengkap"];
                        }
                        var x = Instantiate(leaderboard_prefabs, leaderboard_SpawnPoint.position, leaderboard_SpawnPoint.rotation);
                        x.transform.SetParent(leaderboard_SpawnPoint);
                        x.name = "prefabs_leaderboard_" + (i + 1);
                        x.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "" + nomor_urut;
                        x.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "" + nama;
                        x.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = "" + jsonData["data"][i]["nilai"];
                    }

                    int jumlah = leaderboard_page * leaderboard_jumlah_data;

                    if (jumlah < total_data)
                    {
                        leaderboard_next.SetActive(true);
                    }
                    else
                    {
                        leaderboard_next.SetActive(false);
                    }

                    int jumlah_2 = (leaderboard_page * leaderboard_jumlah_data) - leaderboard_jumlah_data;
                    if (jumlah_2 < leaderboard_jumlah_data)
                    {
                        leaderboard_prev.SetActive(false);
                    }
                    else
                    {
                        leaderboard_prev.SetActive(true);
                    }
                }
                else
                {
                    var x = Instantiate(leaderboard_prefabs, leaderboard_SpawnPoint.position, leaderboard_SpawnPoint.rotation);
                    x.transform.SetParent(leaderboard_SpawnPoint);
                    x.name = "prefabs_leaderboard_0" ;
                    x.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "";
                    x.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "Belum Ada Data !" ;
                    x.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = "";
                    leaderboard_next.SetActive(false);
                    leaderboard_prev.SetActive(false);
                }
                leaderboard_loading.SetActive(false);
            }
        }
        else if (status == "error")
        {
            if(currentScene == PlayerPrefs.GetString("halaman_login"))
            {
                login_popup_gagal.SetActive(true);
                Debug.Log("Keterangan Dari Server : " + jsonData["keterangan"]);
            }
            else if (currentScene == PlayerPrefs.GetString("halaman_register"))
            {
                register_popup_gagal.SetActive(true);
                Debug.Log("Keterangan Dari Server : " + jsonData["keterangan"]);
            }
            else if(currentScene == PlayerPrefs.GetString("halaman_hasil"))
            {
                Debug.Log("Keterangan Dari Server : " + jsonData["keterangan"]);
            }
        }

    }


    async UniTask<string> GetWebRequest(string url, CancellationToken token)
    {
       // Debug.Log(url);
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            Debug.Log("No internet connection!");
        }
        else
        {
            WWWForm form = new WWWForm();

            if (currentScene == halaman_login)
            {
                form.AddField("email", login_email.text);
                form.AddField("password", login_password.text);
                form.AddField("project_key", project_key);
            }
            else if (currentScene == PlayerPrefs.GetString("halaman_register"))
            {
                form.AddField("email", register_email.text);
                form.AddField("password", register_password.text);
                form.AddField("project_key", PlayerPrefs.GetString("project_key"));
                form.AddField("nama_lengkap", register_nama_lengkap.text);
            }
            else if (currentScene == PlayerPrefs.GetString("halaman_menu"))
            {
                form.AddField("project_key", PlayerPrefs.GetString("project_key"));
            }
            else if (currentScene == PlayerPrefs.GetString("halaman_hasil"))
            {
                form.AddField("integrasi_id", PlayerPrefs.GetString("integrasi_id"));
                form.AddField("nilai", PlayerPrefs.GetInt("nilai"));
            }
            else if (currentScene == PlayerPrefs.GetString("halaman_leaderboard"))
            {
                form.AddField("project_key", PlayerPrefs.GetString("project_key"));
                form.AddField("take", leaderboard_jumlah_data);
                form.AddField("page", leaderboard_page);
            }

            var cts = new CancellationTokenSource();
            cts.CancelAfterSlim(TimeSpan.FromSeconds(5)); // 5sec timeout.
            try
            {
                var progress = Progress.Create<float>(
                    x => SetProgress(x)
                );
                // var op = await UnityWebRequest.Post(url, form).SendWebRequest().WithCancellation(token);
                var op = await UnityWebRequest.Post(url, form).SendWebRequest().ToUniTask(progress);

                output = op.downloadHandler.text;
            }
            catch (OperationCanceledException ex)
            {
                if (ex.CancellationToken == cts.Token)
                {
                    UnityEngine.Debug.Log("Timeout");
                }
            }

        }
        return output;
    }

    void SetProgress(float x)
    {
        if(x == 1)
        {
            loading_progress.text = "Progress: " + (x * 100) + "%";
            if(currentScene == PlayerPrefs.GetString("halaman_menu"))
            {
                menu_mulai.SetActive(true);
                menu_mulai.GetComponent<Button>().enabled = true;

            }
        }
        else
        {
            loading_progress.text = "Progress: " + (x * 100).ToString("F2") + "%";
        }
    }

  
    async UniTask<Sprite> GetImageFromWebRequest(string url, CancellationToken token)
    {
        var unityWebRequestTexture = await UnityWebRequestTexture
            .GetTexture(url)
            .SendWebRequest()
            .WithCancellation(token);

        Texture2D texture = ((DownloadHandlerTexture)unityWebRequestTexture.downloadHandler).texture;
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2());
        return sprite;
    }


    public void Report(float value)
    {
        throw new NotImplementedException();
    }

    public void PindahHalaman(string HalamanSelanjutnya)
    {
        SceneManager.LoadScene(HalamanSelanjutnya);
    }

    void AcakSoal(int total_soal)
    {
        List<int> ints = new List<int>();
        List<int> values = new List<int>();

        for (int i = 0; i < total_soal; ++i)
        {
            ints.Add(i);
        }

        for (int i = 0; i < total_soal; ++i)
        {
            int index = UnityEngine.Random.Range(0, ints.Count);
            values.Add(ints[index]);
            ints.RemoveAt(index);
        }

        for (int i = 0; i < values.Count; ++i)
        {
            PlayerPrefs.SetInt("urutan_" + i, values[i]);
        }
    }

    public void Jawaban_User(string jawaban)
    {
        DisableButton();
        StartCoroutine(Cek_Jawaban(jawaban));
    }

    IEnumerator Cek_Jawaban(string jawaban)
    {

        if (jawaban_benar.ToUpper() == jawaban.ToUpper())
        {
            int nilai = PlayerPrefs.GetInt("nilai");
            nilai = nilai + bobot;
            PlayerPrefs.SetInt("nilai", nilai);
            suaraFX.clip = suaraBenar;
            suaraFX.Play();
        }
        else
        {
            suaraFX.clip = suaraSalah;
            suaraFX.Play();
        }

        yield return new WaitForSeconds(1f);

        int step = PlayerPrefs.GetInt("step");
        step = step + 1;
        PlayerPrefs.SetInt("step", step);

        if (PlayerPrefs.GetInt("step") < PlayerPrefs.GetInt("total_soal"))
        {
            SceneManager.LoadScene(PlayerPrefs.GetString("halaman_kuis"));
        }
        else
        {
            SceneManager.LoadScene(PlayerPrefs.GetString("halaman_hasil"));
        }
    }

    public void DisableButton()
    {
        button_A.enabled = false;
        button_B.enabled = false;
        button_C.enabled = false;
        button_D.enabled = false;
    }

    public void OpenPopup(GameObject gameObject)
    {
        gameObject.SetActive(true);
    }

    public void ClosePopup(GameObject gameObject)
    {
        gameObject.SetActive(false);
    }

    public void Button_Logout()
    {
        PlayerPrefs.SetInt("login_"+ PlayerPrefs.GetString("project_key"), 0);
        SceneManager.LoadScene(PlayerPrefs.GetString("halaman_login"));
        PlayerPrefs.DeleteAll();
    }

    public void Button_Leaderboard_next() {
        leaderboard_loading.SetActive(true);
        leaderboard_next.SetActive(false);
        ClearLeaderboard();
        leaderboard_page++;
        Button_Proses();

    }

    public void Button_Leaderboard_prev()
    {
        leaderboard_loading.SetActive(true);
        leaderboard_prev.SetActive(false);
        ClearLeaderboard();
        leaderboard_page--;
        Button_Proses();
    }

    void ClearLeaderboard()
    {
        int no = 0;
        foreach(Transform t in leaderboard_SpawnPoint)
        {
            if(no != 0)
            {
                Destroy(t.gameObject);
            }
            no++;
        }
    }

    // ini untuk handle suara dan cached file suara //
    private string cachedFilePath;
    AudioClip output_audio;

    public async void Button_Sound_FX()
    {
        string url_sound = PlayerPrefs.GetString("host");

        if (File.Exists(cachedFilePath))
        {
            PlayCachedAudio();
        }
        else
        {
            AudioClip result = await GetAudioRequest(suara_url, this.GetCancellationTokenOnDestroy());
            suaraFX.clip = result;
            suaraFX.Play();
        }
    }

    async UniTask<AudioClip> GetAudioRequest(string url, CancellationToken token)
    {
        var cts = new CancellationTokenSource();
        cts.CancelAfterSlim(TimeSpan.FromSeconds(5)); // 5sec timeout.
        try
        {
            var progress = Progress.Create<float>(
                x => SetProgress(x)
            );
            var op = await UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG).SendWebRequest().ToUniTask(progress);
            output_audio = DownloadHandlerAudioClip.GetContent(op);
            File.WriteAllBytes(cachedFilePath, op.downloadHandler.data);
        }
        catch (OperationCanceledException ex)
        {
            if (ex.CancellationToken == cts.Token)
            {
                UnityEngine.Debug.Log("Timeout");
            }
        }
        return output_audio;
    }


    private void PlayCachedAudio()
    {
        StartCoroutine(LoadCachedAudio());
    }

    private IEnumerator LoadCachedAudio()
    {
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + cachedFilePath, AudioType.MPEG))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error loading cached audio: " + www.error);
            }
            else
            {
                AudioClip audioClip = DownloadHandlerAudioClip.GetContent(www);
                suaraFX.clip = audioClip;
                suaraFX.Play();
            }
        }
    }
    private void OnDestroy()
    {
        if (File.Exists(cachedFilePath))
        {
            File.Delete(cachedFilePath);
            Debug.Log("Cached audio file deleted.");
        }
    }
}
