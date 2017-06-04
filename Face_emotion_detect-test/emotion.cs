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

using System.Media;
using System.Runtime.InteropServices;
using System.Net.Http.Headers;
using System.Net.Http;
using Newtonsoft.Json;



namespace Reconocimiento_facial
{

    public partial class emotion : Form
    {
        public emotion()
        {
            InitializeComponent();
        }
        public DateTime date { get; }
        private void emotion_Load(object sender, EventArgs e)
        {
            Console.WriteLine(date +  ":: Emotion API 1.0 Initiated");
            pictureBox1.ImageLocation = Application.StartupPath + "/img/1.png";
            button1.Text = "Getpics";


        }

        private void btn_close_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            pictureBox1.ImageLocation = (Application.StartupPath + "/face_analysis/" + Convert.ToString(listBox1.SelectedItem));
        }

        private void getPics()
        {
            Console.WriteLine("getPics is Activated");
            int picsnum=1;
            while(true)
            {
                if (File.Exists(Application.StartupPath + "/face_analysis/" + picsnum + ".png"))
                { listBox1.Items.Add(picsnum + ".png");
                    Console.WriteLine(date + ":: File {0}.png found", picsnum);
                  
                   
                }
                else
                {
                    Console.WriteLine("============================");
                    Console.WriteLine("Total {0} Pics Found",picsnum-2);
                    break;
                }
  picsnum++;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            getPics();
        }

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [DllImportAttribute("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd,
                         int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        private static extern bool ReleaseCapture();
       
        private bool dragging = false;
        private Point dragCursorPoint;
        private Point dragFormPoint;

        private void menuStrip1_MouseDown(object sender, MouseEventArgs e)
        {
            dragging = true;
            dragCursorPoint = Cursor.Position;
            dragFormPoint = this.Location;

            Console.WriteLine("Mouse Down event");
        }

        private void menuStrip1_MouseMove(object sender, MouseEventArgs e)
        {
            if (dragging)
            {
                Point dif = Point.Subtract(Cursor.Position, new Size(dragCursorPoint));
                this.Location = Point.Add(dragFormPoint, new Size(dif));
                Console.WriteLine("Mouse Down event" + Cursor.Position);
            }
        }

        private void menuStrip1_MouseUp(object sender, MouseEventArgs e)
        {
            dragging = false;
        }

       
        static byte[] GetImageAsByteArray(string imageFilePath)
        {
            FileStream fileStream = new FileStream(imageFilePath, FileMode.Open, FileAccess.Read);
            BinaryReader binaryReader = new BinaryReader(fileStream);
            return binaryReader.ReadBytes((int)fileStream.Length);
        }

        async void MakeRequest(string imageFilePath)
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
                Console.WriteLine(JsonHelper.FormatJson(responseContent));
                getjsonRich.getjsonRiches(JsonHelper.FormatJson(responseContent));
                richTextBox1.Text = JsonHelper.FormatJson(responseContent);
            }

            
            Console.WriteLine(responseContent);
            DeserializeEmotions();
          
 Reconocimiento_facial.emotion emo = new Reconocimiento_facial.emotion();
                emo.getjson(responseContent);
            
            void DeserializeEmotions()
            {
                var emotions = JsonConvert.DeserializeObject<Emotion[]>(responseContent);
                var scores = emotions[0].scores;
                var highestScore = scores.Values.OrderByDescending(score => score).First();
               
                var highestEmotion = scores.Keys.First(key => scores[key] == highestScore);
                Console.WriteLine(highestEmotion + " : " + highestScore);
                faceemotion.result rs = new faceemotion.result();

                rs.getresult(highestEmotion + " : " + highestScore);
                rs.ShowDialog();

               
            }

        }
      

       

        private void button2_Click(object sender, EventArgs e)
        {
            MakeRequest(pictureBox1.ImageLocation);
        }

        
        
        class getjsonRich : emotion 
        { public static string responsedata;
            public static void getjsonRiches(string Jsonbyte)
            {
                Console.WriteLine("getJsonrich activated" + JsonHelper.FormatJson(Jsonbyte));
                responsedata = Jsonbyte;
            }
           
            
            public void printresult()
            {
                richTextBox1.Text = responsedata;
            }
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
        public void getjson(string json)
        {
            Console.WriteLine("getjson activated & {0}", json);
            
        }
        //  [TestClass]
        public class DeserializeEmotion
        {
         //   [TestMethod]
            public void DeserializeEmotions()
            {
                var emotions = JsonConvert.DeserializeObject<Emotion[]>(JSON);
                var scores = emotions[0].scores;
                var highestScore = scores.Values.OrderByDescending(score => score).First();
                //probably a more elegant way to do this.
                var highestEmotion = scores.Keys.First(key => scores[key] == highestScore);
                
            }

            private string JSON =
                "[{'faceRectangle': {'left': 68,'top': 97,'width': 64,'height': 97},'scores': {'anger': 0.00300731952,'contempt': 5.14648448E-08,'disgust': 9.180124E-06,'fear': 0.0001912825,'happiness': 0.9875571,'neutral': 0.0009861537,'sadness': 1.889955E-05,'surprise': 0.008229999}}]";

           
        }

        
        
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
}
