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
            int horizotal2 = 12;
            int vertical2 = 40;
            int fileNumber = 0;
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {

                   Image image = Image.FromFile(@"keyPadButtons\key"+ fileNumber +".png");
                   keyPadButtons[i, j] = new PictureBox();
                   keyPadButtons[i, j].Size = new Size(32, 25);
                   keyPadButtons[i, j].Location = new Point(horizotal, vertical);
                   keyPadButtons[i, j].Tag = new TagInfo { name = "keyPadButton", value = 0 };
                   keyPadButtons[i, j].Image = image;

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

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    if (j == 1)
                    {
                        horizotal2 = 400;
                    }
                    else
                    {
                        horizotal2 = 12;
                    }
                    Image sideButton = Image.FromFile(@"keyPadButtons\screenSideButton" + j + ".png");
                    screenButtons[i, j] = new PictureBox();
                    screenButtons[i, j].Size = new Size(25, 17);
                    screenButtons[i, j].Location = new Point(horizotal2, vertical2);
                    screenButtons[i, j].Tag = new TagInfo { name = "screenButton", value = 0 };
                    screenButtons[i, j].Image = sideButton;

                   

                    screenButtons[i, j].Show();
                    this.Controls.Add(screenButtons[i, j]);
                    this.Controls.SetChildIndex(screenButtons[i, j], 0);
                    screenButtons[i, j].Click += new EventHandler(screenButtonsClick);
                }
                vertical2 += 50;
            }
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
