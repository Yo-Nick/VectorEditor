using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VectorEditor.Classes
{
    // Эллипс (аналогично прямоугольнику)
    public class ObjEllipse : Obj
    {
        public ObjEllipse(double x1, double y1, double x2, double y2, Color color, byte width)
        {
            pColor = color;
            pWidth = width;
            typeObj = 5;
            points = new TXY[2] { new TXY(x1, y1), new TXY(x2, y2) };
            MakeShape();
        }

        public override void MakeShape()
        {
            double x1 = Math.Min(points[0].x, points[1].x);
            double y1 = Math.Min(points[0].y, points[1].y);
            double x2 = Math.Max(points[0].x, points[1].x);
            double y2 = Math.Max(points[0].y, points[1].y);
            Shape[0] = new TXY(x1, y1);
            Shape[1] = new TXY(x2, y1);
            Shape[2] = new TXY(x2, y2);
            Shape[3] = new TXY(x1, y2);
            Pc = new TXY((x1 + x2) / 2, (y1 + y2) / 2);
        }

        public override void DrawObj(Graphics g, Func<double, int> II, Func<double, int> JJ)
        {
            using (Pen pen = GetPen())
            {
                int x1 = II(points[0].x);
                int y1 = JJ(points[0].y);
                int x2 = II(points[1].x);
                int y2 = JJ(points[1].y);
                g.DrawEllipse(pen, Math.Min(x1, x2), Math.Min(y1, y2),
                              Math.Abs(x2 - x1), Math.Abs(y2 - y1));
            }
        }

        public override bool FindPoint(double x, double y, out int index)
        {
            index = -1;
            double eps = 5.0;
            for (int i = 0; i < points.Length; i++)
            {
                double dx = x - points[i].x;
                double dy = y - points[i].y;
                if (dx * dx + dy * dy < eps * eps)
                {
                    index = i;
                    return true;
                }
            }
            return false;
        }
    }
}
