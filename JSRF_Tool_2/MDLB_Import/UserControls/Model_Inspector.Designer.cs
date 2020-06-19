namespace JSRF_Tool_2.MDLB_Import
{
    partial class Model_Inspector
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
            this.lab_smd_name = new System.Windows.Forms.Label();
            this.cb_mdl_part_type = new System.Windows.Forms.ComboBox();
            this.cb_vertex_def_size = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // lab_smd_name
            // 
            this.lab_smd_name.AutoSize = true;
            this.lab_smd_name.Location = new System.Drawing.Point(4, 6);
            this.lab_smd_name.Name = "lab_smd_name";
            this.lab_smd_name.Size = new System.Drawing.Size(57, 13);
            this.lab_smd_name.TabIndex = 0;
            this.lab_smd_name.Text = "model.smd";
            // 
            // cb_mdl_part_type
            // 
            this.cb_mdl_part_type.BackColor = System.Drawing.SystemColors.Control;
            this.cb_mdl_part_type.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cb_mdl_part_type.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cb_mdl_part_type.Items.AddRange(new object[] {
            "Visual model",
            "Shadow model",
            "Bone"});
            this.cb_mdl_part_type.Location = new System.Drawing.Point(181, 2);
            this.cb_mdl_part_type.Name = "cb_mdl_part_type";
            this.cb_mdl_part_type.Size = new System.Drawing.Size(121, 21);
            this.cb_mdl_part_type.TabIndex = 1;
            this.cb_mdl_part_type.Visible = false;
            // 
            // cb_vertex_def_size
            // 
            this.cb_vertex_def_size.BackColor = System.Drawing.SystemColors.Control;
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
            this.cb_vertex_def_size.Location = new System.Drawing.Point(308, 2);
            this.cb_vertex_def_size.Name = "cb_vertex_def_size";
            this.cb_vertex_def_size.Size = new System.Drawing.Size(125, 21);
            this.cb_vertex_def_size.TabIndex = 6;
            // 
            // Model_Inspector
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.cb_vertex_def_size);
            this.Controls.Add(this.cb_mdl_part_type);
            this.Controls.Add(this.lab_smd_name);
            this.Name = "Model_Inspector";
            this.Size = new System.Drawing.Size(441, 25);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lab_smd_name;
        private System.Windows.Forms.ComboBox cb_mdl_part_type;
        private System.Windows.Forms.ComboBox cb_vertex_def_size;
    }
}
