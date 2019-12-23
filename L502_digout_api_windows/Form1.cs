using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;

using x502api;
using lpcieapi;


namespace L502_digout_api_windows
{
    public partial class MainForm : Form
    {
        const uint RECV_BUF_SIZE = 8 * 1024 * 1024;
        const uint RECV_TOUT = 250;
        //
        bool imp_mode = false;
        //
        UInt32 state_outputs = 0;
        bool out1 = false;
        bool out2 = false;
        bool out3 = false;
        bool out4 = false;
        bool out5 = false;
        bool out6 = false;
        bool out7 = false;
        bool out8 = false;
        bool out9 = false;
        bool out10 = false;
        bool out11 = false;
        bool out12 = false;
        bool out13 = false;
        bool out14 = false;
        bool out15 = false;
        bool out16 = false;
        //
        string[] output_names=new string[16];
        //

        //
        X502.DevRec[] devrecs;
        X502 hnd; /* Описатель модуля с которым работаем (null, если связи нет) */
        //Thread thread; /* Объект потока для синхронного сбора */
        //bool reqStop; /* Запрос на останов потока сбора данных */
        //bool threadRunning; /* Признак, идет ли сбор данных в отдельном потоке */
        //UInt32[] rcv_buf; /* буфер для приема сырых данных */
        //double[] adcData; /* буфер для данных АЦП */
        //UInt32[] dinData; /* буфер для отсчетов цифровых входов */
        //UInt32 adcSize, dinSize; /* размер действительных данных в adcData и dinData */
        //UInt32 firstLch; /* номер логического канала,  которому соответствует отсчет в adcData[0] */

        public MainForm()
        {
            InitializeComponent();


            /* rcv_buf = new UInt32[RECV_BUF_SIZE];
             dinData = new UInt32[RECV_BUF_SIZE];
             adcData = new double[RECV_BUF_SIZE];
             threadRunning = false;*/

            refreshDevList();
        }

        private void btnRefreshDeviceList_Click(object sender, EventArgs e)
        {
            refreshDevList();
        }

        private void cbbSerialList_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void refreshDevList()
        {
            cbbSerialList.Items.Clear();

            //получаем список серийных номеров
            X502.DevRec[] pci_devrecs;
            X502.DevRec[] usb_devrecs;
            Int32 res = L502.GetDevRecordsList(out pci_devrecs, 0);
            res = E502.UsbGetDevRecordsList(out usb_devrecs, 0);

            devrecs = new X502.DevRec[pci_devrecs.Length + usb_devrecs.Length];
            pci_devrecs.CopyTo(devrecs, 0);
            usb_devrecs.CopyTo(devrecs, pci_devrecs.Length);

            /* заполняем полученные серийные номера в ComboBox */
            for (int i = 0; i < devrecs.Length; i++)
            {
                cbbSerialList.Items.Add(devrecs[i].DevName + ", " + devrecs[i].Serial);
            }
            if (devrecs.Length > 0)
            {
                cbbSerialList.SelectedIndex = 0;
            }

            updateControls();
        }
        private void updateControls()
        {
            btnRefreshDeviceList.Enabled = hnd == null;
            cbbSerialList.Enabled = hnd == null;
            btnOpen.Text = hnd == null ? "Установить связь с устройством" :
                                               "Разорвать связь с устройством";

            button18.Text = imp_mode == false ? "Запустить импульсный режим" : "Отключить импульсный режим";
            btnOpen.Enabled = (hnd != null) || (cbbSerialList.SelectedItem != null);
            btnAsyncDigOut.Enabled = (hnd != null);

            button1.Enabled = (hnd != null);
            button2.Enabled = (hnd != null);
            button3.Enabled = (hnd != null);
            button4.Enabled = (hnd != null);
            button5.Enabled = (hnd != null);
            button6.Enabled = (hnd != null);
            button7.Enabled = (hnd != null);
            button8.Enabled = (hnd != null);
            button9.Enabled = (hnd != null);
            button10.Enabled = (hnd != null);
            button11.Enabled = (hnd != null);
            button12.Enabled = (hnd != null);
            button13.Enabled = (hnd != null);
            button14.Enabled = (hnd != null);
            button15.Enabled = (hnd != null);
            button16.Enabled = (hnd != null);

            groupBoxOut1.Enabled = (hnd != null);
            groupBoxOut2.Enabled = (hnd != null);
            groupBoxOut3.Enabled = (hnd != null);
            groupBoxOut4.Enabled = (hnd != null);
            groupBoxOut5.Enabled = (hnd != null);
            groupBoxOut6.Enabled = (hnd != null);
            groupBoxOut7.Enabled = (hnd != null);
            groupBoxOut8.Enabled = (hnd != null);
            groupBoxOut9.Enabled = (hnd != null);
            groupBoxOut10.Enabled = (hnd != null);
            groupBoxOut11.Enabled = (hnd != null);
            groupBoxOut12.Enabled = (hnd != null);
            groupBoxOut13.Enabled = (hnd != null);
            groupBoxOut14.Enabled = (hnd != null);
            groupBoxOut15.Enabled = (hnd != null);
            groupBoxOut16.Enabled = (hnd != null);

        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            if (hnd == null)
            {
                lpcie.Errs res;

                int idx = cbbSerialList.SelectedIndex;
                if (idx >= 0)
                {
                    /* создаем описатель модуля */
                    hnd = X502.Create(devrecs[idx].DevName);
                    /* устанавливаем связь по выбранному серийному номеру */
                    res = hnd.Open(devrecs[idx]);
                    if (res == lpcie.Errs.OK)
                    {
                        showDevInfo();
                        groupBox1.BackColor = Color.LawnGreen;
                    }
                    else
                    {
                        MessageBox.Show(X502.GetErrorString(res), "Ошибка открытия модуля", MessageBoxButtons.OK,
                                    MessageBoxIcon.Error);
                        hnd = null;
                    }
                }
            }
            else
            {
                deviceClose();
                groupBox1.BackColor = Color.Silver;
            }

            updateControls();
        }

