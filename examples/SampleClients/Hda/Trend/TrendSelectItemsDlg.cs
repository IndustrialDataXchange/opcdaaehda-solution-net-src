#region Copyright (c) 2011-2022 Technosoftware GmbH. All rights reserved
//-----------------------------------------------------------------------------
// Copyright (c) 2011-2022 Technosoftware GmbH. All rights reserved
// Web: https://technosoftware.com  
// 
// Purpose: 
// 
//
// The Software is subject to the Technosoftware GmbH Source Code License Agreement, 
// which can be found here:
// https://technosoftware.com/documents/Source_License_Agreement.pdf
// 
// The Software is based on the OPC .NET API Sample Code.
//-----------------------------------------------------------------------------
#endregion Copyright (c) 2011-2022 Technosoftware GmbH. All rights reserved

#region Using Directives

using System;
using System.Collections;
using System.Windows.Forms;

using SampleClients.Common;

using Technosoftware.DaAeHdaClient.Hda;

#endregion

namespace SampleClients.Hda.Trend
{	
	/// <summary>
	/// A dialog used to modify the parameters of an item.
	/// </summary>
	public class TrendSelectItemsDlg : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Button cancelBtn_;
		private System.Windows.Forms.Button okBtn_;
		private System.Windows.Forms.Panel buttonsPn_;
		private System.Windows.Forms.Panel mainPn_;
		private TrendItemsCtrl itemsCtrl_;
		private System.ComponentModel.IContainer components = null;

		public TrendSelectItemsDlg()
		{
			// Required for Windows Form Designer support
			InitializeComponent();
            Icon = ClientUtils.GetAppIcon();
        }
		
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			okBtn_ = new System.Windows.Forms.Button();
			cancelBtn_ = new System.Windows.Forms.Button();
			buttonsPn_ = new System.Windows.Forms.Panel();
			mainPn_ = new System.Windows.Forms.Panel();
			itemsCtrl_ = new TrendItemsCtrl();
			buttonsPn_.SuspendLayout();
			mainPn_.SuspendLayout();
			SuspendLayout();
			// 
			// OkBTN
			// 
			okBtn_.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			okBtn_.DialogResult = System.Windows.Forms.DialogResult.OK;
			okBtn_.Location = new System.Drawing.Point(4, 8);
			okBtn_.Name = "okBtn_";
			okBtn_.TabIndex = 1;
			okBtn_.Text = "OK";
			// 
			// CancelBTN
			// 
			cancelBtn_.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			cancelBtn_.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			cancelBtn_.Location = new System.Drawing.Point(248, 8);
			cancelBtn_.Name = "cancelBtn_";
			cancelBtn_.TabIndex = 0;
			cancelBtn_.Text = "Cancel";
			// 
			// ButtonsPN
			// 
			buttonsPn_.Controls.Add(cancelBtn_);
			buttonsPn_.Controls.Add(okBtn_);
			buttonsPn_.Dock = System.Windows.Forms.DockStyle.Bottom;
			buttonsPn_.Location = new System.Drawing.Point(0, 242);
			buttonsPn_.Name = "buttonsPn_";
			buttonsPn_.Size = new System.Drawing.Size(328, 36);
			buttonsPn_.TabIndex = 0;
			// 
			// MainPN
			// 
			mainPn_.Controls.Add(itemsCtrl_);
			mainPn_.Dock = System.Windows.Forms.DockStyle.Fill;
			mainPn_.DockPadding.Left = 4;
			mainPn_.DockPadding.Right = 4;
			mainPn_.DockPadding.Top = 4;
			mainPn_.Location = new System.Drawing.Point(0, 0);
			mainPn_.Name = "mainPn_";
			mainPn_.Size = new System.Drawing.Size(328, 242);
			mainPn_.TabIndex = 32;
			// 
			// ItemsCTRL
			// 
			itemsCtrl_.AllowDrop = true;
			itemsCtrl_.Dock = System.Windows.Forms.DockStyle.Fill;
			itemsCtrl_.Location = new System.Drawing.Point(4, 4);
			itemsCtrl_.Name = "itemsCtrl_";
			itemsCtrl_.Size = new System.Drawing.Size(320, 238);
			itemsCtrl_.TabIndex = 0;
			itemsCtrl_.ItemPicked += new TrendItemsCtrl.ItemPickedEventHandler(ItemsCTRL_ItemPicked);
			// 
			// TrendSelectItemsDlg
			// 
			AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			ClientSize = new System.Drawing.Size(328, 278);
			Controls.Add(mainPn_);
			Controls.Add(buttonsPn_);
			Name = "TrendSelectItemsDlg";
			StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			Text = "Select Items";
			buttonsPn_.ResumeLayout(false);
			mainPn_.ResumeLayout(false);
			ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// Prompts the user to edit the properties of a trend.
		/// </summary>
		public TsCHdaItem[] ShowDialog(TsCHdaTrend trend, ArrayList excludeList)
		{
			if (trend == null) throw new ArgumentNullException("trend");
			
			// initialize the controls.
			itemsCtrl_.Initialize(trend, false, excludeList);

			// show the dialog.
			if (ShowDialog() != DialogResult.OK)
			{
				return null;
			}

			// return selected items.
			return itemsCtrl_.GetItems(true);
		}

		/// <summary>
		/// Called when an item is double clicked in the list.
		/// </summary>
		private void ItemsCTRL_ItemPicked(TsCHdaItem[] items)
		{
			DialogResult = DialogResult.OK;
		}
	}
}
