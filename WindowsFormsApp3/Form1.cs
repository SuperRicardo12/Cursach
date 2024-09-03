using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace WindowsFormsApp3
{
    public partial class Form1 : Form
    {
        private Building building;
        private Lift lift;

        public Form1()
        {
            InitializeComponent();
            DoubleBuffered = true;
            building = new Building(ClientSize.Width, ClientSize.Height);
            lift = new Lift(building, Invalidate);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            building.Draw(e.Graphics);
            lift.Draw(e.Graphics);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            lift.MoveToFloor(1);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            lift.MoveToFloor(2);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            lift.MoveToFloor(3);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            lift.MoveToFloor(4);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            lift.MoveToFloor(5);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            lift.MoveToFloor(6);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            lift.MoveToFloor(7);
        }

        private void button8_Click(object sender, EventArgs e)
        {
            lift.MoveToFloor(8);
        }

        private void button9_Click(object sender, EventArgs e)
        {
            lift.MoveToFloor(9);
        }
    }

    public class Building
    {
        public int Width { get; }
        public int Height { get; }
        public int FloorHeight { get; } = 30;
        public int ShaftWidth { get; } = 40;
        public int LeftSideWidth => (Width - ShaftWidth) / 2;
        public int RightSideWidth => LeftSideWidth;
        public int X { get; }
        public int Y { get; }

        public Building(int clientWidth, int clientHeight)
        {
            Width = 200;
            Height = 9 * FloorHeight;
            X = (clientWidth - Width) / 2;
            Y = 50;
        }

        public void Draw(Graphics g)
        {
            g.Clear(Color.White);
            for (int i = 0; i < 9; i++)
            {
                int floorY = Y + i * FloorHeight;
                g.DrawRectangle(Pens.Black, X, floorY, LeftSideWidth, FloorHeight);
                g.DrawRectangle(Pens.Black, X + LeftSideWidth + ShaftWidth, floorY, RightSideWidth, FloorHeight);
                DrawFloorNumber(g, 9 - i, floorY); 
            }
            DrawShaft(g);
            DrawTitle(g);
        }

        private void DrawShaft(Graphics g)
        {
            int shaftX = X + LeftSideWidth;
            g.DrawLine(Pens.Black, shaftX, Y, shaftX, Y + Height);
            g.DrawLine(Pens.Black, shaftX + ShaftWidth, Y, shaftX + ShaftWidth, Y + Height);
            g.DrawLine(Pens.Black, shaftX, Y, shaftX + ShaftWidth, Y);
            g.DrawLine(Pens.Black, shaftX, Y + Height, shaftX + ShaftWidth, Y + Height);
        }

        private void DrawTitle(Graphics g)
        {
            string title = "Maksos Inc.";
            Font titleFont = new Font("Arial", 16);
            SizeF titleSize = g.MeasureString(title, titleFont);
            int titleX = X + (Width - (int)titleSize.Width) / 2;
            int titleY = Y - (int)titleSize.Height - 10;
            g.DrawString(title, titleFont, Brushes.Black, titleX, titleY);

            Rectangle titleRect = new Rectangle(titleX - 5, titleY - 5, (int)titleSize.Width + 10, (int)titleSize.Height + 10);
            g.DrawRectangle(Pens.Black, titleRect);
            g.DrawLine(Pens.Black, titleRect.Left, titleRect.Bottom, titleRect.Left, Y);
            g.DrawLine(Pens.Black, titleRect.Right, titleRect.Bottom, titleRect.Right, Y);
        }

        private void DrawFloorNumber(Graphics g, int floorNumber, int y)
        {
            string floorText = floorNumber.ToString();
            Font floorFont = new Font("Arial", 10);
            SizeF textSize = g.MeasureString(floorText, floorFont);
            int textX = X - (int)textSize.Width - 10;
            int textY = y + (FloorHeight - (int)textSize.Height) / 2;
            g.DrawString(floorText, floorFont, Brushes.Black, textX, textY);
        }
    }

    public class Lift
    {
        private int liftY;
        private int targetY;
        private Queue<int> floorQueue;
        private bool isPaused;
        private Timer liftTimer;
        private Timer pauseTimer;
        private Building building;
        private Action onLiftMoved;

        public Lift(Building building, Action onLiftMoved)
        {
            this.building = building;
            this.onLiftMoved = onLiftMoved;
            liftY = CalculateLiftY(1);
            floorQueue = new Queue<int>();
            isPaused = false;

            liftTimer = new Timer { Interval = 20 };
            liftTimer.Tick += LiftTimer_Tick;

            pauseTimer = new Timer { Interval = 2000 };
            pauseTimer.Tick += PauseTimer_Tick;
        }

        //мусорный комментарий
        public void Draw(Graphics g)
        {
            int liftWidth = building.ShaftWidth - 20;
            int liftHeight = building.FloorHeight - 4;
            int liftX = building.X + building.LeftSideWidth + (building.ShaftWidth - liftWidth) / 2;
            g.FillRectangle(Brushes.Gray, liftX, liftY, liftWidth, liftHeight);

            int ropeX = liftX + liftWidth / 2;
            g.DrawLine(Pens.Black, ropeX, building.Y, ropeX, liftY);
        }

        public void MoveToFloor(int floor)
        {
            EnqueueFloor(floor);
        }

        private void EnqueueFloor(int floor)
        {
            if (!floorQueue.Contains(floor) && (floor != GetCurrentFloor()))
            {
                floorQueue.Enqueue(floor);
                if (!liftTimer.Enabled && !isPaused)
                {
                    ProcessNextFloor();
                }
            }
        }

        private void LiftTimer_Tick(object sender, EventArgs e)
        {
            if (liftY > targetY) liftY -= 2;
            else if (liftY < targetY) liftY += 2;

            if (Math.Abs(liftY - targetY) <= 2)
            {
                liftY = targetY;
                liftTimer.Stop();
                PauseAndCheckQueue();
            }

            onLiftMoved.Invoke();
        }

        private void PauseTimer_Tick(object sender, EventArgs e)
        {
            pauseTimer.Stop();
            isPaused = false;
            ProcessNextFloor();
        }

        private void ProcessNextFloor()
        {
            if (floorQueue.Count > 0)
            {
                int nextFloor = floorQueue.Dequeue();
                targetY = CalculateLiftY(nextFloor);
                liftTimer.Start();
            }
        }

        private void PauseAndCheckQueue()
        {
            isPaused = true;
            pauseTimer.Start();
        }

        private int CalculateLiftY(int floor)
        {
            return building.Y + building.Height - floor * building.FloorHeight + 2;
        }

        private int GetCurrentFloor()
        {
            return 9 - ((liftY - building.Y) / building.FloorHeight);
        }
    }

}
