        private void deviceClose()
        {
            if (hnd != null)
            {
                // закрытие связи с модулем
                hnd.Close();
                // память освободится диспетчером мусора, т.к. нет больше ссылок
                hnd = null;
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load("settings.xml");
            int i = 0;
            foreach (XmlNode node in doc.DocumentElement)
            {
                output_names[i] = node.Attributes[0].Value;
                i++;
            }
            groupBoxOut1.Text=label3.Text = output_names[0];
            groupBoxOut2.Text = label4.Text = output_names[1];
            groupBoxOut3.Text = label5.Text = output_names[2];
            groupBoxOut4.Text = label6.Text = output_names[3];
            groupBoxOut5.Text = label7.Text = output_names[4];
            groupBoxOut6.Text = label8.Text = output_names[5];
            groupBoxOut7.Text = label9.Text = output_names[6];
            groupBoxOut8.Text = label10.Text = output_names[7];

            groupBoxOut9.Text = label19.Text = output_names[8];
            groupBoxOut10.Text = label18.Text = output_names[9];
            groupBoxOut11.Text = label17.Text = output_names[10];
            groupBoxOut12.Text = label16.Text = output_names[11];
            groupBoxOut13.Text = label15.Text = output_names[12];
            groupBoxOut14.Text = label14.Text = output_names[13];
            groupBoxOut15.Text = label12.Text = output_names[14];
            groupBoxOut16.Text = label11.Text = output_names[15];

        }

        private void showDevInfo()
        {
            /* получаем информацию о модуле */
            X502.Info devinfo = hnd.DevInfo;
            /* отображаем ее на панели */
            chkBfPresent.Checked = (devinfo.DevFlags & X502.DevFlags.BF_PRESENT) != 0;
            chkDacPresent.Checked = (devinfo.DevFlags & X502.DevFlags.DAC_PRESENT) != 0;
            chkGalPresent.Checked = (devinfo.DevFlags & X502.DevFlags.GAL_PRESENT) != 0;
            chkEthSupport.Checked = (devinfo.DevFlags & X502.DevFlags.IFACE_SUPPORT_ETH) != 0;

            edtFpgaVer.Text = devinfo.FpgaVerString;
            edtPldaVer.Text = string.Format("{0}", devinfo.PldaVer);
            edtMcuVer.Text = devinfo.McuFirmwareVerString;
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void btnAsyncDigOut_Click(object sender, EventArgs e)
        {
            if (hnd != null)
            {
                UInt32 val = Convert.ToUInt32(edtAsyncDigOut.Text, 16);
                lpcie.Errs err = hnd.AsyncOutDig(val, 0);
                if (err != lpcie.Errs.OK)
                {
                    MessageBox.Show(X502.GetErrorString(err), "Ошибка асинхронного вывода на цифровые линии",
                               MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void edtAsyncDigOut_TextChanged(object sender, EventArgs e)
        {

        }

        private void groupBox2_Enter(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (hnd != null)
            {
                if (out1 == false)
                {
                    state_outputs = state_outputs + 1;
                    lpcie.Errs err = hnd.AsyncOutDig(state_outputs, 0);
                    if (err != lpcie.Errs.OK)
                    {
                        MessageBox.Show(X502.GetErrorString(err), "Ошибка асинхронного вывода на цифровой выход №1",
                                   MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    out1 = true;
                    button1.BackColor = Color.Lime;
                }
                else
                {
                    state_outputs = state_outputs - 1;
                    lpcie.Errs err = hnd.AsyncOutDig(state_outputs, 0);
                    if (err != lpcie.Errs.OK)
                    {
                        MessageBox.Show(X502.GetErrorString(err), "Ошибка асинхронного вывода на цифровой выход №1",
                                   MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    out1 = false;
                    button1.BackColor = Color.Silver;
                }
            }
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (hnd != null)
            {
                if (out2 == false)
                {
                    state_outputs = state_outputs + 2;
                    lpcie.Errs err = hnd.AsyncOutDig(state_outputs, 0);
                    if (err != lpcie.Errs.OK)
                    {
                        MessageBox.Show(X502.GetErrorString(err), "Ошибка асинхронного вывода на цифровой выход №2",
                                   MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    out2 = true;
                    button2.BackColor = Color.Lime;
                }
                else
                {
                    state_outputs = state_outputs - 2;
                    lpcie.Errs err = hnd.AsyncOutDig(state_outputs, 0);
                    if (err != lpcie.Errs.OK)
                    {
                        MessageBox.Show(X502.GetErrorString(err), "Ошибка асинхронного вывода на цифровой выход №2",
                                   MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    out2 = false;
                    button2.BackColor = Color.Silver;
                }
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            deviceClose();
        }

        private void label3_Click(object sender, EventArgs e)
        {
          
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (hnd != null)
            {
                if (out3 == false)
                {
                    state_outputs = state_outputs + 4;
                    lpcie.Errs err = hnd.AsyncOutDig(state_outputs, 0);
                    if (err != lpcie.Errs.OK)
                    {
                        MessageBox.Show(X502.GetErrorString(err), "Ошибка асинхронного вывода на цифровой выход №3",
                                   MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    out3 = true;
                    button3.BackColor = Color.Lime;
                }
                else
                {
                    state_outputs = state_outputs - 4;
                    lpcie.Errs err = hnd.AsyncOutDig(state_outputs, 0);
                    if (err != lpcie.Errs.OK)
                    {
                        MessageBox.Show(X502.GetErrorString(err), "Ошибка асинхронного вывода на цифровой выход №3",
                                   MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    out3 = false;
                    button3.BackColor = Color.Silver;
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (hnd != null)
            {
                if (out4 == false)
                {
                    state_outputs = state_outputs + 8;
                    lpcie.Errs err = hnd.AsyncOutDig(state_outputs, 0);
                    if (err != lpcie.Errs.OK)
                    {
                        MessageBox.Show(X502.GetErrorString(err), "Ошибка асинхронного вывода на цифровой выход №4",
                                   MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    out4 = true;
                    button4.BackColor = Color.Lime;
                }
                else
                {
                    state_outputs = state_outputs - 8;
                    lpcie.Errs err = hnd.AsyncOutDig(state_outputs, 0);
                    if (err != lpcie.Errs.OK)
                    {
                        MessageBox.Show(X502.GetErrorString(err), "Ошибка асинхронного вывода на цифровой выход №4",
                                   MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    out4 = false;
                    button4.BackColor = Color.Silver;
                }
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (hnd != null)
            {
                if (out5 == false)
                {
                    state_outputs = state_outputs + 16;
                    lpcie.Errs err = hnd.AsyncOutDig(state_outputs, 0);
                    if (err != lpcie.Errs.OK)
                    {
                        MessageBox.Show(X502.GetErrorString(err), "Ошибка асинхронного вывода на цифровой выход №5",
                                   MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    out5 = true;
                    button5.BackColor = Color.Lime;
                }
                else
                {
                    state_outputs = state_outputs - 16;
                    lpcie.Errs err = hnd.AsyncOutDig(state_outputs, 0);
                    if (err != lpcie.Errs.OK)
                    {
                        MessageBox.Show(X502.GetErrorString(err), "Ошибка асинхронного вывода на цифровой выход №5",
                                   MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    out5 = false;
                    button5.BackColor = Color.Silver;
                }
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (hnd != null)
            {
                if (out6 == false)
                {
                    state_outputs = state_outputs + 32;
                    lpcie.Errs err = hnd.AsyncOutDig(state_outputs, 0);
                    if (err != lpcie.Errs.OK)
                    {
                        MessageBox.Show(X502.GetErrorString(err), "Ошибка асинхронного вывода на цифровой выход №6",
                                   MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    out6 = true;
                    button6.BackColor = Color.Lime;
                }
                else
                {
                    state_outputs = state_outputs - 32;
                    lpcie.Errs err = hnd.AsyncOutDig(state_outputs, 0);
                    if (err != lpcie.Errs.OK)
                    {
                        MessageBox.Show(X502.GetErrorString(err), "Ошибка асинхронного вывода на цифровой выход №6",
                                   MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    out6 = false;
                    button6.BackColor = Color.Silver;
                }
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (hnd != null)
            {
                if (out7 == false)
                {
                    state_outputs = state_outputs + 64;
                    lpcie.Errs err = hnd.AsyncOutDig(state_outputs, 0);
                    if (err != lpcie.Errs.OK)
                    {
                        MessageBox.Show(X502.GetErrorString(err), "Ошибка асинхронного вывода на цифровой выход №7",
                                   MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    out7 = true;
                    button7.BackColor = Color.Lime;
                }
                else
                {
                    state_outputs = state_outputs - 64;
                    lpcie.Errs err = hnd.AsyncOutDig(state_outputs, 0);
                    if (err != lpcie.Errs.OK)
                    {
                        MessageBox.Show(X502.GetErrorString(err), "Ошибка асинхронного вывода на цифровой выход №7",
                                   MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    out7 = false;
                    button7.BackColor = Color.Silver;
                }
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (hnd != null)
            {
                if (out8 == false)
                {
                    state_outputs = state_outputs + 128;
                    lpcie.Errs err = hnd.AsyncOutDig(state_outputs, 0);
                    if (err != lpcie.Errs.OK)
                    {
                        MessageBox.Show(X502.GetErrorString(err), "Ошибка асинхронного вывода на цифровой выход №8",
                                   MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    out8 = true;
                    button8.BackColor = Color.Lime;
                }
                else
                {
                    state_outputs = state_outputs - 128;
                    lpcie.Errs err = hnd.AsyncOutDig(state_outputs, 0);
                    if (err != lpcie.Errs.OK)
                    {
                        MessageBox.Show(X502.GetErrorString(err), "Ошибка асинхронного вывода на цифровой выход №8",
                                   MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    out8 = false;
                    button8.BackColor = Color.Silver;
                }
            }
        }

        private void button17_Click(object sender, EventArgs e)
        {
            if (hnd != null)
            {
                if (out9 == false)
                {
                    state_outputs = state_outputs + 256;
                    lpcie.Errs err = hnd.AsyncOutDig(state_outputs, 0);
                    if (err != lpcie.Errs.OK)
                    {
                        MessageBox.Show(X502.GetErrorString(err), "Ошибка асинхронного вывода на цифровой выход №9",
                                   MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    out9 = true;
                    button17.BackColor = Color.Lime;
                }
                else
                {
                    state_outputs = state_outputs - 256;
                    lpcie.Errs err = hnd.AsyncOutDig(state_outputs, 0);
                    if (err != lpcie.Errs.OK)
                    {
                        MessageBox.Show(X502.GetErrorString(err), "Ошибка асинхронного вывода на цифровой выход №9",
                                   MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    out9 = false;
                    button17.BackColor = Color.Silver;
                }
            }
        }

        private void button16_Click(object sender, EventArgs e)
        {
            if (hnd != null)
            {
                if (out10 == false)
                {
                    state_outputs = state_outputs + 512;
                    lpcie.Errs err = hnd.AsyncOutDig(state_outputs, 0);
                    if (err != lpcie.Errs.OK)
                    {
                        MessageBox.Show(X502.GetErrorString(err), "Ошибка асинхронного вывода на цифровой выход №10",
                                   MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    out10 = true;
                    button16.BackColor = Color.Lime;
                }
                else
                {
                    state_outputs = state_outputs - 512;
                    lpcie.Errs err = hnd.AsyncOutDig(state_outputs, 0);
                    if (err != lpcie.Errs.OK)
                    {
                        MessageBox.Show(X502.GetErrorString(err), "Ошибка асинхронного вывода на цифровой выход №10",
                                   MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    out10 = false;
                    button16.BackColor = Color.Silver;
                }
            }
        }

        private void button15_Click(object sender, EventArgs e)
        {
            if (hnd != null)
            {
                if (out11 == false)
                {
                    state_outputs = state_outputs + 1024;
                    lpcie.Errs err = hnd.AsyncOutDig(state_outputs, 0);
                    if (err != lpcie.Errs.OK)
                    {
                        MessageBox.Show(X502.GetErrorString(err), "Ошибка асинхронного вывода на цифровой выход №11",
                                   MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    out11 = true;
                    button15.BackColor = Color.Lime;
                }
                else
                {
                    state_outputs = state_outputs - 1024;
                    lpcie.Errs err = hnd.AsyncOutDig(state_outputs, 0);
                    if (err != lpcie.Errs.OK)
                    {
                        MessageBox.Show(X502.GetErrorString(err), "Ошибка асинхронного вывода на цифровой выход №11",
                                   MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    out11 = false;
                    button15.BackColor = Color.Silver;
                }
            }
        }

        private void button14_Click(object sender, EventArgs e)
        {
            if (hnd != null)
            {
                if (out12 == false)
                {
                    state_outputs = state_outputs + 2048;
                    lpcie.Errs err = hnd.AsyncOutDig(state_outputs, 0);
                    if (err != lpcie.Errs.OK)
                    {
                        MessageBox.Show(X502.GetErrorString(err), "Ошибка асинхронного вывода на цифровой выход №12",
                                   MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    out12 = true;
                    button14.BackColor = Color.Lime;
                }
                else
                {
                    state_outputs = state_outputs - 2048;
                    lpcie.Errs err = hnd.AsyncOutDig(state_outputs, 0);
                    if (err != lpcie.Errs.OK)
                    {
                        MessageBox.Show(X502.GetErrorString(err), "Ошибка асинхронного вывода на цифровой выход №12",
                                   MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    out12 = false;
                    button14.BackColor = Color.Silver;
                }
            }
        }

        private void button13_Click(object sender, EventArgs e)
        {
            if (hnd != null)
            {
                if (out13 == false)
                {
                    state_outputs = state_outputs + 4096;
                    lpcie.Errs err = hnd.AsyncOutDig(state_outputs, 0);
                    if (err != lpcie.Errs.OK)
                    {
                        MessageBox.Show(X502.GetErrorString(err), "Ошибка асинхронного вывода на цифровой выход №13",
                                   MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    out13 = true;
                    button13.BackColor = Color.Lime;
                }
                else
                {
                    state_outputs = state_outputs - 4096;
                    lpcie.Errs err = hnd.AsyncOutDig(state_outputs, 0);
                    if (err != lpcie.Errs.OK)
                    {
                        MessageBox.Show(X502.GetErrorString(err), "Ошибка асинхронного вывода на цифровой выход №13",
                                   MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    out13 = false;
                    button13.BackColor = Color.Silver;
                }
            }
        }

        private void button12_Click(object sender, EventArgs e)
        {
            if (hnd != null)
            {
                if (out14 == false)
                {
                    state_outputs = state_outputs + 8192;
                    lpcie.Errs err = hnd.AsyncOutDig(state_outputs, 0);
                    if (err != lpcie.Errs.OK)
                    {
                        MessageBox.Show(X502.GetErrorString(err), "Ошибка асинхронного вывода на цифровой выход №14",
                                   MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    out14 = true;
                    button12.BackColor = Color.Lime;
                }
                else
                {
                    state_outputs = state_outputs - 8192;
                    lpcie.Errs err = hnd.AsyncOutDig(state_outputs, 0);
                    if (err != lpcie.Errs.OK)
                    {
                        MessageBox.Show(X502.GetErrorString(err), "Ошибка асинхронного вывода на цифровой выход №14",
                                   MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    out14 = false;
                    button12.BackColor = Color.Silver;
                }
            }
        }

        private void button11_Click(object sender, EventArgs e)
        {
            if (hnd != null)
            {
                if (out15 == false)
                {
                    state_outputs = state_outputs + 16384;
                    lpcie.Errs err = hnd.AsyncOutDig(state_outputs, 0);
                    if (err != lpcie.Errs.OK)
                    {
                        MessageBox.Show(X502.GetErrorString(err), "Ошибка асинхронного вывода на цифровой выход №15",
                                   MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    out15 = true;
                    button11.BackColor = Color.Lime;
                }
                else
                {
                    state_outputs = state_outputs - 16384;
                    lpcie.Errs err = hnd.AsyncOutDig(state_outputs, 0);
                    if (err != lpcie.Errs.OK)
                    {
                        MessageBox.Show(X502.GetErrorString(err), "Ошибка асинхронного вывода на цифровой выход №15",
                                   MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    out15 = false;
                    button11.BackColor = Color.Silver;
                }
            }
        }

        private void button10_Click(object sender, EventArgs e)
        {
            if (hnd != null)
            {
                if (out16 == false)
                {
                    state_outputs = state_outputs + 32768;
                    lpcie.Errs err = hnd.AsyncOutDig(state_outputs, 0);
                    if (err != lpcie.Errs.OK)
                    {
                        MessageBox.Show(X502.GetErrorString(err), "Ошибка асинхронного вывода на цифровой выход №16",
                                   MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    out16 = true;
                    button10.BackColor = Color.Lime;
                }
                else
                {
                    state_outputs = state_outputs - 32768;
                    lpcie.Errs err = hnd.AsyncOutDig(state_outputs, 0);
                    if (err != lpcie.Errs.OK)
                    {
                        MessageBox.Show(X502.GetErrorString(err), "Ошибка асинхронного вывода на цифровой выход №16",
                                   MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    out16 = false;
                    button10.BackColor = Color.Silver;
                }
            }
        }

        private void button18_Click(object sender, EventArgs e)
        {
            if(imp_mode==false)
            {
                UInt32 imp_length_val = Convert.ToUInt32(imp_length.Text, 10), period, delay;
                UInt32[] imp_array = new UInt32[imp_length_val];
                if (imp_state1.CheckState == CheckState.Checked)
                {
                    period = Convert.ToUInt32(imp_period1.Text, 10);
                    delay = Convert.ToUInt32(imp_delay1.Text, 10);
                    for (UInt32 j = delay; j < period + delay; j++)
                    {
                        imp_array[j] = imp_array[j] + 1;
                    }
                }
                if (imp_state2.CheckState == CheckState.Checked)
                {
                    period = Convert.ToUInt32(imp_period2.Text, 10);
                    delay = Convert.ToUInt32(imp_delay2.Text, 10);
                    for (UInt32 j = delay; j < period + delay; j++)
                    {
                        imp_array[j] = imp_array[j] + 2;
                    }
                }
                if (imp_state3.CheckState == CheckState.Checked)
                {
                    period = Convert.ToUInt32(imp_period3.Text, 10);
                    delay = Convert.ToUInt32(imp_delay3.Text, 10);
                    for (UInt32 j = delay; j < period + delay; j++)
                    {
                        imp_array[j] = imp_array[j] + 4;
                    }
                }
                if (imp_state4.CheckState == CheckState.Checked)
                {
                    period = Convert.ToUInt32(imp_period4.Text, 10);
                    delay = Convert.ToUInt32(imp_delay4.Text, 10);
                    for (UInt32 j = delay; j < period + delay; j++)
                    {
                        imp_array[j] = imp_array[j] + 8;
                    }
                }
                if (imp_state5.CheckState == CheckState.Checked)
                {
                    period = Convert.ToUInt32(imp_period5.Text, 10);
                    delay = Convert.ToUInt32(imp_delay5.Text, 10);
                    for (UInt32 j = delay; j < period + delay; j++)
                    {
                        imp_array[j] = imp_array[j] + 16;
                    }
                }
                if (imp_state6.CheckState == CheckState.Checked)
                {
                    period = Convert.ToUInt32(imp_period6.Text, 10);
                    delay = Convert.ToUInt32(imp_delay6.Text, 10);
                    for (UInt32 j = delay; j < period + delay; j++)
                    {
                        imp_array[j] = imp_array[j] + 32;
                    }
                }
                if (imp_state7.CheckState == CheckState.Checked)
                {
                    period = Convert.ToUInt32(imp_period7.Text, 10);
                    delay = Convert.ToUInt32(imp_delay7.Text, 10);
                    for (UInt32 j = delay; j < period + delay; j++)
                    {
                        imp_array[j] = imp_array[j] + 64;
                    }
                }
                if (imp_state8.CheckState == CheckState.Checked)
                {
                    period = Convert.ToUInt32(imp_period8.Text, 10);
                    delay = Convert.ToUInt32(imp_delay8.Text, 10);
                    for (UInt32 j = delay; j < period + delay; j++)
                    {
                        imp_array[j] = imp_array[j] + 128;
                    }
                }
                if (imp_state9.CheckState == CheckState.Checked)
                {
                    period = Convert.ToUInt32(imp_period9.Text, 10);
                    delay = Convert.ToUInt32(imp_delay9.Text, 10);
                    for (UInt32 j = delay; j < period + delay; j++)
                    {
                        imp_array[j] = imp_array[j] + 256;
                    }
                }
                if (imp_state10.CheckState == CheckState.Checked)
                {
                    period = Convert.ToUInt32(imp_period10.Text, 10);
                    delay = Convert.ToUInt32(imp_delay10.Text, 10);
                    for (UInt32 j = delay; j < period + delay; j++)
                    {
                        imp_array[j] = imp_array[j] + 512;
                    }
                }
                if (imp_state11.CheckState == CheckState.Checked)
                {
                    period = Convert.ToUInt32(imp_period11.Text, 10);
                    delay = Convert.ToUInt32(imp_delay11.Text, 10);
                    for (UInt32 j = delay; j < period + delay; j++)
                    {
                        imp_array[j] = imp_array[j] + 1024;
                    }
                }
                if (imp_state12.CheckState == CheckState.Checked)
                {
                    period = Convert.ToUInt32(imp_period12.Text, 10);
                    delay = Convert.ToUInt32(imp_delay12.Text, 10);
                    for (UInt32 j = delay; j < period + delay; j++)
                    {
                        imp_array[j] = imp_array[j] + 2048;
                    }
                }
                if (imp_state13.CheckState == CheckState.Checked)
                {
                    period = Convert.ToUInt32(imp_period13.Text, 10);
                    delay = Convert.ToUInt32(imp_delay13.Text, 10);
                    for (UInt32 j = delay; j < period + delay; j++)
                    {
                        imp_array[j] = imp_array[j] + 4096;
                    }
                }
                if (imp_state14.CheckState == CheckState.Checked)
                {
                    period = Convert.ToUInt32(imp_period14.Text, 10);
                    delay = Convert.ToUInt32(imp_delay14.Text, 10);
                    for (UInt32 j = delay; j < period + delay; j++)
                    {
                        imp_array[j] = imp_array[j] + 8192;
                    }
                }
                if (imp_state15.CheckState == CheckState.Checked)
                {
                    period = Convert.ToUInt32(imp_period15.Text, 10);
                    delay = Convert.ToUInt32(imp_delay15.Text, 10);
                    for (UInt32 j = delay; j < period + delay; j++)
                    {
                        imp_array[j] = imp_array[j] + 16384;
                    }
                }
                if (imp_state16.CheckState == CheckState.Checked)
                {
                    period = Convert.ToUInt32(imp_period16.Text, 10);
                    delay = Convert.ToUInt32(imp_delay16.Text, 10);
                    for (UInt32 j = delay; j < period + delay; j++)
                    {
                        imp_array[j] = imp_array[j] + 32768;
                    }
                }

                imp_mode = true;
                //запись в буфер платы (постоянный режим)
                if(imp_single.CheckState==CheckState.Checked)
                {
                    hnd.StreamsEnable(X502.Streams.DOUT);
                    hnd.PreloadStart();
                    hnd.PrepareData(null,null, imp_array,imp_length_val,X502.DacOutFlags.VOLT,null);
                    hnd.Send(imp_array, imp_length_val, 10);
                    hnd.StreamsStart();
                }
                else if(imp_single.CheckState == CheckState.Unchecked)
                {
                    hnd.StreamsEnable(X502.Streams.DOUT);
                    hnd.OutCycleLoadStart(imp_length_val);
                    hnd.Send(imp_array, imp_length_val, 10);
                    hnd.OutCycleSetup(X502.OutCycleFlags.FORCE);
                    hnd.StreamsStart();
                    updateControls();
                }
                
            }
            else
            {
                hnd.StreamsStop();
                hnd.AsyncOutDig(0,0);
                imp_mode = false;
                updateControls();


                imp_state1.CheckState = CheckState.Unchecked;
                imp_state2.CheckState = CheckState.Unchecked;
                imp_state3.CheckState = CheckState.Unchecked;
                imp_state4.CheckState = CheckState.Unchecked;
                imp_state5.CheckState = CheckState.Unchecked;
                imp_state6.CheckState = CheckState.Unchecked;
                imp_state7.CheckState = CheckState.Unchecked;
                imp_state8.CheckState = CheckState.Unchecked;
                imp_state9.CheckState = CheckState.Unchecked;
                imp_state10.CheckState = CheckState.Unchecked;
                imp_state11.CheckState = CheckState.Unchecked;
                imp_state12.CheckState = CheckState.Unchecked;
                imp_state13.CheckState = CheckState.Unchecked;
                imp_state14.CheckState = CheckState.Unchecked;
                imp_state15.CheckState = CheckState.Unchecked;
                imp_state16.CheckState = CheckState.Unchecked;
            }
            
        }

        private void imp_state1_CheckedChanged(object sender, EventArgs e)
        {
            if(imp_state1.CheckState==CheckState.Checked)
            {
                groupBoxOut1.BackColor = Color.LawnGreen;
            }
            else
            {
                groupBoxOut1.BackColor = Color.White;
            }
        }

        private void imp_state2_CheckedChanged(object sender, EventArgs e)
        {
            if (imp_state2.CheckState == CheckState.Checked)
            {
                groupBoxOut2.BackColor = Color.LawnGreen;
            }
            else
            {
                groupBoxOut2.BackColor = Color.White;
            }
        }

        private void imp_state3_CheckedChanged(object sender, EventArgs e)
        {
            if (imp_state3.CheckState == CheckState.Checked)
            {
                groupBoxOut3.BackColor = Color.LawnGreen;
            }
            else
            {
                groupBoxOut3.BackColor = Color.White;
            }
        }

        private void imp_state4_CheckedChanged(object sender, EventArgs e)
        {
            if (imp_state4.CheckState == CheckState.Checked)
            {
                groupBoxOut4.BackColor = Color.LawnGreen;
            }
            else
            {
                groupBoxOut4.BackColor = Color.White;
            }
        }

        private void imp_state5_CheckedChanged(object sender, EventArgs e)
        {
            if (imp_state5.CheckState == CheckState.Checked)
            {
                groupBoxOut5.BackColor = Color.LawnGreen;
            }
            else
            {
                groupBoxOut5.BackColor = Color.White;
            }
        }

        private void imp_state6_CheckedChanged(object sender, EventArgs e)
        {
            if (imp_state6.CheckState == CheckState.Checked)
            {
                groupBoxOut6.BackColor = Color.LawnGreen;
            }
            else
            {
                groupBoxOut6.BackColor = Color.White;
            }
        }

        private void imp_state7_CheckedChanged(object sender, EventArgs e)
        {
            if (imp_state7.CheckState == CheckState.Checked)
            {
                groupBoxOut7.BackColor = Color.LawnGreen;
            }
            else
            {
                groupBoxOut7.BackColor = Color.White;
            }
        }

        private void imp_state8_CheckedChanged(object sender, EventArgs e)
        {
            if (imp_state8.CheckState == CheckState.Checked)
            {
                groupBoxOut8.BackColor = Color.LawnGreen;
            }
            else
            {
                groupBoxOut8.BackColor = Color.White;
            }
        }

        private void imp_state9_CheckedChanged(object sender, EventArgs e)
        {
            if (imp_state9.CheckState == CheckState.Checked)
            {
                groupBoxOut9.BackColor = Color.LawnGreen;
            }
            else
            {
                groupBoxOut9.BackColor = Color.White;
            }
        }

        private void imp_state10_CheckedChanged(object sender, EventArgs e)
        {
            if (imp_state10.CheckState == CheckState.Checked)
            {
                groupBoxOut10.BackColor = Color.LawnGreen;
            }
            else
            {
                groupBoxOut10.BackColor = Color.White;
            }
        }

        private void imp_state11_CheckedChanged(object sender, EventArgs e)
        {
            if (imp_state11.CheckState == CheckState.Checked)
            {
                groupBoxOut11.BackColor = Color.LawnGreen;
            }
            else
            {
                groupBoxOut11.BackColor = Color.White;
            }
        }

        private void imp_state12_CheckedChanged(object sender, EventArgs e)
        {
            if (imp_state12.CheckState == CheckState.Checked)
            {
                groupBoxOut12.BackColor = Color.LawnGreen;
            }
            else
            {
                groupBoxOut12.BackColor = Color.White;
            }
        }

        private void imp_state13_CheckedChanged(object sender, EventArgs e)
        {
            if (imp_state13.CheckState == CheckState.Checked)
            {
                groupBoxOut13.BackColor = Color.LawnGreen;
            }
            else
            {
                groupBoxOut13.BackColor = Color.White;
            }
        }

        private void imp_state14_CheckedChanged(object sender, EventArgs e)
        {
            if (imp_state14.CheckState == CheckState.Checked)
            {
                groupBoxOut14.BackColor = Color.LawnGreen;
            }
            else
            {
                groupBoxOut14.BackColor = Color.White;
            }
        }

        private void imp_state15_CheckedChanged(object sender, EventArgs e)
        {
            if (imp_state15.CheckState == CheckState.Checked)
            {
                groupBoxOut15.BackColor = Color.LawnGreen;
            }
            else
            {
                groupBoxOut15.BackColor = Color.White;
            }
        }

        private void imp_state16_CheckedChanged(object sender, EventArgs e)
        {
            if (imp_state16.CheckState == CheckState.Checked)
            {
                groupBoxOut16.BackColor = Color.LawnGreen;
            }
            else
            {
                groupBoxOut16.BackColor = Color.White;
            }
        }
    }
}
