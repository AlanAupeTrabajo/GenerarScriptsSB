using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace GenerarScriptsConsultora
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnGenerar_Click(object sender, EventArgs e)
        {
            var i = tcBD.SelectedIndex;
            TabControl tab = new TabControl();
            List<string> paises = new List<string>();

            if (i == 0) tab = tcTiposBD57;


            var scriptIni = new StringBuilder();
            int cantVacio = 0;
            string lineaTxt = "";
            foreach (Control control in tab.SelectedTab.Controls)
            {
                if (control.GetType().ToString() == "System.Windows.Forms.CheckBox")
                {
                    CheckBox chkPais = (CheckBox)control;
                    if (chkPais.Checked)
                    {
                        paises.Add(chkPais.Tag.ToString());
                    }
                    continue;
                }

                if (control.GetType().ToString() == "System.Windows.Forms.TextBox")
                {
                    TextBox txtScript = (TextBox)control;
                    if (txtScript.Tag != null && txtScript.Tag.ToString() == "txtScript")
                    {
                        //script = txtScript.Text.Trim();

                        if (txtScript.Lines.Length > 0)
                            foreach (var linea in txtScript.Lines)
                            {
                                lineaTxt = linea.TrimEnd();

                                cantVacio += lineaTxt == "" ? 1 : 0;

                                if (cantVacio == 2)
                                {
                                    cantVacio = 0;
                                    continue;
                                }

                                scriptIni.Append(lineaTxt);
                                scriptIni.AppendLine();
                            }
                    }
                }
            }

            var script = scriptIni.ToString();
            if (script == "") return;

            StringBuilder scriptGenerado = new StringBuilder();
            scriptGenerado.Append("GO");
            scriptGenerado.AppendLine();
            foreach (string pais in paises)
            {
                scriptGenerado.Append("USE " + pais);
                scriptGenerado.AppendLine();
                scriptGenerado.Append("GO");
                scriptGenerado.AppendLine();
                scriptGenerado.Append(script);
                scriptGenerado.AppendLine();
                scriptGenerado.Append("GO");
                scriptGenerado.AppendLine();
            }

            txtResultado.Text = scriptGenerado.ToString();

            //foreach (Control control in tab.SelectedTab.Controls)
            //{
            //    if (control.GetType().ToString() == "System.Windows.Forms.TextBox")
            //    {
            //        TextBox txtResult = (TextBox)control;
            //        if (txtResult.Tag != null && txtResult.Tag.ToString() == "txtResultado")
            //        {
            //            txtResult.Text = scriptGenerado.ToString();
            //        }
            //    }
            //}
        }

        private void btnSalir_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnExportar_Click(object sender, EventArgs e)
        {
            if (txtResultado.Text.Trim() == "")
            {
                return;
            }

            var i = tcBD.SelectedIndex;
            TabControl tab = new TabControl();
            if (i == 0) tab = tcTiposBD57;

            foreach (Control control in tab.SelectedTab.Controls)
            {
                if (control.GetType().ToString() == "System.Windows.Forms.TextBox")
                {
                    TextBox txtResult = (TextBox)control;
                    if (txtResult.Tag != null && txtResult.Tag.ToString() == "txtResultado")
                    {
                        Clipboard.SetDataObject(txtResult.Text);
                        if (txtDireccion.Text.Trim() == "")
                        {
                            MessageBox.Show("Se copió al portapapeles.", "Generar Scripts", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }

                        break;
                    }
                }
            }

            if (txtDireccion.Text.Trim() != "")
            {
                ExportarScriptEnDireccion();
            }

        }

        private void ExportarScriptEnDireccion()
        {
            try
            {

                var direc = txtDireccion.Text.Trim();
                if (File.Exists(direc))
                {
                    MessageBox.Show("Ya existe un archivo con el mismo nombre \n " + direc, "Generar Scripts", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    // Create the file.
                    using (FileStream fs = File.Create(direc))
                    {
                        Byte[] info = new UTF8Encoding(true).GetBytes(txtResultado.Text);
                        fs.Write(info, 0, info.Length);
                    }

                    MessageBox.Show("Ya esta creado el archivo \n " + direc, "Generar Scripts", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

            }
            catch (Exception ex)
            {
            }
        }

        // SaveFile(Encoding.UTF8);
        private static void SaveFile(Encoding encoding)
        {
            Console.WriteLine("Encoding: {0}", encoding.EncodingName);
            string filename = string.Concat(@"c:\file-", encoding.EncodingName, ".txt");
            StreamWriter streamWriter = new StreamWriter(filename, false, encoding);
            streamWriter.WriteLine("I am feeling great.");
            streamWriter.Close();
        }

        private void btnLimpiar_Click(object sender, EventArgs e)
        {
            textBox1.Text = string.Empty; txtResultado.Text = string.Empty;
        }

    }
}
