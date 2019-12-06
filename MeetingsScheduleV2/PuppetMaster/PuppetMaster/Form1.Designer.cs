namespace MeetingsScheduleV2
{
    partial class PuppetMaster
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
            this.instruction = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.script = new System.Windows.Forms.TextBox();
            this.button2 = new System.Windows.Forms.Button();
            this.errorMessage = new System.Windows.Forms.Label();
            this.results = new System.Windows.Forms.ListBox();
            this.label3 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // instruction
            // 
            this.instruction.Location = new System.Drawing.Point(32, 104);
            this.instruction.Name = "instruction";
            this.instruction.Size = new System.Drawing.Size(526, 22);
            this.instruction.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(29, 71);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(73, 17);
            this.label1.TabIndex = 1;
            this.label1.Text = "Instruction";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(157, 132);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(211, 57);
            this.button1.TabIndex = 2;
            this.button1.Text = "execute";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.execute);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(29, 213);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(44, 17);
            this.label2.TabIndex = 4;
            this.label2.Text = "Script";
            // 
            // script
            // 
            this.script.Location = new System.Drawing.Point(31, 233);
            this.script.Name = "script";
            this.script.Size = new System.Drawing.Size(526, 22);
            this.script.TabIndex = 3;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(157, 266);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(211, 57);
            this.button2.TabIndex = 5;
            this.button2.Text = "execute script";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.executeScript);
            // 
            // errorMessage
            // 
            this.errorMessage.AutoSize = true;
            this.errorMessage.ForeColor = System.Drawing.Color.Red;
            this.errorMessage.Location = new System.Drawing.Point(280, 326);
            this.errorMessage.Name = "errorMessage";
            this.errorMessage.Size = new System.Drawing.Size(0, 17);
            this.errorMessage.TabIndex = 6;
            // 
            // results
            // 
            this.results.BackColor = System.Drawing.SystemColors.InfoText;
            this.results.Font = new System.Drawing.Font("Courier New", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.results.ForeColor = System.Drawing.SystemColors.Window;
            this.results.FormattingEnabled = true;
            this.results.ItemHeight = 22;
            this.results.Location = new System.Drawing.Point(564, 58);
            this.results.Name = "results";
            this.results.Size = new System.Drawing.Size(617, 290);
            this.results.TabIndex = 7;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(561, 26);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(55, 17);
            this.label3.TabIndex = 8;
            this.label3.Text = "Results";
            // 
            // PuppetMaster
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1193, 374);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.results);
            this.Controls.Add(this.errorMessage);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.script);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.instruction);
            this.Name = "PuppetMaster";
            this.Text = "MeetingsScheduleV2";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox instruction;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox script;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Label errorMessage;
        private System.Windows.Forms.ListBox results;
        private System.Windows.Forms.Label label3;
    }
}

