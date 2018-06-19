namespace WebkitReporter
{
    partial class ReportPreview
    {
        /// <summary>
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다.
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마십시오.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ReportPreview));
            this.panel1 = new System.Windows.Forms.Panel();
            this.C_OpenPdf = new System.Windows.Forms.CheckBox();
            this.B_Pdf = new System.Windows.Forms.Button();
            this.B_Print = new System.Windows.Forms.Button();
            this.ReportView = new WebKit.WebKitBrowser();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.C_OpenPdf);
            this.panel1.Controls.Add(this.B_Pdf);
            this.panel1.Controls.Add(this.B_Print);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(875, 37);
            this.panel1.TabIndex = 3;
            // 
            // C_OpenPdf
            // 
            this.C_OpenPdf.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.C_OpenPdf.AutoSize = true;
            this.C_OpenPdf.Location = new System.Drawing.Point(618, 12);
            this.C_OpenPdf.Name = "C_OpenPdf";
            this.C_OpenPdf.Size = new System.Drawing.Size(81, 16);
            this.C_OpenPdf.TabIndex = 1;
            this.C_OpenPdf.Text = "Open PDF";
            this.C_OpenPdf.UseVisualStyleBackColor = true;
            // 
            // B_Pdf
            // 
            this.B_Pdf.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.B_Pdf.Location = new System.Drawing.Point(705, 8);
            this.B_Pdf.Name = "B_Pdf";
            this.B_Pdf.Size = new System.Drawing.Size(75, 23);
            this.B_Pdf.TabIndex = 0;
            this.B_Pdf.Text = "PDF";
            this.B_Pdf.UseVisualStyleBackColor = true;
            this.B_Pdf.Click += new System.EventHandler(this.B_Pdf_Click);
            // 
            // B_Print
            // 
            this.B_Print.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.B_Print.Location = new System.Drawing.Point(786, 8);
            this.B_Print.Name = "B_Print";
            this.B_Print.Size = new System.Drawing.Size(75, 23);
            this.B_Print.TabIndex = 0;
            this.B_Print.Text = "Print";
            this.B_Print.UseVisualStyleBackColor = true;
            this.B_Print.Click += new System.EventHandler(this.B_Print_Click);
            // 
            // ReportView
            // 
            this.ReportView.BackColor = System.Drawing.Color.White;
            this.ReportView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ReportView.Location = new System.Drawing.Point(0, 37);
            this.ReportView.Name = "ReportView";
            this.ReportView.Size = new System.Drawing.Size(875, 643);
            this.ReportView.TabIndex = 4;
            this.ReportView.Url = null;
            this.ReportView.Load += new System.EventHandler(this.webKitBrowser1_Load);
            this.ReportView.VisibleChanged += new System.EventHandler(ReportView_VisibleChanged);
            // 
            // ReportPreview
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(875, 680);
            this.Controls.Add(this.ReportView);
            this.Controls.Add(this.panel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ReportPreview";
            this.Text = "Preview";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }



        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button B_Print;
        private WebKit.WebKitBrowser ReportView;
        private System.Windows.Forms.Button B_Pdf;
        private System.Windows.Forms.CheckBox C_OpenPdf;

    }
}

