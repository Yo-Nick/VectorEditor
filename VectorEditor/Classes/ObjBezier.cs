using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VectorEditor.Classes
{
    // Кривая Безье
    public class ObjBezier : Obj
    {
        public bool IsClosed => closed;
        private bool closed = false;

        public ObjBezier(Color color, byte width)
        {
            pColor = color;
            pWidth = width;
            typeObj = 6;
            points = new TXY[0];
        }

        public void AddPoint(double x, double y)
        {
            Array.Resize(ref points, points.Length + 1);
            points[points.Length - 1] = new TXY(x, y);
            if (points.Length >= 4)
                MakeShape();
        }

        public void CloseCurve()
        {
            if (points.Length >= 4 && !closed)
            {
                closed = true;
                Array.Resize(ref points, points.Length + 1);
                points[points.Length - 1] = points[0];
                MakeShape();
            }
        }

        public override void MakeShape()
        {
            if (points.Length < 2) return;
            double xMin = points[0].x, xMax = points[0].x;
            double yMin = points[0].y, yMax = points[0].y;
            foreach (var p in points)
            {
                if (p.x < xMin) xMin = p.x;
                if (p.x > xMax) xMax = p.x;
                if (p.y < yMin) yMin = p.y;
                if (p.y > yMax) yMax = p.y;
            }
            Shape[0] = new TXY(xMin, yMin);
            Shape[1] = new TXY(xMax, yMin);
            Shape[2] = new TXY(xMax, yMax);
            Shape[3] = new TXY(xMin, yMax);
            Pc = new TXY((xMin + xMax) / 2, (yMin + yMax) / 2);
        }

        public override void DrawObj(Graphics g, Func<double, int> II, Func<double, int> JJ)
        {
            if (points.Length == 0) return;

            using (Pen pen = GetPen())
            {
                // Если только одна точка – рисуем её
                if (points.Length == 1)
                {
                    int x = II(points[0].x);
                    int y = JJ(points[0].y);
                    g.DrawEllipse(pen, x - 3, y - 3, 6, 6);
                    return;
                }

                PointF[] pts = new PointF[points.Length];
                for (int i = 0; i < points.Length; i++)
                {
                    pts[i] = new PointF(II(points[i].x), JJ(points[i].y));
                }

                if (closed && points.Length >= 4)
                {
                    List<PointF> list = new List<PointF>(pts);
                    list.Add(pts[0]);
                    g.DrawClosedCurve(pen, list.ToArray(), 0.5f, FillMode.Alternate);
                }
                else
                {
                    // Если точек достаточно для Безье – рисуем Безье, иначе ломаную
                    if ((points.Length - 1) % 3 == 0 && points.Length >= 4)
                    {
                        g.DrawBeziers(pen, pts);
                    }
                    else
                    {
                        // Рисуем ломаную линию, чтобы видеть процесс
                        g.DrawLines(pen, pts);
                    }
                }
            }

            // Рисуем опорные точки (для наглядности)
            using (Brush brush = new SolidBrush(Color.Red))
            {
                foreach (var p in points)
                {
                    int x = II(p.x) - 2;
                    int y = JJ(p.y) - 2;
                    g.FillEllipse(brush, x, y, 4, 4);
                }
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

        public int FindNodePoint(double x, double y)
        {
            int idx;
            if (FindPoint(x, y, out idx))
                return idx;
            return -1;
        }
    }
}