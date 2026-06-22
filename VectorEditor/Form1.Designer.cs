namespace VectorEditor
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton btnMove;
        private System.Windows.Forms.ToolStripButton btnRect;
        private System.Windows.Forms.ToolStripButton btnEllipse;
        private System.Windows.Forms.ToolStripButton btnBezier;
        private System.Windows.Forms.ToolStripButton btnText;
        private System.Windows.Forms.ToolStripButton btnRotate;
        private System.Windows.Forms.ToolStripButton btnMovePoint;
        private System.Windows.Forms.ToolStripSeparator sep1;
        private System.Windows.Forms.ToolStripButton btnSave;
        private System.Windows.Forms.ToolStripButton btnLoad;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.btnMove = new System.Windows.Forms.ToolStripButton();
            this.btnRect = new System.Windows.Forms.ToolStripButton();
            this.btnEllipse = new System.Windows.Forms.ToolStripButton();
            this.btnBezier = new System.Windows.Forms.ToolStripButton();
            this.btnText = new System.Windows.Forms.ToolStripButton();
            this.btnRotate = new System.Windows.Forms.ToolStripButton();
            this.btnMovePoint = new System.Windows.Forms.ToolStripButton();
            this.sep1 = new System.Windows.Forms.ToolStripSeparator();
            this.btnSave = new System.Windows.Forms.ToolStripButton();
            this.btnLoad = new System.Windows.Forms.ToolStripButton();

            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();

            // toolStrip1
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.btnMove,
                this.btnRect,
                this.btnEllipse,
                this.btnBezier,
                this.btnText,
                this.btnRotate,
                this.btnMovePoint,
                this.sep1,
                this.btnSave,
                this.btnLoad
            });
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(800, 25);
            this.toolStrip1.TabIndex = 0;
            this.toolStrip1.Text = "toolStrip1";

            // btnMove
            this.btnMove.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnMove.Text = "Move";
            this.btnMove.Tag = 0; // tl_Move
            this.btnMove.Click += new System.EventHandler(this.ToolButtonClick);

            // btnRect
            this.btnRect.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnRect.Text = "Rect";
            this.btnRect.Tag = 17; // tl_Rect
            this.btnRect.Click += new System.EventHandler(this.ToolButtonClick);

            // btnEllipse
            this.btnEllipse.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnEllipse.Text = "Ellipse";
            this.btnEllipse.Tag = 18; // tl_Ellipse
            this.btnEllipse.Click += new System.EventHandler(this.ToolButtonClick);

            // btnBezier
            this.btnBezier.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnBezier.Text = "Bezier";
            this.btnBezier.Tag = 1; // tl_AddLineBz
            this.btnBezier.Click += new System.EventHandler(this.ToolButtonClick);

            // btnText
            this.btnText.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnText.Text = "Text";
            this.btnText.Tag = 10; // tl_Text
            this.btnText.Click += new System.EventHandler(this.ToolButtonClick);

            // btnRotate
            this.btnRotate.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnRotate.Text = "Rotate";
            this.btnRotate.Tag = 4; // tl_Rotate
            this.btnRotate.Click += new System.EventHandler(this.ToolButtonClick);

            // btnMovePoint
            this.btnMovePoint.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnMovePoint.Text = "MovePoint";
            this.btnMovePoint.Tag = 8; // tl_MovePoint
            this.btnMovePoint.Click += new System.EventHandler(this.ToolButtonClick);

            // sep1
            this.sep1.Name = "sep1";

            // btnSave
            this.btnSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnSave.Text = "Save";
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);

            // btnLoad
            this.btnLoad.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnLoad.Text = "Load";
            this.btnLoad.Click += new System.EventHandler(this.btnLoad_Click);

            // FormMain
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 600);
            this.Controls.Add(this.toolStrip1);
            this.Name = "FormMain";
            this.Text = "Vector Editor";
            this.KeyPreview = true;

            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}