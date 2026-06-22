using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VectorEditor.Classes
{
    // Текстовый объект
    public class ObjText : Obj
    {
        public string Text { get; set; }
        public Font Font { get; set; }

        public ObjText(double x, double y, string text, Color color, byte width, Font font)
        {
            pColor = color;
            pWidth = width;
            typeObj = 10; // по книге
            points = new TXY[1] { new TXY(x, y) };
            Text = text;
            Font = font;
            MakeShape();
        }

        public override void MakeShape()
        {
            // Приблизительный размер (в мировых координатах)
            // Для точности нужен Graphics, но здесь мы просто зададим фиксированный размер
            // В реальном приложении нужно использовать MeasureString.
            double w = 2.0; // примерно 2 единицы
            double h = 0.5;
            Shape[0] = new TXY(points[0].x - w / 2, points[0].y - h / 2);
            Shape[1] = new TXY(points[0].x + w / 2, points[0].y - h / 2);
            Shape[2] = new TXY(points[0].x + w / 2, points[0].y + h / 2);
            Shape[3] = new TXY(points[0].x - w / 2, points[0].y + h / 2);
            Pc = new TXY(points[0].x, points[0].y);
        }

        public override void DrawObj(Graphics g, Func<double, int> II, Func<double, int> JJ)
        {
            if (string.IsNullOrEmpty(Text)) return;
            using (Brush brush = new SolidBrush(pColor))
            {
                int x = II(points[0].x);
                int y = JJ(points[0].y);
                g.DrawString(Text, Font, brush, x, y);
            }
        }

        public override bool FindPoint(double x, double y, out int index)
        {
            index = -1;
            double eps = 5.0;
            double dx = x - points[0].x;
            double dy = y - points[0].y;
            if (dx * dx + dy * dy < eps * eps)
            {
                index = 0;
                return true;
            }
            return false;
        }
    }
}
