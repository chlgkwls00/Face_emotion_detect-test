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
//using System.Speech.Synthesis;
using System.Media;
using System.Runtime.InteropServices;
using System.Net.Http.Headers;
using System.Net.Http;
using Newtonsoft.Json;


namespace Reconocimiento_facial
{


    public partial class Reconocimiento : Form
    {
        #region Basic Setting
        [DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private extern static void ReleaseCapture();

        [DllImport("user32.DLL", EntryPoint = "SendMessage")]
        private extern static void SendMessage(System.IntPtr hWnd, int wMsg, int wParam, int lParam);

        Rectangle sizeGripRectangle;
        bool inSizeDrag = false;
        const int GRIP_SIZE = 15;

        int w = 0;
        int h = 0;
        #endregion
        #region variable
        public int heigth, width;
        public string[] Labels;
        DBCon dbc = new DBCon();
        int con = 0;
        SoundPlayer media = new SoundPlayer();

        Image<Bgr, Byte> currentFrame;
        Capture grabber;
        HaarCascade face;
        HaarCascade eye;
        MCvFont font = new MCvFont(FONT.CV_FONT_HERSHEY_TRIPLEX, 0.8d, 0.8d);
        Image<Gray, byte> result, TrainedFace = null;
        Image<Gray, byte> gray = null;
        List<Image<Gray, byte>> trainingImages = new List<Image<Gray, byte>>();
        List<string> labels = new List<string>();
        List<string> NamePersons = new List<string>();
        int ContTrain, NumLabels, t;
        string name, Labelsinfo, names = null;
        public static string emotiondata1;
        #endregion

        public void Getscore(string score)
        {
            emotiondata1 = score;
          //  Console.WriteLine("getscore" + emotiondata1 + score);
        }
        public Reconocimiento()
        {
            //Console.WriteLine("getscore1" + emotiondata1);
            InitializeComponent();
            heigth = this.Height; width = this.Width;

            face = new HaarCascade("haarcascade_frontalface_default.xml");
            try
            {
                dbc.ObtenerBytesImagen();

                string[] Labels = dbc.Name;
                NumLabels = dbc.TotalUser;
                ContTrain = NumLabels;
                string LoadFaces;

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
                MessageBox.Show(e + "등록된 얼굴인식 데이터가 없습니다.", "인식 실패", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        
        
       
       
        public int picnum1 = 1;
        public void FrameGrabber(object sender, EventArgs e)
        {
            lblNumeroDetect.Text = "0";
            NamePersons.Add("");
            try
            {
                currentFrame = Grabber.QueryFrame().Resize(565, 259, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);

                gray = currentFrame.Convert<Gray, Byte>();
                Console.WriteLine("getscore" + emotiondata1);
label7.Text = emotiondata1;
                MCvAvgComp[][] facesDetected = gray.DetectHaarCascade(face, 1.2, 10, Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DO_CANNY_PRUNING, new Size(20, 20));


                foreach (MCvAvgComp f in facesDetected[0])
                {
                    //Console.WriteLine("getscore2" + emotiondata1);
                    t = t + 1;
                    result = currentFrame.Copy(f.rect).Convert<Gray, byte>().Resize(100, 100, INTER.CV_INTER_CUBIC);

                    currentFrame.Draw(f.rect, new Bgr(Color.Blue), 1);
                    
                    if (trainingImages.ToArray().Length != 0)
                    {
                      //  Console.WriteLine("getscore3" + emotiondata1);

                        MCvTermCriteria termCrit = new MCvTermCriteria(ContTrain, 0.001);


                        EigenObjectRecognizer recognizer = new EigenObjectRecognizer(trainingImages.ToArray(), labels.ToArray(), ref termCrit);
                        var fa = new Image<Gray, byte>[trainingImages.Count];

                        name = recognizer.Recognize(result);
                       
                            Console.WriteLine(emotiondata1);
                                
                        if (emotiondata1 == null)
                        { }
                        else{
                            
                            label7.Text = emotiondata1;
                            currentFrame.Draw(emotiondata1, ref font, new Point(f.rect.X - 2, f.rect.Y - 2), new Bgr(Color.Red));
                        }
                        Reconocimiento_facial.emotion emo = new Reconocimiento_facial.emotion();

                        Console.WriteLine(f.rect.X + "," + f.rect.Y);
                    }



                    NamePersons[t - 1] = name;
                    NamePersons.Add("");

                    lblNumeroDetect.Text = facesDetected[0].Length.ToString();
                    lblNadie.Text = name;

                    //    Console.WriteLine(date + ": Face Detected!");
                    /*
                    if (name != null)
                    {

                        Console.WriteLine("Known Face :" + name);




                        if (Directory.Exists(Application.StartupPath + "/face_analysis"))
                        {
                            Console.WriteLine("디렉토리 발견 저장합니다.");
                        }


                        else
                        {
                            Console.WriteLine("파일 저장 디렉토리가 존재 하지않습니다. 프로그램과 같은 위치에 있는지 확인해 주세요");
                            Console.WriteLine("이 위치에 디렉토리를 만들까요? (Y/n)" + Application.StartupPath);
                            string a = Console.ReadLine();
                            if (a == "Y")
                            {
                                Console.WriteLine("디렉토리 생성 완료");
                                Directory.CreateDirectory(Application.StartupPath + "/face_analysis");
                            }
                            else
                            {
                                Console.WriteLine("입력 오류 입니다. 프로그램 오류가 발생될 수 있습니다 (GDI+)");
                            }
                         

                           
                        }
                        String picname;
                        picname = Convert.ToString(picnum1 + ".png");


                        Bitmap bmp = new Bitmap(imageBoxFrameGrabber.ClientSize.Width, imageBoxFrameGrabber.ClientSize.Height);
                        imageBoxFrameGrabber.DrawToBitmap(bmp, imageBoxFrameGrabber.ClientRectangle);
                        bmp.Save(Application.StartupPath + "/face_analysis/" + picname);


                        Console.WriteLine("Saving Picture to Designated Location : " + Application.StartupPath + "/face_analysis/" + picname);
                        picnum1 = picnum1 + 1;
                        if (File.Exists(Application.StartupPath + "/face_analysis/" + picname))
                            Console.WriteLine("File Saved");
                        else
                        {
                            Console.WriteLine("File saving Error");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Unknown Face, Register Face by clicking register btn");
                    }
                    */


                }
                t = 0;


                for (int nnn = 0; nnn < facesDetected[0].Length; nnn++)
                {
                    names = names + NamePersons[nnn] + ", ";
                }


                imageBoxFrameGrabber.Image = currentFrame;
                name = "";

                NamePersons.Clear();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                MessageBox.Show(ex.Message);
            }
        }
        
        public DateTime date { get; }
        public Capture Grabber { get => grabber; set => grabber = value; }
        public HaarCascade Eye { get => eye; set => eye = value; }

        public void Reconocimiento_Load(object sender, EventArgs e)
        {
            menuStrip1.Visible = false;
            faceemotion.Startup startup = new faceemotion.Startup();
            startup.ShowDialog();
            if (startup.IsDisposed)
            {
                MessageBox.Show("비정상적인 경로입니다!");
                Application.Exit();
            }
            else
            {
                
            }
            #region Form Settings

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

            Console.WriteLine("====디버깅 모드====");
            imageBoxFrameGrabber.ImageLocation = "img/1.png";
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " ==" + "Form Activated");
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

        private void btn_close_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btn_minimize_Click_1(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void btn_close_Click_1(object sender, EventArgs e)
        {

            MessageBox.Show("Program Terminating", "Information");
            Reconocer();
            Desconectar();
            Application.Exit();
        }

       

        
        

        
        public void button1_Click(object sender, EventArgs e)
        {
            emotion emotion = new emotion();
            emotion.Show();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            switch (checkBox1.Checked)
            {
                case true:
                    timer1.Interval = (int)numericUpDown1.Value;
                    timer1.Enabled = true;


                    break;
                case false:
                    timer1.Enabled = false;
                    Console.WriteLine("AutoSave Disabled");
                    break;


            }


        }
        

        private void timer1_Tick(object sender, EventArgs e)
        {
            #region Diretory&save
            if (name != null)
            {

                Console.WriteLine("Known Face :" + name);




                if (Directory.Exists(Application.StartupPath + "/face_analysis"))
                {
                    Console.WriteLine("디렉토리 발견 저장합니다.");
                }


                else
                {
                    Console.WriteLine("파일 저장 디렉토리가 존재 하지않습니다. 프로그램과 같은 위치에 있는지 확인해 주세요");
                    Console.WriteLine("이 위치에 디렉토리를 만들까요? (Y/n)" + Application.StartupPath);
                    string a = Console.ReadLine();
                    if (a == "Y")
                    {
                        Console.WriteLine("디렉토리 생성 완료");
                        Directory.CreateDirectory(Application.StartupPath + "/face_analysis");
                    }
                    else
                    {
                        Console.WriteLine("입력 오류 입니다. 프로그램 오류가 발생될 수 있습니다 (GDI+)");
                    }



                }
                String picname;
                picname = Convert.ToString(picnum1 + ".png");


                Bitmap bmp = new Bitmap(imageBoxFrameGrabber.ClientSize.Width, imageBoxFrameGrabber.ClientSize.Height);
                imageBoxFrameGrabber.DrawToBitmap(bmp, imageBoxFrameGrabber.ClientRectangle);
                bmp.Save(Application.StartupPath + "/face_analysis/" + picname);


                Console.WriteLine("Saving Picture to Designated Location : " + Application.StartupPath + "/face_analysis/" + picname);
                picnum1 = picnum1 + 1;
                if (File.Exists(Application.StartupPath + "/face_analysis/" + picname))
                    Console.WriteLine("File Saved");
                else
                {
                    Console.WriteLine("File saving Error");
                }
            }
            else
            {
                Console.WriteLine("Unknown Face, Register Face by clicking register btn");
            }
            #endregion
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            label4.Text = Convert.ToString((double)numericUpDown1.Value * 0.001) + "Seconds";
            timer1.Interval = (int)numericUpDown1.Value;
        }

        

        private void button2_Click(object sender, EventArgs e)
        {
            #region Diretory&save


            if (Directory.Exists(Application.StartupPath + "/Emotion"))
            {
                Console.WriteLine("디렉토리 발견 저장합니다.");
            }


            else
            {
                Console.WriteLine("파일 저장 디렉토리가 존재 하지않습니다. 프로그램과 같은 위치에 있는지 확인해 주세요");
                Console.WriteLine("이 위치에 디렉토리를 만들까요? (Y/n)" + Application.StartupPath);
                string a = Console.ReadLine();
                if (a == "Y")
                {
                    Console.WriteLine("디렉토리 생성 완료");
                    Directory.CreateDirectory(Application.StartupPath + "/Emotion");
                }
                else
                {
                    Console.WriteLine("입력 오류 입니다. 프로그램 오류가 발생될 수 있습니다 (GDI+)");
                }



            }
            string picname;
            picname = Convert.ToString("Emotion" + ".png");


            Bitmap bmp = new Bitmap(imageBoxFrameGrabber.ClientSize.Width, imageBoxFrameGrabber.ClientSize.Height);
            imageBoxFrameGrabber.DrawToBitmap(bmp, imageBoxFrameGrabber.ClientRectangle);
              bmp.Save(Application.StartupPath + "/Emotion/" + picname);


            Console.WriteLine("Saving Picture to Designated Location : " + Application.StartupPath + "/Emotion/" + picname);

            if (File.Exists(Application.StartupPath + "/Emotion/" + picname))
            {
                Console.WriteLine("File Saved");
                MakeRequest(Application.StartupPath + "/Emotion/" + picname);
                
            }
            else
            {
                Console.WriteLine("File saving Error");
            }
            #endregion
        }
    
           
            
        
        static byte[] GetImageAsByteArray(string imageFilePath)
        {
            FileStream fileStream = new FileStream(imageFilePath, FileMode.Open, FileAccess.Read);
            BinaryReader binaryReader = new BinaryReader(fileStream);
            return binaryReader.ReadBytes((int)fileStream.Length);
        }

        async void MakeRequest(string imageFilePath)
        {
            try
            {
                var client = new HttpClient();

                // Request headers
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "db40ac8e03ae4ef7bec43227ec452048");

                string uri = "https://westus.api.cognitive.microsoft.com/emotion/v1.0/recognize?";
                HttpResponseMessage response;
                string responseContent;


                byte[] byteData = GetImageAsByteArray(imageFilePath);

                using (var content = new ByteArrayContent(byteData))
                {

                    content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    response = await client.PostAsync(uri, content);
                    responseContent = response.Content.ReadAsStringAsync().Result;
                    Console.WriteLine(content);

                    Reconocimiento_facial.Reconocimiento rs = new Reconocimiento_facial.Reconocimiento();

                    rs.emotion_json = responseContent;
                    Console.WriteLine("이모션제이슨" + rs.emotion_json);
                    Console.WriteLine(JsonHelper.FormatJson(responseContent));


                    Console.WriteLine("Deemotion _ getmyjson activated");

                    Console.WriteLine("De Emotion 동작");
                    //   [TestMethod]
                    DeserializeEmotions();
                    void DeserializeEmotions()
                    {

                        Console.WriteLine("DeserializeEmotion 함수 호출됨");
                        string score1;

                        var emotions = JsonConvert.DeserializeObject<Emotion[]>(responseContent);
                        var scores = emotions[0].scores;
                        var highestScore = scores.Values.OrderByDescending(score => score).First();
                        //probably a more elegant way to do this.
                        var highestEmotion = scores.Keys.First(key => scores[key] == highestScore);

                        score1 = highestEmotion;
                        switch (score1)
                        {
                            case "neutral":
                                score1 = "중립";
                                break;
                            case "happiness":
                                score1 = "기쁜";
                                break;
                            case "sadness":
                                score1 = "슬픔";
                                break;
                            case "surprise":
                                score1 = "놀람";
                                break;
                            case "disgust":
                                score1 = "불쾌한";
                                break;
                            case "fear":
                                score1 = "두랴운";
                                break;
                            case "contempt":
                                score1 = "경멸";
                                break;

                        }
                        
                        rs.Getscore(score1);
                        Reconocimiento_facial.Reconocimiento rec = new Reconocimiento();
                        rs.set_result(score1);
                        Console.WriteLine("감정데이터 종합본 정상 등록" + score1);


                    }





                }
            }
            catch(Exception ex2)
            {
                Console.WriteLine(ex2);
            }
}

        private void set_result(string score1)
        {
            throw new NotImplementedException();
        }

        public string myjson;
            public void getmyjson(string myjsonorg)
            {
                myjsonorg = myjson;
                Console.WriteLine(myjson + ":::::" + myjsonorg);
            }

           
    
        
            private void StateWin()
             {

            if (this.btn_maximize.Text == "1")
            {
                this.btn_maximize.Text = "2";
                this.Location = new Point(0, 0);
                this.Size = new Size(Screen.PrimaryScreen.WorkingArea.Width, Screen.PrimaryScreen.WorkingArea.Height);
            }
            else if (this.btn_maximize.Text == "2")
            {
                this.btn_maximize.Text = "1";
                this.Size = new Size(width, heigth);
                this.StartPosition = FormStartPosition.CenterScreen;
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            switch (checkBox2.Checked)
            {
                case true:
                    timer2.Interval = (int)numericUpDown1.Value;
                    timer2.Enabled = true;


                    break;
                case false:
                    timer2.Enabled = false;
                    Console.WriteLine("LM API Disabled");
                    break;

            }
             }

        private void timer2_Tick(object sender, EventArgs e)
        {
            button2.PerformClick();
            switch (label7.Text)
            {
                case "놀람":
                    pictureBox1.Image = Bitmap.FromFile("/gif/surprise.gif");
                       
                    break;
                case "기쁜":
                    pictureBox1.Image = Bitmap.FromFile("/gif/happy.gif");
                    break;
                case "슬픔":
                    pictureBox1.Image = Bitmap.FromFile("/gif/sad.gif");
                    break;


            }
        }

        private void label7_Click(object sender, EventArgs e)
        {

        }
        private void label7_TextChanged(object sender, EventArgs e)
        {
            switch(label7.Text)
            {
                case "놀람":
                    pictureBox1.ImageLocation =  ("/gif/surprise.gif");
                    break;
                case "기쁜":
                    pictureBox1.ImageLocation = ("/gif/happy.gif");
                    break;
                case "슬픔":
                    pictureBox1.ImageLocation = ("/gif/sad.gif");
                    break;


            }
            
        }

        private void menuStrip1_MouseDown(object sender, MouseEventArgs e)
        {
     
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
            w = this.Width;
            h = this.Height;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            switch (button4.Text)
            {
                case "Camera Feed Connect":
                    Reconocer();
                    button4.Text = "Disconnect";
                    break;
                case "Disconnect":
                    Desconectar();
                    break;
            }
        }

        private void Reconocer()
        {
            throw new NotImplementedException();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Reconocer();
            Desconectar();
            Registrar r = new Registrar();
            r.ShowDialog();
        }

        private void btnEncender_Click(object sender, EventArgs e)
        {
            Reconocer();
        }
      
        private void Desconectar()
        {
            Application.Idle -= new EventHandler(FrameGrabber);
          
                Grabber.Dispose();

            imageBoxFrameGrabber.ImageLocation = "img/1.png";
            lblNadie.Text = string.Empty;
            lblNumeroDetect.Text = string.Empty;
            button4.Text = "Camera Feed Connect";
            
        }
public string emotion_json="";
    } 
     public class FaceRectangle
        {
            public int left { get; set; }
            public int top { get; set; }
            public int width { get; set; }
            public int height { get; set; }
        }

        public class Emotion
        {
            public FaceRectangle faceRectangle { get; set; }
            public IDictionary<string, double> scores { get; set; }
        }
        

        //  [TestClass]
       

        
    }
    class JsonHelper
    {
        private const string INDENT_STRING = "    ";
        public static string FormatJson(string str)
        {
            var indent = 0;
            var quoted = false;
            var sb = new StringBuilder();
            for (var i = 0; i < str.Length; i++)
            {
                var ch = str[i];
                switch (ch)
                {
                    case '{':
                    case '[':
                        sb.Append(ch);
                        if (!quoted)
                        {
                            sb.AppendLine();
                            Enumerable.Range(0, ++indent).ForEach(item => sb.Append(INDENT_STRING));
                        }
                        break;
                    case '}':
                    case ']':
                        if (!quoted)
                        {
                            sb.AppendLine();
                            Enumerable.Range(0, --indent).ForEach(item => sb.Append(INDENT_STRING));
                        }
                        sb.Append(ch);
                        break;
                    case '"':
                        sb.Append(ch);
                        bool escaped = false;
                        var index = i;
                        while (index > 0 && str[--index] == '\\')
                            escaped = !escaped;
                        if (!escaped)
                            quoted = !quoted;
                        break;
                    case ',':
                        sb.Append(ch);
                        if (!quoted)
                        {
                            sb.AppendLine();
                            Enumerable.Range(0, indent).ForEach(item => sb.Append(INDENT_STRING));
                        }
                        break;
                    case ':':
                        sb.Append(ch);
                        if (!quoted)
                            sb.Append(" ");
                        break;
                    default:
                        sb.Append(ch);
                        break;
                }
            }
            return sb.ToString();
        }
    }

    static class Extensions
    {
        public static void ForEach<T>(this IEnumerable<T> ie, Action<T> action)
        {
            foreach (var i in ie)
            {
                action(i);
            }
        }
    }


