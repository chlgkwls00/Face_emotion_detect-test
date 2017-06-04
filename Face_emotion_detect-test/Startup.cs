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
using System.Security.Cryptography;


namespace faceemotion
{
    public partial class Startup : Form
    {
        public Startup()
        {
            
            InitializeComponent();
        }

        private void Startup_Load(object sender, EventArgs e)
        {
            label1.Hide();

            this.BackgroundImage = Properties.Resources.StartPopup;

            Delay(3000);

            label1.Show();
            this.BackgroundImage = Properties.Resources.StartPopupLoad;


            Delay(2000);

            label1.Text = "프로그램 무결성 검사 준비중...";

            try
            {

                //Pass the filepath and filename to the StreamWriter Constructor
               
                StreamReader sr = new StreamReader(Application.StartupPath + "/data_integrity.txt");
                label1.Text = "Read md5 File...";

               string md5_file = sr.ReadLine();
              


                Delay(1000);
                label1.Text = "Verifying the hash...";
                Delay(1000);
                if ((VerifyMd5Hash(md5_file)))
                {
                    label1.Text = "Verifying Complete : PASS";
                    Delay(2000);
                    label1.Text = "File Integrity Verifying";
                    Delay(1000);
                    StreamReader sf = new StreamReader(Application.StartupPath + "/file_integrity.txt");
                    string fileinteg = sf.ReadLine();
                    label1.Text = "Verifying [" + fileinteg + "]";
                    while (sf.ReadLine() != null)
                    {
                        fileinteg = sf.ReadLine();
                        label1.Text = "Verifying [" + fileinteg + "]";

                        if (File.Exists(Application.StartupPath + "/" + fileinteg))
                        {
                            label1.Font = new Font(label1.Font.FontFamily, 21);
                            Console.WriteLine("File Exists ::" + Application.StartupPath + "/" + fileinteg);
                            Delay(500);
                        }
                        else
                        {
                           
                            label1.Text = "File Integrity Verifying Failed";
                            MessageBox.Show("File " + fileinteg + "is not exist");
                            sf.Close();
                            Delay(3000);
                            label1.Text = "검증 실패  프로그램이 종료됩니다.";
                            Delay(2000);
                            Application.Exit();
                            break;

                        }

                    }

                

                }
                else
                {
                    label1.Text = "Verifying Complete : FAILED";
                    Delay(1000);

                   MessageBox.Show(faceemotion.Startup.ExecutingHash.GetExecutingFileHash() + "::" + md5_file);
                     
                    if (md5_file == "force_execute=1")
                    {
                        label1.Text = "Force Exeucte = 1 - InDev";
                        Delay(1000);
                        this.Hide();
                        if (sr.ReadLine() == "get_md5=1")
                        {
                            label1.Text = "In Dev Enabled = 1";
                            Delay(500);
                            sr.Close();
                            StreamWriter md5WR = new StreamWriter(Application.StartupPath + "/data_integrity.txt");
                            md5WR.WriteLine(faceemotion.Startup.ExecutingHash.GetExecutingFileHash());
                            
                            md5WR.Close();
                        }
                    }
                    else
                    {
                        if (sr.ReadLine() == "get_md5=1")
                        {
                            label1.Text = "In Dev Enabled = 1";
                            Delay(500);
                            sr.Close();
                            StreamWriter md5WR = new StreamWriter(Application.StartupPath + "/data_integrity.txt");
                            md5WR.WriteLine(faceemotion.Startup.ExecutingHash.GetExecutingFileHash());
                            md5WR.Close();

                        }
                        label1.Text = "검증 실패  프로그램이 종료됩니다.";
                        Delay(2000);
                        sr.Close();
                        Application.Exit();
                    }
                   
                }

                //Close the file
              

            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
                label1.Text = "Error, Please Check Program Integrity";
                MessageBox.Show(ex.Message, "System Intergrity Verifying Failed");
            }
            finally
            {
                Console.WriteLine("Executing finally block.");
                label1.Text = "Data Integrity Verifying Complete : PASS";
                Delay(1000);
                this.Hide();
            }
            

          
        }
        static bool VerifyMd5Hash(string hash)
        {
            // Hash the input.

           
            // Create a StringComparer an compare the hashes.
            StringComparer comparer = StringComparer.OrdinalIgnoreCase;

            if (faceemotion.Startup.ExecutingHash.GetExecutingFileHash() == hash)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private static DateTime Delay(int MS)
        {
            DateTime ThisMoment = DateTime.Now;
            TimeSpan duration = new TimeSpan(0, 0, 0, 0, MS);
            DateTime Afterwards = ThisMoment.Add(duration);
            while (Afterwards >= ThisMoment)
            {
                System.Windows.Forms.Application.DoEvents();
                ThisMoment = DateTime.Now;
            }
            return DateTime.Now;
        }
        public string checkMD5(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    return Encoding.Default.GetString(md5.ComputeHash(stream));
                }
            }
        }

        internal static class ExecutingHash
        {
            public static string GetExecutingFileHash()
            {
                return MD5(GetSelfBytes());
            }

            private static string MD5(byte[] input)
            {
                return MD5(ASCIIEncoding.ASCII.GetString(input));
            }

            private static string MD5(string input)
            {
                MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();

                byte[] originalBytes = ASCIIEncoding.Default.GetBytes(input);
                byte[] encodedBytes = md5.ComputeHash(originalBytes);

                return BitConverter.ToString(encodedBytes).Replace("-", "");
            }

            private static byte[] GetSelfBytes()
            {
                string path = Application.ExecutablePath;

                FileStream running = File.OpenRead(path);

                byte[] exeBytes = new byte[running.Length];
                running.Read(exeBytes, 0, exeBytes.Length);

                running.Close();

                return exeBytes;
            }
        }
       

    }
}
