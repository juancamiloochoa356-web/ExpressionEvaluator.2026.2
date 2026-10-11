
using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using Backend;

namespace Frontend.Windows
{
    public partial class Form1 : Form
    {
        private TextBox pantalla = new TextBox();

        public Form1()
        {
            InitializeComponent();
            ConstruirCalculadora();
        }

        private void ConstruirCalculadora()
        {
            Text = "Functions Evaluator";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(470, 340);
            BackColor = Color.Black;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;

            pantalla = new TextBox
            {
                Location = new Point(10, 10),
                Size = new Size(440, 42),
                Font = new Font("Arial", 19),
                BackColor = Color.Green,
                ForeColor = Color.White,
                ReadOnly = true,
                TextAlign = HorizontalAlignment.Left
            };

            Controls.Add(pantalla);

            string[,] botones =
            {
                { "7", "8", "9", "(", ")", "Delete" },
                { "4", "5", "6", "*", "/", "Clear" },
                { "1", "2", "3", "+", "-", "^" },
                { "0", ".", "=", "", "", "" }
            };

            int xInicial = 10;
            int yInicial = 65;
            int ancho = 55;
            int alto = 52;
            int espacio = 7;

            for (int fila = 0; fila < 4; fila++)
            {
                for (int columna = 0; columna < 6; columna++)
                {
                    string texto = botones[fila, columna];

                    if (string.IsNullOrEmpty(texto))
                        continue;

                    int anchoBoton = ancho;

                    if (texto == "Delete" || texto == "Clear")
                        anchoBoton = 132;

                    if (texto == "=")
                        anchoBoton = 269;

                    Button boton = new Button
                    {
                        Text = texto,
                        Font = new Font("Arial", 15),
                        FlatStyle = FlatStyle.Flat,
                        Size = new Size(anchoBoton, alto),
                        Location = new Point(
                            xInicial + columna * (ancho + espacio),
                            yInicial + fila * (alto + espacio)),
                        BackColor = EsOperacion(texto)
                            ? Color.DarkOrange
                            : Color.White,
                        ForeColor = Color.Black
                    };

                    boton.Click += Boton_Click;
                    Controls.Add(boton);
                }
            }
        }

        private bool EsOperacion(string texto)
        {
            return texto == "(" || texto == ")" ||
                   texto == "*" || texto == "/" ||
                   texto == "+" || texto == "-" ||
                   texto == "^" || texto == "=" ||
                   texto == "Delete" || texto == "Clear";
        }

        private void Boton_Click(object? sender, EventArgs e)
        {
            if (sender is not Button boton)
                return;

            string valor = boton.Text;

            if (valor == "Clear")
            {
                pantalla.Clear();
                return;
            }

            if (valor == "Delete")
            {
                if (pantalla.Text.Length > 0)
                    pantalla.Text = pantalla.Text.Substring(
                        0, pantalla.Text.Length - 1);

                return;
            }

            if (valor == "=")
            {
                try
                {
                    double resultado =
                        ExpressionEvaluator.Evaluate(pantalla.Text);

                    pantalla.Text = resultado.ToString(
                        "G15", CultureInfo.InvariantCulture);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        ex.Message,
                        "Error en la expresion",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }

                return;
            }

            pantalla.Text += valor;
        }
    }
}
