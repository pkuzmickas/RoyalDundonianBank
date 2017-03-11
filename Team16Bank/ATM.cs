using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Team16Bank
{
    public partial class ATM : Form
    {
        private PictureBox[,] screenButtons = new PictureBox[4, 2];
        private PictureBox[,] keyPadButtons = new PictureBox[4, 4];
        private Label account = new Label();
        public ATM()
        {
            InitializeComponent();
        }

        private void ATM_Load(object sender, EventArgs e)
        {
            int horizotal = 28;
            int vertical = 271;
            int fileNumber = 1;
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {

                     Image image = Image.FromFile(@"keyPadButtons\key"+ fileNumber +".png");
                    //if(fileNumber==12 || fileNumber==4 || fileNumber ==8 || fileNumber==16) image.
                    keyPadButtons[i, j] = new PictureBox();
                    keyPadButtons[i, j].Size = new Size(32, 25);
                    keyPadButtons[i, j].Location = new Point(horizotal, vertical);
                    keyPadButtons[i, j].Tag = new TagInfo { name = "screenButton", value = 0 };
                   keyPadButtons[i, j].Image = image;
                    //keyPadButtons[i, j].BackColor = Color.Transparent;

                    /*screenButtons[i,j].BackColor = System.Drawing.Color.Transparent;
                    screenButtons[i, j].BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
                    screenButtons[i, j].Image = image;
                    screenButtons[i, j].Location = new System.Drawing.Point(horizotal, vertical);
                    screenButtons[i, j].Name = "pictureBox1";
                    screenButtons[i, j].Size = new System.Drawing.Size(52, 40);
                    screenButtons[i, j].TabIndex = 0;
                    screenButtons[i, j].TabStop = false;
                    screenButtons[i, j].Click += new System.EventHandler(screenButtonsClick);*/
                    if ((j + 1) % 4 == 0)
                    {
                        vertical += 30;
                        horizotal = 28;
                    }
                    else
                    {
                        if (j == 2) horizotal += 44;
                        else horizotal += 37;
                    }
                    keyPadButtons[i, j].Show();
                    this.Controls.Add(keyPadButtons[i, j]);
                    this.Controls.SetChildIndex(keyPadButtons[i, j], 0);
                    keyPadButtons[i, j].Click += new EventHandler(screenButtonsClick);
                    fileNumber++;
                }
            }
            keyPadButtons[0, 3].SizeMode = PictureBoxSizeMode.StretchImage;
            keyPadButtons[0, 3].Size = new Size(37, 25);
            keyPadButtons[1, 3].SizeMode = PictureBoxSizeMode.StretchImage;
            keyPadButtons[1, 3].Size = new Size(37, 25);
            keyPadButtons[2, 3].SizeMode = PictureBoxSizeMode.StretchImage;
            keyPadButtons[2, 3].Size = new Size(37, 25);
            keyPadButtons[3, 3].SizeMode = PictureBoxSizeMode.StretchImage;
            keyPadButtons[3, 3].Size = new Size(37, 25);
        }
        private void screenButtonsClick(object sender, EventArgs e)
        {
        
        }
    }

    public class TagInfo
    {
        public string name;
        public int value;      
    }
}
