using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Formatters.Soap;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;


namespace Serialize
{
    public partial class Form1 : Form
    {
        private Point[] points = null;
        public Form1()
        {
            InitializeComponent();
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            };
        }

        private void btnCreate_Click(object sender, EventArgs e)
        {
            points = new Point[5];

            var rnd = new Random();
            for (int i = 0; i < points.Length; i++)
                points[i] = rnd.Next(3) % 2 == 0 ? new Point() : new Point3D();

            listBox1.DataSource = null;
            listBox1.DataSource = points;
        }

        private void btnSort_Click(object sender, EventArgs e)
        {
            if (points == null)
                return;

            Array.Sort(points);

            listBox1.DataSource = null;
            listBox1.DataSource = points;
        }

        private void btnSerialize_Click(object sender, EventArgs e)
        {
            var dlg = new SaveFileDialog();
            dlg.Filter = "SOAP|*.soap|XML|*.xml|JSON|*.json|Binary|*.bin";

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            using (var fs =
                new FileStream(dlg.FileName, FileMode.Create, FileAccess.Write))
            {
                switch (Path.GetExtension(dlg.FileName))
                {
                    case ".bin":
                        var bf = new BinaryFormatter();
                        bf.Serialize(fs, points);
                        break;
                    case ".soap":
                        var sf = new SoapFormatter();
                        sf.Serialize(fs, points);
                        break;
                    case ".xml":
                        var xf = new XmlSerializer(typeof(Point[]), new[] { typeof(Point3D) });
                        xf.Serialize(fs, points);
                        break;
                    case ".json":
                        var jf = new JsonSerializer();
                        using (var w = new StreamWriter(fs))
                            jf.Serialize(w, points);
                        break;
                }
            }

        }

        private void btnDeserialize_Click(object sender, EventArgs e)
        {
            var dlg = new OpenFileDialog();
            dlg.Filter = "SOAP|*.soap|XML|*.xml|JSON|*.json|Binary|*.bin";

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            using (var fs =
                new FileStream(dlg.FileName, FileMode.Open, FileAccess.Read))
            {
                switch (Path.GetExtension(dlg.FileName))
                {
                    case ".bin":
                        var bf = new BinaryFormatter();
                        points = (Point[])bf.Deserialize(fs);
                        break;
                    case ".soap":
                        var sf = new SoapFormatter();
                        points = (Point[])sf.Deserialize(fs);
                        break;
                    case ".xml":
                        var xf = new XmlSerializer(typeof(Point[]), new[] { typeof(Point3D) });
                        points = (Point[])xf.Deserialize(fs);
                        break;
                    case ".json":
                        var jf = new JsonSerializer();
                        object[] lst;
                        using (var r = new StreamReader(fs))
                            lst = (object[])jf.Deserialize(r, typeof(object[]));
                        List<Point> help = new List<Point>();
                        for(int i=0; i<lst.Length; i++)
                        {
                            string a = lst[i].ToString();
                            if (Point.is_3D(a))
                                help.Add(new Point3D(Point.get_val(a, 'X'), Point.get_val(a, 'Y'), Point.get_val(a, 'Z')));
                            else
                                help.Add(new Point(Point.get_val(a, 'X'), Point.get_val(a, 'Y')));
                        }
                        points = help.ToArray();
                        break;

                }
            }
            listBox1.DataSource = null;
            listBox1.DataSource = points;
        }
    }
}

[Serializable]
public class Point : IComparable
{
    public int X { get; set; }
    public int Y { get; set; }
    protected static Random rnd = new Random();
    public static bool is_3D(string s)
    {
        for(int i=0; i<s.Length; i++)
        {
            if (s[i] == 'Z')
                return true;
        }
        return false;
    }
    public static int get_val(string s, char name)
    {
        for(int idx=0; idx<s.Length; idx++)
        {
            if (s[idx] != name)
                continue;
            bool was = false;
            int res = 0;
            for (int i = idx + 1; i < s.Length; i++)
            {
                if (s[i] >= '0' && s[i] <= '9')
                {
                    was = true;
                    res = res * 10 + (s[i] - '0');
                }
                else if (was)
                    break;
            }
            return res;
            
        }
        return 0;
    }
    public Point()
    {
        //rnd = new Random();
        X = rnd.Next(10);
        Y = rnd.Next(10);
    }
    public Point(int x, int y)
    {
        this.X = x;
        this.Y = y;
    }
    public virtual double metric()
    {
        return Math.Sqrt(X * X + Y * Y);
    }
    public override string ToString()
    {
        return $"({X}, {Y})";
    }
    public virtual int CompareTo(object obj)
    {
        if (obj == null) return 1;
        Point b = obj as Point;
        double m1 = this.metric(), m2 = b.metric();
        if (m1 == m2)
            return 0;
        if (m1 < m2)
            return -1;
        return 1;
    }
}


[Serializable]
public class Point3D : Point
{
    public int Z { get; set; }

    public Point3D() : base()
    {   
        Z = rnd.Next(10);
    }

    public Point3D(int x, int y, int z) : base(x, y)
    {
        Z = z;
    }

    public override double metric()
    {
        return Math.Sqrt(X * X + Y * Y + Z * Z);
    }

    public override string ToString()
    {
        return string.Format($"({X} , {Y}, {Z})");
    }
    public override int CompareTo(object obj)
    {
        if (obj == null) return 1;
        Point b = obj as Point;
        double m1 = this.metric(), m2 = b.metric();
        if (m1 == m2)
            return 0;
        if (m1 < m2)
            return -1;
        return 1;
    }
}
