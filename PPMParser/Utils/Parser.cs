using PPMParser.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace PPMParser.Utils
{
    static class Parser
    {
        public static Bitmap ParsePPMFileToBitMap(string path)
        {
            Bitmap image = null;
            using (FileStream stream = new FileStream(path, FileMode.Open))
            {
                var type = DetectFileType(stream);
                image = type == FileType.P3 ? P3Parse(stream) : P6Parse(stream);
            }
            return image;
        }


        private static string GetStringFromEnumerable(this IEnumerable<char> value)
        {
            StringBuilder builder = new();
            foreach (var c in value)
            {
                builder.Append(c);
            }
            return builder.ToString();
        }

        private static Bitmap P3Parse(FileStream file)
        {
            Bitmap image = null;
            int w;
            int h;
            int maxColor;
            double multiplier;                
            image = Setup(file, out w, out h, out maxColor, out multiplier);
            int x = 0, y = 0;
            int[] colors = new int[3];
            int modulo = 0;
            var bufferSize = 1024 * 5000;
            var buffer = new byte[bufferSize];
            StringBuilder s = new StringBuilder();
            while (true)
            {
                var i = file.Read(buffer, 0, bufferSize);
                if (i == 0)
                    break;

                bool comment = false;
                for (int j = 0; j < i; j++)
                {
                    var b = buffer[j];
                    if (comment)
                    {
                        if (b == '\n')
                            comment = false;
                        continue;
                    }
                    if (b == '#')
                    {
                        comment = true;
                    }
                    else if (b != '\n' && b != ' ' && b != '\t')
                    {
                        s.Append((char)b);
                    }
                    else if (s.Length > 0)
                    {
                        var value = s.ToString();
                        s.Clear();
                        int d;
                        if (Int32.TryParse(value, out d))
                        {
                            colors[modulo] = (int)(d * multiplier);
                            modulo = (modulo + 1) % 3;
                            if (modulo == 0)
                            {
                                Color c = Color.FromArgb(colors[0], colors[1], colors[2]);
                                image.SetPixel(x, y, c);
                                x++;
                                if (x >= w)
                                {
                                    x = 0;
                                    y++;
                                }
                            }
                        }
                        else
                            throw new ArgumentException("bad value");
                    }
                }
            }
            return image;
        }

        private static Bitmap Setup(FileStream file,out int w, out int h, out int maxColor, out double multiplier)
        {
            List<char> tempData = new();
            bool setup = false;
            w = h = maxColor =0;
            multiplier = 0.0;
            Bitmap image = null;
            while (!setup)
            {
                char temp = (char)file.ReadByte();
                if (temp == '#')
                {
                    while (temp != '\n')
                    {
                        temp = (char)file.ReadByte();
                    }
                }
                else if (temp != ' ' && temp != '\t' && temp != '\n')
                {
                    tempData.Add(temp);
                }
                else if (tempData.Count > 0)
                {
                    var valueString = tempData.GetStringFromEnumerable();
                    int value;
                    if (!int.TryParse(valueString, out value))
                    {
                        throw new ArgumentException("bad value");
                    }
                    if (w == 0)
                    {
                        w = value;
                    }
                    else if (h == 0)
                    {
                        h = value;
                        image = new Bitmap(w, h);
                    }
                    else if (maxColor == 0)
                    {
                        maxColor = value;
                        setup = true;
                        multiplier = (double)255 / maxColor;
                    }
                    tempData = new();
                }
            }

            return image; 
        }

        private static Bitmap P6Parse(FileStream file)
        {
            Bitmap image = null;
            int w;
            int h;
            int maxColor;
            double multiplier;
            image = Setup(file, out w, out h, out maxColor, out multiplier);
            int x = 0, y = 0;
            int[] colors = new int[3];
            int modulo = 0;
            var bufferSize = 1024 * 5000;
            var buffer = new byte[bufferSize];

            while (true)
            {
                var i = file.Read(buffer, 0, bufferSize);
                if (i == 0)
                    break;

                for (int j = 0; j < i; j++)
                {
                    colors[modulo] = (int)(buffer[j] * multiplier);
                    modulo = (modulo + 1) % 3;
                    if (modulo == 0)
                    {
                        Color c = Color.FromArgb(colors[0], colors[1], colors[2]);
                        image.SetPixel(x, y, c);
                        x++;
                        if (x >= w)
                        {
                            x = 0;
                            y++;
                            if (y == h)
                                break;
                        }
                    }
                }
            }
            return image;
        }

        private static FileType DetectFileType(FileStream stream)
        {
            byte[] data = new byte[2];
            stream.Read(data, 0, 2);
            while ((char)stream.ReadByte() != '\n') ;
            string ln = $"{(char)data[0]}{(char)data[1]}";
            return ln switch
            {
                var l when l == "P3" => FileType.P3,
                var l when l == "P6" => FileType.P6,
                _ => throw new ArgumentException("invalid ppm type")
            };

        }



    }
}
