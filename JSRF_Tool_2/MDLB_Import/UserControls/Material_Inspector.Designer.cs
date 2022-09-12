namespace JSRF_ModTool.MDLB_Import
{
    partial class Material_Inspector
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.txtb_hb = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.cb_material_id = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtb_unk_id2 = new System.Windows.Forms.TextBox();
            this.btn_remove = new System.Windows.Forms.Button();
            this.lab_id = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.panel4 = new System.Windows.Forms.Panel();
            this.color_0 = new JSRF_ModTool.MDLB_Import.Color();
            this.txtb_unk_id1 = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtb_hb
            // 
            this.txtb_hb.Location = new System.Drawing.Point(526, 50);
            this.txtb_hb.Name = "txtb_hb";
            this.txtb_hb.Size = new System.Drawing.Size(73, 20);
            this.txtb_hb.TabIndex = 3;
            this.txtb_hb.Text = "0";
            this.txtb_hb.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.txtb_hb.TextChanged += new System.EventHandler(this.txtb_hb_TextChanged);
            this.txtb_hb.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtb_hb_KeyPress);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = System.Drawing.Color.Black;
            this.label4.Location = new System.Drawing.Point(533, 34);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(50, 13);
            this.label4.TabIndex = 1;
            this.label4.Text = "Unk ID 3";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.label2.Location = new System.Drawing.Point(241, 33);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(69, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Material ID";
            // 
            // cb_material_id
            // 
            this.cb_material_id.BackColor = System.Drawing.SystemColors.Control;
            this.cb_material_id.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cb_material_id.Location = new System.Drawing.Point(215, 49);
            this.cb_material_id.Name = "cb_material_id";
            this.cb_material_id.Size = new System.Drawing.Size(121, 21);
            this.cb_material_id.Sorted = true;
            this.cb_material_id.TabIndex = 1;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.Black;
            this.label3.Location = new System.Drawing.Point(451, 34);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(50, 13);
            this.label3.TabIndex = 1;
            this.label3.Text = "Unk ID 2";
            // 
            // txtb_unk_id2
            // 
            this.txtb_unk_id2.Location = new System.Drawing.Point(444, 50);
            this.txtb_unk_id2.Name = "txtb_unk_id2";
            this.txtb_unk_id2.Size = new System.Drawing.Size(73, 20);
            this.txtb_unk_id2.TabIndex = 2;
            this.txtb_unk_id2.Text = "0";
            this.txtb_unk_id2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.txtb_unk_id2.TextChanged += new System.EventHandler(this.txtb_hb_TextChanged);
            this.txtb_unk_id2.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtb_hb_KeyPress);
            // 
            // btn_remove
            // 
            this.btn_remove.Location = new System.Drawing.Point(590, 1);
            this.btn_remove.Name = "btn_remove";
            this.btn_remove.Size = new System.Drawing.Size(19, 22);
            this.btn_remove.TabIndex = 4;
            this.btn_remove.Text = "x";
            this.btn_remove.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btn_remove.UseVisualStyleBackColor = true;
            this.btn_remove.Click += new System.EventHandler(this.btn_remove_Click);
            // 
            // lab_id
            // 
            this.lab_id.AutoSize = true;
            this.lab_id.BackColor = System.Drawing.SystemColors.ControlDark;
            this.lab_id.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lab_id.ForeColor = System.Drawing.Color.White;
            this.lab_id.Location = new System.Drawing.Point(3, 3);
            this.lab_id.Name = "lab_id";
            this.lab_id.Size = new System.Drawing.Size(23, 15);
            this.lab_id.TabIndex = 5;
            this.lab_id.Text = "[0]";
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.ControlDark;
            this.panel1.Controls.Add(this.lab_id);
            this.panel1.Controls.Add(this.btn_remove);
            this.panel1.Location = new System.Drawing.Point(0, -1);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(611, 23);
            this.panel1.TabIndex = 6;
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.SystemColors.ControlDark;
            this.panel2.Location = new System.Drawing.Point(0, 83);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(611, 2);
            this.panel2.TabIndex = 7;
            // 
            // panel3
            // 
            this.panel3.BackColor = System.Drawing.SystemColors.ControlDark;
            this.panel3.Location = new System.Drawing.Point(608, 21);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(2, 64);
            this.panel3.TabIndex = 8;
            // 
            // panel4
            // 
            this.panel4.BackColor = System.Drawing.SystemColors.ControlDark;
            this.panel4.Location = new System.Drawing.Point(0, 21);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(2, 64);
            this.panel4.TabIndex = 9;
            // 
            // color_0
            // 
            this.color_0.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.color_0.Location = new System.Drawing.Point(12, 28);
            this.color_0.Name = "color_0";
            this.color_0.Size = new System.Drawing.Size(166, 47);
            this.color_0.TabIndex = 0;
            // 
            // txtb_unk_id1
            // 
            this.txtb_unk_id1.Location = new System.Drawing.Point(363, 50);
            this.txtb_unk_id1.Name = "txtb_unk_id1";
            this.txtb_unk_id1.Size = new System.Drawing.Size(73, 20);
            this.txtb_unk_id1.TabIndex = 11;
            this.txtb_unk_id1.Text = "0";
            this.txtb_unk_id1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.txtb_unk_id1.TextChanged += new System.EventHandler(this.txtb_hb_TextChanged);
            this.txtb_unk_id1.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtb_hb_KeyPress);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.Black;
            this.label1.Location = new System.Drawing.Point(374, 34);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(50, 13);
            this.label1.TabIndex = 10;
            this.label1.Text = "Unk ID 1";
            // 
            // Material_Inspector
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.txtb_unk_id1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.panel4);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.cb_material_id);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtb_unk_id2);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtb_hb);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.color_0);
            this.Name = "Material_Inspector";
            this.Size = new System.Drawing.Size(610, 85);
            this.Load += new System.EventHandler(this.MaterialProperties_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Color color_0;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btn_remove;
        private System.Windows.Forms.Label lab_id;
        private System.Windows.Forms.Panel panel1;
        public System.Windows.Forms.ComboBox cb_material_id;
        public System.Windows.Forms.TextBox txtb_unk_id2;
        public System.Windows.Forms.TextBox txtb_hb;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Panel panel4;
        public System.Windows.Forms.TextBox txtb_unk_id1;
        private System.Windows.Forms.Label label1;
    }
}
