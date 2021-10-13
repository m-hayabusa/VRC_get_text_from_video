
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using System;

public class Parser : UdonSharpBehaviour
{
    private string[] filelist;
    private string[][][] result;

    void Start()
    {
        this.parse("たべたいものリスト␞2␞もの␟たべたさ␞␝蟹␟1␞唐揚げ␟255␜かいものリスト␞2␞もの␟個数␞␝調整豆乳 1L␟1␞唐揚げ␟1");

        Debug.Log(this.getString("たべたいものリスト", 0, "もの"));
        Debug.Log(string.Format("{0}を{1}つ", this.getString("かいものリスト", 0, "もの"), this.getString("かいものリスト", 0, "個数")));
    }

    public int parse(string input)
    {
        string[] files = input.Split('␜'); // File List
        result = new string[files.Length][][];
        filelist = new string[files.Length];

        for (int filenum = 0; filenum < files.Length; filenum++)
        {
            string[] file = files[filenum].Split('␝'); // 0: Header, 1: Body

            if (file.Length < 2) break;

            string[] header = file[0].Split('␞');

            if (header.Length < 3) break;

            filelist[filenum] = header[0];
            result[filenum] = new string[int.Parse(header[1]) + 1][];
            result[filenum][0] = header[2].Split('␟'); //0をそのままキー名にします

            // int cursor = 0;

            string[] body = file[1].Split('␞');

            for (int cursor = 0; cursor < int.Parse(header[1]); cursor++)
            {
                string row = body[cursor];
                if (row == "") break;

                result[filenum][cursor + 1] = row.Split('␟');
            }
        }

        return 0;
    }

    public string getString(string filename, int row, string key)
    {
        row++;
        int file = Array.IndexOf(filelist, filename);
        if (file < 0 || row < 0 || result[file].Length < row) return "";

        int col = Array.IndexOf(result[file][0], key);
        if (col < 0) return "";

        return result[file][row][col];
    }
}
