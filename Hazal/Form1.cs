using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Data.SqlClient;
using System.Configuration;
using System.IO.Ports; //ardunio kartı için giriş - çıkış
using AForge.Video; // yeni frame oluşturmak için
using AForge.Video.DirectShow;
using AForge.Imaging.Filters;
using AForge.Imaging;


namespace Hazal
{
    public partial class Form1 : Form
    {
        private FilterInfoCollection videocikti; // bilgisayardaki bütün kameralar dizi halinde sıralanır
        private VideoCaptureDevice izle; // kullanılan kamera seçilir

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            portBox.DataSource = SerialPort.GetPortNames(); // ardunioda tüm portlar sıralandı
            int num = portBox.Items.Count;

            if (num == 0)
            {
                toolStripLabel1.Text = "Port yok!";
                portBox.Enabled = false;
                btnBaglan.Enabled = false; //aktifleştirilemedi
            }

            else
            {
                toolStripLabel1.Text = num + "adet Port var";
            }

            videocikti = new FilterInfoCollection(FilterCategory.VideoInputDevice); 
            // bilgisayarımdaki tüm kameralar bir değişkene atandı

            foreach (FilterInfo VideoCaptureDevice in videocikti)
            {

                camBox1.Items.Add(VideoCaptureDevice.Name); //camboxlar listelendi
                camBox1.SelectedIndex = 0; //yazsaydım ilk açtığımızda ekran boş gözükür ve biz seçerdik
            }

         
        }


        private void btnBaslat_Click(object sender, EventArgs e)
        {
            izle = new VideoCaptureDevice (videocikti[camBox1.SelectedIndex].MonikerString); 
            //liste olarak kullanılabilecek kameralar çıkar ve seçilir
            izle.NewFrame += new NewFrameEventHandler(izle_newFrame);
            //gerçek zamanlı olarak ekranda yeni çıkan bütün kareler işlenir, bu karelerde işlem yapılabilir

            izle.Start();
        }
        void izle_newFrame(object sender, NewFrameEventArgs eventArgs)
        {
            Bitmap goruntu = (Bitmap)eventArgs.Frame.Clone();
            Bitmap goruntu1 = (Bitmap)eventArgs.Frame.Clone();
            kaynakBox.Image = goruntu;

            //kırmızı için 
            if (radioButton1.Checked)
            {
                EuclideanColorFiltering filter = new EuclideanColorFiltering(); // öklid filtresi oluşturuldu
                filter.CenterColor = new RGB(Color.FromArgb(215, 0, 0)); // kırmızı renk seçildi
                filter.Radius = 100; //Belirlenen radiusun içierisi boyunca kırmızı algılanacak
                filter.ApplyInPlace(goruntu1); //goruntu1 üzerine uygulanan filtre ile background siyah oldu
                nesnebul(goruntu1);
            }

            //yeşil için 
            if (radioButton2.Checked)
            {
                EuclideanColorFiltering filter = new EuclideanColorFiltering();
                filter.CenterColor = new RGB(Color.FromArgb(0, 255, 0));
                filter.Radius = 100;
                filter.ApplyInPlace(goruntu1);
                nesnebul(goruntu1);
            }

            //mavi için
            if (radioButton3.Checked)
            {
                EuclideanColorFiltering filter = new EuclideanColorFiltering();
                filter.CenterColor = new RGB(Color.FromArgb(30, 144, 255));
                filter.Radius = 100;
                filter.ApplyInPlace(goruntu1);
                nesnebul(goruntu1);
            }
            
        }




        public void nesnebul(Bitmap goruntu1) // işlenen görüntü
        {

            BlobCounter blobCounter = new BlobCounter(); // obje say komutu ile şekil tespiti yapılır
            blobCounter.MinWidth = 5; // değer küçüldükte hassasiyet artar ve şeklin frame alınması hızlanır
            blobCounter.MinHeight = 5;
            blobCounter.FilterBlobs = true; // filtre uygulandı
            blobCounter.ProcessImage(goruntu1); 
            //Blob görüntü parçalarıdır.Bu filtre işlenmiş görüntü (goruntu1) e uygulanıp tekrar işlendi
            Rectangle[] rects = blobCounter.GetObjectsRectangles();
            // oluşturulan  her kareden bilgi alınıp, diziye aktarıldı



            islemBox.Image = goruntu1; //islenmis son görüntü yüklendi
            foreach (Rectangle recs in rects)
            {

                if (rects.Length > 0) // eğer karelerden bilgi alındıysa
                {
                    Rectangle objectRect = rects[0]; // yakalanan ilk kare obje oldu
                    Graphics g = kaynakBox.CreateGraphics(); // kontrol amaçlı grafiksel nesne oluşturuldu
                    using (Pen pen = new Pen(Color.FromArgb(0, 250, 0), 3)) 
                        // kaynakboxta objeyi çevreleyen dikdörtgenin rengi ve kalınlığı
                    {
                        g.DrawRectangle(pen, objectRect);
                    }

                    int objectX = objectRect.X + (objectRect.Width / 2); //  yenien boyutlandırma yapıldı
                    int objectY = objectRect.Y + (objectRect.Height / 2);

                    // 360 a 275 lik picture box boyutuna yaklaşık değerler verildi
                    if (objectX < 105 && objectY < 92) 
                    {

                        serialPort1.Write("2");

                    }
                    else if ((objectX > 105 && objectX < 220) && (objectY < 92))
                    {

                        serialPort1.Write("3");
                    }
                    else if ((objectX > 220 && objectX < 350) && (objectY < 92))
                    {

                        serialPort1.Write("4");
                    }
                    else if ((objectX < 105) && (objectY > 92 && objectY < 185))
                    {

                        serialPort1.Write("5");
                    }
                    else if ((objectX > 105 && objectX < 220) && (objectY > 92 && objectY < 185))
                    {

                        serialPort1.Write("6");
                    }
                    else if ((objectX > 220 && objectX < 360) && (objectY > 92 && objectY < 185))
                    {

                        serialPort1.Write("7");
                    }
                    else if ((objectX < 105) && (objectY > 185 && objectY < 275))
                    {

                        serialPort1.Write("8");
                    }
                    else if ((objectX > 105 && objectX < 220) && (objectY > 185 && objectY < 275))
                    {

                        serialPort1.Write("9");
                    }
                    else if ((objectX > 230) && (objectY > 185 && objectY < 275))
                    {

                        serialPort1.Write("10");
                    }


                }
            }
        }


        private void toolStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

       
        private void btnBaglan_Click(object sender, EventArgs e)
        { 
            //ardunio kısmı için 
            serialPort1.PortName = portBox.SelectedItem.ToString(); // Benim stringim port boxtaki seçilendir  
            serialPort1.BaudRate = 9600; // haberleşmenin gerçekleştiği hız
            serialPort1.Open();
            if (serialPort1.IsOpen)
            {
                toolStripLabel1.Text = portBox.SelectedItem.ToString() + "portuna bağlandı"; 
                //seçiilen string buraya aktarıldı

            }
        }

    }
}
