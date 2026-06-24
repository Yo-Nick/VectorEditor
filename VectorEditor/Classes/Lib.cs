using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VectorEditor.Classes
{
    // Глобальные статические переменные (аналог класса Lib в книге)
    public static class Lib
    {
        public static int numObj = -1;         // номер выделенного объекта
        public static int numShape = -1;       // номер угловой точки шейпа
        public static int numPoint = -1;       // номер выделенной точки кривой
        public static Color defaultColor = Color.Black;
        public static byte defaultWidth = 2;
    }
}
