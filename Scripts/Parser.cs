
using UdonSharp;
using System;

namespace nekomimiStudio.video2String
{
    public class Parser : UdonSharpBehaviour
    {
        private string[] filelist;
        private string[][][] result;
        public bool isParseEnd = false;

        public int parse(string input)
        {
            const char FS = (char)28;
            const char GS = (char)29;
            const char RS = (char)30;
            const char US = (char)31;

            string[] files = input.Split(FS); // File List
            result = new string[files.Length][][];
            filelist = new string[files.Length];

            for (int filenum = 0; filenum < files.Length; filenum++)
            {
                string[] file = files[filenum].Split(GS); // 0: Header, 1: Body

                if (file.Length < 2) break;

                string[] header = file[0].Split(RS);

                if (header.Length < 3) break;

                filelist[filenum] = header[0];
                result[filenum] = new string[int.Parse(header[1]) + 1][];
                result[filenum][0] = header[2].Split(US); //0をそのままキー名にします

                // int cursor = 0;

                string[] body = file[1].Split(RS);

                for (int cursor = 0; cursor < int.Parse(header[1]); cursor++)
                {
                    string row = body[cursor];
                    if (row == "") break;

                    result[filenum][cursor + 1] = row.Split(US);
                }
            }

            isParseEnd = true;
            return 0;
        }

        public int getLength(string filename)
        {
            if (filelist == null) return 0;
            int file = Array.IndexOf(filelist, filename);
            if (file < 0) return 0;
            return result[file].Length - 1; // 0にキー名入れてるのでそのぶんズレる
        }
        public string getString(string filename, int row, string key)
        {
            if (filelist == null) return null;
            row++;
            int file = Array.IndexOf(filelist, filename);
            if (file < 0 || row < 0 || result[file].Length <= row) return null;

            int col = Array.IndexOf(result[file][0], key);
            if (col < 0) return null;

            return result[file][row][col];
        }
    }
}