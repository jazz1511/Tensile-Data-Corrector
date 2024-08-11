using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Runtime.ConstrainedExecution;

namespace Tensile_Data_Correction
{
    public partial class Form1 : Form
    {
        string input_file = "";
        string input_file_location = "";
        string input_file_name = "";
        string output_file = "";
        string output_file_location = "";
        string output_file_name = "";
        bool store_peak_data;
        bool is_percentage_strain;
        int peak_window_size;
        int peak_threshold;
        int peak_for_YS;
        int peak_for_UTS;

        List<double> strain_orig = new List<double>();
        List<double> stress_orig = new List<double>();
        List<double> strain_corr = new List<double>();
        List<double> stress_corr = new List<double>();
        List<double> peak_counter = new List<double>();
        List<double> peak_counter_above_threshold = new List<double>();
        List<double> peak_counter_below_threshold = new List<double>();

        OpenFileDialog ofd = new OpenFileDialog
        {
            InitialDirectory = @"C:\",
            Title = "Choose Input CSV File",

            CheckFileExists = true,
            CheckPathExists = true,

            DefaultExt = "csv",
            Filter = "CSV files (*.csv)|*.csv",

            RestoreDirectory = true,
            ReadOnlyChecked = true,
            ShowReadOnly = true
        };

        FolderBrowserDialog fbd = new FolderBrowserDialog
        {
            ShowNewFolderButton = true,
            InitialDirectory = @"C:\"
        };

