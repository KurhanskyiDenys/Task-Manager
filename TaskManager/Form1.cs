using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Management;
using Microsoft.VisualBasic;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.IO;
using System.Runtime.CompilerServices;


namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {

        private List<Process> processes = null;

        public Form1()
        {
            InitializeComponent();
        }

        private void GetProcesses()
        {
            processes.Clear();

            processes = Process.GetProcesses().ToList<Process>();

        }

        private void RefreshProcessesList()
        {
            listView1.Items.Clear();

            double memSize = 0;

            foreach (Process p in processes)
            {
                memSize = 0;

                PerformanceCounter pc = new PerformanceCounter();
                pc.CategoryName = "Process";
                pc.CounterName = "Working Set - Private";
                pc.InstanceName = p.ProcessName;

                memSize = (double)pc.NextValue() / (1000 * 1000);

                string[] row = new string[] { p.ProcessName.ToString(), Math.Round(memSize, 1).ToString() };

                listView1.Items.Add(new ListViewItem(row));

                pc.Close();
                pc.Dispose();
            }

            Text = "Запущено " + processes.Count.ToString() + " процессов!";
        }

        private void KillProcess(Process process)
        {
            process.Kill();

            process.WaitForExit();
        }

        private void KillProcessAndChildren(int pid)
        {
            if (pid == 0)
            {
                return;
            }

            ManagementObjectSearcher searcher = new ManagementObjectSearcher("Select * Form Win32_Process Where ParentProcessID =" + pid);

            ManagementObjectCollection objectCollection = searcher.Get();

            foreach (ManagementObject obj in objectCollection)
            {
                KillProcessAndChildren(Convert.ToInt32(obj["ProcessID"]));
            }

            try
            {
                Process p = Process.GetProcessById(pid);

                p.Kill();

                p.WaitForExit();
            }
            catch (ArgumentException) { }
        }

        private int GetParentProcessId(Process p)
        {
            int parentId = 0;

            try
            {
                ManagementObject managementObject = new ManagementObject("win32_process.handle=' " + p.Id + "'");

                managementObject.Get();

                parentId = Convert.ToInt32(managementObject["ParentProcessId"]);
            }
            catch (Exception) { }

            return parentId;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            processes = new List<Process>();

            GetProcesses();

            RefreshProcessesList();

        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            GetProcesses();

            RefreshProcessesList();

        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            try
            {
                if (listView1.SelectedItems[0] != null)
                {
                    Process processToKill = processes.Where((x) => x.ProcessName == listView1.SelectedItems[0].SubItems[0].Text).ToList()[0];

                    KillProcess(processToKill);

                    GetProcesses();

                    RefreshProcessesList();

                }
            }
            catch (Exception) { }


        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            try
            {
                if (listView1.SelectedItems[0] != null)
                {
                    Process processToKill = processes.Where((x) => x.ProcessName == listView1.SelectedItems[0].SubItems[0].Text).ToList()[0];

                    KillProcessAndChildren(GetParentProcessId(processToKill));

                    GetProcesses();


                    RefreshProcessesList();


                }
            }
            catch (Exception) { }


        }

        private void завершитьДеревоПроцессовToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (listView1.SelectedItems[0] != null)
                {
                    Process processToKill = processes.Where((x) => x.ProcessName == listView1.SelectedItems[0].SubItems[0].Text).ToList()[0];

                    KillProcessAndChildren(GetParentProcessId(processToKill));

                    GetProcesses();


                    RefreshProcessesList();


                }
            }
            catch (Exception) { }

        }

        private void запуститьЗадачцToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string path = Interaction.InputBox("Введите имя программы", "Запуск новой задачи");

            try
            {
                Process.Start(path); 
            }
            catch (Exception) { }
        }

    }
}
