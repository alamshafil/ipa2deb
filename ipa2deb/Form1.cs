using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ipa2deb
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.InitialDirectory = "c:\\";
            openFileDialog1.Filter = "Apple IPA File (*.ipa)|*.ipa";
            openFileDialog1.FilterIndex = 2;
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = openFileDialog1.FileName; 
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox2.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if(string.IsNullOrEmpty(textBox1.Text))
            {
                MessageBox.Show("You didn't select an IPA file.", "ipa2deb", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

            } else if(string.IsNullOrEmpty(textBox2.Text))
            {
                MessageBox.Show("You didn't select an output folder.", "ipa2deb", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

            } else
            {
                if (Directory.Exists(Directory.GetCurrentDirectory() + @".\temp"))
                {
                    if (MessageBox.Show("The temp folder already exists, press OK and try again.", "ipa2deb", MessageBoxButtons.OK, MessageBoxIcon.Information) == DialogResult.OK)
                    {
                        Directory.Delete(Directory.GetCurrentDirectory() + @".\temp", true);
                    }
                }
                else
                {

                    Cursor.Current = Cursors.AppStarting;
                    string name = textBox1.Text.Split(Path.DirectorySeparatorChar).Last().Replace(".ipa", "");

                    Directory.CreateDirectory(Directory.GetCurrentDirectory() + @".\temp");
                    Directory.CreateDirectory(Directory.GetCurrentDirectory() + @".\temp\zip");
                    ZipFile.ExtractToDirectory(textBox1.Text, Directory.GetCurrentDirectory() + @".\temp\zip");
                    Directory.CreateDirectory(Directory.GetCurrentDirectory() + @".\temp\app");

                    string dir = Directory.GetDirectories(Directory.GetCurrentDirectory() + @"\temp\zip\Payload\")[0];
                    dir = dir.Split(Path.DirectorySeparatorChar).Last();

                    Directory.Move(Directory.GetCurrentDirectory() + @".\temp\zip\Payload\" + dir, Directory.GetCurrentDirectory() + @".\temp\app\" + dir);

                    Directory.Delete(Directory.GetCurrentDirectory() + @".\temp\zip", true);

                    Directory.CreateDirectory(Directory.GetCurrentDirectory() + @".\temp\deb\");
                    Directory.CreateDirectory(Directory.GetCurrentDirectory() + @".\temp\deb\DEBIAN");
                    Directory.CreateDirectory(Directory.GetCurrentDirectory() + @".\temp\deb\Applications");

                    Directory.Move(Directory.GetCurrentDirectory() + @".\temp\app\" + dir, Directory.GetCurrentDirectory() + @".\temp\deb\Applications\" + dir);
                    Directory.Delete(Directory.GetCurrentDirectory() + @".\temp\app", true);

                    using (StreamWriter sw = new StreamWriter(Directory.GetCurrentDirectory() + @".\temp\deb\DEBIAN\control"))
                    {
                        sw.WriteLine("Package: com.ipa2deb." + name);
                        sw.WriteLine("Name:" + name);
                        sw.WriteLine("Version: 1.0");
                        sw.WriteLine("Description: This app was converted with ipa2deb.");
                        sw.WriteLine("Maintainer: " + Environment.UserName);
                        sw.WriteLine("Author: ipa2deb");
                        sw.Close();
                    }

                    string exePath = Directory.GetCurrentDirectory() + @"\temp\deb\wpkg.exe";
                    File.WriteAllBytes(exePath, ipa2deb.Properties.Resources.wpkg);

                    var processInfo = new ProcessStartInfo(exePath, $"-b {Path.Combine(Directory.GetCurrentDirectory() + @"\temp\deb\")}");
                    processInfo.WorkingDirectory = textBox2.Text;
                    processInfo.CreateNoWindow = true;
                    processInfo.UseShellExecute = false;

                    var process = Process.Start(processInfo);

                    process.WaitForExit();

                    Console.WriteLine("ExitCode: {0}", process.ExitCode);
                    process.Close();

                    File.Copy(Directory.GetCurrentDirectory() + @"\temp\deb\ipa2deb.deb", textBox2.Text + @"\" + name + ".deb" );

                    Directory.Delete(@".\temp", true);
                    Cursor.Current = Cursors.Default;

                    MessageBox.Show("Finished converting ipa to deb!", "ipa2deb", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

    }
}
