namespace JSRF_ModTool.MDLB_Import
{
    partial class MDLB_import_options_Form
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btn_import = new System.Windows.Forms.Button();
            this.btn_cancel = new System.Windows.Forms.Button();
            this.panel_models = new System.Windows.Forms.Panel();
            this.lab_main_model_name = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel4 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.label5 = new System.Windows.Forms.Label();
            this.cb_vertex_def_size = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.panel3 = new System.Windows.Forms.Panel();
            this.btn_add_material = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.panel_materials = new System.Windows.Forms.Panel();
            this.panel5 = new System.Windows.Forms.Panel();
            this.label3 = new System.Windows.Forms.Label();
            this.txtb_drawDist_w = new System.Windows.Forms.TextBox();
            this.panel1.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel5.SuspendLayout();
            this.SuspendLayout();
            // 
            // btn_import
            // 
            this.btn_import.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_import.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
            this.btn_import.Location = new System.Drawing.Point(805, 5);
            this.btn_import.Name = "btn_import";
            this.btn_import.Size = new System.Drawing.Size(151, 46);
            this.btn_import.TabIndex = 1;
            this.btn_import.Text = "Import";
            this.btn_import.UseVisualStyleBackColor = true;
            this.btn_import.Click += new System.EventHandler(this.btn_import_Click);
            // 
            // btn_cancel
            // 
            this.btn_cancel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_cancel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
            this.btn_cancel.Location = new System.Drawing.Point(962, 5);
            this.btn_cancel.Name = "btn_cancel";
            this.btn_cancel.Size = new System.Drawing.Size(151, 46);
            this.btn_cancel.TabIndex = 1;
            this.btn_cancel.Text = "Cancel";
            this.btn_cancel.UseVisualStyleBackColor = true;
            this.btn_cancel.Click += new System.EventHandler(this.btn_cancel_Click);
            // 
            // panel_models
            // 
            this.panel_models.AutoScroll = true;
            this.panel_models.BackColor = System.Drawing.SystemColors.ControlLight;
            this.panel_models.Location = new System.Drawing.Point(9, 66);
            this.panel_models.Name = "panel_models";
            this.panel_models.Size = new System.Drawing.Size(442, 601);
            this.panel_models.TabIndex = 2;
            // 
            // lab_main_model_name
            // 
            this.lab_main_model_name.AutoSize = true;
            this.lab_main_model_name.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lab_main_model_name.ForeColor = System.Drawing.Color.White;
            this.lab_main_model_name.Location = new System.Drawing.Point(28, 26);
            this.lab_main_model_name.Name = "lab_main_model_name";
            this.lab_main_model_name.Size = new System.Drawing.Size(111, 16);
            this.lab_main_model_name.TabIndex = 3;
            this.lab_main_model_name.Text = "main_model.smd";
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.ControlDark;
            this.panel1.Controls.Add(this.panel4);
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Controls.Add(this.label5);
            this.panel1.Controls.Add(this.cb_vertex_def_size);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.label4);
            this.panel1.Controls.Add(this.lab_main_model_name);
            this.panel1.Location = new System.Drawing.Point(9, 7);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(442, 51);
            this.panel1.TabIndex = 4;
            // 
            // panel4
            // 
            this.panel4.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.panel4.Location = new System.Drawing.Point(180, -2);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(1, 55);
            this.panel4.TabIndex = 8;
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.panel2.Location = new System.Drawing.Point(304, 1);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(1, 55);
            this.panel2.TabIndex = 7;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.label5.ForeColor = System.Drawing.SystemColors.Control;
            this.label5.Location = new System.Drawing.Point(205, 9);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(69, 13);
            this.label5.TabIndex = 6;
            this.label5.Text = "Model type";
            this.label5.Visible = false;
            // 
            // cb_vertex_def_size
            // 
            this.cb_vertex_def_size.BackColor = System.Drawing.SystemColors.ControlDark;
            this.cb_vertex_def_size.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cb_vertex_def_size.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cb_vertex_def_size.Items.AddRange(new object[] {
            "12",
            "20",
            "24",
            "28",
            "32",
            "36",
            "40",
            "44",
            "48",
            "56"});
            this.cb_vertex_def_size.Location = new System.Drawing.Point(308, 24);
            this.cb_vertex_def_size.Name = "cb_vertex_def_size";
            this.cb_vertex_def_size.Size = new System.Drawing.Size(125, 21);
            this.cb_vertex_def_size.TabIndex = 5;
            this.cb_vertex_def_size.SelectedValueChanged += new System.EventHandler(this.cb_vertex_def_size_SelectedValueChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.label2.ForeColor = System.Drawing.SystemColors.Control;
            this.label2.Location = new System.Drawing.Point(308, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(125, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Vertex definition size";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = System.Drawing.Color.White;
            this.label4.Location = new System.Drawing.Point(34, 4);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(105, 18);
            this.label4.TabIndex = 3;
            this.label4.Text = "Main model: ";
            // 
            // panel3
            // 
            this.panel3.BackColor = System.Drawing.SystemColors.ControlDark;
            this.panel3.Controls.Add(this.btn_add_material);
            this.panel3.Controls.Add(this.label1);
            this.panel3.Location = new System.Drawing.Point(479, 7);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(608, 51);
            this.panel3.TabIndex = 4;
            // 
            // btn_add_material
            // 
            this.btn_add_material.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_add_material.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.btn_add_material.Location = new System.Drawing.Point(532, 0);
            this.btn_add_material.Name = "btn_add_material";
            this.btn_add_material.Size = new System.Drawing.Size(76, 51);
            this.btn_add_material.TabIndex = 4;
            this.btn_add_material.Text = "Add material";
            this.btn_add_material.UseVisualStyleBackColor = true;
            this.btn_add_material.Click += new System.EventHandler(this.btn_add_material_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(22, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 18);
            this.label1.TabIndex = 3;
            this.label1.Text = "Materials";
            // 
            // panel_materials
            // 
            this.panel_materials.AutoScroll = true;
            this.panel_materials.BackColor = System.Drawing.SystemColors.Control;
            this.panel_materials.Location = new System.Drawing.Point(479, 66);
            this.panel_materials.Name = "panel_materials";
            this.panel_materials.Size = new System.Drawing.Size(635, 601);
            this.panel_materials.TabIndex = 2;
            // 
            // panel5
            // 
            this.panel5.BackColor = System.Drawing.SystemColors.ControlDark;
            this.panel5.Controls.Add(this.label3);
            this.panel5.Controls.Add(this.txtb_drawDist_w);
            this.panel5.Controls.Add(this.btn_import);
            this.panel5.Controls.Add(this.btn_cancel);
            this.panel5.Location = new System.Drawing.Point(0, 673);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(1117, 57);
            this.panel5.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.White;
            this.label3.Location = new System.Drawing.Point(12, 22);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(94, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "Draw Distance:";
            // 
            // txtb_drawDist_w
            // 
            this.txtb_drawDist_w.BackColor = System.Drawing.SystemColors.ControlLight;
            this.txtb_drawDist_w.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtb_drawDist_w.Location = new System.Drawing.Point(112, 19);
            this.txtb_drawDist_w.Name = "txtb_drawDist_w";
            this.txtb_drawDist_w.Size = new System.Drawing.Size(67, 20);
            this.txtb_drawDist_w.TabIndex = 2;
            this.txtb_drawDist_w.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.txtb_drawDist_w.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtb_numeric_KeyPress);
            // 
            // MDLB_import_options_Form
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1117, 730);
            this.Controls.Add(this.panel5);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.panel_materials);
            this.Controls.Add(this.panel_models);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "MDLB_import_options_Form";
            this.Text = "Model import options";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MDLB_import_options_Form_FormClosed);
            this.Load += new System.EventHandler(this.MDLB_import_options_Form_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.panel5.ResumeLayout(false);
            this.panel5.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button btn_import;
        private System.Windows.Forms.Button btn_cancel;
        private System.Windows.Forms.Panel panel_models;
        private System.Windows.Forms.Label lab_main_model_name;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panel_materials;
        private System.Windows.Forms.Button btn_add_material;
        private System.Windows.Forms.ComboBox cb_vertex_def_size;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Panel panel5;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtb_drawDist_w;
    }
}