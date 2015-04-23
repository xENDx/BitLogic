using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BitLogic;
using System.Drawing;

namespace BitLogic_Testing
{
    class Program
    {
        static void Main(string[] args)
        {

            BitArray a = new BitArray(300);
            BitArray b = a.Clone();
            a[0] = true;
            a[1] = false;
            // ...

            a = a & b;
            a = a | b;
            a = a ^ b;

            BitArray.Copy(a, b, 0, 10, 20);
            a.Equals(b);

            int index;
            index = a.Search(new BitArray("xx x", 'x')); // first index of 1101

            SquareBitArray aa = new SquareBitArray(100, 100);
            SquareBitArray bb = aa.Clone();

            aa = aa & bb;
            aa = aa | bb;
            aa = aa ^ bb;
            aa = ~aa;

            SquareBitArray.Copy(aa, bb, 0, 1, 10, 11, 20, 21); // Copy a 20x21 area from point 0,1 in aa to 10,11 in bb
            aa.Equals(bb);

            string CurrentDirectory = System.IO.Directory.GetCurrentDirectory();
            CurrentDirectory = CurrentDirectory.Substring(0, CurrentDirectory.IndexOf("bin"));

            // flood find an area
            Bitmap bitmap = (Bitmap)Bitmap.FromFile(CurrentDirectory+"/img/test floodfind in.png");
            Point at = new Point(10, 10);
            int colorat = bitmap.GetPixel(10, 10).ToArgb();

            SquareBitArray FloodFound = SquareBitArray.FloodFind<int>((x, y) => { return bitmap.GetPixel(x, y).ToArgb(); }, bitmap.Width, bitmap.Height, at, (color) => { return color == colorat; });

            bitmap = new Bitmap(bitmap.Width, bitmap.Height);
            foreach (Point p in FloodFound.ForEach(true))
                bitmap.SetPixel(p.X, p.Y, Color.Black);
            bitmap.Save(CurrentDirectory+"/img/test floodfind out.png");
            

        }
    }
}
