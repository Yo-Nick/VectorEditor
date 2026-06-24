using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using VectorEditor.Classes;
using System.IO;

namespace VectorEditor
{
    public partial class Form1 : Form
    {
        #region Константы и первоначальные значения
        // Константы инструментов
        private const byte tl_Move = 0;
        private const byte tl_AddLineBz = 1;
        private const byte tl_Rotate = 4;
        private const byte tl_MovePoint = 8;
        private const byte tl_Text = 10;
        private const byte tl_Rect = 17;
        private const byte tl_Ellipse = 18;

        // Текущий инструмент
        private byte flTools = tl_Move;

        // Параметры окна на экране (в пикселях)
        private int I1, J1, I2, J2;
        private Bitmap bitmap;
        private Graphics gScreen;
        private Page page;

        // Флаги рисования
        private bool drawing = false;
        private MouseEventArgs e0, e1;
        private TXY tmpStart; // начальная точка в мировых координатах

        // Для временного хранения при рисовании
        private Bitmap bitmapTmp;

        //Поля для цвета линий
        private Color currentColor = Color.Black; // Текущий цвет для рисования
        private ColorDialog colorDialog = new ColorDialog(); // Диалог выбора цвета

        #endregion

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        public Form1()
        {
            InitializeComponent();
            // Создаём буфер
            this.ClientSize = new Size(800, 600);
            bitmap = new Bitmap(ClientSize.Width, ClientSize.Height);
            gScreen = CreateGraphics();

            // Инициализация страницы (мировые координаты, например, от -5 до 15 по X, от -5 до 15 по Y)
            page = new Page(-5, -5, 15, 15);
            SaveState();
            I1 = 0; J1 = 0; I2 = ClientSize.Width; J2 = ClientSize.Height;

            // Подписываемся на события мыши и клавиатуры
            this.MouseDown += FormMain_MouseDown;
            this.MouseMove += FormMain_MouseMove;
            this.MouseUp += FormMain_MouseUp;
            this.MouseWheel += FormMain_MouseWheel;
            this.Paint += FormMain_Paint;
            this.KeyDown += FormMain_KeyDown;
            this.Resize += FormMain_Resize;
            this.MouseEnter += FormMain_MouseEnter;
            this.MouseLeave += FormMain_MouseLeave;



            AddColorButton();
            UpdateCursor(); // Устанавливаем начальный курсор
        }

        #region ДОБАВЛЕННЫЙ МЕТОД и ОБРАБОТЧИК ДЛЯ КНОПКИ ЦВЕТА 
        private void AddColorButton()
        {
            // Ищем существующую панель инструментов или создаем новую
            ToolStrip toolStrip = null;
            foreach (Control ctrl in this.Controls)
            {
                if (ctrl is ToolStrip ts)
                {
                    toolStrip = ts;
                    break;
                }
            }

            if (toolStrip == null)
            {
                toolStrip = new ToolStrip();
                toolStrip.Dock = DockStyle.Top;
                this.Controls.Add(toolStrip);
            }

            // Создаем кнопку выбора цвета
            ToolStripButton btnColor = new ToolStripButton();
            btnColor.Text = "Цвет"; // Просто текст, без фона
            btnColor.DisplayStyle = ToolStripItemDisplayStyle.Text; // Только текст
                                                                    // Убираем установку BackColor - не меняем фон кнопки
            btnColor.Margin = new Padding(5, 0, 5, 0);
            btnColor.Click += BtnColor_Click;
            toolStrip.Items.Add(btnColor);

            // Добавляем разделитель
            toolStrip.Items.Add(new ToolStripSeparator());
        }

        private void BtnColor_Click(object sender, EventArgs e)
        {
            colorDialog.Color = currentColor;
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                currentColor = colorDialog.Color;
                Lib.defaultColor = currentColor;

                // МЕНЯЕМ ТОЛЬКО ТЕКСТ, А НЕ ЦВЕТ КНОПКИ
                if (sender is ToolStripButton btn)
                {
                    // Показываем название цвета в тексте кнопки
                    btn.Text = $"Цвет: {currentColor.Name}";
                    // Или можно просто оставить "Цвет" без изменений
                    // btn.Text = "Цвет";
                }
            }
        }
        #endregion


