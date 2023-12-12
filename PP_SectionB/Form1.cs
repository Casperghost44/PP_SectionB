using System.Drawing;

namespace PP_SectionB
{

    public partial class Form1 : Form
    {
        private static readonly object lockObject = new object();
        private static readonly SolidBrush brush = new SolidBrush(Color.Black);
        private static bool paintedAll = false;
        List<Coordinate> coordinates = new List<Coordinate>();
        public Form1()
        {
            InitializeComponent();

        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            Graphics graphics = e.Graphics;
            Pen myPen = new Pen(Color.Black, 1);

            //graphics.DrawEllipse(myPen, 20, 200, 10, 10);
            
            int x = 20;
            int y = 150;
            int count_x = 0;
            int count_y = 1;
            while (coordinates.Count < 1500)
            {
                if (coordinates.Count % 60 == 0 && coordinates.Count != 0)
                {
                    count_y += 1;
                    count_x = 1;
                }
                else
                {
                    count_x += 1;
                }

                coordinates.Add(new Coordinate()
                {
                    X = x * count_x,
                    Y = (20 * count_y) + y
                });
            }

            foreach (var item in coordinates)
            {
                graphics.DrawEllipse(myPen, item.X, item.Y, 10, 10);
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {

            int numWorker = (int)WorkerN.Value;
            Dictionary<int, HashSet<Coordinate>> painted = new Dictionary<int, HashSet<Coordinate>>();
            DateTime startTime = DateTime.Now;
            InitializeThreads(numWorker, painted);
            DateTime endTime = DateTime.Now;
            TimeSpan elapsed = endTime - startTime;

            if (paintedAll)
            {
                MessageBox.Show($"All circles are painted by {elapsed.TotalSeconds} s by {numWorker} workers");
            }

           
        }
        private void InitializeThreads(int numThreads, Dictionary<int, HashSet<Coordinate>> PaintedCircles)
        {
            Thread[] workers = new Thread[numThreads];

            for (int i = 0; i < numThreads; i++)
            {
                PaintedCircles[i] = new HashSet<Coordinate>(); 
                int workerId = i; // Capture the loop variable
                workers[i] = new Thread(() => PaintCircles(workerId, PaintedCircles));
                workers[i].Start();
            }

            for (int i = 0; i < numThreads; i++)
            {
                workers[i].Join();
                
            }
            paintedAll = true;
        }
        private void PaintCircles(int workerId, Dictionary<int, HashSet<Coordinate>> PaintedCircles)
        {
            Graphics graphics = CreateGraphics();
            List<Coordinate> copyOfCoordinates;
            lock (lockObject)
            {
                copyOfCoordinates = new List<Coordinate>(coordinates);
            }

            foreach (var item in copyOfCoordinates)
            {
                bool painted;
                lock (PaintedCircles)
                {
                    painted = PaintedCircles.Any(pair => pair.Value.Contains(item));
                }
                if (!painted)
                {
                    lock (lockObject)
                    {
                        graphics.FillEllipse(brush, item.X, item.Y, 10, 10);
                        lock (PaintedCircles)
                        {
                            PaintedCircles[workerId].Add(item);
                        }
                    }

                    
                    Thread.Sleep(20);
                }
            }
        }

    }
    public class Coordinate
    {
        public int X { get; set; }
        public int Y { get; set; }
    }
}