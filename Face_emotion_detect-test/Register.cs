using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using System.Data.OleDb;
using System.Runtime.InteropServices;

namespace Reconocimiento_facial
{
    public partial class Registrar : Form
    {
        #region Dlls para poder hacer el movimiento del Form
        [DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private extern static void ReleaseCapture();

        [DllImport("user32.DLL", EntryPoint = "SendMessage")]
        private extern static void SendMessage(System.IntPtr hWnd, int wMsg, int wParam, int lParam);
        
        int w = 0;
        int h = 0;

        Rectangle sizeGripRectangle;
        bool inSizeDrag = false;
        const int GRIP_SIZE = 15;
        #endregion

        public int heigth, width;

        public string[] Labels;
        DBCon dbc = new DBCon();
        int con = 0,ini=0,fin;
       //모든 변수, 벡터와 haarcascades 선언
        Image<Bgr, Byte> currentFrame;
        Capture grabber;         
        HaarCascade face;
        MCvFont font = new MCvFont(FONT.CV_FONT_HERSHEY_TRIPLEX, 0.5d, 0.5d);
        Image<Gray, byte> result, TrainedFace = null;
        Image<Gray, byte> gray = null;
        List<Image<Gray, byte>> trainingImages = new List<Image<Gray, byte>>();
        List<string> labels = new List<string>();
        List<string> labels1 = new List<string>();
        List<string> NamePersons = new List<string>();
        int ContTrain, NumLabels, t;
        string name;
        
        public Registrar()
        {
            InitializeComponent();
            heigth = this.Height; width = this.Width;
            //haarcascades BY 측면 GARGAMOS 감지
            face = new HaarCascade("haarcascade_frontalface_default.xml");
            try
            {                
                dbc.ObtenerBytesImagen();//부하는 각 이미지에 대한 얼굴과 라벨을               
                Labels = dbc.Name; 
                NumLabels = dbc.TotalUser;
                ContTrain = NumLabels;

                
                for (int tf = 0; tf < NumLabels; tf++)
                {
                    con = tf;
                    Bitmap bmp = new Bitmap(dbc.ConvertByteToImg(con));

                    trainingImages.Add(new Image<Gray, byte>(bmp));
                    labels.Add(Labels[tf]);
                    
                }               
            }
            catch (Exception e)
            {
                MessageBox.Show(e + " 데이터베이스에는 얼굴이 없습니다, 적어도 하나의 얼굴을 추가하시기 바랍니다", "데이터베이스 자료 로드 오류", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
       
        private void btn_detectar_Click(object sender, EventArgs e)
        {
            try
            {
                        
                grabber = new Capture();
                grabber.QueryFrame();

               
                Application.Idle += new EventHandler(FrameGrabber);
                this.button1.Enabled = true;
                btn_detectar.Enabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }           
        }

        void FrameGrabber(object sender, EventArgs e)
        {
            lblNumeroDetect.Text = "0";
            NamePersons.Add("");
            try
            {

               
                try
                {
                    currentFrame = grabber.QueryFrame().Resize(320, 240, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
                }
                catch (Exception)
                {                    
                    imageBoxFrameGrabber.Image = null;
                }

           
                gray = currentFrame.Convert<Gray, Byte>();

               
                MCvAvgComp[][] facesDetected = gray.DetectHaarCascade(face, 1.2, 10, Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DO_CANNY_PRUNING, new Size(20, 20));

               
                foreach (MCvAvgComp f in facesDetected[0])
                {
                    t = t + 1;
                    result = currentFrame.Copy(f.rect).Convert<Gray, byte>().Resize(640, 480, INTER.CV_INTER_CUBIC);
             
                    currentFrame.Draw(f.rect, new Bgr(Color.LightGreen), 1);

                    NamePersons[t - 1] = name;
                    NamePersons.Add("");
                   
                    lblNumeroDetect.Text = facesDetected[0].Length.ToString();
                

                }
                t = 0;
                
              
                imageBoxFrameGrabber.Image = currentFrame;
                name = "";
                      
                NamePersons.Clear();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
    }

        private void btn_primero_Click(object sender, EventArgs e)
        {
            pictureBox1.Image = dbc.ConvertByteToImg(0);
            label4.Text = dbc.Name[0];
        }

        private void btn_siguiente_Click(object sender, EventArgs e)
        {                           
            if (ini < NumLabels-1)
            {
                ini++;
                pictureBox1.Image = dbc.ConvertByteToImg(ini);
                label4.Text = dbc.Name[ini];
            }
        }

        private void btn_anterior_Click(object sender, EventArgs e)
        {
            if (ini > 0)
            {
                ini--;
                pictureBox1.Image = dbc.ConvertByteToImg(ini);
                label4.Text = dbc.Name[ini];
            }
        }

        private void btn_ultimo_Click(object sender, EventArgs e)
        {
           ini = NumLabels - 1;
           pictureBox1.Image = dbc.ConvertByteToImg(ini);
           label4.Text = dbc.Name[ini];
        }

        private void btn_loadImgsBD_Click(object sender, EventArgs e)
        {
            groupBox2.Enabled = true;
            pictureBox1.Image = dbc.ConvertByteToImg(0);
            label4.Text = dbc.Name[0];
        }

        private void btn_minimize_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void btn_close_Click(object sender, EventArgs e)
        {
            if (!btn_detectar.Enabled)
            {
                Application.Idle -= new EventHandler(FrameGrabber);
                grabber.Dispose();
                this.Close();
            }
            this.Close();
        }

        private void menuStrip1_MouseDown(object sender, MouseEventArgs e)
        {
           
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
            w = this.Width;
            h = this.Height;
        }

        private void btn_agregar_Click(object sender, EventArgs e)
        {
            try
            {
              
                ContTrain = ContTrain + 1;

               
                gray = grabber.QueryGrayFrame().Resize(320, 240, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);

              
                MCvAvgComp[][] facesDetected = gray.DetectHaarCascade(face, 1.2, 10,Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DO_CANNY_PRUNING,new Size(20, 20));

                
                foreach (MCvAvgComp f in facesDetected[0])
                {
                    TrainedFace = currentFrame.Copy(f.rect).Convert<Gray, byte>();
                    break;
                }

                
                TrainedFace = result.Resize(100, 100, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
                trainingImages.Add(TrainedFace);
                labels.Add(txt_nombre.Text);

              
                imageBox2.Image = TrainedFace;
                    dbc.ConvertImgToBinary(txt_nombre.Text,txt_codigo.Text, imageBox2.Image.Bitmap);
                //}                
                MessageBox.Show("성공적으로 추가", "Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);

            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
              
        private void button3_Click(object sender, EventArgs e)
        {
            imageBox2.Image = null;
            this.txt_codigo.Clear();
            this.txt_nombre.Clear();
        }

       

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                Application.Idle -= new EventHandler(FrameGrabber);
                grabber.Dispose();
                imageBoxFrameGrabber.ImageLocation = "img/1.png";
                btn_detectar.Enabled = true;
                button1.Enabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Resgistrar_Load(object sender, EventArgs e)
        {
            #region[Metodo deredimension de formulario sin borde]

            SetGripRectangle();
            this.Paint += (o, ea) => { ControlPaint.DrawSizeGrip(ea.Graphics, this.BackColor, sizeGripRectangle); };

            this.MouseUp += delegate { inSizeDrag = false; };
            this.MouseDown += (o, ea) =>
            {
                if (IsInSizeGrip(ea.Location))
                    inSizeDrag = true;
            };
            this.MouseMove += (o, ea) =>
            {
                if (inSizeDrag)
                {
                    this.Width = ea.Location.X + GRIP_SIZE / 2;
                    this.Height = ea.Location.Y + GRIP_SIZE / 2;
                    SetGripRectangle();
                    this.Invalidate();
                }
            };
            #endregion
            imageBoxFrameGrabber.ImageLocation = "img/1.png";
        }

        private void btn_maximize_Click(object sender, EventArgs e)
        {
            StateWin();
        }

        private void StateWin()
        {
            
            if (this.btn_maximize.Text == "1")
            {
                this.btn_maximize.Text = "2";
                this.Location = new Point(0, 0);
                this.Size = new Size(Screen.PrimaryScreen.WorkingArea.Width, Screen.PrimaryScreen.WorkingArea.Height);
            }
            else if(this.btn_maximize.Text == "2")
            {
                this.btn_maximize.Text = "1";
                this.Size = new Size(width, heigth);
                this.StartPosition = FormStartPosition.CenterScreen;
            }
        }
       
        private void SetGripRectangle()
        {
            sizeGripRectangle = new Rectangle(
                       this.Width - GRIP_SIZE,
                       this.Height - GRIP_SIZE, GRIP_SIZE, GRIP_SIZE);
        }

        private bool IsInSizeGrip(Point tmp)
        {
            if (tmp.X >= sizeGripRectangle.X
              && tmp.X <= this.Width
              && tmp.Y >= sizeGripRectangle.Y
              && tmp.Y <= this.Height
                )
                return true;
            else
                return false;
        }
    }
}
