
namespace HuiDesktop.NextGen.Pod.ManagerGui
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.Label label1;
            this.readPodsButton = new System.Windows.Forms.Button();
            this.podListBox = new System.Windows.Forms.ListBox();
            this.rootTextBox = new System.Windows.Forms.TextBox();
            this.infoTextBox = new System.Windows.Forms.TextBox();
            label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(252, 418);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(53, 12);
            label1.TabIndex = 3;
            label1.Text = "根路径：";
            // 
            // readPodsButton
            // 
            this.readPodsButton.Location = new System.Drawing.Point(12, 415);
            this.readPodsButton.Name = "readPodsButton";
            this.readPodsButton.Size = new System.Drawing.Size(236, 21);
            this.readPodsButton.TabIndex = 0;
            this.readPodsButton.Text = "读取所有Pod";
            this.readPodsButton.UseVisualStyleBackColor = true;
            this.readPodsButton.Click += new System.EventHandler(this.ReadAllPods);
            // 
            // podListBox
            // 
            this.podListBox.FormattingEnabled = true;
            this.podListBox.ItemHeight = 12;
            this.podListBox.Location = new System.Drawing.Point(12, 12);
            this.podListBox.Name = "podListBox";
            this.podListBox.Size = new System.Drawing.Size(236, 400);
            this.podListBox.TabIndex = 1;
            this.podListBox.SelectedIndexChanged += new System.EventHandler(this.PodListBoxSelectedIndexChanged);
            // 
            // rootTextBox
            // 
            this.rootTextBox.Location = new System.Drawing.Point(311, 415);
            this.rootTextBox.Name = "rootTextBox";
            this.rootTextBox.Size = new System.Drawing.Size(477, 21);
            this.rootTextBox.TabIndex = 2;
            // 
            // infoTextBox
            // 
            this.infoTextBox.Location = new System.Drawing.Point(254, 12);
            this.infoTextBox.Multiline = true;
            this.infoTextBox.Name = "infoTextBox";
            this.infoTextBox.ReadOnly = true;
            this.infoTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.infoTextBox.Size = new System.Drawing.Size(534, 397);
            this.infoTextBox.TabIndex = 4;
            this.infoTextBox.Text = "Welcome!";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.infoTextBox);
            this.Controls.Add(label1);
            this.Controls.Add(this.rootTextBox);
            this.Controls.Add(this.podListBox);
            this.Controls.Add(this.readPodsButton);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button readPodsButton;
        private System.Windows.Forms.ListBox podListBox;
        private System.Windows.Forms.TextBox rootTextBox;
        private System.Windows.Forms.TextBox infoTextBox;
    }
}

