using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class pengolahSoal : MonoBehaviour
{
    public TextAsset AssetSoal;
    private string[] soal;

    private string[,] soalBag;

    int indexSoal;
    int maxSoal;
    bool ambilSoal;
    char kunciJ;

    bool[] soalSelesai;

    public Text txtSoal, txtOpsiA, txtOpsiB, txtOpsiC, txtOpsiD;

    bool isHasil;
    private float durasi;
    public float durasiPenilaian;

    int jwBenar, jwSalah;
    float nilai ;

    public GameObject panel;
    public GameObject imgPenilaian, imgHasil;
    public Text txtHasil;

    // Start is called before the first frame update
    void Start()
    {
        durasi=durasiPenilaian;
        soal = AssetSoal.ToString().Split('#');

        soalSelesai=new bool[soal.Length];

        soalBag = new string[soal.Length,6];
        maxSoal = 10;

        OlahSoal();
        ambilSoal = true;
        TampilkanSoal();


        //print(soalBag[1,3]);
    }
    
    private void OlahSoal()
    {
        for(int i=0; i<soal.Length; i++)
        {
            string[] tempSoal = soal[i].Split('+');
            for (int j = 0; j<tempSoal.Length; j++)
            {
                soalBag[i,j]=tempSoal[j];
                continue;
            }
            continue;
        }
    }

    private void TampilkanSoal()
    {
        if(indexSoal<maxSoal)
        {
            if(ambilSoal)
            {
                for (int i=0; i<soal.Length; i++)
                {
                    int randomIndexSoal = Random.Range(0,soal.Length);
                    print("Random : "+randomIndexSoal);
                    if(!soalSelesai[randomIndexSoal])
                    {
                     txtSoal.text = soalBag[randomIndexSoal,0];
                     txtOpsiA.text = soalBag[randomIndexSoal,1];
                     txtOpsiB.text = soalBag[randomIndexSoal,2];
                     txtOpsiC.text = soalBag[randomIndexSoal,3];
                     txtOpsiD.text = soalBag[randomIndexSoal,4];
                     kunciJ = soalBag[randomIndexSoal,5][0];

                     soalSelesai[randomIndexSoal] = true;

                     ambilSoal = false;
                     break;
                    }
                    else
                    {
                        continue;
                    }
                }
                

            }
        }
    }
    
    public void Opsi (string opsiHuruf)
    {
        CheckJawaban(opsiHuruf[0]);

       if(indexSoal==maxSoal-1)
       {
        isHasil=true;
       }
       else
       {
        indexSoal++;
        ambilSoal=true;
       }
       panel.SetActive(true);
        
    }

    private float hitungNilai()
    {
        return nilai = (float)jwBenar/maxSoal*100;
    }

    public Text txtPenilaian;
    
    private void CheckJawaban(char huruf)
    {
        string penilaian;


        if (huruf.Equals(kunciJ))
        {
            penilaian="Benar";
            jwBenar++;
        }
        else
        {
            penilaian="Salah";
            jwSalah++;
        }
        txtPenilaian.text=penilaian;
    }

    // Update is called once per frame
    void Update()
    {
        if(panel.activeSelf)
        {
            durasiPenilaian-=Time.deltaTime;

            if(isHasil)
            {
                imgPenilaian.SetActive(true);
                imgHasil.SetActive(false);

                if(durasiPenilaian<=0)
                {
                    txtHasil.text = "Jumlah Benar : "+jwBenar+"\nJumlah Salah :"+jwSalah+"\n\nSkor Kamu : "+hitungNilai();
                    imgPenilaian.SetActive(false);
                    imgHasil.SetActive(true);

                    durasiPenilaian=0;
                }
                
            }
            else
                {
                    imgPenilaian.SetActive(true);
                    imgHasil.SetActive(false);

                    if(durasiPenilaian<=0)
                    {
                     panel.SetActive(false);
                     durasiPenilaian=durasi;

                     TampilkanSoal();
                    }

                }
        }
        
    }
}
