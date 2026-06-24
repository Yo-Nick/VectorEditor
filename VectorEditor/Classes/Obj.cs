using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VectorEditor.Classes
{
    // Базовый абстрактный класс всех графических объектов
    public abstract class Obj
    {
        public bool select = false;
        public Color pColor = Lib.defaultColor;
        public byte pWidth = Lib.defaultWidth;
        public TXY Pc;              // центр поворота
        public double a = 0;        // угол поворота
        public TXY[] Shape = new TXY[4];  // прямоугольник выделения
        public byte typeObj;
        public TXY[] points;        // опорные точки

        // Абстрактные методы

        //Отрисовка объекта на канве с преобразованием координат через делегаты II и JJ
        public abstract void DrawObj(Graphics g, Func<double, int> II, Func<double, int> JJ);
        //прямоугольник выделения
        public abstract void MakeShape();
        //Поиск опорной точки по координатам. Для перемещения объекта
        public abstract bool FindPoint(double x, double y, out int index);

        // Перемещение всех точек
        public virtual void MoveObj(double dx, double dy)
        {
            for (int i = 0; i < points.Length; i++)
            {
                points[i].x += dx;
                points[i].y += dy;
            }
            for (int i = 0; i < 4; i++)
            {
                Shape[i].x += dx;
                Shape[i].y += dy;
            }
            Pc.x += dx;
            Pc.y += dy;
        }

        // Поворот вокруг точки (x0, y0) на угол al0 (в радианах)
        public virtual void RotateObj(double x0, double y0, double al0)
        {
            for (int i = 0; i < points.Length; i++)
            {
                double dx = points[i].x - x0;
                double dy = points[i].y - y0;
                double R = Math.Sqrt(dx * dx + dy * dy);
                double al = Math.Atan2(dy, dx);
                points[i].x = x0 + R * Math.Cos(al + al0);
                points[i].y = y0 + R * Math.Sin(al + al0);
            }
            a += al0;
            MakeShape();
        }

        // Создание пера
        protected Pen GetPen()
        {
            return new Pen(pColor, pWidth);
        }
    }
}
