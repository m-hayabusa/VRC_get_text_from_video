
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class Parser : UdonSharpBehaviour
{
    private string[] filelist;
    private string[][][] result;

    void Start()
    {
        parse("たべたいものリスト␞2␞もの␟たべたさ␞␝蟹␟2␞唐揚げ␟1␞");

        foreach (var file in result)
        {
            foreach (var row in file)
            {
                Debug.Log(string.Format("{0}, {1}", row[0], row[1]));
            }
        }
    }

    public int parse(string input)
    {
        string[] files = input.Split('␜'); // File List
        result = new string[files.Length][][];
        filelist = new string[files.Length];

        for (int filenum = 0; filenum < files.Length; filenum++)
        {
            string[] file = files[0].Split('␝'); // 0: Header, 1: Body

            string[] header = file[0].Split('␞');
            filelist[filenum] = header[0];
            result[filenum] = new string[int.Parse(header[1]) + 1][];
            result[filenum][0] = header[2].Split('␟'); //0をそのままキー名にします

            // string[] body = file[0].Split('␞');

            int cursor = 0;
            foreach (string row in file[1].Split('␞'))
            {
                if (row == "") break;

                cursor++;  // 0はキー名が入る
                result[filenum][cursor] = row.Split('␟');
            }
        }

        return 0;
    }
    public string getString(int filenum, int row, string key) { return ""; }
}
