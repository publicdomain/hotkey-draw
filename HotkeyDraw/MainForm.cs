
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace HotkeyDraw
{
    /// <summary>
    /// Description of MainForm.
    /// </summary>
    public partial class MainForm : Form
    {
        private bool isDrawing = false;

        private Pen drawPen = new Pen(new SolidBrush(Color.Black), 4);

        private Point selectionPoint;

        private Rectangle drawRectangle = new Rectangle();

        private List<Rectangle> drawRectangleList = new List<Rectangle>();

        private List<Pen> drawRectanglePenList = new List<Pen>();

        private Point[] drawLinePointArray = new Point[2];

        private List<Point[]> drawLinePointArrayList = new List<Point[]>();

        private List<Pen> drawLinePointArrayPenList = new List<Pen>();

        private const int MOD_CONTROL = 0x0002;

        private const int MOD_SHIFT = 0x0004;

        private const int WM_HOTKEY = 0x0312;

        [DllImport("User32")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

        [DllImport("User32")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private int shapeIndex = 0;

        private int timesOne = 0;

        private int timesTwo = 0;

        private bool doneDrawing = false;

        public MainForm()
        {
            this.InitializeComponent();

            RegisterHotKey(this.Handle, 1, MOD_CONTROL + MOD_SHIFT, Convert.ToInt16(Keys.D1));

            RegisterHotKey(this.Handle, 2, MOD_CONTROL + MOD_SHIFT, Convert.ToInt16(Keys.D2));

            RegisterHotKey(this.Handle, 3, MOD_CONTROL + MOD_SHIFT, Convert.ToInt16(Keys.D3));

            RegisterHotKey(this.Handle, 4, MOD_CONTROL + MOD_SHIFT, Convert.ToInt16(Keys.D4));
        }

        private void MainFormLoad(object sender, EventArgs e)
        {
            // TODO
        }

        private void SetCanvasPictureBoxImage()
        {
            this.canvasPictureBox.Image = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height, PixelFormat.Format32bppArgb);

            Graphics graphics = Graphics.FromImage(this.canvasPictureBox.Image);

            graphics.CopyFromScreen(Screen.PrimaryScreen.Bounds.X, Screen.PrimaryScreen.Bounds.Y, 0, 0, Screen.PrimaryScreen.Bounds.Size, CopyPixelOperation.SourceCopy);

            this.canvasPictureBox.Invalidate();
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_HOTKEY)
            {
                if (this.doneDrawing)
                {
                    this.timesOne = 0;
                    this.timesTwo = 0;
                    this.doneDrawing = false;
                }

                switch (m.WParam.ToInt32())
                {
                    case 1:

                        this.timesTwo = 0;

                        this.timesOne++;

                        if (this.timesOne > 3)
                        { this.timesOne = 1; }

                        this.SetDrawing(m.WParam.ToInt32(), Math.Max(this.timesOne, this.timesTwo));

                        break;
                    case 2:

                        this.timesOne = 0;

                        this.timesTwo++;

                        if (this.timesOne > 3)
                        { this.timesTwo = 1; }

                        this.SetDrawing(m.WParam.ToInt32(), Math.Max(this.timesOne, this.timesTwo));

                        break;

                    case 3:
                        this.drawRectangleList.Clear();

                        this.drawLinePointArrayList.Clear();

                        this.canvasPictureBox.Invalidate();

                        break;

                    case 4:
                        this.Close();

                        break;
                }
            }
            else
            {
                base.WndProc(ref m);
            }
        }

        private void SetDrawing(int messageShapeIndex, int colorIndex)
        {
            this.SetCanvasPictureBoxImage();

            this.shapeIndex = messageShapeIndex;

            this.selectionPoint = new Point();

            this.isDrawing = true;

            switch (colorIndex)
            {
                case 1:
                    this.drawPen = new Pen(new SolidBrush(Color.Black), 4);

                    break;

                case 2:
                    this.drawPen = new Pen(new SolidBrush(Color.White), 4);

                    break;

                default:
                    this.drawPen = new Pen(new SolidBrush(Color.Red), 4);

                    break;
            }

            this.canvasPictureBox.Cursor = Cursors.Cross;
        }

        private void CanvasPictureBoxMouseDown(object sender, MouseEventArgs e)
        {
            if (isDrawing)
            {
                this.selectionPoint = e.Location;
            }
        }

        private void CanvasPictureBoxMouseUp(object sender, MouseEventArgs e)
        {
            if (isDrawing)
            {
                isDrawing = false;

                this.shapeIndex = 0;

                this.canvasPictureBox.Image = null;

                this.selectionPoint = new Point();
            }

            if (!this.drawRectangle.IsEmpty)
            {
                this.drawRectangleList.Add(this.drawRectangle);

                this.drawRectanglePenList.Add(this.drawPen);

                this.drawRectangle = new Rectangle();
            }

            if (!this.drawLinePointArray[0].IsEmpty)
            {
                this.drawLinePointArrayList.Add(this.drawLinePointArray);

                this.drawLinePointArrayPenList.Add(this.drawPen);

                this.drawLinePointArray = new Point[2];
            }

            this.doneDrawing = true;

            this.canvasPictureBox.Cursor = DefaultCursor;
        }

        private void CanvasPictureBoxMouseMove(object sender, MouseEventArgs e)
        {
            if (isDrawing && !selectionPoint.IsEmpty)
            {
                Point locationPoint = e.Location;

                switch (this.shapeIndex)
                {
                    case 1:
                        int x = Math.Min(selectionPoint.X, locationPoint.X);

                        int y = Math.Min(selectionPoint.Y, locationPoint.Y);

                        int width = Math.Abs(locationPoint.X - selectionPoint.X);

                        int height = Math.Abs(locationPoint.Y - selectionPoint.Y);

                        drawRectangle = new Rectangle(x, y, width, height);

                        break;

                    case 2:

                        this.drawLinePointArray[0] = this.selectionPoint;

                        this.drawLinePointArray[1] = locationPoint;

                        break;
                }

                this.canvasPictureBox.Invalidate();
            }
        }

        private void CanvasPictureBoxPaint(object sender, PaintEventArgs e)
        {
            if (this.isDrawing)
            {
                switch (this.shapeIndex)
                {
                    case 1:
                        e.Graphics.DrawRectangle(this.drawPen, this.drawRectangle);

                        break;

                    case 2:
                        e.Graphics.DrawLine(this.drawPen, this.drawLinePointArray[0], this.drawLinePointArray[1]);

                        break;
                }
            }

            for (int i = 0; i < this.drawRectangleList.Count; i++)
            {
                e.Graphics.DrawRectangle(this.drawRectanglePenList[i], this.drawRectangleList[i]);
            }

            for (int i = 0; i < this.drawLinePointArrayList.Count; i++)
            {
                e.Graphics.DrawLine(this.drawLinePointArrayPenList[i], this.drawLinePointArrayList[i][0], this.drawLinePointArrayList[i][1]);
            }

            e.Graphics.Save();
        }

        private void MainFormFormClosing(object sender, FormClosingEventArgs e)
        {
            UnregisterHotKey(this.Handle, 1);

            UnregisterHotKey(this.Handle, 2);

            UnregisterHotKey(this.Handle, 3);

            UnregisterHotKey(this.Handle, 4);
        }
    }
}