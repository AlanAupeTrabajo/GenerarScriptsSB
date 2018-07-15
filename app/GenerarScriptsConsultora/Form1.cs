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

        private List<string> GetPaises()
        {
            var i = tcBD.SelectedIndex;
            TabControl tab = new TabControl();
            List<string> paises = new List<string>();

            if (i == 0) tab = tcTiposBD57;

            foreach (Control control in tab.SelectedTab.Controls)
            {
                if (control.GetType().ToString() == "System.Windows.Forms.CheckBox")
                {
                    CheckBox chkPais = (CheckBox)control;
                    if (chkPais.Checked)
                    {
                        paises.Add(chkPais.Tag.ToString());
                    }
                }
            }

            return paises;
        }

        private void btnGenerar_Click(object sender, EventArgs e)
        {
            var i = tcBD.SelectedIndex;
            TabControl tab = new TabControl();
            List<string> paises = new List<string>();

            if (i == 0) tab = tcTiposBD57;

            var scriptIni = new StringBuilder();
            //int cantVacio = 0;
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

                                scriptIni.Append(lineaTxt);
                                scriptIni.AppendLine();
                            }
                    }
                }
            }

            var script = scriptIni.ToString();
            if (script == "") return;

            //Formatear(script);

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
        // no se utiliza es una demo
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

        #region Pase
        private void PaseBtnGenerar_Click(object sender, EventArgs e)
        {
            List<string> paises = GetPaises();

            string siglasEquipo = paseSiglasEquipo.Text.Trim();
            string urlScript = paseUrlScript.Text.Trim();

            if (siglasEquipo == "" || paises.Count == 0 || urlScript == "")
            {
                MessageBox.Show("Debe tener \n Seleccionar Paises \n Siglas del Equipo \n Ruta Script del Pase", "Generar Scripts", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            foreach (string file in Directory.EnumerateFiles(urlScript, "*.sql"))
            {
                //FormatoArchivo(file);
                ReadLine(file, paises);
            }

        }

        private void ReadLine(string direc, List<string> paises)
        {
            if (!File.Exists(direc))
                return;

            string siglasEquipo = paseSiglasEquipo.Text.Trim();
            siglasEquipo = siglasEquipo[0] == '_' ? siglasEquipo : ("_" + siglasEquipo);

            bool isPais = false;
            bool addPais = false;

            StringBuilder scriptGeneradoPase = new StringBuilder();

            using (StreamReader sr = new StreamReader(direc))
            {
                while (sr.Peek() >= 0)
                {
                    var linea = sr.ReadLine();
                    linea = linea.TrimEnd();
                    var lineaAux = linea.Trim().ToLower();
                    var pais = "";
                    if (lineaAux.Contains("use "))
                    {
                        pais = lineaAux.Split(' ')[1];
                        isPais = PaisesContains(paises, pais);
                        if (isPais)
                        {
                            addPais = true;
                        }
                    }

                    if (isPais)
                    {
                        if (addPais)
                        {
                            scriptGeneradoPase.Append("GO");
                            scriptGeneradoPase.AppendLine();
                            scriptGeneradoPase.Append("USE " + pais + siglasEquipo);
                            scriptGeneradoPase.AppendLine();
                            scriptGeneradoPase.Append("GO");
                            scriptGeneradoPase.AppendLine();
                            addPais = false;
                            continue;
                        }

                        scriptGeneradoPase.Append(linea);
                        scriptGeneradoPase.AppendLine();
                    }
                }
            }

            if (scriptGeneradoPase.ToString() == "")
                return;

            direc = NuevaRuta(direc, siglasEquipo);
            ExportarScriptEnDireccion(direc, scriptGeneradoPase);
        }

        private bool PaisesContains(List<string> paises, string paisbuscar)
        {
            bool isPais = false;
            foreach (var pais in paises)
            {
                var paistrim = pais.Trim().ToLower();
                paisbuscar = paisbuscar.Trim().ToLower();
                if (paisbuscar == paistrim
                    || paisbuscar == paistrim + ";"
                    || paisbuscar == "[" + paistrim + "]"
                    || paisbuscar == "[" + paistrim + "];"
                )
                {
                    isPais = true;
                    break;
                }
            }
            return isPais;
        }

        private string NuevaRuta(string direc, string siglas)
        {
            var list = direc.Split('\\');
            var nombre = list[list.Length - 1];
            var carpetaOri = direc.Substring(0, direc.Length - nombre.Length);
            var carpeta = paseUrlGenerado.Text.Trim();

            if (carpeta == "" || (carpeta == carpetaOri && carpetaOri != ""))
            {
                carpeta = carpetaOri + siglas;
            }

            if (!Directory.Exists(carpeta))
            {
                Directory.CreateDirectory(carpeta);
            }
            paseUrlGenerado.Text = carpeta;

            direc = carpeta + "\\" + nombre;
            return direc;
        }

        private void ExportarScriptEnDireccion(string direc, StringBuilder script)
        {
            try
            {
                if (File.Exists(direc))
                {
                    File.Delete(direc);
                }

                using (FileStream fs = File.Create(direc))
                {
                    Byte[] info = new UTF8Encoding(true).GetBytes(script.ToString());
                    fs.Write(info, 0, info.Length);
                }

            }
            catch (Exception ex)
            {
            }
        }

        #endregion

        #region Extra

        private void FormatoArchivo(string direc)
        {
            if (!File.Exists(direc))
                return;


        }

        private string GetNombreArchivo(string direc)
        {

            return direc;
        }

        private void Formatear(string cadena)
        {
            var txtBuild = new StringBuilder();

            cadena = Limpiar(cadena);
            var lista = cadena.Split(';');
            
            foreach (var fila in lista)
            {
                if (fila.Contains("||"))
                {
                    continue;
                }

                var lista2 = fila.Replace("))", "||").Split('|');
                foreach (var campo in lista2)
                {
                    if (campo == "" || campo.Contains("HasColumn"))
                    {
                        continue;
                    }
                    txtBuild.Append(Remplazar(campo) + ";");
                    txtBuild.AppendLine();
                }
            }

            txtResultado.Text = txtBuild.ToString();

        }

        private string Limpiar(string cadena)
        {
            cadena = cadena
                .Replace(" this.", "")
                .Replace(" ", "")
                .Replace("\n", "")
                .Replace("\r", "")
                .Replace("\t", "");

            if (cadena.Contains(" "))
            {
                cadena = Limpiar(cadena);
            }
            return cadena;
        }
        private string Remplazar(string cadena)
        {
            cadena = cadena
                .Replace("row[", "")
                .Replace("]", "")
                .Replace("Convert.", "row.");

            return cadena;
        }
        #endregion
    }
}
