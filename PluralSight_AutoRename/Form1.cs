using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;
using HundredMilesSoftware.UltraID3Lib;
using System.Drawing.Imaging;
using Transitions;
namespace Pluralsight_AutoRename
{
    public partial class Form1 : Form
    {
        //Width : 667 : added
        public Form1()
        {
            InitializeComponent();
        }

        //~~~~~~~~~~~Globals
        string[] fileTypes;
        string lastSelected;

        //~~~~~~~~~~~~~~~~~~~~DRAG MOVE~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

    private void frmDrag_Paint(object sender, PaintEventArgs e)
        {
            //Draws a border to make the Form stand out
            //Just done for appearance, not necessary

            Pen p = new Pen(Color.Gray, 3);
            e.Graphics.DrawRectangle(p, 0, 0, this.Width - 1, this.Height - 1);
            p.Dispose();
        }

        Point lastClick; //Holds where the Form was clicked

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            //Point newLocation = new Point(e.X - lastE.X, e.Y - lastE.Y);
            if (e.Button == MouseButtons.Left) //Only when mouse is clicked
            {
                //Move the Form the same difference the mouse cursor moved;
                this.Left += e.X - lastClick.X;
                this.Top += e.Y - lastClick.Y;
            }
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            lastClick = new Point(e.X, e.Y); //We'll need this for when the Form starts to move
        }

        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            //Point newLocation = new Point(e.X - lastE.X, e.Y - lastE.Y);
            if (e.Button == MouseButtons.Left) //Only when mouse is clicked
            {
                //Move the Form the same difference the mouse cursor moved;
                this.Left += e.X - lastClick.X;
                this.Top += e.Y - lastClick.Y;
            }
        }

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            lastClick = new Point(e.X, e.Y); //We'll need this for when the Form starts to move
        }
        private void label6_MouseMove(object sender, MouseEventArgs e)
        {
            //Point newLocation = new Point(e.X - lastE.X, e.Y - lastE.Y);
            if (e.Button == MouseButtons.Left) //Only when mouse is clicked
            {
                //Move the Form the same difference the mouse cursor moved;
                this.Left += e.X - lastClick.X;
                this.Top += e.Y - lastClick.Y;
            }
        }

        private void label6_MouseDown(object sender, MouseEventArgs e)
        {
            lastClick = new Point(e.X, e.Y); //We'll need this for when the Form starts to move
        }


        private void panel3_MouseMove(object sender, MouseEventArgs e)
        {
            //Point newLocation = new Point(e.X - lastE.X, e.Y - lastE.Y);
            if (e.Button == MouseButtons.Left) //Only when mouse is clicked
            {
                //Move the Form the same difference the mouse cursor moved;
                this.Left += e.X - lastClick.X;
                this.Top += e.Y - lastClick.Y;
            }
        }

        private void panel3_MouseDown(object sender, MouseEventArgs e)
        {
            lastClick = new Point(e.X, e.Y); //We'll need this for when the Form starts to move
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        private void Form1_Load(object sender, EventArgs e)
        {
            this.Width = 300;
            panel4.Location = new Point(-100, 61);
            label5.Location = new Point(300, 65);
            listBox1.Location = new Point(listBox1.Location.X , listBox1.Location.Y -20);
            Transition t1 = new Transition(new TransitionType_EaseInEaseOut(400));
            t1.add(panel4, "Left", 8);

            Transition t2 = new Transition(new TransitionType_EaseInEaseOut(400));
            t2.add(label5, "Left", 196);

            Transition t3 = new Transition(new TransitionType_EaseInEaseOut(600));
            t3.add(listBox1, "Top", listBox1.Location.Y + 20);
            t3.run();

            Transition.runChain(t1, t2);

            //GET valid types
            StreamReader reader = new StreamReader("types.ini");
            string temp = reader.ReadToEnd();
            fileTypes = temp.Split('\n');
            reader.Close();
            
            
            //Analyze();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

            if (listBox1.SelectedIndex > -1)
            {
                label1.Text = listBox2.Items[listBox1.SelectedIndex].ToString();
                if (Path.GetExtension(label1.Text).Equals(".mp4"))
                {
                    Analyze();
                }

                lastSelected = label1.Text;
            }
        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
           // MessageBox.Show(listBox1.SelectedItem.ToString());
        }

        private void listBox1_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string file in files)
            {
                //listBox1.Items.Add(file);
                string path = file;
                if (File.Exists(path))
                {
                    // This path is a file
                    ProcessFile(path);
                }
                else if (Directory.Exists(path))
                {
                    // This path is a directory
                    ProcessDirectory(path);
                }
                else
                {
                    MessageBox.Show("{0} is not a valid file or directory.", path);
                }

            }

            //listBox1.Sorted = true;
            //listBox2.Sorted = true;

            

        }


        private void listBox1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }


        //~~~~~~~~~~~~~~~~~~~~~~~~~USER FUNCTION~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        public void Analyze()

        {
            infoTitle.Text = listBox1.SelectedItem.ToString();
            if (listBox3.Items.Count > listBox1.SelectedIndex)
            {
                if (prefix.Checked)
                {
                    int startValue = int.Parse(prefix_text.Text);
                    int currentValue = startValue + listBox1.SelectedIndex;
                    infoTitle.Text = currentValue.ToString("D2") + " - " + listBox3.Items[listBox1.SelectedIndex].ToString();
                }
                else
                {
                    infoTitle.Text = listBox3.Items[listBox1.SelectedIndex].ToString();
                }
            }
              
        }

        Image albumArt;
        //~~~~~~~~~~~~~~~~~~~~~~~PRE DEFINED FUNCTION~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // Process all files in the directory passed in, recurse on any directories  
        // that are found, and process the files they contain. 
        public void ProcessDirectory(string targetDirectory)
        {
            // Process the list of files found in the directory. 
            string[] fileEntries = Directory.GetFiles(targetDirectory);
            foreach (string fileName in fileEntries)
                ProcessFile(fileName);

            // Recurse into subdirectories of this directory. 
            string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
            foreach (string subdirectory in subdirectoryEntries)
                ProcessDirectory(subdirectory);
        }

        // Insert logic for processing found files here. 
        public void ProcessFile(string path)
        {
            bool valid = false;
            foreach (string type in fileTypes)
            {
                string type2 = type.Replace("\r", "");
                //MessageBox.Show(":"+ Path.GetExtension(path)+":" + (type2) +":");
                if (Path.GetExtension(path).Equals(type2))
                {
                    valid = true;
                    break;
                }
            }

            foreach (string s in listBox2.Items)
            {
                if (path.Equals(s))
                    valid = false;
            }

            if (valid == true)
            {
                if(listBox1.Items.Count > 0)
                if (listBox1.Items[0].Equals("Drag Files Here"))
                    listBox1.Items.RemoveAt(0);
                Transition.run(this, "Width", 669, new TransitionType_EaseInEaseOut(1000));
                button4.Visible = true;
                button5.Visible = true;
                listBox1.Items.Add(Path.GetFileName(path)); 
                listBox2.Items.Add(path); 
            }

        }
   

        private void button1_Click(object sender, EventArgs e)
        {
            if (Path.GetExtension(label1.Text).Equals(".mp4"))
            {
                string fileDirectory = Path.GetDirectoryName(label1.Text);
                string oldFile = Path.GetFullPath(label1.Text);
                string newFile = fileDirectory + "\\" + infoTitle.Text + Path.GetExtension(oldFile);

                try
                {
                    if (cTitle.Checked == true)
                    {
                        System.IO.File.Move(oldFile, newFile);
                    }
                }catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
          
        }

     

        private void button2_Click(object sender, EventArgs e)
        {
            listBox3.Items.Clear();
            string[] splitted = richTextBox2.Text.Split('\n');
            foreach (string s in splitted)
            {
               listBox3.Items.Add(s);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if(listBox2.Items.Count>0)
            {
                for (int c = 0; c < listBox2.Items.Count; c++)
                {
                    listBox1.SetSelected(c, true);
                    button1.PerformClick();
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex >= 0)
            {
                int index = listBox1.SelectedIndex;
                listBox1.Items.RemoveAt(index);
                listBox2.Items.RemoveAt(index);
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.Close();
        }

        private void linkLabel1_MouseEnter(object sender, EventArgs e)
        {
            linkLabel1.LinkColor = Color.Silver;
        }

        private void linkLabel1_MouseLeave(object sender, EventArgs e)
        {
            linkLabel1.LinkColor = Color.White;
        }

        private void linkLabel2_MouseEnter(object sender, EventArgs e)
        {
            linkLabel2.LinkColor = Color.Silver;
        }

        private void linkLabel2_MouseLeave(object sender, EventArgs e)
        {
            linkLabel2.LinkColor = Color.White;
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Transition t3 = new Transition(new TransitionType_EaseInEaseOut(600));
            t3.add(this, "Width", 1200);
            t3.run();
        }

        private void richTextBox2_TextChanged(object sender, EventArgs e)
        {
            listBox3.Items.Clear();
            string[] splitted = richTextBox2.Text.Split('\n');
            foreach (string s in splitted)
            {
                listBox3.Items.Add(s);

            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            listBox2.Items.Clear();
        }

        private void panel3_Paint(object sender, PaintEventArgs e)
        {

        }

        private void up_Click(object sender, EventArgs e)
        {

            listBox1.BeginUpdate();
            listBox2.BeginUpdate();
            ListBox.SelectedIndexCollection col = this.listBox1.SelectedIndices;
            for (int i = 0; i <= this.listBox1.Items.Count - 1; i++)
            {
                if ((col.Contains(i)) && (i > 0))
                {
                    object o = this.listBox1.Items[i];
                    this.listBox1.Items.RemoveAt(i);
                    this.listBox1.Items.Insert(i - 1, o);
                    this.listBox1.SetSelected(i - 1, true);

                    object p = this.listBox2.Items[i];
                    this.listBox2.Items.RemoveAt(i);
                    this.listBox2.Items.Insert(i - 1, p);
                    this.listBox2.SetSelected(i - 1, true);
                }
            }
            listBox1.EndUpdate();
            listBox2.EndUpdate();

        }

        private void down_Click(object sender, EventArgs e)
        {

            listBox1.BeginUpdate();
            listBox2.BeginUpdate();
            ListBox.SelectedIndexCollection col = this.listBox1.SelectedIndices;
            for (int i = 0; i <= this.listBox1.Items.Count - 1; i++)
            {
                if ((col.Contains(i)) && (i < 0))
                {
                    object o = this.listBox1.Items[i];
                    this.listBox1.Items.RemoveAt(i);
                    this.listBox1.Items.Insert(i + 1, o);
                    this.listBox1.SetSelected(i + 1, true);

                    object p = this.listBox2.Items[i];
                    this.listBox2.Items.RemoveAt(i);
                    this.listBox2.Items.Insert(i + 1, p);
                    this.listBox2.SetSelected(i + 1, true);
                }
            }
            listBox1.EndUpdate();
            listBox2.EndUpdate();


        }
    }
}