        //Свойства
        private int II(double x) => I1 + (int)((x - page.xMin) * (I2 - I1) / (page.xMax - page.xMin));
        private int JJ(double y) => J1 + (int)((page.yMax - y) * (J2 - J1) / (page.yMax - page.yMin));
        private double XX(int I) => page.xMin + (I - I1) * (page.xMax - page.xMin) / (I2 - I1);
        private double YY(int J) => page.yMax - (J - J1) * (page.yMax - page.yMin) / (J2 - J1);

        private void FormMain_Resize(object sender, EventArgs e)
        {
            if (bitmap != null) bitmap.Dispose();
            bitmap = new Bitmap(ClientSize.Width, ClientSize.Height);
            I2 = ClientSize.Width;
            J2 = ClientSize.Height;
            Draw();
        }
        private void Draw()
        {
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.White);
                g.FillRectangle(Brushes.White, II(page.xMin), JJ(page.yMax),
                                 II(page.xMax) - II(page.xMin),
                                 JJ(page.yMin) - JJ(page.yMax));
                for (int i = 0; i < page.Count; i++)
                {
                    var obj = page[i];
                    obj.DrawObj(g, II, JJ);
                    if (obj.select)
                    {
                        DrawShape(g, obj.Shape);
                        if (obj is ObjBezier bez && Lib.numPoint >= 0)
                            DrawBezierHelpers(g, bez);
                    }
                }
            }
            gScreen.DrawImage(bitmap, ClientRectangle);
        }

        private void DrawShape(Graphics g, TXY[] shape)
        {
            if (shape == null || shape.Length < 4) return;
            using (Pen pen = new Pen(Color.Gray, 1) { DashStyle = DashStyle.Dash })
            {
                g.DrawLine(pen, II(shape[0].x), JJ(shape[0].y), II(shape[1].x), JJ(shape[1].y));
                g.DrawLine(pen, II(shape[1].x), JJ(shape[1].y), II(shape[2].x), JJ(shape[2].y));
                g.DrawLine(pen, II(shape[2].x), JJ(shape[2].y), II(shape[3].x), JJ(shape[3].y));
                g.DrawLine(pen, II(shape[3].x), JJ(shape[3].y), II(shape[0].x), JJ(shape[0].y));
            }
            Brush brush = Brushes.Green;
            foreach (var p in shape)
            {
                int x = II(p.x) - 3;
                int y = JJ(p.y) - 3;
                g.FillRectangle(brush, x, y, 6, 6);
                g.DrawRectangle(Pens.Black, x, y, 6, 6);
            }
        }

        private void DrawBezierHelpers(Graphics g, ObjBezier bez)
        {
            int idx = Lib.numPoint;
            if (idx < 0 || idx >= bez.points.Length) return;
            using (Pen pen = new Pen(Color.Black, 1))
            {
                if (idx > 0)
                {
                    int x1 = II(bez.points[idx - 1].x);
                    int y1 = JJ(bez.points[idx - 1].y);
                    int x2 = II(bez.points[idx].x);
                    int y2 = JJ(bez.points[idx].y);
                    g.DrawLine(pen, x1, y1, x2, y2);
                }
                if (idx < bez.points.Length - 1)
                {
                    int x1 = II(bez.points[idx + 1].x);
                    int y1 = JJ(bez.points[idx + 1].y);
                    int x2 = II(bez.points[idx].x);
                    int y2 = JJ(bez.points[idx].y);
                    g.DrawLine(pen, x1, y1, x2, y2);
                }
            }
        }

        private void FormMain_Paint(object sender, PaintEventArgs e) => Draw();

        #region Функции мыши
        private void FormMain_MouseDown(object sender, MouseEventArgs e)
        {
            e0 = e;
            double u = XX(e.X);
            double v = YY(e.Y);

            switch (flTools)
            {
                case tl_Move:
                    if (page.FindObj(u, v, out int idx))
                    {
                        page.UnSelectAll();
                        page[idx].select = true;
                        Lib.numObj = idx;
                        Lib.numShape = -1;
                        var shape = page[idx].Shape;
                        double eps = 0.5;
                        for (int i = 0; i < 4; i++)
                        {
                            if (Math.Abs(u - shape[i].x) < eps && Math.Abs(v - shape[i].y) < eps)
                            {
                                Lib.numShape = i;
                                break;
                            }
                        }
                        drawing = true;
                        Draw();
                    }
                    break;

                case tl_Rect:
                case tl_Ellipse:
                    drawing = true;
                    tmpStart = new TXY(u, v);
                    bitmapTmp = (Bitmap)bitmap.Clone();
                    break;

                case tl_AddLineBz:
                    if (Lib.numObj >= 0 && page[Lib.numObj] is ObjBezier bez)
                    {
                        bez.AddPoint(u, v);
                        SaveState();
                        Draw();
                    }
                    else
                    {
                        var newBez = new ObjBezier(Lib.defaultColor, Lib.defaultWidth);
                        newBez.AddPoint(u, v);
                        SaveState();
                        page.Add(newBez);
                        page.UnSelectAll();
                        newBez.select = true;
                        Lib.numObj = page.Count - 1;
                        drawing = true;
                        Draw();
                    }
                    break;

                case tl_Text:
                    string text = Microsoft.VisualBasic.Interaction.InputBox("Введите текст:", "Текст", "Пример");
                    if (!string.IsNullOrEmpty(text))
                    {
                        FontDialog fd = new FontDialog();
                        if (fd.ShowDialog() == DialogResult.OK)
                        {
                            var txtObj = new ObjText(u, v, text, Lib.defaultColor, Lib.defaultWidth, fd.Font);
                            page.Add(txtObj);
                            SaveState();
                            Draw();
                        }
                    }
                    break;

                case tl_Rotate:
                    if (page.FindObj(u, v, out int idxRot))
                    {
                        page.UnSelectAll();
                        page[idxRot].select = true;
                        Lib.numObj = idxRot;
                        drawing = true;
                        Draw();
                    }
                    break;

                case tl_MovePoint:
                    if (Lib.numObj >= 0 && page[Lib.numObj] is ObjBezier bezPoint)
                    {
                        if (bezPoint.FindPoint(u, v, out int ptIdx))
                        {
                            Lib.numPoint = ptIdx;
                            drawing = true;
                            Draw();
                        }
                    }
                    break;
            }
        }

        private void FormMain_MouseMove(object sender, MouseEventArgs e)
        {
            if (!drawing) return;
            double u = XX(e.X);
            double v = YY(e.Y);

            switch (flTools)
            {
                case tl_Move:
                    if (Lib.numObj >= 0)
                    {
                        var obj = page[Lib.numObj];
                        if (Lib.numShape == -1)
                        {
                            SaveState();
                            double dx = u - XX(e0.X);
                            double dy = v - YY(e0.Y);
                            obj.MoveObj(dx, dy);
                            e0 = e;
                        }
                        else
                        {
                            // Упрощённо – не реализуем изменение размера через углы
                            // Можно добавить логику по аналогии с книгой
                        }
                        Draw();
                    }
                    break;

                case tl_Rect:
                case tl_Ellipse:
                    using (Graphics g = Graphics.FromImage(bitmap))
                    {
                        g.DrawImage(bitmapTmp, 0, 0);
                        using (Pen pen = new Pen(currentColor, Lib.defaultWidth)) // ИСПОЛЬЗУЕМ currentColor
                        {
                            int x1 = II(tmpStart.x);
                            int y1 = JJ(tmpStart.y);
                            int x2 = II(u);
                            int y2 = JJ(v);
                            if (flTools == tl_Rect)
                                g.DrawRectangle(pen, Math.Min(x1, x2), Math.Min(y1, y2),
                                                Math.Abs(x2 - x1), Math.Abs(y2 - y1));
                            else
                                g.DrawEllipse(pen, Math.Min(x1, x2), Math.Min(y1, y2),
                                              Math.Abs(x2 - x1), Math.Abs(y2 - y1));
                        }
                    }
                    gScreen.DrawImage(bitmap, ClientRectangle);
                    break;

                case tl_AddLineBz:
                    if (Lib.numObj >= 0 && page[Lib.numObj] is ObjBezier bez)
                    {
                        if (bez.points.Length > 0)
                        {
                            double lastX = bez.points[bez.points.Length - 1].x;
                            double lastY = bez.points[bez.points.Length - 1].y;
                            double dx = u - lastX;
                            double dy = v - lastY;
                            if (dx * dx + dy * dy > 0.5 * 0.5)
                            {
                                if (bez.points.Length >= 4)
                                {
                                    double firstX = bez.points[0].x;
                                    double firstY = bez.points[0].y;
                                    double d1 = u - firstX;
                                    double d2 = v - firstY;
                                    if (d1 * d1 + d2 * d2 < 0.5 * 0.5)
                                    {
                                        bez.CloseCurve();
                                        drawing = false;
                                        Draw();
                                        return;
                                    }
                                }
                                bez.AddPoint(u, v);
                                Draw();
                            }
                        }
                    }
                    break;

                case tl_Rotate:
                    if (Lib.numObj >= 0)
                    {
                        var obj = page[Lib.numObj];
                        double cx = obj.Pc.x;
                        double cy = obj.Pc.y;
                        double angleStart = Math.Atan2(YY(e0.Y) - cy, XX(e0.X) - cx);
                        double angleNow = Math.Atan2(v - cy, u - cx);
                        double delta = angleNow - angleStart;
                        if (Math.Abs(delta) > 0.01)
                        {
                            SaveState();
                            obj.RotateObj(cx, cy, delta);
                            e0 = e;
                            Draw();
                        }
                    }
                    break;

                case tl_MovePoint:
                    if (Lib.numObj >= 0 && page[Lib.numObj] is ObjBezier bezMv && Lib.numPoint >= 0)
                    {
                        SaveState();
                        int idx = Lib.numPoint;
                        bezMv.points[idx] = new TXY(u, v);
                        bezMv.MakeShape();
                        Draw();
                    }
                    break;
            }
        }

        private void FormMain_MouseUp(object sender, MouseEventArgs e)
        {
            if (!drawing) return;
            double u = XX(e.X);
            double v = YY(e.Y);

            switch (flTools)
            {
                case tl_Rect:
                case tl_Ellipse:
                    if (Math.Abs(u - tmpStart.x) > 0.1 || Math.Abs(v - tmpStart.y) > 0.1)
                    {
                        Obj obj;
                        if (flTools == tl_Rect)
                            obj = new ObjRect(tmpStart.x, tmpStart.y, u, v, currentColor, Lib.defaultWidth); // ИСПОЛЬЗУЕМ currentColor
                        else
                            obj = new ObjEllipse(tmpStart.x, tmpStart.y, u, v, currentColor, Lib.defaultWidth); // ИСПОЛЬЗУЕМ currentColor
                        page.Add(obj);
                        SaveState();
                        Console.Write("Тест для пул реквеста");
                        page.UnSelectAll();
                        obj.select = true;
                        Lib.numObj = page.Count - 1;
                    }
                    drawing = false;
                    Draw();
                    break;

                case tl_Move:
                case tl_AddLineBz:
                case tl_Rotate:
                case tl_MovePoint:
                    drawing = false;
                    Lib.numPoint = -1;
                    Draw();
                    break;
            }
        }

        private void FormMain_MouseWheel(object sender, MouseEventArgs e)
        {
            double centerX = (page.xMin + page.xMax) / 2;
            double centerY = (page.yMin + page.yMax) / 2;
            double factor = e.Delta > 0 ? 0.9 : 1.1;
            double newWidth = (page.xMax - page.xMin) * factor;
            double newHeight = (page.yMax - page.yMin) * factor;
            page.SetWindow(centerX - newWidth / 2, centerY - newHeight / 2,
                           centerX + newWidth / 2, centerY + newHeight / 2);
            Draw();
        }

        #endregion

        #region Выбор инструмента
        public void SetTool(byte tool)
        {
            flTools = tool;
            if (tool != tl_Move && tool != tl_Rotate && tool != tl_MovePoint)
            {
                page.UnSelectAll();
                Draw();
            }
            // ========== ДОБАВЛЕННАЯ СТРОКА ==========
            UpdateCursor(); // Обновляем курсор при смене инструмента
        }

        // Обработчики для кнопок (вызываются из дизайнера)
        private void ToolButtonClick(object sender, EventArgs e)
        {
            if (sender is ToolStripButton btn && btn.Tag != null)
            {
                byte tool = Convert.ToByte(btn.Tag);
                SetTool(tool);
            }
        }

        #endregion

        #region Сохранение и загрузка файла
        public void SaveProject(string fileName) => page.Save(fileName);
        public void LoadProject(string fileName)
        {
            page.Load(fileName);
            undoStack.Clear();
            SaveState();
            Draw();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Vector files (*.vec)|*.vec";
            if (sfd.ShowDialog() == DialogResult.OK)
                SaveProject(sfd.FileName);
        }
        private void btnLoad_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Vector files (*.vec)|*.vec";
            if (ofd.ShowDialog() == DialogResult.OK)
                LoadProject(ofd.FileName);
        }

        #endregion

        #region ЭКСПОРТ
        private void btnExport_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "JPEG Image|*.jpg|DXF |*.dxf";
            sfd.Title = "Экспорт изображения";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                string ext = Path.GetExtension(sfd.FileName).ToLower();
                try
                {
                    switch (ext)
                    {
                        case ".jpg": ExportJPEG(sfd.FileName); break;
                        case ".dxf": ExportDXF(sfd.FileName); break;
                        default: MessageBox.Show("Unsupported format."); break;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошбика Экспорта: {ex.Message}");
                }
            }
        }

        #region Экспорт в .jpeg
        private void ExportJPEG(string fileName)
        {
            bitmap.Save(fileName, System.Drawing.Imaging.ImageFormat.Jpeg);
        }
        #endregion
        #region Экспорт в .dxf
        private void ExportDXF(string fileName)
        {
            using (StreamWriter sw = new StreamWriter(fileName))
            {
                // Заголовок
                sw.WriteLine("0");
                sw.WriteLine("SECTION");
                sw.WriteLine("2");
                sw.WriteLine("HEADER");
                sw.WriteLine("0");
                sw.WriteLine("ENDSEC");

                // Таблица слоёв
                sw.WriteLine("0");
                sw.WriteLine("SECTION");
                sw.WriteLine("2");
                sw.WriteLine("TABLES");
                sw.WriteLine("0");
                sw.WriteLine("TABLE");
                sw.WriteLine("2");
                sw.WriteLine("LAYER");
                sw.WriteLine("70");
                sw.WriteLine("1");
                sw.WriteLine("0");
                sw.WriteLine("LAYER");
                sw.WriteLine("2");
                sw.WriteLine("0");
                sw.WriteLine("70");
                sw.WriteLine("0");
                sw.WriteLine("62");
                sw.WriteLine("7"); // белый
                sw.WriteLine("6");
                sw.WriteLine("CONTINUOUS");
                sw.WriteLine("0");
                sw.WriteLine("ENDTAB");
                sw.WriteLine("0");
                sw.WriteLine("ENDSEC");

                // Секция ENTITIES
                sw.WriteLine("0");
                sw.WriteLine("SECTION");
                sw.WriteLine("2");
                sw.WriteLine("ENTITIES");

                // Обходим объекты
                for (int i = 0; i < page.Count; i++)
                {
                    Obj obj = page[i];
                    if (obj is ObjRect rect)
                    {
                        double x1 = rect.points[0].x;
                        double y1 = rect.points[0].y;
                        double x2 = rect.points[1].x;
                        double y2 = rect.points[1].y;
                        WriteLine(sw, x1, y1, x2, y1, obj);
                        WriteLine(sw, x2, y1, x2, y2, obj);
                        WriteLine(sw, x2, y2, x1, y2, obj);
                        WriteLine(sw, x1, y2, x1, y1, obj);
                    }
                    else if (obj is ObjEllipse ell)
                    {
                        double cx = (ell.points[0].x + ell.points[1].x) / 2;
                        double cy = (ell.points[0].y + ell.points[1].y) / 2;
                        double rx = Math.Abs(ell.points[1].x - ell.points[0].x) / 2;
                        double ry = Math.Abs(ell.points[1].y - ell.points[0].y) / 2;
                        var pts = GetEllipsePoints(cx, cy, rx, ry, 30);
                        WritePolyline(sw, pts, obj);
                    }
                    else if (obj is ObjBezier bez)
                    {
                        List<PointF> pts = new List<PointF>();
                        var segments = GetBezierSegments(bez);
                        foreach (var seg in segments)
                        {
                            // Аппроксимируем каждый сегмент 15 отрезками
                            for (int t = 0; t <= 15; t++)
                            {
                                double u = t / 15.0;
                                double x = (1 - u) * (1 - u) * (1 - u) * seg[0].X +
                                           3 * (1 - u) * (1 - u) * u * seg[1].X +
                                           3 * (1 - u) * u * u * seg[2].X +
                                           u * u * u * seg[3].X;
                                double y = (1 - u) * (1 - u) * (1 - u) * seg[0].Y +
                                           3 * (1 - u) * (1 - u) * u * seg[1].Y +
                                           3 * (1 - u) * u * u * seg[2].Y +
                                           u * u * u * seg[3].Y;
                                pts.Add(new PointF((float)x, (float)y));
                            }
                        }
                        if (bez.IsClosed && pts.Count > 0)
                            pts.Add(pts[0]);
                        if (pts.Count > 1)
                            WritePolyline(sw, pts, obj);
                    }
                    else if (obj is ObjText txt)
                    {
                        sw.WriteLine("0");
                        sw.WriteLine("TEXT");
                        sw.WriteLine("8");
                        sw.WriteLine("0");
                        sw.WriteLine("10");
                        sw.WriteLine(txt.points[0].x.ToString(CultureInfo.InvariantCulture));
                        sw.WriteLine("20");
                        sw.WriteLine(txt.points[0].y.ToString(CultureInfo.InvariantCulture));
                        sw.WriteLine("30");
                        sw.WriteLine("0");
                        sw.WriteLine("40");
                        sw.WriteLine(txt.Font.Size.ToString(CultureInfo.InvariantCulture));
                        sw.WriteLine("1");
                        sw.WriteLine(txt.Text);
                        sw.WriteLine("50");
                        sw.WriteLine("0");
                        sw.WriteLine("62");
                        sw.WriteLine(ColorToDXF(txt.pColor));
                    }
                }

                sw.WriteLine("0");
                sw.WriteLine("ENDSEC");
                sw.WriteLine("0");
                sw.WriteLine("EOF");
            }
        }

        // Вспомогательные методы для DXF
        private void WriteLine(StreamWriter sw, double x1, double y1, double x2, double y2, Obj obj)
        {
            sw.WriteLine("0");
            sw.WriteLine("LINE");
            sw.WriteLine("8");
            sw.WriteLine("0");
            sw.WriteLine("10");
            sw.WriteLine(x1.ToString(CultureInfo.InvariantCulture));
            sw.WriteLine("20");
            sw.WriteLine(y1.ToString(CultureInfo.InvariantCulture));
            sw.WriteLine("30");
            sw.WriteLine("0");
            sw.WriteLine("11");
            sw.WriteLine(x2.ToString(CultureInfo.InvariantCulture));
            sw.WriteLine("21");
            sw.WriteLine(y2.ToString(CultureInfo.InvariantCulture));
            sw.WriteLine("31");
            sw.WriteLine("0");
            sw.WriteLine("62");
            sw.WriteLine(ColorToDXF(obj.pColor));
        }

        private void WritePolyline(StreamWriter sw, List<PointF> pts, Obj obj)
        {
            if (pts.Count < 2) return;
            sw.WriteLine("0");
            sw.WriteLine("POLYLINE");
            sw.WriteLine("8");
            sw.WriteLine("0");
            sw.WriteLine("66");
            sw.WriteLine("1");
            sw.WriteLine("62");
            sw.WriteLine(ColorToDXF(obj.pColor));
            foreach (var p in pts)
            {
                sw.WriteLine("0");
                sw.WriteLine("VERTEX");
                sw.WriteLine("8");
                sw.WriteLine("0");
                sw.WriteLine("10");
                sw.WriteLine(p.X.ToString(CultureInfo.InvariantCulture));
                sw.WriteLine("20");
                sw.WriteLine(p.Y.ToString(CultureInfo.InvariantCulture));
                sw.WriteLine("30");
                sw.WriteLine("0");
            }
            sw.WriteLine("0");
            sw.WriteLine("SEQEND");
        }

        private List<PointF> GetEllipsePoints(double cx, double cy, double rx, double ry, int segments)
        {
            List<PointF> pts = new List<PointF>();
            for (int i = 0; i <= segments; i++)
            {
                double angle = 2 * Math.PI * i / segments;
                double x = cx + rx * Math.Cos(angle);
                double y = cy + ry * Math.Sin(angle);
                pts.Add(new PointF((float)x, (float)y));
            }
            return pts;
        }

        private List<PointF[]> GetBezierSegments(ObjBezier bez)
        {
            List<PointF[]> segs = new List<PointF[]>();
            if (bez.points.Length < 4) return segs;
            int i = 1;
            while (i + 2 < bez.points.Length)
            {
                PointF[] seg = new PointF[4];
                seg[0] = new PointF((float)bez.points[i - 1].x, (float)bez.points[i - 1].y);
                seg[1] = new PointF((float)bez.points[i].x, (float)bez.points[i].y);
                seg[2] = new PointF((float)bez.points[i + 1].x, (float)bez.points[i + 1].y);
                seg[3] = new PointF((float)bez.points[i + 2].x, (float)bez.points[i + 2].y);
                segs.Add(seg);
                i += 3;
            }
            return segs;
        }

        private int ColorToDXF(Color c)
        {
            if (c == Color.Black) return 0;
            if (c == Color.Red) return 1;
            if (c == Color.Yellow) return 2;
            if (c == Color.Green) return 3;
            if (c == Color.Cyan) return 4;
            if (c == Color.Blue) return 5;
            if (c == Color.Magenta) return 6;
            if (c == Color.White) return 7;
            return 7; // по умолчанию белый
        }

        #endregion

        #endregion

        #region Изменение темы приложения
        private void ApplyTheme(bool dark)
        {
            if (dark)
            {
                // Тёмная тема
                this.BackColor = Color.FromArgb(45, 45, 48);
                this.ForeColor = Color.White;                 
                toolStrip1.BackColor = Color.FromArgb(45, 45, 48);
                toolStrip1.ForeColor = Color.White;

            }
            else
            {
                // Светлая тема (по умолч)
                this.BackColor = SystemColors.Control;
                this.ForeColor = SystemColors.ControlText;
                toolStrip1.BackColor = SystemColors.Control;
                toolStrip1.ForeColor = SystemColors.ControlText;
            }

            // Принудительно перерисовываем форму
            this.Invalidate();
        }

        private void btnLightTheme_Click(object sender, EventArgs e)
        {
            ApplyTheme(false);
        }

        private void btnDarkTheme_Click(object sender, EventArgs e)
        {
            ApplyTheme(true);
        }

        #endregion

        #region Возможность Ctrl + Z

        private Stack<byte[]> undoStack = new Stack<byte[]>();
        private const int MaxUndo = 30;
        private bool isUndoing = false;

         
        private void SaveState()
        {
            if (isUndoing) return;
            using (MemoryStream ms = new MemoryStream())
            {
                page.SaveToStream(ms);
                byte[] state = ms.ToArray();
                undoStack.Push(state);
                if (undoStack.Count > MaxUndo)
                {
                    // Удаляем самые старые
                    var temp = undoStack.Reverse().Take(MaxUndo).ToList();
                    undoStack.Clear();
                    foreach (var s in temp) undoStack.Push(s);
                }
            }
        }

        private void Undo()
        {
            if (undoStack.Count == 0) return;
            isUndoing = true;
            try
            {
                byte[] state = undoStack.Pop();
                using (MemoryStream ms = new MemoryStream(state))
                {
                    page.LoadFromStream(ms);
                }
                page.UnSelectAll();
                Lib.numObj = -1;
                Lib.numPoint = -1;
                drawing = false;
                Draw();
            }
            finally
            {
                isUndoing = false;
            }
        }


        private void FormMain_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.Z)
            {
                Undo();
                e.Handled = true;
                return;
            }

            if (e.KeyCode == Keys.Delete && Lib.numObj >= 0 && Lib.numObj < page.Count)
            {
                page.RemoveAt(Lib.numObj);
                SaveState();
                Lib.numObj = -1;
                Draw();
            }
        }

        #endregion

        #region Различные типы курсоров
        private void UpdateCursor()
        {
            switch (flTools)
            {
                case tl_Move:
                    this.Cursor = Cursors.SizeAll; // Обычный курсор
                    break;
                case tl_Rect:

                case tl_Ellipse:
                    this.Cursor = Cursors.Cross; // Крестик для рисования
                    break;
                case tl_AddLineBz:
                    this.Cursor = Cursors.Cross; // Перо для рисования линий
                    break;
                case tl_Rotate:
                    this.Cursor = Cursors.SizeAll; // Для поворота
                    break;
                case tl_Text:
                    this.Cursor = Cursors.IBeam; // Для текста
                    break;
                case tl_MovePoint:
                    this.Cursor = Cursors.Hand; // Рука для перемещения точек
                    break;
                default:
                    this.Cursor = Cursors.Default;
                    break;
            }
        }
        private void FormMain_MouseEnter(object sender, EventArgs e)
        {
            UpdateCursor();
        }
        

        // ========== ДОБАВЛЕННЫЙ ОБРАБОТЧИК ==========
        private void FormMain_MouseLeave(object sender, EventArgs e)
        {
            // Когда мышь покидает форму, возвращаем стандартный курсор
            this.Cursor = Cursors.Default;
        }
        #endregion
    }
}