        public Form1()
        {
            InitializeComponent();

            checkBox2.Enabled = false;
            checkBox3.Enabled = false;
            label4.Enabled = false;
            label5.Enabled = false;
            label6.Enabled = false;
            label7.Enabled = false;
            textBox4.Enabled = false;
            textBox5.Enabled = false;
            textBox6.Enabled = false;
            textBox7.Enabled = false;

            label8.Enabled = false;
            label9.Enabled = false;
            label10.Enabled = false;
            label11.Enabled = false;
            label12.Enabled = false;
            label13.Enabled = false;
            label14.Enabled = false;
            label15.Enabled = false;
            label16.Enabled = false;
            label17.Enabled = false;
            label18.Enabled = false;
            label19.Enabled = false;
            label20.Enabled = false;
            label21.Enabled = false;

            checkBox4.Enabled = false;
            checkBox5.Enabled = false;
            checkBox6.Enabled = false;

            formsPlot1.Reset();
            formsPlot1.Plot.Axes.Left.TickLabelStyle.FontSize = 12;
            formsPlot1.Plot.Axes.Bottom.TickLabelStyle.FontSize = 12;
            formsPlot1.Plot.Axes.Left.Label.Text = "Stress (in MPa)";
            formsPlot1.Plot.Axes.Left.Label.FontSize = 18;
            if (checkBox3.Checked)
                formsPlot1.Plot.Axes.Bottom.Label.Text = "Strain (in %)";
            else
                formsPlot1.Plot.Axes.Bottom.Label.Text = "Strain";
            formsPlot1.Plot.Axes.Bottom.Label.FontSize = 18;
            formsPlot1.Plot.ShowLegend();
            formsPlot1.Plot.Legend.Font.Size = 18;
            formsPlot1.Refresh();

            formsPlot1.Enabled = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = ofd.FileName;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                textBox2.Text = fbd.SelectedPath;
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (!checkBox1.Checked)
            {
                checkBox2.Enabled = false;
                checkBox3.Enabled = false;
                label4.Enabled = false;
                label5.Enabled = false;
                label6.Enabled = false;
                label7.Enabled = false;
                textBox4.Enabled = false;
                textBox5.Enabled = false;
                textBox6.Enabled = false;
                textBox7.Enabled = false;
            }
            else
            {
                checkBox2.Enabled = true;
                checkBox3.Enabled = true;
                label4.Enabled = true;
                label5.Enabled = true;
                label6.Enabled = true;
                label7.Enabled = true;
                textBox4.Enabled = true;
                textBox5.Enabled = true;
                textBox6.Enabled = true;
                textBox7.Enabled = true;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == "")
            {
                MessageBox.Show("Input data file not selected !!!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            else
            {
                input_file = textBox1.Text;
                if (Path.IsPathFullyQualified(input_file))
                {
                    input_file_location = Path.GetDirectoryName(input_file);

                    if (Path.GetFileName(input_file) == "")
                    {
                        MessageBox.Show("Path for Input data file is incorrect !!!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    input_file_name = Path.GetFileName(input_file);
                }
                else
                {
                    MessageBox.Show("Path for Input data file does not exist !!!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            if (textBox2.Text == "")
                output_file_location = input_file_location;
            else
            {
                output_file_location = textBox2.Text;
                if (!Path.IsPathFullyQualified(output_file_location))
                {
                    MessageBox.Show(" Path for Output data file does not exist !!!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            if (textBox3.Text == "")
                output_file_name = "output.csv";
            else
                output_file_name = textBox3.Text;

            output_file = Path.Combine(output_file_location, output_file_name);

            if (checkBox2.Checked)
                store_peak_data = true;
            else
                store_peak_data = false;

            if (checkBox3.Checked)
                is_percentage_strain = true;
            else
                is_percentage_strain = false;

            // TODO: Deal with potential errors
            peak_window_size = Int32.Parse(textBox4.Text);
            peak_threshold = Int32.Parse(textBox5.Text);
            peak_for_YS = Int32.Parse(textBox6.Text);
            if (textBox7.Text == "")
                peak_for_UTS = -1;
            else
                peak_for_UTS = Int32.Parse(textBox7.Text);

            // Run program
            Process process = new Process();
            process.StartInfo.FileName = "exec.exe";
            process.StartInfo.Arguments =
                "-in=" + "\"" + input_file + "\"" +
                " -out=" + "\"" + output_file + "\"" +
                " -pfn=" + "\"" + Path.Combine(output_file_location, "peak_data.csv") + "\"" +
                " -lfn=" + "\"" + Path.Combine(output_file_location, "log_data.txt") + "\"" +
                " -spd=" + store_peak_data +
                " -ips=" + is_percentage_strain +
                " -pws=" + peak_window_size +
                " -pt=" + peak_threshold +
                " -pys=" + peak_for_YS +
                " -puts=" + peak_for_UTS;
            process.StartInfo.CreateNoWindow = true;
            process.Start();
            process.WaitForExit();

            // Read the generated output file
            double YS = 0.0, UTS = 0.0, EL_YS = 0.0, EL_UTS = 0.0, EL_break = 0.0, E_orig = 0.0, E_corr = 0.0;
            strain_orig.Clear();
            stress_orig.Clear();
            strain_corr.Clear();
            stress_corr.Clear();

            StreamReader reader = new StreamReader(output_file);
            string line;

            if (!reader.EndOfStream)
            {
                line = reader.ReadLine() ?? "";
                YS = double.Parse(line.Split(',')[1]);
                line = reader.ReadLine() ?? "";
                UTS = double.Parse(line.Split(",")[1]);
                line = reader.ReadLine() ?? "";
                EL_YS = double.Parse(line.Split(",")[1]);
                line = reader.ReadLine() ?? "";
                EL_UTS = double.Parse(line.Split(",")[1]);
                line = reader.ReadLine() ?? "";
                EL_break = double.Parse(line.Split(",")[1]);
                line = reader.ReadLine() ?? "";
                E_orig = double.Parse(line.Split(",")[1]);
                line = reader.ReadLine() ?? "";
                E_corr = double.Parse(line.Split(",")[1]);

                line = reader.ReadLine() ?? "";
                line = reader.ReadLine() ?? "";
                line = reader.ReadLine() ?? "";
                line = reader.ReadLine() ?? "";
                line = reader.ReadLine() ?? "";
            }

            while (!reader.EndOfStream)
            {
                line = reader.ReadLine() ?? "";

                if (line == "")
                    continue;

                var values = line.Split(",");

                if (values.Length == 5)
                {
                    strain_orig.Add(double.Parse(values[0]));
                    stress_orig.Add(double.Parse(values[1]));
                    strain_corr.Add(double.Parse(values[3]));
                    stress_corr.Add(double.Parse(values[4]));
                }
                else if (values.Length == 2)
                {
                    strain_orig.Add(double.Parse(values[0]));
                    stress_orig.Add(double.Parse(values[1]));
                }
                else
                    continue;
            }
            reader.Close();

            // Read the generated peak file
            StreamReader reader_peak = new StreamReader(Path.Combine(output_file_location, "peak_data.csv"));
            peak_counter.Clear();
            peak_counter_above_threshold.Clear();
            peak_counter_below_threshold.Clear();
            while (!reader_peak.EndOfStream)
            {
                line = reader_peak.ReadLine() ?? "";

                if (line == "")
                    continue;

                var values = line.Split(",");

                if (values.Length < 3)
                    continue;

                peak_counter.Add(double.Parse(values[2]));

                if (double.Parse(values[2]) >= peak_threshold)
                    peak_counter_above_threshold.Add(double.Parse(values[2]));
                else
                    peak_counter_above_threshold.Add(0);

                if (double.Parse(values[2]) < peak_threshold)
                    peak_counter_below_threshold.Add(double.Parse(values[2]));
                else
                    peak_counter_below_threshold.Add(0);
            }
            reader_peak.Close();

            // Show and Plot the results

            label8.Enabled = true;
            label9.Enabled = true;
            label10.Enabled = true;
            label11.Enabled = true;
            label12.Enabled = true;
            label13.Enabled = true;
            label14.Enabled = true;
            label15.Enabled = true;
            label16.Enabled = true;
            label17.Enabled = true;
            label18.Enabled = true;
            label19.Enabled = true;
            label20.Enabled = true;
            label21.Enabled = true;
            checkBox4.Enabled = true;
            checkBox5.Enabled = true;
            checkBox6.Enabled = true;

            label15.Text = YS.ToString();
            label16.Text = UTS.ToString();
            label17.Text = EL_YS.ToString();
            label18.Text = EL_UTS.ToString();
            label19.Text = EL_break.ToString();
            label20.Text = E_orig.ToString();
            label21.Text = E_corr.ToString();

            formsPlot1.Enabled = true;

            formsPlot1.Reset();
            ScottPlot.Plottables.Scatter plot1, plot2, plot3, plot4;
            if (checkBox4.Checked)
            {
                plot1 = formsPlot1.Plot.Add.Scatter(strain_orig, stress_orig);
                plot1.Label = "Original Data";
            }
            if (checkBox5.Checked)
            {
                plot2 = formsPlot1.Plot.Add.Scatter(strain_corr, stress_corr);
                plot2.Label = "Corrected Data";
            }
            if (checkBox2.Checked && checkBox6.Checked)
            {
                plot3 = formsPlot1.Plot.Add.Scatter(strain_orig, peak_counter_above_threshold);
                plot3.Label = "Peaks above threshold";
                plot4 = formsPlot1.Plot.Add.Scatter(strain_orig, peak_counter_below_threshold);
                plot4.Label = "Peaks below threshold";
            }
            formsPlot1.Plot.Axes.AutoScale();
            formsPlot1.Plot.Axes.Left.TickLabelStyle.FontSize = 12;
            formsPlot1.Plot.Axes.Bottom.TickLabelStyle.FontSize = 12;
            formsPlot1.Plot.Axes.Left.Label.Text = "Stress (in MPa)";
            formsPlot1.Plot.Axes.Left.Label.FontSize = 18;
            if (checkBox3.Checked)
                formsPlot1.Plot.Axes.Bottom.Label.Text = "Strain (in %)";
            else
                formsPlot1.Plot.Axes.Bottom.Label.Text = "Strain";
            formsPlot1.Plot.Axes.Bottom.Label.FontSize = 18;
            formsPlot1.Plot.ShowLegend();
            formsPlot1.Plot.Legend.Font.Size = 18;
            formsPlot1.Refresh();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Process.Start("explorer", "\"" + output_file + "\"");
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Process.Start("explorer", "\"" + Path.Combine(output_file_location, "peak_data.csv") + "\"");
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Process.Start("explorer", "\"" + Path.Combine(output_file_location, "log_data.txt") + "\"");
        }

        private void button7_Click(object sender, EventArgs e)
        {
            Process process = new Process();
            process.StartInfo.FileName = "exec.exe";
            process.StartInfo.Arguments = "-?";
            process.Start();
            process.WaitForExitAsync();
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            formsPlot1.Reset();
            ScottPlot.Plottables.Scatter plot1, plot2, plot3, plot4;
            if (checkBox4.Checked)
            {
                plot1 = formsPlot1.Plot.Add.Scatter(strain_orig, stress_orig);
                plot1.Label = "Original Data";
            }
            if (checkBox5.Checked)
            {
                plot2 = formsPlot1.Plot.Add.Scatter(strain_corr, stress_corr);
                plot2.Label = "Corrected Data";
            }
            if (checkBox2.Checked && checkBox6.Checked)
            {
                plot3 = formsPlot1.Plot.Add.Scatter(strain_orig, peak_counter_above_threshold);
                plot3.Label = "Peaks above threshold";
                plot4 = formsPlot1.Plot.Add.Scatter(strain_orig, peak_counter_below_threshold);
                plot4.Label = "Peaks below threshold";
            }
            formsPlot1.Plot.Axes.AutoScale();
            formsPlot1.Plot.Axes.Left.TickLabelStyle.FontSize = 12;
            formsPlot1.Plot.Axes.Bottom.TickLabelStyle.FontSize = 12;
            formsPlot1.Plot.Axes.Left.Label.Text = "Stress (in MPa)";
            formsPlot1.Plot.Axes.Left.Label.FontSize = 18;
            if (checkBox3.Checked)
                formsPlot1.Plot.Axes.Bottom.Label.Text = "Strain (in %)";
            else
                formsPlot1.Plot.Axes.Bottom.Label.Text = "Strain";
            formsPlot1.Plot.Axes.Bottom.Label.FontSize = 18;
            formsPlot1.Plot.ShowLegend();
            formsPlot1.Plot.Legend.Font.Size = 18;
            formsPlot1.Refresh();
        }

        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            formsPlot1.Reset();
            ScottPlot.Plottables.Scatter plot1, plot2, plot3, plot4;
            if (checkBox4.Checked)
            {
                plot1 = formsPlot1.Plot.Add.Scatter(strain_orig, stress_orig);
                plot1.Label = "Original Data";
            }
            if (checkBox5.Checked)
            {
                plot2 = formsPlot1.Plot.Add.Scatter(strain_corr, stress_corr);
                plot2.Label = "Corrected Data";
            }
            if (checkBox2.Checked && checkBox6.Checked)
            {
                plot3 = formsPlot1.Plot.Add.Scatter(strain_orig, peak_counter_above_threshold);
                plot3.Label = "Peaks above threshold";
                plot4 = formsPlot1.Plot.Add.Scatter(strain_orig, peak_counter_below_threshold);
                plot4.Label = "Peaks below threshold";
            }
            formsPlot1.Plot.Axes.AutoScale();
            formsPlot1.Plot.Axes.Left.TickLabelStyle.FontSize = 12;
            formsPlot1.Plot.Axes.Bottom.TickLabelStyle.FontSize = 12;
            formsPlot1.Plot.Axes.Left.Label.Text = "Stress (in MPa)";
            formsPlot1.Plot.Axes.Left.Label.FontSize = 18;
            if (checkBox3.Checked)
                formsPlot1.Plot.Axes.Bottom.Label.Text = "Strain (in %)";
            else
                formsPlot1.Plot.Axes.Bottom.Label.Text = "Strain";
            formsPlot1.Plot.Axes.Bottom.Label.FontSize = 18;
            formsPlot1.Plot.ShowLegend();
            formsPlot1.Plot.Legend.Font.Size = 18;
            formsPlot1.Refresh();
        }

        private void checkBox6_CheckedChanged(object sender, EventArgs e)
        {
            formsPlot1.Reset();
            ScottPlot.Plottables.Scatter plot1, plot2, plot3, plot4;
            if (checkBox4.Checked)
            {
                plot1 = formsPlot1.Plot.Add.Scatter(strain_orig, stress_orig);
                plot1.Label = "Original Data";
            }
            if (checkBox5.Checked)
            {
                plot2 = formsPlot1.Plot.Add.Scatter(strain_corr, stress_corr);
                plot2.Label = "Corrected Data";
            }
            if (checkBox2.Checked && checkBox6.Checked)
            {
                plot3 = formsPlot1.Plot.Add.Scatter(strain_orig, peak_counter_above_threshold);
                plot3.Label = "Peaks above threshold";
                plot4 = formsPlot1.Plot.Add.Scatter(strain_orig, peak_counter_below_threshold);
                plot4.Label = "Peaks below threshold";
            }
            formsPlot1.Plot.Axes.AutoScale();
            formsPlot1.Plot.Axes.Left.TickLabelStyle.FontSize = 12;
            formsPlot1.Plot.Axes.Bottom.TickLabelStyle.FontSize = 12;
            formsPlot1.Plot.Axes.Left.Label.Text = "Stress (in MPa)";
            formsPlot1.Plot.Axes.Left.Label.FontSize = 18;
            if (checkBox3.Checked)
                formsPlot1.Plot.Axes.Bottom.Label.Text = "Strain (in %)";
            else
                formsPlot1.Plot.Axes.Bottom.Label.Text = "Strain";
            formsPlot1.Plot.Axes.Bottom.Label.FontSize = 18;
            formsPlot1.Plot.ShowLegend();
            formsPlot1.Plot.Legend.Font.Size = 18;
            formsPlot1.Refresh();
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            formsPlot1.Reset();
            formsPlot1.Plot.Axes.Left.TickLabelStyle.FontSize = 12;
            formsPlot1.Plot.Axes.Bottom.TickLabelStyle.FontSize = 12;
            formsPlot1.Plot.Axes.Left.Label.Text = "Stress (in MPa)";
            formsPlot1.Plot.Axes.Left.Label.FontSize = 18;
            if (checkBox3.Checked)
                formsPlot1.Plot.Axes.Bottom.Label.Text = "Strain (in %)";
            else
                formsPlot1.Plot.Axes.Bottom.Label.Text = "Strain";
            formsPlot1.Plot.Axes.Bottom.Label.FontSize = 18;
            formsPlot1.Plot.ShowLegend();
            formsPlot1.Plot.Legend.Font.Size = 18;
            formsPlot1.Refresh();
            formsPlot1.Enabled = false;
            checkBox4.Enabled = false;
            checkBox5.Enabled = false;
            checkBox6.Enabled = false;
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            formsPlot1.Reset();
            formsPlot1.Plot.Axes.Left.TickLabelStyle.FontSize = 12;
            formsPlot1.Plot.Axes.Bottom.TickLabelStyle.FontSize = 12;
            formsPlot1.Plot.Axes.Left.Label.Text = "Stress (in MPa)";
            formsPlot1.Plot.Axes.Left.Label.FontSize = 18;
            if (checkBox3.Checked)
                formsPlot1.Plot.Axes.Bottom.Label.Text = "Strain (in %)";
            else
                formsPlot1.Plot.Axes.Bottom.Label.Text = "Strain";
            formsPlot1.Plot.Axes.Bottom.Label.FontSize = 18;
            formsPlot1.Plot.ShowLegend();
            formsPlot1.Plot.Legend.Font.Size = 18;
            formsPlot1.Refresh();
            formsPlot1.Enabled = false;
            checkBox4.Enabled = false;
            checkBox5.Enabled = false;
            checkBox6.Enabled = false;

            label15.Text = string.Empty;
            label16.Text = string.Empty;
            label17.Text = string.Empty;
            label18.Text = string.Empty;
            label19.Text = string.Empty;
            label20.Text = string.Empty;
            label21.Text = string.Empty;

            label15.Enabled = false;
            label16.Enabled = false;
            label17.Enabled = false;
            label18.Enabled = false;
            label19.Enabled = false;
            label20.Enabled = false;
            label21.Enabled = false;
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            formsPlot1.Reset();
            formsPlot1.Plot.Axes.Left.TickLabelStyle.FontSize = 12;
            formsPlot1.Plot.Axes.Bottom.TickLabelStyle.FontSize = 12;
            formsPlot1.Plot.Axes.Left.Label.Text = "Stress (in MPa)";
            formsPlot1.Plot.Axes.Left.Label.FontSize = 18;
            if (checkBox3.Checked)
                formsPlot1.Plot.Axes.Bottom.Label.Text = "Strain (in %)";
            else
                formsPlot1.Plot.Axes.Bottom.Label.Text = "Strain";
            formsPlot1.Plot.Axes.Bottom.Label.FontSize = 18;
            formsPlot1.Plot.ShowLegend();
            formsPlot1.Plot.Legend.Font.Size = 18;
            formsPlot1.Refresh();
            formsPlot1.Enabled = false;
            checkBox4.Enabled = false;
            checkBox5.Enabled = false;
            checkBox6.Enabled = false;
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            formsPlot1.Reset();
            formsPlot1.Plot.Axes.Left.TickLabelStyle.FontSize = 12;
            formsPlot1.Plot.Axes.Bottom.TickLabelStyle.FontSize = 12;
            formsPlot1.Plot.Axes.Left.Label.Text = "Stress (in MPa)";
            formsPlot1.Plot.Axes.Left.Label.FontSize = 18;
            if (checkBox3.Checked)
                formsPlot1.Plot.Axes.Bottom.Label.Text = "Strain (in %)";
            else
                formsPlot1.Plot.Axes.Bottom.Label.Text = "Strain";
            formsPlot1.Plot.Axes.Bottom.Label.FontSize = 18;
            formsPlot1.Plot.ShowLegend();
            formsPlot1.Plot.Legend.Font.Size = 18;
            formsPlot1.Refresh();
            formsPlot1.Enabled = false;
            checkBox4.Enabled = false;
            checkBox5.Enabled = false;
            checkBox6.Enabled = false;
        }

        private void textBox6_TextChanged(object sender, EventArgs e)
        {
            formsPlot1.Reset();
            formsPlot1.Plot.Axes.Left.TickLabelStyle.FontSize = 12;
            formsPlot1.Plot.Axes.Bottom.TickLabelStyle.FontSize = 12;
            formsPlot1.Plot.Axes.Left.Label.Text = "Stress (in MPa)";
            formsPlot1.Plot.Axes.Left.Label.FontSize = 18;
            if (checkBox3.Checked)
                formsPlot1.Plot.Axes.Bottom.Label.Text = "Strain (in %)";
            else
                formsPlot1.Plot.Axes.Bottom.Label.Text = "Strain";
            formsPlot1.Plot.Axes.Bottom.Label.FontSize = 18;
            formsPlot1.Plot.ShowLegend();
            formsPlot1.Plot.Legend.Font.Size = 18;
            formsPlot1.Refresh();
            formsPlot1.Enabled = false;
            checkBox4.Enabled = false;
            checkBox5.Enabled = false;
            checkBox6.Enabled = false;

            label15.Text = string.Empty;
            label16.Text = string.Empty;
            label17.Text = string.Empty;
            label18.Text = string.Empty;
            label19.Text = string.Empty;
            label20.Text = string.Empty;
            label21.Text = string.Empty;

            label15.Enabled = false;
            label16.Enabled = false;
            label17.Enabled = false;
            label18.Enabled = false;
            label19.Enabled = false;
            label20.Enabled = false;
            label21.Enabled = false;
        }

        private void textBox7_TextChanged(object sender, EventArgs e)
        {
            formsPlot1.Reset();
            formsPlot1.Plot.Axes.Left.TickLabelStyle.FontSize = 12;
            formsPlot1.Plot.Axes.Bottom.TickLabelStyle.FontSize = 12;
            formsPlot1.Plot.Axes.Left.Label.Text = "Stress (in MPa)";
            formsPlot1.Plot.Axes.Left.Label.FontSize = 18;
            if (checkBox3.Checked)
                formsPlot1.Plot.Axes.Bottom.Label.Text = "Strain (in %)";
            else
                formsPlot1.Plot.Axes.Bottom.Label.Text = "Strain";
            formsPlot1.Plot.Axes.Bottom.Label.FontSize = 18;
            formsPlot1.Plot.ShowLegend();
            formsPlot1.Plot.Legend.Font.Size = 18;
            formsPlot1.Refresh();
            formsPlot1.Enabled = false;
            checkBox4.Enabled = false;
            checkBox5.Enabled = false;
            checkBox6.Enabled = false;

            label15.Text = string.Empty;
            label16.Text = string.Empty;
            label17.Text = string.Empty;
            label18.Text = string.Empty;
            label19.Text = string.Empty;
            label20.Text = string.Empty;
            label21.Text = string.Empty;

            label15.Enabled = false;
            label16.Enabled = false;
            label17.Enabled = false;
            label18.Enabled = false;
            label19.Enabled = false;
            label20.Enabled = false;
            label21.Enabled = false;
        }
    }
}