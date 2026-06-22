using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VectorEditor.Classes
{
    // Вспомогательная структура точки с углом поворота
    public struct TXY
    {
        public double x, y, alf;
        public TXY(double x, double y, double alf = 0)
        {
            this.x = x;
            this.y = y;
            this.alf = alf;
        }
    }
}
