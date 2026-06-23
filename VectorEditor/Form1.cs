using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using VectorEditor.Classes;

namespace VectorEditor
{
    public partial class Form1 : Form
    {
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

        // ========== ДОБАВЛЕННЫЕ ПОЛЯ ДЛЯ ЦВЕТА ==========
        private Color currentColor = Color.Black; // Текущий цвет для рисования
        private ColorDialog colorDialog = new ColorDialog(); // Диалог выбора цвета

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

        // ========== ДОБАВЛЕННЫЙ МЕТОД ДЛЯ КНОПКИ ЦВЕТА ==========
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

        // ========== ДОБАВЛЕННЫЙ ОБРАБОТЧИК КНОПКИ ЦВЕТА ==========
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

        private void FormMain_Resize(object sender, EventArgs e)
        {
            if (bitmap != null) bitmap.Dispose();
            bitmap = new Bitmap(ClientSize.Width, ClientSize.Height);
            I2 = ClientSize.Width;
            J2 = ClientSize.Height;
            Draw();
        }

        private int II(double x) => I1 + (int)((x - page.xMin) * (I2 - I1) / (page.xMax - page.xMin));
        private int JJ(double y) => J1 + (int)((page.yMax - y) * (J2 - J1) / (page.yMax - page.yMin));
        private double XX(int I) => page.xMin + (I - I1) * (page.xMax - page.xMin) / (I2 - I1);
        private double YY(int J) => page.yMax - (J - J1) * (page.yMax - page.yMin) / (J2 - J1);

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
                        Draw();
                    }
                    else
                    {
                        var newBez = new ObjBezier(Lib.defaultColor, Lib.defaultWidth);
                        newBez.AddPoint(u, v);
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
                            obj.RotateObj(cx, cy, delta);
                            e0 = e;
                            Draw();
                        }
                    }
                    break;

                case tl_MovePoint:
                    if (Lib.numObj >= 0 && page[Lib.numObj] is ObjBezier bezMv && Lib.numPoint >= 0)
                    {
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

        private void FormMain_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete && Lib.numObj >= 0 && Lib.numObj < page.Count)
            {
                page.RemoveAt(Lib.numObj);
                Lib.numObj = -1;
                Draw();
            }
        }

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

        public void SaveProject(string fileName) => page.Save(fileName);
        public void LoadProject(string fileName) { page.Load(fileName); Draw(); }

        // Обработчики для кнопок (вызываются из дизайнера)
        private void ToolButtonClick(object sender, EventArgs e)
        {
            if (sender is ToolStripButton btn && btn.Tag != null)
            {
                byte tool = Convert.ToByte(btn.Tag);
                SetTool(tool);
            }
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
    }
}