namespace JSRF_ModTool.MDLB_Import
{
    partial class Color
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
            this.txtb_R = new System.Windows.Forms.TextBox();
            this.txtb_G = new System.Windows.Forms.TextBox();
            this.txtb_B = new System.Windows.Forms.TextBox();
            this.txtb_A = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // txtb_R
            // 
            this.txtb_R.Location = new System.Drawing.Point(4, 21);
            this.txtb_R.Name = "txtb_R";
            this.txtb_R.Size = new System.Drawing.Size(34, 20);
            this.txtb_R.TabIndex = 0;
            this.txtb_R.Text = "255";
            this.txtb_R.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.txtb_R.TextChanged += new System.EventHandler(this.txtb_numeric_TextChanged);
            this.txtb_R.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtb_numeric_KeyPress);
            // 
            // txtb_G
            // 
            this.txtb_G.Location = new System.Drawing.Point(43, 21);
            this.txtb_G.Name = "txtb_G";
            this.txtb_G.Size = new System.Drawing.Size(34, 20);
            this.txtb_G.TabIndex = 1;
            this.txtb_G.Text = "255";
            this.txtb_G.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.txtb_G.TextChanged += new System.EventHandler(this.txtb_numeric_TextChanged);
            this.txtb_G.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtb_numeric_KeyPress);
            // 
            // txtb_B
            // 
            this.txtb_B.Location = new System.Drawing.Point(83, 21);
            this.txtb_B.Name = "txtb_B";
            this.txtb_B.Size = new System.Drawing.Size(34, 20);
            this.txtb_B.TabIndex = 2;
            this.txtb_B.Text = "255";
            this.txtb_B.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.txtb_B.TextChanged += new System.EventHandler(this.txtb_numeric_TextChanged);
            this.txtb_B.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtb_numeric_KeyPress);
            // 
            // txtb_A
            // 
            this.txtb_A.Location = new System.Drawing.Point(124, 21);
            this.txtb_A.Name = "txtb_A";
            this.txtb_A.Size = new System.Drawing.Size(34, 20);
            this.txtb_A.TabIndex = 3;
            this.txtb_A.Text = "255";
            this.txtb_A.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.txtb_A.TextChanged += new System.EventHandler(this.txtb_numeric_TextChanged);
            this.txtb_A.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtb_numeric_KeyPress);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(14, 5);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(15, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "R";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(53, 5);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(15, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "G";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(93, 5);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(14, 13);
            this.label3.TabIndex = 1;
            this.label3.Text = "B";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(134, 5);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(14, 13);
            this.label4.TabIndex = 1;
            this.label4.Text = "A";
            // 
            // Color
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtb_A);
            this.Controls.Add(this.txtb_B);
            this.Controls.Add(this.txtb_G);
            this.Controls.Add(this.txtb_R);
            this.Name = "Color";
            this.Size = new System.Drawing.Size(164, 45);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtb_R;
        private System.Windows.Forms.TextBox txtb_G;
        private System.Windows.Forms.TextBox txtb_B;
        private System.Windows.Forms.TextBox txtb_A;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
    }
}
