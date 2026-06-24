using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace VectorEditor.Classes
{
    // Класс страницы, хранит список всех векторных объектов,
    //параметры окна (xMin, yMin, xMax, yMax) и обеспечивает
    // сохранение/загрузку в бинарном формате (для файлов и для отмены действий).
    public class Page
    {
        // ---- Параметры видимого окна в мировых координатах ----
        public double xMin, yMin, xMax, yMax; 
        public double pageWidth, pageHeight;

        // ---- Список объектов ----
        private List<Obj> objects = new List<Obj>();

        public int Count => objects.Count; //кол-во объектов на странцие
        public Obj this[int index] => objects[index];//Индексатор для доступа к объектам по индексу

        public Page(double xMin, double yMin, double xMax, double yMax)
        {
            this.xMin = xMin;
            this.yMin = yMin;
            this.xMax = xMax;
            this.yMax = yMax;
            pageWidth = xMax - xMin;
            pageHeight = yMax - yMin;
        }

        // ---- Управление списком объектов ----
        public void Add(Obj obj) => objects.Add(obj);
        public void Remove(Obj obj) => objects.Remove(obj);
        public void RemoveAt(int index) => objects.RemoveAt(index);
        public void Clear() => objects.Clear();

        //Снять выделение со всех объектов и сбросить глобальные индексы
        public void UnSelectAll()
        {
            foreach (var o in objects) o.select = false;
            Lib.numObj = -1;
            Lib.numShape = -1;
            Lib.numPoint = -1;
        }


        // Поиск объекта по координатам. Проверяет попадание в прямоугольник Shape.
        // Идёт с конца списка (верхние объекты имеют  больший приоритет)
        public bool FindObj(double x, double y, out int index)
        {
            index = -1;
            for (int i = objects.Count - 1; i >= 0; i--)
            {
                var obj = objects[i];
                if (x >= obj.Shape[0].x && x <= obj.Shape[2].x &&
                    y >= obj.Shape[0].y && y <= obj.Shape[2].y)
                {
                    index = i;
                    return true;
                }
            }
            return false;
        }

        //Установка окна просмотра
        public void SetWindow(double xMin, double yMin, double xMax, double yMax)
        {
            this.xMin = xMin;
            this.yMin = yMin;
            this.xMax = xMax;
            this.yMax = yMax;
            pageWidth = xMax - xMin;
            pageHeight = yMax - yMin;
        }

        // ---- Сохранение/загрузка в файл ----
        public void Save(string fileName)
        {
            using (FileStream fs = File.Open(fileName, FileMode.Create))
                SaveToStream(fs);
        }

        public void Load(string fileName)
        {
            using (FileStream fs = File.Open(fileName, FileMode.Open))
                LoadFromStream(fs);
        }

        // ---- Сохранение/загрузка в поток (для Undo) ----
        public void SaveToStream(Stream stream)
        {
            using (BinaryWriter bw = new BinaryWriter(stream, System.Text.Encoding.UTF8, true))
            {
                // Параметры окна
                bw.Write(xMin); bw.Write(yMin); bw.Write(xMax); bw.Write(yMax);
                // Количество объектов
                bw.Write(objects.Count);
                // Сохраняем каждый объект
                foreach (var obj in objects)
                {
                    bw.Write(obj.typeObj);
                    bw.Write(obj.pColor.ToArgb());
                    bw.Write(obj.pWidth);
                    bw.Write(obj.a);
                    bw.Write(obj.points.Length);
                    foreach (var p in obj.points)
                    {
                        bw.Write(p.x);
                        bw.Write(p.y);
                        bw.Write(p.alf);
                    }
                    // Для текста дополнительно сохраняем строку и шрифт
                    if (obj is ObjText txt)
                    {
                        bw.Write(txt.Text ?? "");
                        bw.Write(txt.Font.Name);
                        bw.Write(txt.Font.Size);
                        bw.Write((int)txt.Font.Style);
                    }
                }
            }
        }

        public void LoadFromStream(Stream stream)
        {
            using (BinaryReader br = new BinaryReader(stream, System.Text.Encoding.UTF8, true))
            {
                xMin = br.ReadDouble(); yMin = br.ReadDouble(); xMax = br.ReadDouble(); yMax = br.ReadDouble();
                int count = br.ReadInt32();
                objects.Clear();
                for (int i = 0; i < count; i++)
                {
                    byte type = br.ReadByte();
                    Color color = Color.FromArgb(br.ReadInt32());
                    byte width = br.ReadByte();
                    double angle = br.ReadDouble();
                    int ptCount = br.ReadInt32();
                    TXY[] pts = new TXY[ptCount];
                    for (int j = 0; j < ptCount; j++)
                    {
                        double x = br.ReadDouble();
                        double y = br.ReadDouble();
                        double alf = br.ReadDouble();
                        pts[j] = new TXY(x, y, alf);
                    }
                    Obj obj = null;
                    switch (type)
                    {
                        case 4:
                            if (ptCount >= 2)
                                obj = new ObjRect(pts[0].x, pts[0].y, pts[1].x, pts[1].y, color, width);
                            break;
                        case 5:
                            if (ptCount >= 2)
                                obj = new ObjEllipse(pts[0].x, pts[0].y, pts[1].x, pts[1].y, color, width);
                            break;
                        case 6:
                            obj = new ObjBezier(color, width);
                            foreach (var p in pts)
                                (obj as ObjBezier).AddPoint(p.x, p.y);
                            break;
                        case 10:
                            string text = br.ReadString();
                            string fontName = br.ReadString();
                            float fontSize = br.ReadSingle();
                            FontStyle style = (FontStyle)br.ReadInt32();
                            Font font = new Font(fontName, fontSize, style);
                            if (ptCount >= 1)
                                obj = new ObjText(pts[0].x, pts[0].y, text, color, width, font);
                            break;
                    }
                    if (obj != null)
                    {
                        obj.a = angle;
                        obj.MakeShape();
                        objects.Add(obj);
                    }
                }
            }
        }
    }
}